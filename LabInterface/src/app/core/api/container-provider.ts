import {environment} from '../../../environments/environment';
import { DockerContainer } from '../../shared/Interfaces/containers/containers.model';
import {ApiResult} from '../../shared/Interfaces/api-result-model';

export class ContainerProvider {

    private BASE_URL : string = environment.dockerApiUrl;

    // Move into an "ApiInterrogator" ? With several manners to api request ?
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
            throw new Error(`Error occured: ${response.statusText}`);
          }

          return await response.json() as T;
        } catch (error) {
          console.error("Erreur API docker:", error);
          throw error;
        }
    }

    private async getFromApi<T>(endpoint: string): Promise<T> {
        try {
            const response = await fetch(`${this.BASE_URL}/${endpoint}`, {
                method: 'GET',
                headers: {
                  'Content-Type': 'application/json',
                },
            });

          if (!response.ok) {
            throw new Error(`Error occured: ${response.statusText}`);
          }

          return await response.json() as T;
        } catch (error) {
          console.error("Erreur API docker:", error);
          throw error;
        }
    }

  public async getAllContainers(): Promise<DockerContainer[]> {
      return await this.getFromApi<DockerContainer[]>('containers');
  }

  public async getContainerById(id : string): Promise<DockerContainer> {
    return await this.getFromApi<DockerContainer>('containersById/' + id);
  }

  public async getContainerByProjectName(projectName : string): Promise<DockerContainer[]> {
    return await this.getFromApi<DockerContainer[]>('containersByProjectName/' + projectName);
  }

  public async StartContainer(id : string): Promise<ApiResult> {
    return await this.requestToApi<ApiResult>('startContainers/' + id);
  }

  public async StopContainer(id : string): Promise<ApiResult> {
    return await this.requestToApi<ApiResult>('stopContainer/' + id);
  }
}
