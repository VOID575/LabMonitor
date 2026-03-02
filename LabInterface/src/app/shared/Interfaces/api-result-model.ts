import { ErrorType } from '../Enums/error-type';

export interface ApiResult {
  isSuccess: boolean;
  ErrorType: ErrorType;
  ErrorMessage: string;
  OriginalErrorCode?: any;
}
