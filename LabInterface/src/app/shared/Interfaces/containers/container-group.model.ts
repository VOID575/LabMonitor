import { DockerContainer } from './containers.model';
import {ContainerState} from '../../Enums/container-state';

export interface ContainerGroup {
  projectName: string;
  containers: DockerContainer[];
  activeCount: number;
  totalCount: number;
  groupState: ContainerState;
  PrivatePort: number;
  PublicPOrt : number;
  type :
  totalCpu: number;
  totalMemory: number;
}
