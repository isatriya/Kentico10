using System;
using System.Data;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.MediaLibrary.Web.UI;
using CMS.SiteProvider;


public partial class CMSModules_MediaLibrary_Controls_LiveControls_MediaFilePreview : MediaFilePreview
{
    #region "Variables"

    private DataRow mData;


    /// <summary>
    /// Indicates whether control was binded.
    /// </summary>
    private bool binded;

    #endregion


    #region "Properties"

    /// <summary>
    /// Output object width (image/video/flash)
    /// </summary>
    public int Width
    {
        get;
        set;
    }


    /// <summary>
    /// Output object height (image/video/flash)
    /// </summary>
    public int Height
    {
        get;
        set;
    }


    /// <summary>
    /// Output image max side size.
    /// </summary>
    public int MaxSideSize
    {
        get;
        set;
    }

    #endregion


    protected override void OnDataBinding(EventArgs e)
    {
        base.OnDataBinding(e);
        // Get data row
        mData = GetData(this);
    }


    protected override void Render(HtmlTextWriter writer)
    {
        base.Render(writer);
        bool writeHtml = !binded;

        // Reload data
        ReloadData(false);

        if (writeHtml)
        {
            writer.Write(ltlOutput.Text);
        }
    }


    protected void Page_PreRender(object sender, EventArgs e)
    {
        ReloadData(false);
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    public void ReloadData(bool forceReload)
    {
        if (binded && !forceReload)
        {
            return;
        }

        if (mData != null)
        {
            MediaFileInfo mfi = new MediaFileInfo(mData);
            SiteInfo currentSite = SiteContext.CurrentSite;
            
            SiteInfo si = mfi.FileSiteID == currentSite.SiteID ? currentSite : SiteInfoProvider.GetSiteInfo(mfi.FileSiteID);
            if (si != null)
            {
                if (DisplayActiveContent)
                {
                    string url = "";
                    
                    // Complete URL only for other site than current
                    bool completeUrl = (si.SiteID != currentSite.SiteID);
                    
                    if (UseSecureLinks)
                    {
                        url = completeUrl 
                            ? MediaFileInfoProvider.GetMediaFileAbsoluteUrl(si.SiteName, mfi.FileGUID, mfi.FileName) 
                            : MediaFileInfoProvider.GetMediaFileUrl(mfi.FileGUID, mfi.FileName);
                    }
                    else
                    {
                        MediaLibraryInfo li = MediaLibraryInfoProvider.GetMediaLibraryInfo(mfi.FileLibraryID);
                        if (li != null)
                        {
                            url = completeUrl 
                                ? MediaFileInfoProvider.GetMediaFileAbsoluteUrl(si.SiteName, li.LibraryFolder, mfi.FilePath) 
                                : MediaFileInfoProvider.GetMediaFileUrl(si.SiteName, li.LibraryFolder, mfi.FilePath);
                        }
                    }

                    url = UrlResolver.ResolveUrl(url);

                    if (ImageHelper.IsImage(mfi.FileExtension) && File.Exists(MediaFileInfoProvider.GetMediaFilePath(mfi.FileLibraryID, mfi.FilePath)))
                    {
                        // Get new dimensions
                        int[] newDims = ImageHelper.EnsureImageDimensions(Width, Height, MaxSideSize, mfi.FileImageWidth, mfi.FileImageHeight);

                        // If dimensions changed use secure link
                        if ((newDims[0] != mfi.FileImageWidth) || (newDims[1] != mfi.FileImageHeight))
                        {
                            url = completeUrl 
                                ? MediaFileInfoProvider.GetMediaFileAbsoluteUrl(si.SiteName, mfi.FileGUID, mfi.FileName) 
                                : MediaFileInfoProvider.GetMediaFileUrl(mfi.FileGUID, mfi.FileName);

                            url = UrlResolver.ResolveUrl(url);
                        }
                        else
                        {
                            // Use width and height from properties in case dimensions are bigger than original
                            newDims[0] = Width;
                            newDims[1] = Height;
                        }

                        // Initialize image parameters
                        ImageParameters imgParams = new ImageParameters();
                        imgParams.Alt = mfi.FileDescription;
                        imgParams.Width = newDims[0];
                        imgParams.Height = newDims[1];
                        imgParams.Url = url;

                        ltlOutput.Text = MediaHelper.GetImage(imgParams);
                    }
                    else if (MediaHelper.IsFlash(mfi.FileExtension))
                    {
                        // Initialize flash parameters
                        FlashParameters flashParams = new FlashParameters();
                        flashParams.Url = url;
                        flashParams.Width = Width;
                        flashParams.Height = Height;

                        ltlOutput.Text = MediaHelper.GetFlash(flashParams);
                    }
                    else if (MediaHelper.IsAudio(mfi.FileExtension))
                    {
                        // Initialize audio/video parameters
                        AudioVideoParameters audioParams = new AudioVideoParameters();

                        audioParams.SiteName = SiteContext.CurrentSiteName;
                        audioParams.Url = url;
                        audioParams.Width = Width;
                        audioParams.Height = Height;
                        audioParams.Extension = mfi.FileExtension;

                        ltlOutput.Text = MediaHelper.GetAudioVideo(audioParams);
                    }
                    else if (MediaHelper.IsVideo(mfi.FileExtension))
                    {
                        // Initialize audio/video parameters
                        AudioVideoParameters videoParams = new AudioVideoParameters();

                        videoParams.SiteName = SiteContext.CurrentSiteName;
                        videoParams.Url = url;
                        videoParams.Width = Width;
                        videoParams.Height = Height;
                        videoParams.Extension = mfi.FileExtension;

                        ltlOutput.Text = MediaHelper.GetAudioVideo(videoParams);
                    }
                    else
                    {
                        ltlOutput.Text = ShowPreviewOrIcon(mfi, Width, Height, MaxSideSize, PreviewSuffix, IconSet, Page);
                    }
                }
                else
                {
                    ltlOutput.Text = ShowPreviewOrIcon(mfi, Width, Height, MaxSideSize, PreviewSuffix, IconSet, Page);
                }
            }
        }
        binded = true;
    }


    #region "Private methods"

    /// <summary>
    /// Returns DataRow from current binding item.
    /// </summary>
    /// <param name="ctrl">Control</param>
    private DataRow GetData(Control ctrl)
    {
        while (ctrl != null)
        {
            if (ctrl is IDataItemContainer)
            {
                return ((DataRowView)((IDataItemContainer)ctrl).DataItem).Row;
            }
            ctrl = ctrl.Parent;
        }
        return null;
    }

    #endregion
}