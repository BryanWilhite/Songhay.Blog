import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AppBlogEntryComponent } from './app-blog-entry.component';

describe('AppBlogEntryComponent', () => {
    let component: AppBlogEntryComponent;
    let fixture: ComponentFixture<AppBlogEntryComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [AppBlogEntryComponent]
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
