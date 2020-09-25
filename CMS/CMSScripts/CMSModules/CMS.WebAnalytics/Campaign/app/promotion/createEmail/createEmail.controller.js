(function (angular, _) {

    angular.module('cms.webanalytics/campaign/promotion/createEmail.controller', [])
        .controller('CreateEmailController', controller);

    /*@ngInject*/
    function controller($scope, $q, $uibModalInstance, items, newsletterResource, messagesService, serverDataService) {
        var ctrl = this;

        ctrl.items = items;
        ctrl.EMAIL_REGEXP = serverDataService.getEmailRegexp();
        ctrl.emailCampaignType = _.isEmpty(items) ? 'new' : 'assign';
        ctrl.data = {};

        ctrl.emailPattern = { value: ctrl.EMAIL_REGEXP, display: 'afterSubmission' };
        ctrl.emailCampaignTypeOptions = [
        {
            id: 'new-email-new-campaign',
            label: 'campaign.create.email.select.type.new',
            value: 'new'
        },
        {
            id: 'assign-email-campaign',
            label: 'campaign.create.email.select.type.assign',
            value: 'assign'
        }];


        var campaignsPromise = createCampaignsPromise(),
            templatePromise = createTemplatePromise();

        ctrl.isNewCampaignType = function () {
            return ctrl.emailCampaignType === 'new';
        };

        $q.all([campaignsPromise, templatePromise]).catch(function (error) {
            requestFailed(error);
            $uibModalInstance.dismiss();
        });

        ctrl.dismiss = function () {
            $uibModalInstance.dismiss();
        };

        ctrl.confirm = function () {
            if ($scope.newEmailForm.$invalid) {
                $scope.newEmailForm.$setSubmitted();
                return;
            }

            var email = prepareEmailToCreate();

            createIssueModelPromise(email).then(function (issueModel) {
                ctrl.newIssueModel = issueModel;
            }).catch(function (error) {
                requestFailed(error);
            }).finally(function () {
                $uibModalInstance.close(ctrl.newIssueModel.id);
            });
        };

        function createCampaignsPromise() {
            return newsletterResource.getAllEmailCampaigns().$promise.then(function (data) {
                return newslettersToOptions(data);
            }).then(function (options) {
                return newsletterResource.issues({ issueIds: _.pluck(ctrl.items, 'id') })
                    .$promise
                    .then(function (issueModels) {
                        return createHints(options, issueModels);
                    });
            }).then(function (options) {
                // Assign hints to html select directive
                ctrl.data.emailCampaigns = options;
            });
        }

        function createTemplatePromise() {
            return newsletterResource.templates(function (data) {
                var templates = data.map(function (template) {
                    return {
                        id: template.id,
                        name: template.displayName,
                        type: template.type.toLowerCase()
                    }
                });
                ctrl.data.emailTemplates = _.groupBy(templates, 'type');
            }).$promise;
        }

        function requestFailed(response) {
            if (response && response.data) {
                messagesService.sendError(response.data);
            }
        }

        function newslettersToOptions(newsletters) {
            // Convert to select option format
            return _.map(newsletters, function (newsletter) {
                return {
                    id: newsletter.id,
                    name: newsletter.displayName
                }
            });
        }

        function createHints(options, issueModels) {
            if (!issueModels || !issueModels.length) {
                return options;
            }

            var newsletters = _.pluck(issueModels, 'newsletterId'),
                hints = options.filter(function (option) {
                    return newsletters.indexOf(option.id) >= 0;
                });

            if (!hints || !hints.length) {
                return options;
            }

            hints.push({
                id: 0,
                name: '-------',
                disabled: true
            });

            return hints.concat(options);
        }

        function prepareEmailToCreate() {
            return {
                subject: ctrl.emailSubject,
                displayName: ctrl.emailDisplayName,
                senderName: ctrl.emailSenderName,
                senderEmail: ctrl.emailSenderAddress,
                templates: {
                    issue: ctrl.templateIssue,
                    unsubscription: ctrl.templateUnsubscription
                }
            }
        }

        function createIssueModelPromise(email) {
            if (ctrl.emailCampaignType === 'new') {
                // Create static email campaign and then email
                return newsletterResource
                    .createEmailCampaign(email)
                    .$promise.then(function (newsletterModel) {
                        return createNewIssuePromise(newsletterModel.id, email.subject);
                    });
            }
            else {
                // Create email only
                return createNewIssuePromise(ctrl.emailCampaignSelect, email.subject);
            }
        }

        function createNewIssuePromise(id, subject) {
            return newsletterResource.createNewIssue({ newsletterId: id }, '"' + subject + '"').$promise;
        }
    }

}(angular, _));