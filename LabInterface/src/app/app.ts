import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  // Fetch the right component based on the route and display it automatically
  template: '<router-outlet></router-outlet>',

})
export class App {
  title = 'LabInterface';
}
