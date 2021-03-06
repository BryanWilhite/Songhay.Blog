import { Component, OnInit, VERSION } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material';

import { BlogEntriesService } from '../services/songhay-blog-entries.service';

/**
 * top-level App component
 *
 * @export
 * @class AppComponent
 */
@Component({
    selector: 'app-root',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
    /**
     * App title
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
    clientFrameworkVersion: string;

    /**
     * server info
     *
     * @type {string}
     * @memberof AppComponent
     */
    serverAssemblyInfo: string;

    /**
     * server version
     *
     * @type {string}
     * @memberof AppComponent
     */
    serverAssemblyVersion: string;

    /**
     * Creates an instance of AppComponent.
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

        this.clientFrameworkVersion = `${VERSION.major}.${VERSION.minor}.${
            VERSION.patch
        }`;
    }

    /**
     * implements OnInit
     *
     * @memberof AppComponent
     */
    ngOnInit(): void {
        this.indexService.appDataLoaded.subscribe(() => {
            this.serverAssemblyInfo = `${
                this.indexService.assemblyInfo.assemblyTitle
            } ${this.indexService.assemblyInfo.assemblyVersion} ${
                this.indexService.assemblyInfo.assemblyCopyright
            }`;
            this.serverAssemblyVersion =
                this.indexService.assemblyInfo.assemblyVersion;
        });
    }
}
