import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

/**
 * shared routines for the app {Router}
 *
 * @export
 * @class RouterUtility
 */
@Injectable()
export class RouterUtility {
    /**
     *Creates an instance of RouterUtility.
     * @param {Router} router
     * @memberof RouterUtility
     */
    constructor(private router: Router) {}

    /**
     * navigates to search experience
     *
     * @param {FormGroup} indexFormGroup
     * @memberof RouterUtility
     */
    navigateToSearch(indexFormGroup: FormGroup): void {
        const searchTerm: string = indexFormGroup.value['indexFilter'];
        if (!searchTerm) {
            return;
        }
        this.router.navigate([`blog/search/${searchTerm}`]);
    }
}
