import {
    ChangeDetectionStrategy,
    Component,
    Input,
    OnInit
} from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { of ,  Subject ,  Observable } from 'rxjs';
import {
    debounceTime,
    distinctUntilChanged,
    map,
    startWith,
    switchMap
} from 'rxjs/operators';

import { BlogEntriesService } from '../../services/songhay-blog-entries.service';
import { BlogEntry } from '../../models/songhay-blog-entry';

/**
 * index list component
 *
 * @export
 * @class AppIndexListComponent
 * @implements {OnInit}
 */
@Component({
    changeDetection: ChangeDetectionStrategy.OnPush,
    selector: 'app-index-list',
    styleUrls: ['./app-index-list.component.scss'],
    templateUrl: './app-index-list.component.html'
})
export class AppIndexListComponent implements OnInit {
    /**
     * index reactive form group
     *
     * @type {FormGroup}
     * @memberof AppIndexGroupsComponent
     */
    indexFormGroup: FormGroup;

    /**
     * the current page of the index list
     *
     * @type {number}
     * @memberof AppIndexListComponent
     */
    currentPage: number;

    /**
     * observable index
     *
     * @type {Observable<Array<BlogEntry>>}
     * @memberof AppIndexListComponent
     */
    index$: Observable<Array<BlogEntry>>;

    /**
     * input binding of @type {BlogEntriesService}
     *
     * @type {BlogEntriesService}
     * @memberof AppIndexListComponent
     */
    @Input() indexService: BlogEntriesService;

    private filterIndexSubject: Subject<string>;

    /**
     * Creates an instance of AppIndexListComponent.
     * @memberof AppIndexListComponent
     */
    constructor() {
        this.filterIndexSubject = new Subject<string>();
    }

    /**
     * filters observable index
     *
     * @param {string} particle
     * @memberof AppIndexListComponent
     */
    filterIndex(particle: string): void {
        this.filterIndexSubject.next(particle);
    }

    /**
     * implements @type {OnInit.ngOnInit}
     *
     * @memberof AppIndexListComponent
     */
    ngOnInit(): void {
        this.initializeIndexFormGroup();
        this.initializeIndex();
        this.currentPage = 1;
    }

    private initializeIndex(): void {
        this.index$ = this.indexFormGroup.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            startWith(this.indexFormGroup.value as { indexFilter: string }),
            switchMap(i =>
                of(this.indexService.index).pipe(
                    map(j => this.indexService.filterEntries(j, i.indexFilter))
                )
            )
        );
    }

    private initializeIndexFormGroup(): void {
        const defaultFilter = '';
        this.indexFormGroup = new FormGroup({
            indexFilter: new FormControl(defaultFilter)
        });
    }
}
