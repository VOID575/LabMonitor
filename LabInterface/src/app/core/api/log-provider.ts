import {environment} from '../../../environments/environment';
import { DockerContainer } from '../../shared/Interfaces/containers/containers.model';
import { ContainerGroup } from '../../shared/Interfaces/containers/container-group.model';
import {ApiResult} from '../../shared/Interfaces/api-result-model';

export class LogProvider {

  private BASE_URL : string = environment.dockerHubUrl;

  private async requestToApi<T>(endpoint: string, data?: object): Promise<T> {
    try {
      const response = await fetch(`${this.BASE_URL}/${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(`Error occured while getting all containers: ${response.statusText}`);
      }
      const result = await response.json();

      return await result as T;
    } catch (error) {
      console.error("Erreur API docker:", error);
      throw error;
    }
  }

  // TODO : Manage log streams
  public async getLogStream(): Promise<string> {
    return await this.requestToApi<string>('logs');
  }

  // TODO : getMachineResourceStream()
}
