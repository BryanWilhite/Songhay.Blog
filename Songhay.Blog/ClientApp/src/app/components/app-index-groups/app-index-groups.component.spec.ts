import { NO_ERRORS_SCHEMA } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';

import {
    BaseRequestOptions,
    Http,
    HttpModule,
    XHRBackend
} from '@angular/http';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

import { AppIndexGroupsHostMockComponent } from '../../mocks/components/app-index-groups-host-mock';
import { AppIndexGroupsComponent } from './app-index-groups.component';

describe('AppIndexGroupsComponent', () => {
    let component: AppIndexGroupsComponent;
    let componentHost: AppIndexGroupsHostMockComponent;
    let fixtureHost: ComponentFixture<AppIndexGroupsHostMockComponent>;
    let whenStablePromise: Promise<any>;

    const initializeComponentAndDetectChanges = function(): Promise<any> {
        fixtureHost = TestBed.createComponent(AppIndexGroupsHostMockComponent);
        componentHost = fixtureHost.componentInstance;
        component = componentHost.hostedComponent;

        fixtureHost.detectChanges();

        whenStablePromise = fixtureHost
            .whenStable() // when tasks complete
            .then(() => fixtureHost.detectChanges());
        return whenStablePromise;
    };

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                AppIndexGroupsHostMockComponent,
                AppIndexGroupsComponent
            ],
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
            imports: [HttpModule],
            schemas: [NO_ERRORS_SCHEMA]
        })
            .compileComponents()
            .then(initializeComponentAndDetectChanges);
    }));

    it('should be instanced with ngOnInit() procedures complete', () => {
        expect(component).not.toBeUndefined(
            'The hosted component is expected to be defined.'
        );
        expect(component).not.toBeNull(
            'The expected hosted component is not here.'
        );
        expect(component.indexGroupingOptions).not.toBeNull(
            'The expected `indexGroupingOptions` is not here.'
        );
        expect(component.indexGroupingOptions.length).toBe(
            2,
            'The expected number of `indexGroupingOptions` is not here.'
        );
        expect(component.indexFormGroup).not.toBeUndefined(
            'The `indexFormGroup` is expected to be defined.'
        );
        expect(component.indexFormGroup).not.toBeNull(
            'The expected `indexFormGroup` is not here.'
        );
        expect(component.indexGroups$).not.toBeUndefined(
            'The `indexGroups$` is expected to be defined.'
        );
        expect(component.indexGroups$).not.toBeNull(
            'The expected `indexGroups$` is not here.'
        );
    });
});
