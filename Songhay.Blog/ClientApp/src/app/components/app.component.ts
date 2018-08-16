import { Component, OnInit, VERSION } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material';

import { BlogEntriesService } from '../services/songhay-blog-entries.service';

/**
 * top-level App component
 *
 * @export
 * @class AppComponent
 * @implements {OnInit}
 */
@Component({
    selector: 'app-root',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html'
})
export class AppComponent {
    /**
     * App Title
     *
     * @type {string}
     * @memberof AppComponent
     */
    appTitle: string;

    /**
     * App version
     *
     * @type {string}
     * @memberof AppComponent
     */
    frameworkVersion: string;

    /**
     *Creates an instance of AppComponent.
     * @param {MatIconRegistry} iconRegistry
     * @param {BlogEntriesService} indexService
     * @param {DomSanitizer} sanitizer
     * @memberof AppComponent
     */
    constructor(
        public indexService: BlogEntriesService,
        iconRegistry: MatIconRegistry,
        sanitizer: DomSanitizer
    ) {
        iconRegistry.addSvgIconSetInNamespace(
            'rx',
            sanitizer.bypassSecurityTrustResourceUrl('assets/svg/sprites.svg')
        );

        this.appTitle = '>Day Path_';
        this.frameworkVersion = `${VERSION.major}.${VERSION.minor}.${VERSION.patch}`;
    }
}
