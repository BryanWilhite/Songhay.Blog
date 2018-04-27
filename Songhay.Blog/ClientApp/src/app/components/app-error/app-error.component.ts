import { Component } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material';

@Component({
    selector: 'app-error',
    styleUrls: ['./app-error.component.scss'],
    templateUrl: './app-error.component.html'
})
export class AppErrorComponent {
    constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
        iconRegistry.addSvgIconSetInNamespace(
            'rx',
            sanitizer.bypassSecurityTrustResourceUrl('assets/svg/sprites.svg')
        );
    }
}
