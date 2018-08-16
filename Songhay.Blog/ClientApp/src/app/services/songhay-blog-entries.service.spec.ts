import { TestBed, getTestBed } from '@angular/core/testing';
import {
    BaseRequestOptions,
    Http,
    HttpModule,
    Response,
    XHRBackend
} from '@angular/http';
import { AppScalars } from '../models/songhay-app-scalars';
import { BlogEntriesService } from './songhay-blog-entries.service';
import { MathUtility } from './songhay-math.utility';

describe('BlogEntriesService', () => {
    const math: MathUtility = new MathUtility();
    const testBed: TestBed = getTestBed();
    let service: BlogEntriesService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                BaseRequestOptions,
                BlogEntriesService,
                {
                    deps: [XHRBackend, BaseRequestOptions],
                    provide: Http,
                    useFactory: (
                        backend: XHRBackend,
                        defaultOptions: BaseRequestOptions
                    ) => new Http(backend, defaultOptions)
                }
            ],
            imports: [HttpModule]
        });
    });

    it('should load App data', done => {
        AppScalars.appDataLocation = './base/src/assets/data/app.json';

        service = testBed.get(BlogEntriesService);
        expect(service).not.toBeNull();

        service
            .loadAppData()
            .then(responseOrVoid => {
                const response = responseOrVoid as Response;
                expect(response).toBeDefined(
                    'The expected response is not defined.'
                );
                expect(response).not.toBeNull(
                    'The expected response is not here.'
                );
                expect(response.ok).toBe(
                    true,
                    'The expected OK response is not here.'
                );

                expect(service.isError).toEqual(
                    false,
                    'Service in error state is unexpected.'
                );
                expect(service.isLoaded).toEqual(
                    true,
                    'The expected Service loaded state is not here.'
                );
                expect(service.isLoading).toEqual(
                    false,
                    'The expected Service loading state is not here.'
                );

                expect(service.index).not.toBeNull(
                    'The expected Service Index is not here.'
                );
                expect(service.index.length).toBeGreaterThan(
                    0,
                    'The expected Service Index entries are not here.'
                );

                const i = math.getRandom(0, service.index.length);
                console.log(`service.index[${i}]`, service.index[i]);

                done();
            })
            .catch(response => {
                console.log(`${BlogEntriesService.loadAppDataMethodName}() catch response: `, response);

                done();
            });
    });

    it('should load entry from live service (when available)', done => {

        AppScalars.baseApiRoute = 'https://songhayblog-staging.azurewebsites.net/api/blog';

        service = testBed.get(BlogEntriesService);
        expect(service).not.toBeNull();

        const slug = 'asp-net-web-api-ready-state-4-2017';

        service
            .loadEntry(slug)
            .then(responseOrVoid => {
                const response = responseOrVoid as Response;
                expect(response).toBeDefined(
                    'The expected response is not defined.'
                );
                expect(response).not.toBeNull(
                    'The expected response is not here.'
                );
                expect(response.ok).toEqual(
                    true,
                    'The expected OK response is not here.'
                );

                expect(service.isError).toEqual(
                    false,
                    'Service in error state is unexpected.'
                );
                expect(service.isLoaded).toEqual(
                    true,
                    'The expected Service loaded state is not here.'
                );
                expect(service.isLoading).toEqual(
                    false,
                    'The expected Service loading state is not here.'
                );

                expect(service.entry).toBeDefined(
                    'The expected Blog Entry is not defined.'
                );

                expect(service.entry).not.toBeNull(
                    'The expected Blog Entry is not here.'
                );

                expect(service.entry.slug).toBe(
                    slug,
                    'The expected Blog Entry Slug is not here.'
                );

                done();
            })
            .catch(response => {
                console.log('loadEntry() catch response: ', response);
                const progress = response.json() as ProgressEvent;
                if (progress != null && progress.type === 'error') {
                    console.log(
                        'loadEntry() catch ProgressEvent: there was an error:',
                        progress
                    );
                }

                done();
            });
    });
});
