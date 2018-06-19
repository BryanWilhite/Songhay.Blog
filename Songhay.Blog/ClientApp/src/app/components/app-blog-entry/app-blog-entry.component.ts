import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

@Component({
    selector: 'app-app-blog-entry',
    templateUrl: './app-blog-entry.component.html',
    styleUrls: ['./app-blog-entry.component.scss']
})
export class AppBlogEntryComponent implements OnInit {
    constructor(
        public indexService: BlogEntriesService,
        private route: ActivatedRoute,
        private sanitizer: DomSanitizer
    ) {}

    private slug: string;
    private trustedContent: SafeHtml;
    private trustedTitle: SafeHtml;

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.slug = params['slug'] as string;
            this.indexService.loadEntry(this.slug).then(() => {
                this.trustedContent = this.sanitizer.bypassSecurityTrustHtml(
                    this.indexService.entry.content
                );
                this.trustedTitle = this.sanitizer.bypassSecurityTrustHtml(
                    this.indexService.entry.title
                );
            });
        });
    }
}
