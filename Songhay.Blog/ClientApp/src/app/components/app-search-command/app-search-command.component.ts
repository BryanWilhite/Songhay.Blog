import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

/**
 * app search component
 *
 * @export
 * @class AppSearchCommandComponent
 */
@Component({
    selector: 'app-search-command',
    styleUrls: ['./app-search-command.component.scss'],
    templateUrl: './app-search-command.component.html'
})
export class AppSearchCommandComponent {
    /**
     *Creates an instance of RouterUtility.
     * @param {Router} router
     * @memberof AppSearchCommandComponent
     */
    constructor(private router: Router) {}

    @Input() indexFormGroup: FormGroup;

    private navigateToSearch(): void {
        const searchTerm: string = this.indexFormGroup.value['indexFilter'];
        if (!searchTerm) {
            return;
        }
        this.router.navigate([`blog/search/${searchTerm}`]);
    }
}
