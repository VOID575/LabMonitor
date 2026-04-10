import {DockerContainer} from '../../shared/Interfaces/containers/containers.model';
import {ContainerGroup} from '../../shared/Interfaces/containers/container-group.model';
import {ContainerState} from '../../shared/Enums/container-state';

export class ContainerManager {

  public groupContainers(containers: DockerContainer[]): ContainerGroup[] {
    const groupsMap = new Map<string, DockerContainer[]>();

    for (const container of containers) {
      const projectName : string = container.labels.projectName != "" ?  container.labels.projectName : 'Standalone';

      if (!groupsMap.has(projectName)) {
        groupsMap.set(projectName, []);
      }
      groupsMap.get(projectName)!.push(container);
    }

    const result: ContainerGroup[] = [];

    groupsMap.forEach((groupContainers, projectName) => {
      const activeCount = groupContainers.filter(c => c.state === 'running').length;
      const totalCount = groupContainers.length;

      let groupState: ContainerState = ContainerState.Running;
      if (activeCount === 0) groupState = ContainerState.Exited;
      else if (activeCount < totalCount) groupState = ContainerState.Warning;

      result.push({
        projectName : projectName,
        containers: groupContainers,
        activeCount : activeCount,
        totalCount : totalCount,
        groupState : groupState,
        totalCpu: 0,
        totalMemory: 0
      });
    });

    return result;
  }

}
