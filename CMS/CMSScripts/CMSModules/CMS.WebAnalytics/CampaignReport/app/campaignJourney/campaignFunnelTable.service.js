(function (angular, _) {
    'use strict';

    angular.module('cms.webanalytics/campaignreport/campaignFunnelTable.service', [])
        .service('campaignFunnelTableService', funnelTableService);

    function funnelTableService() {
        var that = this;

        /* Transforms data organized by conversions to data organized by source name */
        that.initTableData = function (conversions, sourceDetails) {
            if (!sourceDetails || !conversions || !conversions.length) {
                return [];
            }

            /* For each source name: collect info about that source from all campaigns, not all sources are relavant (have hits) for funnel conversion */
            /* Filer only sources with hits for funnel conversions */
            return sourceDetails.map(function (sourceDetail) {
                var sourceName = sourceDetail.name;
                var sourceHits = getSourceHits(conversions, sourceName);

                return {
                    name: sourceName,
                    hits: sourceHits,
                    conversionRate: getConversionRate(sourceHits[0], sourceHits[sourceHits.length - 1]),
                    link: sourceDetail.details
                };
            }).filter(function(source) {
                return source.hits.some(isBiggerThan0);
            });
        };

        /* Returns a number 0.0 - 1.0 calculated as last conversion hits to first conversion hits ratio */
        that.getTotalConversionRate = function (conversions) {
            if (conversions && conversions.length) {
                var first = conversions[0].hits;
                var last = conversions[conversions.length - 1].hits;

                return getConversionRate(first, last);
            }

            return 0;
        };

        /* PhantomJS used in tests do not support lambda syntax, that is why this test is implemented as a method. */
        function isBiggerThan0(element) {
            return element > 0;
        }
        
        function getConversionRate(first, last) {
            if (first === 0) {
                return 0;
            }

            return (last || 0) / first;
        }

        /* Returns number of conversion hits from given source */
        function findConversionSourceHits(conversion, sourceName) {
            var source = _.find(conversion.sources, function (source) {
                return source.name === sourceName;
            });

            return source ? source.hits : 0;
        }

        /* Returns array of hit counts for given array of conversion. First number in result corresponds to first conversion etc. */
        function getSourceHits(conversions, sourceName) {
            return conversions.map(function (conversion) {
                return findConversionSourceHits(conversion, sourceName);
            });
        }
    }
}(angular, _));