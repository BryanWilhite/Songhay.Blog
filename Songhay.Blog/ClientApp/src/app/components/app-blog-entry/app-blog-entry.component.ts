import { Component, OnInit } from '@angular/core';
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
        private route: ActivatedRoute
    ) {}

    private slug: string;

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.slug = params['slug'] as string;
        });
    }
}
