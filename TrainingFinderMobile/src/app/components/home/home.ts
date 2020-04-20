import { Component, OnInit } from "@angular/core";
import { HttpClient, HttpHeaders, HttpRequest } from "@angular/common/http";
import { ActivatedRoute } from "@angular/router";
import "rxjs/Rx";

@Component({
    selector: "home",
    templateUrl: "home.html",
})
export class HomeComponent implements OnInit {

    public content: string;

    public constructor(private http: HttpClient, private route: ActivatedRoute) {
        this.content = "";
    }

    public ngOnInit() {
        this.route.queryParams.subscribe(params => {
            let headers = new HttpHeaders({ "Authorization": "Basic " + params["jwt"] });
            this.http.get("http://192.168.0.104:4000/users",{ headers: headers })
                .subscribe(result => {
                    this.content = result.toString();
                });
        });
    }

}