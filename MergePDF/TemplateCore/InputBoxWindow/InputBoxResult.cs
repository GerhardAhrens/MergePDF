namespace System.Windows
{
    public sealed class InputBoxResult<T>
    {
        public bool IsOk { get; init; }

        public T Value { get; init; }
    }
}
