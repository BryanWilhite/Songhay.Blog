import { NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

import { AppIndexGroupsHostMockComponent } from '../../mocks/components/app-index-groups-host-mock';
import { AppIndexGroupsComponent } from './app-index-groups.component';

describe(AppIndexGroupsComponent.name, () => {
    const service = jasmine.createSpyObj(BlogEntriesService.name, [BlogEntriesService.filterEntriesMethodName]);
    service.index = null;

    let component: AppIndexGroupsComponent;
    let componentHost: AppIndexGroupsHostMockComponent;
    let fixtureHost: ComponentFixture<AppIndexGroupsHostMockComponent>;
    let whenStablePromise: Promise<any>;

    const initializeComponentAndDetectChanges = function (): Promise<any> {
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
            providers: [{ provide: BlogEntriesService, useValue: service }],
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
