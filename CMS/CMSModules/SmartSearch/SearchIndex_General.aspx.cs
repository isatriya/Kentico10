using System;

using CMS.Search;
using CMS.UIControls;

[EditedObject(SearchIndexInfo.OBJECT_TYPE, "indexId")]
public partial class CMSModules_SmartSearch_SearchIndex_General : GlobalAdminPage
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ucIndexInfo.SearchIndex = EditedObject as SearchIndexInfo;

        ucSearchIndexEdit.AsyncIndexTaskStarted += (sender, args) => ucIndexInfo.LoadData();
        ucSearchIndexEdit.OnSaved += UcSearchIndexEdit_OnSaved;
    }


    private void UcSearchIndexEdit_OnSaved(object sender, EventArgs e)
    {
        ucIndexInfo.SearchIndex = EditedObject as SearchIndexInfo;
        ucIndexInfo.LoadData();
    }
}
