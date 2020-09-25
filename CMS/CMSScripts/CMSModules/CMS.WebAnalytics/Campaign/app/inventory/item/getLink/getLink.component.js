(function (angular) {
    'use strict';

    angular.module('cms.webanalytics/campaign/inventory/item/getLink.component', [
        'cms.webanalytics/campaign/dialogHeader.component',
        'cms.webanalytics/campaign/dialogFooter.component',
        'cms.webanalytics/campaign/cms.urlHelper.service'
    ])
        .component('cmsGetLink', getLink());


    function getLink() {
        return {
            bindings: {
                assetLink: '<',
                utmCode: '<'
            },
            templateUrl: 'cms.webanalytics/campaign/inventory/item/getLink/getLink.component.html',
            controller: controller,
            replace: true
        };
    }

    /*@ngInject*/
    function controller($uibModal) {
        var ctrl = this;

        ctrl.openDialog = function () {
            $uibModal.open({
                size: {width: '60%', height: '70%'},
                templateUrl: 'cms.webanalytics/campaign/inventory/item/getLink/getLink.dialog.html',
                bindToController: true,
                controllerAs: '$ctrl',
                controller: modalController,
                resolve: {
                    assetLink: function () { return ctrl.assetLink; },
                    utmCampaign: function () { return ctrl.utmCode; }
                }
            });
        };
    }

    /*@ngInject*/
    function modalController($uibModalInstance, assetLink, utmCampaign, resolveFilter, urlHelperService) {
        var ctrl = this,
            urlHelper = urlHelperService.urlHelper;

        ctrl.emptyUtmSourceText = resolveFilter("campaign.getcontentlink.dialog.emptyutmsource");
        ctrl.link = ctrl.emptyUtmSourceText;

        ctrl.dismiss = function () {
            $uibModalInstance.dismiss();
        };

        ctrl.onChange = function () {
            ctrl.link = buildLink(assetLink, utmCampaign, ctrl.utmSource, ctrl.utmMedium);
        };

        ctrl.textAreaClick = function (event) {
            event.target.select();
        };

        function buildLink(originalLink, utmCampaign, utmSource, utmMedium) {
            if (!utmSource) {
                return ctrl.emptyUtmSourceText;
            }

            var originalLinkWithoutQueryString = urlHelper.removeQueryString(originalLink),
                queryParams = urlHelper.getParameters(originalLink);

            if (utmCampaign) {
                queryParams.utm_campaign = utmCampaign;
            }

            queryParams.utm_source = utmSource;

            if (utmMedium) {
                queryParams.utm_medium = utmMedium;
            }

            return originalLinkWithoutQueryString + urlHelper.buildQueryString(queryParams);
        }
    }
}(angular));
