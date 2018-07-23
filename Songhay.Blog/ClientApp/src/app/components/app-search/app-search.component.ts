import * as _ from 'lodash';

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';
import { RestPagingMetadata } from '../../services/songhay-rest-paging-metadata';

@Component({
    selector: 'app-search',
    templateUrl: './app-search.component.html',
    styleUrls: ['./app-search.component.scss']
})
export class AppSearchComponent implements OnInit {
    constructor(
        public indexService: BlogEntriesService,
        private restPagingMetadata: RestPagingMetadata,
        private route: ActivatedRoute
    ) {}

    hasPageNumbers: boolean;
    pageNumberList: number[];

    private pagingJson: any;
    private searchTerm: string;
    private skipValue: number;

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.hasPageNumbers = false;
            this.pageNumberList = [];
            this.skipValue = 0;
            this.searchTerm = params['searchTerm'] as string;
            if (!this.searchTerm) {
                return;
            }
            this.indexService
                .search(this.searchTerm, this.skipValue)
                .then(response => {
                    this.pagingJson = response.json();
                    if (!this.skipValue) {
                        this.initializePaging();
                    }
                    this.setPageNumberList();
                });
        });
    }

    pageSearchResults(index: number) {
        this.skipValue = index;
        this.indexService
            .search(this.searchTerm, this.skipValue)
            .then(response => (this.pagingJson = response.json()));
    }

    private initializePaging() {
        this.restPagingMetadata.totalSetSize = this.pagingJson['@odata.count'];
        this.restPagingMetadata.resultSetSize = this.pagingJson.value.length;
    }

    private setPageNumberList() {
        _(this.restPagingMetadata.toNumberOfPages()).times(i => {
            this.pageNumberList.push(++i);
        });

        this.hasPageNumbers = this.pageNumberList.length > 1;
    }
}
