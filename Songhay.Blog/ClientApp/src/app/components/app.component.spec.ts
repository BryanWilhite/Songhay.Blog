import { NO_ERRORS_SCHEMA, DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { BlogEntriesService } from '../services/songhay-blog-entries.service';
import { MatIconRegistry } from '@angular/material';
import { DomSanitizer } from '@angular/platform-browser';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
    const service = jasmine.createSpyObj('BlogEntriesService', [
        BlogEntriesService.loadAppDataMethodName
    ]);

    const matIconRegistryMemberName = 'addSvgIconSetInNamespace';
    const mockMatIconRegistry = jasmine.createSpyObj('MatIconRegistry', [
        matIconRegistryMemberName
    ]);

    const domSanitizerMemberName = 'bypassSecurityTrustResourceUrl';
    const mockDomSanitizer = jasmine.createSpyObj('DomSanitizer', [
        domSanitizerMemberName
    ]);

    let de: DebugElement;
    let component: AppComponent;
    let fixture: ComponentFixture<AppComponent>;

    const initializeComponentAndDetectChanges = function() {
        fixture = TestBed.createComponent(AppComponent);
        fixture.detectChanges();
        component = fixture.componentInstance;
        de = fixture.debugElement;
    };

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            providers: [
                { provide: BlogEntriesService, useValue: service },
                { provide: MatIconRegistry, useValue: mockMatIconRegistry },
                { provide: DomSanitizer, useValue: mockDomSanitizer }
            ],
            declarations: [AppComponent],
            schemas: [NO_ERRORS_SCHEMA]
        })
            .compileComponents()
            .then(initializeComponentAndDetectChanges);
    }));

    it('should be instanced', () => {
        expect(component).not.toBeNull();
        expect(
            mockMatIconRegistry[matIconRegistryMemberName].calls.count()
        ).toBe(1, 'The expected registry calls are not here.');
        expect(mockDomSanitizer[domSanitizerMemberName].calls.count()).toBe(
            1,
            'The expected sanitizer calls are not here.'
        );
    });
    it('should display app title', () => {
        expect(de.query(By.css('.app.title')).nativeElement.innerText).toEqual(
            component.appTitle,
            'The expected app Title is not here.'
        );
    });
    it('should display framework version', () => {
        expect(
            de.query(By.css('.app.footer .framework.version')).nativeElement
                .innerText
        ).toEqual(
            component.clientFrameworkVersion,
            'The expected framework version is not here.'
        );
    });
});
