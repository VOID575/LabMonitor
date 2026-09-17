import {DockerContainerLabel} from './container-label.model';
import {ContainerPort} from './container-port.model';
import {ContainerState} from '../../Enums/container-state'

export interface DockerContainer {
  id: string;
  name: string;
  state: ContainerState;
  status: string;
  labels: DockerContainerLabel;
  ports : ContainerPort[];
  cpuUsage?: number;
  memoryUsage?: number;
}
