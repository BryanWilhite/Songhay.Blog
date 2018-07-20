import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

@Component({
    selector: 'app-search',
    templateUrl: './app-search.component.html',
    styleUrls: ['./app-search.component.scss']
})
export class AppSearchComponent implements OnInit {
    constructor(
        public indexService: BlogEntriesService,
        private route: ActivatedRoute
    ) {}

    private searchTerm: string;

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.searchTerm = params['searchTerm'] as string;
        });
    }
}
