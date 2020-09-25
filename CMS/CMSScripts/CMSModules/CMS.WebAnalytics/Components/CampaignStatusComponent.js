cmsdefine(['CMS/Application'], function (app) {

    'use strict';

    return {
        bindings: {
            status: '<'
        },
        replace : true,
        templateUrl: app.getData('applicationUrl') + 'CMSScripts/CMSModules/CMS.WebAnalytics/Components/CampaignStatusComponent.html'
    };
});