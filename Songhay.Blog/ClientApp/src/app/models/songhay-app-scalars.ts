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
    static appDataLocation = 'https://songhaystorage.blob.core.windows.net/day-path-blog/app.json';

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
}
