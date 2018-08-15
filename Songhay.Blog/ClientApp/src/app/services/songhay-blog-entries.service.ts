import * as _ from 'lodash';

import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import 'rxjs/add/operator/toPromise';

import { AppScalars } from '../models/songhay-app-scalars';
import { AssemblyInfo } from '../models/songhay-assembly-info';
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
    constructor(private client: Http) {
        this.initialize();
    }

    /**
     * Returns server assembly info.
     *
     * @type {AssemblyInfo}
     * @memberof BlogEntriesService
     */
    assemblyInfo: AssemblyInfo;

    /**
     * Returns the @type {BlogEntry}.
     *
     * @type {BlogEntry}
     * @memberof BlogEntriesService
     */
    entry: BlogEntry;

    /**
     * Returns the @type {BlogEntry} set.
     *
     * @type {BlogEntry[]}
     * @memberof BlogEntriesService
     */
    index: BlogEntry[];

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

    /**
     * Filters the specified entries with the specified particle.
     *
     * @param {BlogEntry[]} entries
     * @param {string} particle
     * @returns {Array<BlogEntry>}
     * @memberof BlogEntriesService
     */
    filterEntries(entries: BlogEntry[], particle: string): BlogEntry[] {
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
        return entries.filter(i => contains(particle, i.title));
    }

    /**
     * Promises to load a Blog entry.
     *
     * @param {string} slug
     * @param {string} entryLocation
     * @returns {Promise<Response>}
     * @memberof BlogEntriesService
     */
    loadEntry(slug: string, entryLocation: string = null): Promise<Response> {
        this.initialize();

        const uri = entryLocation
            ? entryLocation
            : `${AppScalars.baseApiRoute}/entry/${slug}`;

        const wrapPromise = (resolve: any, reject: any) => {
            this.client
                .get(uri)
                .toPromise()
                .then(
                    responseOrVoid => {
                        const response = responseOrVoid as Response;
                        if (!response) {
                            reject('response is not truthy.');
                            return;
                        }

                        this.entry = response.json() as BlogEntry;
                        if (!this.entry) {
                            reject('Blog entry is not truthy.');
                            return;
                        }

                        this.isLoaded = true;
                        this.isLoading = false;

                        resolve(responseOrVoid);
                    },
                    error => {
                        this.isError = true;
                        this.isLoaded = false;
                        reject(error);
                    }
                );
        };

        const promise = new Promise<Response>(wrapPromise);
        return promise;
    }

    /**
     * Promises to load index data.
     *
     * @returns {Promise<HttpResponse>}
     * @memberof BlogEntriesService
     */
    loadIndex(): Promise<Response> {
        this.initialize();

        const wrapPromise = (resolve: any, reject: any) => {
            this.client
                .get(AppScalars.indexLocation)
                .toPromise()
                .then(
                    responseOrVoid => {
                        const response = responseOrVoid as Response;
                        if (!response) {
                            return;
                        }

                        this.index = response.json() as BlogEntry[];
                        if (!this.index) {
                            reject('index is not truthy.');
                            return;
                        }

                        _(this.index).each((blogEntry: BlogEntry) => {
                            blogEntry.itemCategoryObject = this.getItemCategoryProperties(
                                blogEntry
                            );
                            blogEntry.sortOrdinal = this.getSortOrdinal(
                                blogEntry
                            );
                        });

                        this.index = _(this.index)
                            .orderBy(['sortOrdinal'], ['desc'])
                            .value();

                        this.isLoaded = true;
                        this.isLoading = false;

                        resolve(responseOrVoid);
                    },
                    error => {
                        this.isError = true;
                        this.isLoaded = false;
                        reject(error);
                    }
                );
        };

        const promise = new Promise<Response>(wrapPromise);
        return promise;
    }

    /**
     * Promises to load server metadata.
     *
     * @returns {Promise<HttpResponse>}
     * @memberof BlogEntriesService
     */
    loadServerMeta(): Promise<Response> {
        this.initialize();

        const wrapPromise = (resolve: any, reject: any) => {
            this.client
                .get(AppScalars.serverMetaLocation)
                .toPromise()
                .then(
                    responseOrVoid => {
                        const response = responseOrVoid as Response;
                        if (!response) {
                            return;
                        }

                        this.assemblyInfo = response.json() as AssemblyInfo;
                        if (!this.assemblyInfo) {
                            reject('assemblyInfo is not truthy.');
                            return;
                        }

                        this.isLoaded = true;
                        this.isLoading = false;

                        resolve(responseOrVoid);
                    },
                    error => {
                        this.isError = true;
                        this.isLoaded = false;
                        reject(error);
                    }
                );
        };

        const promise = new Promise<Response>(wrapPromise);
        return promise;
    }

    /**
     * Promises to search index data.
     *
     * @param {string} searchText
     * @param {number} skipValue
     * @returns {Promise<Response>}
     * @memberof BlogEntriesService
     */
    search(searchText: string, skipValue: number): Promise<Response> {
        this.initialize();

        const uri = `${AppScalars.baseApiSearchRoute}/${searchText}/${skipValue}`;

        const wrapPromise = (resolve: any, reject: any) => {
            this.client
                .get(uri)
                .toPromise()
                .then(
                    responseOrVoid => {
                        const response = responseOrVoid as Response;
                        if (!response) {
                            return;
                        }

                        this.isLoaded = true;
                        this.isLoading = false;

                        resolve(responseOrVoid);
                    },
                    error => {
                        this.isError = true;
                        this.isLoaded = false;
                        reject(error);
                    }
                );
        };

        const promise = new Promise<Response>(wrapPromise);
        return promise;
    }

    private getItemCategoryProperties(blogEntry: BlogEntry): object {
        const o = JSON.parse(`{ ${blogEntry.itemCategory} }`);
        const topics = Object.keys(o).filter(function(v) {
            return v ? v.indexOf('topic-') === 0 : false;
        });
        o.topic = _(topics).isEmpty()
            ? '<!--zzz-->[no topic]'
            : `<!-- ${_(topics).first()} --> ${o[_(topics).first()]}`;
        o.topics = topics.map(function(v) {
            return {
                key: v,
                value: o[v]
            };
        });
        return o;
    }

    private getSortOrdinal(blogEntry: BlogEntry): string {
        if (!blogEntry.itemCategoryObject) {
            return '';
        }
        const pad = function(num, size) {
            let s = String(num);
            while (s.length < size) {
                s = `0${s}`;
            }
            return s;
        };
        return (
            blogEntry.itemCategoryObject['year'] +
            '-' +
            pad(blogEntry.itemCategoryObject['month'], 2) +
            '-' +
            pad(blogEntry.itemCategoryObject['day'], 2) +
            '-' +
            blogEntry.slug
        );
    }

    private initialize(): void {
        this.index = null;
        this.isError = false;
        this.isLoaded = false;
        this.isLoading = true;
    }
}
