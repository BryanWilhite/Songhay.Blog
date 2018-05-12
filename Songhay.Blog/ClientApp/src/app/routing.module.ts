import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AppBlogEntryComponent } from './components/app-blog-entry/app-blog-entry.component';
import { AppIndexComponent } from './components/app-index/app-index.component';

const routes: Routes = [
    { path: '', redirectTo: '/index/groups', pathMatch: 'full' },
    { path: 'blog/entry/:slug', component: AppBlogEntryComponent },
    { path: 'index/:style', component: AppIndexComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class RoutingModule {}
