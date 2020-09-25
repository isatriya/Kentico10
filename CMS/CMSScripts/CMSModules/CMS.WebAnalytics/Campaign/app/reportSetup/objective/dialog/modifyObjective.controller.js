(function (angular) {
    'use strict';

    angular.module('cms.webanalytics/campaign/reportSetup/objective/modifyObjective.controller', [
        'cms.webanalytics/campaign/cms.application.service'
    ])
        .controller('ModifyObjectiveController', controller);

    /*@ngInject*/
    function controller($uibModalInstance, objectiveConversions, objective, title) {
        var ctrl = this;

        ctrl.objective = angular.copy(objective || { conversionID: 0 });
        ctrl.objectiveConversions = prepareObjectiveConversions(objectiveConversions, ctrl.objective.conversionID);
        ctrl.title = title;
        ctrl.targetRegexPattern = '^[1-9][0-9]*$';
        

        ctrl.dismiss = function () {
            $uibModalInstance.dismiss();
        };

        ctrl.confirm = function () {
            ctrl.form.$setSubmitted();            
            if (ctrl.form.$valid) {

                if (!objectiveChanged()) {
                    $uibModalInstance.dismiss();
                }

                $uibModalInstance.close(ctrl.objective);
            }
        };

        function objectiveChanged() {
            return !objective
                || (objective.conversionID !== ctrl.objective.conversionID)
                || (objective.value !== ctrl.objective.value);
        }

        function prepareObjectiveConversions(objectiveConversions, conversionID) {
            var conversions = objectiveConversions.filter(function (conversion) {
                return !conversion.isFunnelStep;
            }).map(function (conversion) {
                var conversionName = conversion.activityName;
                if (conversion.name !== '') {
                    conversionName = conversionName + ': ' + conversion.name;
                }

                return {
                    id: conversion.id,
                    name: conversionName,
                    selected: conversion.id === conversionID
                }
            });

            conversions.unshift({
                id: 0,
                name: 'Select',
                selected: conversionID === 0
            });

            return conversions;
        }
    }
}(angular));