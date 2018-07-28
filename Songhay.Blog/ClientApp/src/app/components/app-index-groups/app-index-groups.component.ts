import * as _ from 'lodash';

import { DomSanitizer } from '@angular/platform-browser';
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';

import {
    debounceTime,
    distinctUntilChanged,
    map,
    startWith,
    switchMap
} from 'rxjs/operators';

import {
    ChangeDetectionStrategy,
    Component,
    Input,
    OnInit
} from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { IIndexFormGroup } from '../../models/songhay-index-form-group';
import { BlogEntriesService } from '../../services/songhay-blog-entries.service';
import { BlogEntry } from '../../models/songhay-blog-entry';
import { IndexGroupingOption } from '../../models/songhay-index-grouping-option';
import { IndexGroup } from '../../models/songhay-index-group';

/**
 * index groups component
 *
 * @export
 * @class AppIndexGroupsComponent
 * @implements {OnInit}
 */
@Component({
    changeDetection: ChangeDetectionStrategy.OnPush,
    selector: 'app-index-groups',
    styleUrls: ['./app-index-groups.component.scss'],
    templateUrl: './app-index-groups.component.html'
})
export class AppIndexGroupsComponent implements OnInit {
    /**
     * index reactive form group
     *
     * @type {FormGroup}
     * @memberof AppIndexGroupsComponent
     */
    indexFormGroup: FormGroup;

    /**
     * index grouping options
     *
     * @type {IndexGroupingOption[]}
     * @memberof AppIndexGroupsComponent
     */
    indexGroupingOptions: IndexGroupingOption[];

    /**
     * observable index groups
     *
     * @type {Observable<IndexGroup[]>}
     * @memberof AppIndexGroupsComponent
     */
    indexGroups$: Observable<IndexGroup[]>;

    /**
     * input binding of @type {BlogEntriesService}
     *
     * @type {BlogEntriesService}
     * @memberof AppIndexGroupsComponent
     */
    @Input() indexService: BlogEntriesService;

    /**
     * Creates an instance of AppIndexGroupsComponent.
     * @memberof AppIndexGroupsComponent
     */
    constructor(private sanitizer: DomSanitizer) {}

    /**
     * implements @type {OnInit.ngOnInit}
     *
     * @memberof AppIndexGroupsComponent
     */
    ngOnInit(): void {
        this.initializeIndexGroupingOptions();
        this.initializeIndexFormGroup();
        this.initializeIndexGroups();
    }

    private initializeIndexFormGroup(): void {
        const defaultFilter = '';
        this.indexFormGroup = new FormGroup({
            indexGroupingSelection: new FormControl(
                this.indexGroupingOptions[0]
            ),
            indexFilter: new FormControl(defaultFilter)
        });
    }

    private initializeIndexGroups(): void {
        const chainIntoGroups = (
            entries: BlogEntry[],
            indexGroupingOption: IndexGroupingOption
        ) => {
            return _(entries)
                .chain()
                .groupBy((i: BlogEntry) =>
                    _.toString(
                        i.itemCategoryObject[
                            indexGroupingOption.groupByPropertyName
                        ]
                    )
                )
                .map((i: BlogEntry[]) => {
                    if (!i || !i.length) {
                        console.log(
                            'The expected group of Blog entries are not here.'
                        );
                        return;
                    }
                    const firstEntry = i[0];
                    const groupDisplayName =
                        firstEntry.itemCategoryObject[
                            indexGroupingOption.groupByPropertyName
                        ];
                    const indexGroup: IndexGroup = {
                        group: i,
                        groupDisplayName: this.sanitizer.bypassSecurityTrustHtml(
                            groupDisplayName
                        ),
                        isCollapsed: false
                    };
                    return indexGroup;
                })
                .orderBy(
                    ['groupDisplayName'],
                    [indexGroupingOption.sortDescending ? 'desc' : 'asc']
                )
                .value();
        };

        this.indexGroups$ = this.indexFormGroup.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            startWith(this.indexFormGroup.value as IIndexFormGroup),
            switchMap(i =>
                of(this.indexService.index).pipe(
                    map(j => this.indexService.filterEntries(j, i.indexFilter)),
                    map(j => chainIntoGroups(j, i.indexGroupingSelection))
                )
            )
        );
    }

    private initializeIndexGroupingOptions(): void {
        this.indexGroupingOptions = [
            {
                displayName: 'by Date',
                groupByPropertyName: 'dateGroup',
                sortDescending: true
            },
            {
                displayName: 'by Topic',
                groupByPropertyName: 'topic',
                sortDescending: false
            }
        ];
    }
}
