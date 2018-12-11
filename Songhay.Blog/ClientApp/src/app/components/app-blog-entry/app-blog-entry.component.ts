import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

@Component({
    selector: 'app-blog-entry',
    templateUrl: './app-blog-entry.component.html',
    styleUrls: ['./app-blog-entry.component.scss']
})
export class AppBlogEntryComponent implements OnInit {
    constructor(
        public indexService: BlogEntriesService,
        private route: ActivatedRoute,
        private sanitizer: DomSanitizer
    ) {}

    trustedContent: SafeHtml;
    trustedTitle: SafeHtml;

    private slug: string;

    ngOnInit() {
        this.route.params.subscribe(params => {
            // const entryLocation = './assets/data/asp-net-core-angular-client-app-q2-2018.json';
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
