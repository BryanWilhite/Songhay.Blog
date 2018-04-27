/**
 * Defines index grouping options
 * for drop-down/select list UX.
 *
 * @export
 * @class IndexGroupingOptions
 */
export class IndexGroupingOption {
    /**
     * the display name in the UI
     *
     * @type {string}
     * @memberof IndexGroupingOptions
     */
    displayName: string;
    /**
     * the property name
     * of the index backing object
     * to group by
     *
     * @type {string}
     * @memberof IndexGroupingOptions
     */
    groupByPropertyName: string;
    /**
     * sort the group in descending order?
     *
     * @type {boolean}
     * @memberof IndexGroupingOptions
     */
    sortDescending: boolean;
}
