import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AppBlogEntryComponent } from './components/app-blog-entry/app-blog-entry.component';
import { AppIndexComponent } from './components/app-index/app-index.component';
import { AppSearchComponent } from './components/app-search/app-search.component';

const routes: Routes = [
    { path: '', redirectTo: 'index/groups', pathMatch: 'full' },
    { path: 'entry/:slug', redirectTo: 'blog/entry/:slug', pathMatch: 'full' },
    { path: 'blog/entry/:slug', component: AppBlogEntryComponent },
    { path: 'blog/search/:searchTerm', component: AppSearchComponent },
    { path: 'index/:style', component: AppIndexComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class RoutingModule {}
