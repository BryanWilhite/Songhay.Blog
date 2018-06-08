import * as _ from 'lodash';

import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import 'rxjs/add/operator/toPromise';

import { BlogEntry } from '../models/songhay-blog-entry';

/**
 * API for @type {BlogEntry}
 *
 * @export
 * @class BlogEntriesService
 */
@Injectable()
export class BlogEntriesService {
    /**
     * Creates an instance of @type {BlogEntriesService}.
     * @param {Http} client
     * @memberof BlogEntriesService
     */
    constructor(private http: Http) {
        this.client = this.http;
        this.baseApiRoute = './api/blog/';
        this.indexLocation = './assets/data/index.json'; // TODO: inject this from config?
        this.initialize();
    }

    /**
     * Returns the injected @type {Http} from the DI container.
     *
     * @type {Http}
     * @memberof BlogEntriesService
     */
    client: Http;

    /**
     * Returns the base, relative Blog API location.
     *
     * @type {string}
     * @memberof BlogEntriesService
     */
    baseApiRoute: string;

    /**
     * Returns the @type {BlogEntry} set.
     *
     * @type {Array<BlogEntry>}
     * @memberof BlogEntriesService
     */
    index: Array<BlogEntry>;

    /**
     * Returns the @type {string}, locating JSON.
     *
     * @type {string}
     * @memberof BlogEntriesService
     */
    indexLocation: string;

    /**
     * Returns true when the last API promise is rejected.
     *
     * @type {boolean}
     * @memberof BlogEntriesService
     */
    isError: boolean;

    /**
     * Returns true when the last API call loaded data
     * without any errors.
     *
     * @type {boolean}
     * @memberof BlogEntriesService
     */
    isLoaded: boolean;

    /**
     * Returns true when the API call is promising.
     *
     * @type {boolean}
     * @memberof BlogEntriesService
     */
    isLoading: boolean;

    initialize(): void {
        this.index = null;
        this.isError = false;
        this.isLoaded = false;
        this.isLoading = true;
    }

    /**
     * filters the specified entries with the specified particle
     *
     * @param {BlogEntry[]} entries
     * @param {string} particle
     * @returns {Array<BlogEntry>}
     * @memberof BlogEntriesService
     */
    filterEntries(entries: BlogEntry[], particle: string): Array<BlogEntry> {
        const contains = (needle: string, haystack: string) => {
            return (
                needle &&
                haystack &&
                haystack.toLowerCase().indexOf(needle.toLowerCase()) !== -1
            );
        };
        if (!particle) {
            return entries;
        }
        if (particle.length < 2) {
            return entries;
        }
        return entries.filter(i => contains(particle, i.Title));
    }

    /**
     * Promises to load index data.
     *
     * @returns {Promise<HttpResponse>}
     * @memberof BlogEntriesService
     */
    loadIndex(): Promise<Response> {
        this.initialize();

        const promise = this.client.get(this.indexLocation).toPromise();
        promise
            .catch(() => {
                this.isError = true;
                this.isLoaded = false;
            })
            .then(responseOrVoid => {
                const response = <Response>responseOrVoid;
                if (!response) {
                    return;
                }

                this.index = <Array<BlogEntry>>response.json();
                if (!this.index) {
                    return;
                }

                _(this.index).each((blogEntry: BlogEntry) => {
                    blogEntry.ItemCategoryObject = this.getItemCategoryProperties(
                        blogEntry
                    );
                    blogEntry.SortOrdinal = this.getSortOrdinal(blogEntry);
                });

                this.index = _(this.index).orderBy(['SortOrdinal'], ['desc']).value();

                this.isLoaded = true;
                this.isLoading = false;
            });

        return promise;
    }

    private getItemCategoryProperties(blogEntry: BlogEntry): object {
        const o = JSON.parse(`{ ${blogEntry.ItemCategory} }`);
        const topics = Object.keys(o).filter(function (v) {
            return v ? v.indexOf('topic-') === 0 : false;
        });
        o.topic = _(topics).isEmpty()
            ? '<!--zzz-->[no topic]'
            : `<!-- ${_(topics).first()} --> ${o[_(topics).first()]}`;
        o.topics = topics.map(function (v) {
            return {
                key: v,
                value: o[v]
            };
        });
        return o;
    }

    private getSortOrdinal(blogEntry: BlogEntry): string {
        if (!blogEntry.ItemCategoryObject) {
            return '';
        }
        const pad = function (num, size) {
            let s = String(num);
            while (s.length < size) {
                s = `0${s}`;
            }
            return s;
        };
        return (
            blogEntry.ItemCategoryObject['year'] +
            '-' +
            pad(blogEntry.ItemCategoryObject['month'], 2) +
            '-' +
            pad(blogEntry.ItemCategoryObject['day'], 2) +
            '-' +
            blogEntry.Slug
        );
    }
}
