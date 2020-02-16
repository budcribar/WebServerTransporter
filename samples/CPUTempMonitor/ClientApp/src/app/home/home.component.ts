import { Component } from '@angular/core';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { HttpRequest, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public temp = 0.0;
  public state = "Start";
  public oven = "Bake";

  constructor(private http: HttpClient) { }

  public setTemperature() {
    if (this.oven === "Bake") {
      //window.location.origin
      //this.http.post('http://localhost:5000/cputempmonitor/stressor', JSON.stringify(false)).subscribe(() => this.oven = "Cool");

      this.http.get('http://localhost:5000/cputempmonitor/stressor/true', ).subscribe(() => this.oven = "Cool");
    }
    else {
      this.http.get('http://localhost:5000/cputempmonitor/stressor/false').subscribe(() => this.oven = "Bake");

      // this.http.post('http://localhost:5000/cputempmonitor/stressor', JSON.stringify(true)).subscribe(() => this.oven = "Bake");
    }
  }


  public monitorTemperature() {
    
    var myWebSocket = webSocket('ws://localhost:5000/ws');
    
    myWebSocket.asObservable().subscribe((t: number) => this.temp = t);

    if (this.state === "Start") this.state = "Stop"
    else this.state = "Start"
  }
}
