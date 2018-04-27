import { NO_ERRORS_SCHEMA } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';
import { ActivatedRoute, Params } from '@angular/router';

import { ActivatedRouteMock } from '../../mocks/services/activated-route-mock';

import { AppIndexComponent } from './app-index.component';
import { IndexStyles } from '../../models/songhay-index-styles';

describe('AppIndexComponent', () => {
    const serviceMemberName = 'loadIndex';
    const service = jasmine.createSpyObj('BlogEntriesService', [
        serviceMemberName
    ]);

    let component: AppIndexComponent;
    let fixture: ComponentFixture<AppIndexComponent>;
    let whenStablePromise: Promise<any>;

    let spyOnActivatedRouteMock: jasmine.Spy;

    const initializeComponentAndDetectChanges = function(): Promise<any> {
        fixture = TestBed.createComponent(AppIndexComponent);

        const activatedRouteMock = fixture.debugElement.injector.get(
            ActivatedRoute
        );
        spyOnActivatedRouteMock = spyOn(
            activatedRouteMock.params,
            'subscribe'
        ).and.callThrough();

        fixture.detectChanges();
        component = fixture.componentInstance;
        whenStablePromise = fixture
            .whenStable() // when tasks complete
            .then(() => fixture.detectChanges());
        return whenStablePromise;
    };

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [AppIndexComponent],
            providers: [
                { provide: BlogEntriesService, useValue: service },
                { provide: ActivatedRoute, useClass: ActivatedRouteMock }
            ],
            schemas: [NO_ERRORS_SCHEMA]
        })
            .compileComponents()
            .then(initializeComponentAndDetectChanges);
    }));

    it('should be instanced with routing and service calls onInit', () => {
        expect(component).not.toBeNull();

        expect(spyOnActivatedRouteMock.calls.count()).toBe(
            1,
            'The expected number of route-param subscription calls are not here.'
        );
        expect(service[serviceMemberName].calls.count()).toBe(
            1,
            'The expected number of service calls are not here.'
        );
    });

    it('should set `viewStyle`', () => {
        whenStablePromise.then(() => {
            expect(component.viewStyle).toBe(
                IndexStyles.List,
                'The expected `viewStyle` is not here.'
            );
        });
        const activatedRouteMock = fixture.debugElement.injector.get(
            ActivatedRoute
        ) as any;
        const params = { style: 'list' } as Params;
        activatedRouteMock.nextParams(params);
    });
});
