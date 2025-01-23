using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Core.Entities.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public string ErrorKey { get; private set; }
        public string Message { get; private set; }
        public ErrorType? ErrorType { get; private set; }
        public T Data { get; private set; }

        private Result(bool isSuccess, string message, T data, ErrorType? errorType = null, string errorKey = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            ErrorType = errorType;
            ErrorKey = errorKey;
        }

        public static Result<T> Success(T data) => new(true, null, data);
        public static Result<T> Failure(string errorKey, string message, ErrorType errorType) => new(false, message, default, errorType, errorKey);
    }
    public enum ErrorType
    {
        ValidationError,
        BusinessLogicError,
        NotFoundError,
        UnauthorizedError,
        SystemError

    }
}
