import { Component } from '@angular/core';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  private temp = 0.0;

  public monitorTemperature() {
    
    var myWebSocket = webSocket('ws://localhost:5000/ws');
    
    myWebSocket.asObservable().subscribe((t:number) => this.temp = t );
  }
}
