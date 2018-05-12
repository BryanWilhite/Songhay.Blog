import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';
import { IndexStyles } from '../../models/songhay-index-styles';

/**
 * index component
 *
 * @export
 * @class AppIndexComponent
 * @implements {OnInit}
 */
@Component({
    selector: 'app-index',
    styleUrls: ['./app-index.component.scss'],
    templateUrl: './app-index.component.html'
})
export class AppIndexComponent implements OnInit {
    /**
     * the Index layout style
     *
     * @type {IndexStyles}
     * @memberof AppIndexComponent
     */
    viewStyle: IndexStyles;

    /**
     * Creates an instance of AppIndexComponent.
     * @param {BlogEntriesService} indexService
     * @param {ActivatedRoute} route
     * @memberof AppIndexComponent
     */
    constructor(
        public indexService: BlogEntriesService,
        private route: ActivatedRoute
    ) {}

    /**
     * implements @type {OnInit.ngOnInit}
     *
     * @memberof AppIndexComponent
     */
    ngOnInit(): void {
        this.route.params.subscribe(params => {
            this.viewStyle = params['style'] as any;
        });

        this.indexService.loadIndex();
    }
}
