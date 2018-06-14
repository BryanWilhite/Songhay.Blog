import { TestBed, getTestBed } from '@angular/core/testing';
import {
    BaseRequestOptions,
    Http,
    HttpModule,
    Response,
    XHRBackend
} from '@angular/http';
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

    it('should load index', done => {
        service = testBed.get(BlogEntriesService);
        service.indexLocation = './base/src/assets/data/index.json';
        expect(service).not.toBeNull();
        service
            .loadIndex()
            .catch(response => {
                done();
                console.log('loadIndex() catch response: ', response);
            })
            .then(responseOrVoid => {
                const response = <Response>responseOrVoid;
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

                done();

                expect(service.isError).toBe(
                    false,
                    'Service in error state is unexpected.'
                );
                expect(service.isLoaded).toBe(
                    true,
                    'The expected Service loaded state is not here.'
                );
                expect(service.isLoading).toBe(
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
            })
            ;
    });

    it('should load entry from live service (when available)', done => {
        service = testBed.get(BlogEntriesService);
        service.baseApiRoute =
            'https://songhayblog-staging.azurewebsites.net/api/blog';
        expect(service).not.toBeNull();

        const slug = 'asp-net-web-api-ready-state-4-2017';

        service
            .loadEntry(slug)
            .then(responseOrVoid => {
                const response = <Response>responseOrVoid;
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

                done();

                expect(service.isError).toBe(
                    false,
                    'Service in error state is unexpected.'
                );
                expect(service.isLoaded).toBe(
                    true,
                    'The expected Service loaded state is not here.'
                );
                expect(service.isLoading).toBe(
                    false,
                    'The expected Service loading state is not here.'
                );

                console.log('then response: ', response);
            })
            .catch(response => {
                done();
                console.log('loadEntry() catch response: ', response);
            })
            ;
    });
});
