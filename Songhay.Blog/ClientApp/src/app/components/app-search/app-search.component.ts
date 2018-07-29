import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';

/**
 * app search component
 *
 * @export
 * @class AppSearchComponent
 * @implements {OnInit}
 */
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

    /**
     * the current page of search results
     *
     * @type {number}
     * @memberof AppSearchComponent
     */
    currentPage: number;

    /**
     * when true search results are first loaded
     *
     * @type {boolean}
     * @memberof AppSearchComponent
     */
    isFirstLoaded: boolean;

    /**
     * search term
     *
     * @type {string}
     * @memberof AppSearchComponent
     */
    searchTerm: string;

    /**
     * size of all paginated items
     *
     * @type {number}
     * @memberof AppSearchComponent
     */
    totalSetSize: number;

    private pagingJson: any;
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
                    this.isFirstLoaded = true;
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
