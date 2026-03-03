export const AppRoutes = {
  DASHBOARD: '',
  PROJECT_DETAIL: (name: string) => `/project/${name}`,
} as const;
