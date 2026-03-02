import {DockerContainerLabel} from './container-label-model';
import {ContainerState} from '../../Enums/container-state'

export interface DockerContainer {
  id: string;
  name: string;
  state: ContainerState;
  status: string;
  labels: DockerContainerLabel;
  cpuUsage?: number;
  memoryUsage?: number;
}
