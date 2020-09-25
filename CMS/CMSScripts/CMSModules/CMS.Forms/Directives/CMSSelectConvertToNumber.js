cmsdefine([], function() {

    return [function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function(scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (val) {
                    var parsed = parseInt(val, 10);
                    return isNaN(parsed) ? val : parsed;
                });

                ngModel.$formatters.push(function(val) {
                    return '' + val;
                });

                ngModel.$isEmpty = function (value) {
                    var parsed = parseInt(value, 10);
                    return isNaN(parsed) ? !value : parsed === 0;
                }
            }
        }
    }];
});