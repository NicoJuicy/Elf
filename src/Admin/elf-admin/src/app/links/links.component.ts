import { Component, OnInit, ViewChild } from '@angular/core';
import { Link, LinkService, PagedLinkResult } from './link.service';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { EditLinkDialog } from './edit-link-dialog';
import { ShareDialog } from './share-dialog';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { ConfirmationDialog } from '../shared/confirmation-dialog';
import { Clipboard } from '@angular/cdk/clipboard';
import { AppCacheService } from '../shared/appcache.service';
@Component({
    selector: 'app-links',
    templateUrl: './links.component.html',
    styleUrls: ['./links.component.css']
})
export class LinksComponent implements OnInit {
    ENV = environment;
    isLoading = false;
    totalRows = 0;
    pageSize = 10;
    currentPage = 0;
    searchTerm: string;
    pageSizeOptions: number[] = [10, 15, 20, 50, 100];

    displayedColumns: string[] = ['fwToken', 'originUrl', 'note', 'akaName', 'tags', 'isEnabled', 'ttl', 'updateTimeUtc', 'action', 'manage'];
    dataSource: MatTableDataSource<Link> = new MatTableDataSource();

    constructor(
        private toastr: ToastrService,
        public dialog: MatDialog,
        private clipboard: Clipboard,
        private appCache: AppCacheService,
        private linkService: LinkService) { }

    @ViewChild(MatSort) sort: MatSort;
    @ViewChild(MatPaginator) paginator!: MatPaginator;

    ngOnInit(): void {
        this.getLinks();
    }

    addNewLink() {
        let diagRef = this.dialog.open(EditLinkDialog);
        diagRef.afterClosed().subscribe(result => {
            if (result) {
                this.getLinks();
                this.appCache.fetchCache();
            }
        });
    }

    shareLink(link: Link) {
        this.dialog.open(ShareDialog, { data: link });
    }

    editLink(link: Link) {
        let diagRef = this.dialog.open(EditLinkDialog, { data: link });
        diagRef.afterClosed().subscribe(result => {
            if (result) {
                this.getLinks();
                this.appCache.fetchCache();
            }
        });
    }

    search() {
        this.getLinks(true);
    }

    getLinks(reset: boolean = false): void {
        if (reset) {
            this.totalRows = 0;
            this.currentPage = 0;
        }

        this.isLoading = true;

        this.linkService.list(this.pageSize, this.currentPage * this.pageSize, this.searchTerm)
            .subscribe((result: PagedLinkResult) => {
                this.isLoading = false;

                this.dataSource = new MatTableDataSource(result.links);

                this.dataSource.sort = this.sort;
                this.dataSource.paginator = this.paginator;

                setTimeout(() => {
                    this.paginator.pageIndex = this.currentPage;
                    this.paginator.length = result.totalRows;
                });
            });
    }

    checkLink(id: number, isEnabled: boolean): void {
        this.linkService.setEnable(id, isEnabled).subscribe(() => {
            this.toastr.success('Updated');
        });
    }

    deleteLink(id: number): void {
        const dialogRef = this.dialog.open(ConfirmationDialog, {
            data: {
                message: 'Are you sure want to delete this item?',
                buttonText: {
                    ok: 'Yes',
                    cancel: 'No'
                }
            }
        });

        dialogRef.afterClosed().subscribe((confirmed: boolean) => {
            if (confirmed) {
                this.linkService.delete(id).subscribe(() => {
                    this.toastr.success('Deleted');
                    this.getLinks();
                });
            }
        });
    }

    pageChanged(event: PageEvent) {
        console.log({ event });
        this.pageSize = event.pageSize;
        this.currentPage = event.pageIndex;
        this.getLinks();
    }

    copyChip(link: Link) {
        this.clipboard.copy(environment.elfApiBaseUrl + '/fw/' + link.fwToken);
        this.toastr.info('Copied');
    }

    copyAka(link: Link) {
        this.clipboard.copy(environment.elfApiBaseUrl + '/aka/' + link.akaName);
        this.toastr.info('Aka url copied');
    }
}