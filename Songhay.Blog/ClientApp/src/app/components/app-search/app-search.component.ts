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

    currentPage: number;
    totalSetSize: number;

    private pagingJson: any;
    private searchTerm: string;
    private skipValue: number;

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.currentPage = 1;
            this.skipValue = this.currentPage - 1;
            this.totalSetSize = 0;
            this.searchTerm = params['searchTerm'] as string;
            if (!this.searchTerm) {
                return;
            }
            this.indexService
                .search(this.searchTerm, this.skipValue)
                .then(response => {
                    this.pagingJson = response.json();
                    this.totalSetSize = this.pagingJson['@odata.count'];
                });
        });
    }

    pageChanged(pageNumber: number) {
        this.currentPage = pageNumber;
        this.skipValue = this.currentPage - 1;
        this.indexService
            .search(this.searchTerm, this.skipValue)
            .then(response => (this.pagingJson = response.json()));
    }
}
