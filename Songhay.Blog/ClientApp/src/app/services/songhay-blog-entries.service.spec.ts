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
            .catch(response => expect(response).toBeUndefined())
            .then(responseOrVoid => {
                const response = <Response>responseOrVoid;
                expect(response).not.toBeNull();
                expect(response.ok).toBe(true);

                done();

                expect(service.isError).toBe(false);
                expect(service.isLoaded).toBe(true);
                expect(service.isLoading).toBe(false);
                expect(service.index).not.toBeNull();

                expect(service.index).not.toBeNull();
                expect(service.index.length).toBeGreaterThan(0);

                const i = math.getRandom(0, service.index.length);
                console.log(`service.index[${i}]`, service.index[i]);
            });
    });
});
