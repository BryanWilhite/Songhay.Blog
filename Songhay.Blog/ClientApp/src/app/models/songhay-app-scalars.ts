/**
 * centralizes magic strings in this App
 *
 * @export
 * @class AppScalars
 */
export class AppScalars {
    /**
     * location of App data
     *
     * @static
     * @memberof AppScalars
     */
    static appDataLocation = './assets/data/app.json';

    /**
     * location of Blog entries API
     *
     * @static
     * @memberof AppScalars
     */
    static baseApiRoute = './api/blog';

    /**
     * location of Blog entries search API
     *
     * @static
     * @memberof AppScalars
     */
    static baseApiSearchRoute = './api/search/blog';

    /**
     * location of App server metadata
     *
     * @static
     * @memberof AppScalars
     */
    static serverMetaLocation = './assets/data/server-meta.json';
}
