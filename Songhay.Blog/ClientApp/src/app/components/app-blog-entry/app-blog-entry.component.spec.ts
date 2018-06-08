import { NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import {
    BaseRequestOptions,
    Http,
    HttpModule,
    XHRBackend
} from '@angular/http';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

import { ActivatedRoute, Params } from '@angular/router';
import { ActivatedRouteMock } from '../../mocks/services/activated-route-mock';
import { AppBlogEntryComponent } from './app-blog-entry.component';

describe('AppBlogEntryComponent', () => {
    let component: AppBlogEntryComponent;
    let fixture: ComponentFixture<AppBlogEntryComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [AppBlogEntryComponent],
            providers: [
                { provide: ActivatedRoute, useClass: ActivatedRouteMock },
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
        }).compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(AppBlogEntryComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
