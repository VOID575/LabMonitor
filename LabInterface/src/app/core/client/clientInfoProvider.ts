export class ClientInfoProvider {
  // Get the server host name from the window location
  getServerHost(): string {
    return window.location.hostname;
  }
}
