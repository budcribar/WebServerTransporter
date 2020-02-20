import { Component } from '@angular/core';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { HttpRequest, HttpClient } from '@angular/common/http';
import { BatteryCharge } from '../BatteryCharge';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public state = "Start";
  public oven = "Bake";
  public chargeStatus: BatteryCharge[] = [];
  public chargeStatusToPlot: BatteryCharge[];

  private webSocket: WebSocketSubject<BatteryCharge> = null;

  constructor(private http: HttpClient) { }

  public setTemperature() {
    if (this.oven === "Bake") 
      this.http.post(`${window.location.origin}/batterymonitor/stressor/true`, null).subscribe(() => this.oven = "Cool");
    else 
      this.http.post(`${window.location.origin}/batterymonitor/stressor/false`, null).subscribe(() => this.oven = "Bake");
  }

  set ChargeStatus(status: BatteryCharge[]) {
    this.chargeStatus = status;
    this.chargeStatusToPlot = this.chargeStatus.slice(0, 100);
  }

  public monitorCharge() {
    if (this.state === "Start") {
      var scheme = `${window.location.origin}`.replace('http', 'ws');

      this.webSocket = webSocket<BatteryCharge>(`${scheme}/ws`);

      this.webSocket.asObservable().subscribe((t: BatteryCharge) => {
        if (this.chargeStatus.length > 0)
          this.ChargeStatus = [t].concat(this.chargeStatus);
        else
          this.ChargeStatus = [t];
      });

      this.state = "Stop"
    }
    else {
      this.webSocket.complete();
      this.ChargeStatus = [];
      this.state = "Start"
    }
    
  }

  
}
