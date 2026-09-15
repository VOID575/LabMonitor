import {environment} from '../../../environments/environment';
import {ApiResult} from '../../shared/Interfaces/api-result-model';

export class DockerComposeManager {

  private BASE_URL : string = environment.dockerComposeApiUrl;

  // TODO : Move into an "ApiInterrogator" ? With several manners to api request ? Or Abstraction ?
  private async getFromApi<T>(endpoint: string, data?: object): Promise<T> {
    try {
      const response = await fetch(`${this.BASE_URL}/${endpoint}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(`Error occured: ${response.statusText}`);
      }

      const text = await response.text();

      return text ? JSON.parse(text) as T : {} as T;

    } catch (error) {
      console.error("Erreur API docker:", error);
      throw error;
    }
  }

  public async startProject(projectName : string): Promise<ApiResult> {
    return await this.getFromApi<ApiResult>('startProject/' + projectName);
  }

  public async stopProject(projectName : string): Promise<ApiResult> {
    return await this.getFromApi<ApiResult>('stopProject/' + projectName);
  }

  public async downProject(projectName : string): Promise<ApiResult> {
    return await this.getFromApi<ApiResult>('downProject/' + projectName);
  }
}
