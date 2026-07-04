namespace System.Windows
{
    using System.Diagnostics;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Statische Member nicht in generischen Typen deklarieren", Justification = "<Ausstehend>")]
    [DebuggerStepThrough]
    [Serializable]
    public struct Result<TResult>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFail { get; private set; }

        public TResult Value { get; private set; }

        public string FailMessage { get; private set; }

        public string SuccessMessage { get; private set; }

        public Exception Exception { get; private set; }

        public static Result<TResult> Success(TResult result, string successMessage)
        {
            return new Result<TResult>
            {
                IsSuccess = true,
                IsFail = false,
                Value = result,
                SuccessMessage = successMessage
            };
        }

        public static Result<TResult> Success(TResult result)
        {
            return new Result<TResult>
            {
                IsSuccess = true,
                IsFail = false,
                Value = result,
                SuccessMessage = string.Empty
            };
        }


        public static Result<TResult> Fail()
        {
            return new Result<TResult>
            {
                IsSuccess = false,
                Value = default(TResult),
                FailMessage = string.Empty
            };
        }

        public static Result<TResult> Fail(string nonMessage)
        {
            return new Result<TResult>
            {
                IsSuccess = false,
                FailMessage = nonMessage,
                Exception = null
            };
        }

        public static Result<TResult> Fail(Exception ex)
        {
            return new Result<TResult>
            {
                IsSuccess = false,
                FailMessage = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}",
                Exception = ex
            };
        }

        public static Result<TResult> Fail(Exception ex, string nonMessage)
        {
            return new Result<TResult>
            {
                IsSuccess = false,
                FailMessage = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{nonMessage}",
                Exception = ex
            };
        }
    }
}