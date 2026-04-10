export const AppRoutes = {
  DASHBOARD: '',
  PROJECT_DETAIL: (projectName: string) => `/project/${projectName}`,
} as const;
