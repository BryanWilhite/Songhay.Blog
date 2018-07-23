import { Injectable } from '@angular/core';

/**
 * REST paging metadata
 *
 * @export
 * @class RestPagingMetadata
 */
@Injectable()
export class RestPagingMetadata {

    /**
     * Creates an instance of RestPagingMetadata.
     * @memberof RestPagingMetadata
     */
    constructor() {
        this.endPosition = 0;
        this.fromDate = null;
        this.nextUri = '';
        this.previousUri = '';
        this.resultSetSize = 0;
        this.startPosition = 0;
        this.toDate = null;
        this.totalSetSize = 0;
    }

    /**
     * paging end position
     *
     * @type {number}
     * @memberof RestPagingMetadata
     */
    endPosition: number;

    /**
     * from date
     *
     * @type {Date}
     * @memberof RestPagingMetadata
     */
    fromDate: Date;

    /**
     * next-page REST URI
     *
     * @type {string}
     * @memberof RestPagingMetadata
     */
    nextUri: string;

    /**
     * previous page REST URI
     *
     * @type {string}
     * @memberof RestPagingMetadata
     */
    previousUri: string;

    /**
     * paging result-set size
     *
     * @type {number}
     * @memberof RestPagingMetadata
     */
    resultSetSize: number;

    /**
     * paging start position
     *
     * @type {number}
     * @memberof RestPagingMetadata
     */
    startPosition: number;

    /**
     * to date
     *
     * @type {Date}
     * @memberof RestPagingMetadata
     */
    toDate: Date;

    /**
     * total paging set size
     *
     * @type {number}
     * @memberof RestPagingMetadata
     */
    totalSetSize: number;

    /**
     * validates state of this instance
     *
     * @returns {boolean}
     * @memberof RestPagingMetadata
     */
    isValid(): boolean {
        return (
            this.resultSetSize > 0 &&
            this.totalSetSize > 0 &&
            this.endPosition > 0
        );
    }

    /**
     * sets start/end position for next page
     *
     * @memberof RestPagingMetadata
     */
    setNextPage(): void {
        this.endPosition += this.resultSetSize;
        this.startPosition += this.resultSetSize;
    }

    /**
     * returns the total number of pages
     *
     * @returns {number}
     * @memberof RestPagingMetadata
     */
    toNumberOfPages(): number {
        if (this.resultSetSize === 0) {
            return 0;
        }
        return Math.ceil((this.totalSetSize * 1) / this.resultSetSize);
    }
}
