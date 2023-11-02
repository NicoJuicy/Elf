import { ErrorHandler, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { ClipboardModule } from 'ngx-clipboard';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { ReportComponent } from './report/report.component';

//#region Kendo UI
import 'hammerjs';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { GridModule } from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { ChartsModule } from '@progress/kendo-angular-charts';
import { IndicatorsModule } from '@progress/kendo-angular-indicators';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogsModule } from '@progress/kendo-angular-dialog';
import { LabelModule } from '@progress/kendo-angular-label';
import { RecentRequestsComponent } from './report/recent-requests/recent-requests.component';
import { NotificationModule } from '@progress/kendo-angular-notification';
import { BarcodesModule } from '@progress/kendo-angular-barcodes';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { ToolBarModule } from '@progress/kendo-angular-toolbar';
import { NavigationModule } from '@progress/kendo-angular-navigation';
import { IconsModule  } from '@progress/kendo-angular-icons';
//#endregion

//#region MSAL

import { IPublicClientApplication, PublicClientApplication, InteractionType } from '@azure/msal-browser';
import {
  MsalGuard,
  MsalInterceptor,
  MsalBroadcastService,
  MsalInterceptorConfiguration,
  MsalModule,
  MsalService,
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalGuardConfiguration,
  MsalRedirectComponent
} from '@azure/msal-angular';

import { msalConfig, loginRequest, protectedResources } from './auth-config';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication(msalConfig);
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();

  protectedResourceMap.set(protectedResources.linkApi.endpoint, protectedResources.linkApi.scopes);
  protectedResourceMap.set(protectedResources.reportApi.endpoint, protectedResources.reportApi.scopes);
  protectedResourceMap.set(protectedResources.tagApi.endpoint, protectedResources.tagApi.scopes);

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: loginRequest
  };
}

//#endregion

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ReportComponent,
    RecentRequestsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ClipboardModule,
    MsalModule,
    FormsModule,
    ReactiveFormsModule,
    InputsModule,
    GridModule,
    ButtonsModule,
    ChartsModule,
    IndicatorsModule,
    DateInputsModule,
    DialogsModule,
    LabelModule,
    NotificationModule,
    BarcodesModule,
    DropDownsModule,
    LayoutModule,
    ToolBarModule,
    IconsModule,
    NavigationModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService
  ],
  bootstrap: [AppComponent, MsalRedirectComponent]
})
export class AppModule { }
