import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { HttpModule } from '@angular/http';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FlexLayoutModule } from '@angular/flex-layout';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from './material.module';
import { RoutingModule } from './routing.module';

import { BlogEntriesService } from './services/songhay-blog-entries.service';
import { CssUtility } from './services/songhay-css.utility';
import { MathUtility } from './services/songhay-math.utility';

import { AppComponent } from './components/app.component';
import { AppErrorComponent } from './components/app-error/app-error.component';
import { AppIndexComponent } from './components/app-index/app-index.component';
import { AppIndexGroupsComponent } from './components/app-index-groups/app-index-groups.component';
import { AppIndexListComponent } from './components/app-index-list/app-index-list.component';

@NgModule({
    declarations: [
        AppComponent,
        AppErrorComponent,
        AppIndexComponent,
        AppIndexGroupsComponent,
        AppIndexListComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FlexLayoutModule,
        HttpClientModule,
        HttpModule,
        MaterialModule,
        ReactiveFormsModule,
        RoutingModule
    ],
    providers: [BlogEntriesService, CssUtility, MathUtility],
    bootstrap: [AppComponent]
})
export class AppModule {}
