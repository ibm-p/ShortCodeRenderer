using System;
using System.Threading.Tasks;

namespace ShortCodeRenderer.Tasks
{
    public class TaskOr<T>

    {
        public delegate Task<T> MyCustomAsyncDelegate<T>(int param1, string param2);
        private readonly T _value;
        private readonly Func<Task<T>> _factory; 
        
        private TaskOr(T value)
        {
            _value = value;
        }
        public TaskOr(Task<T> task)
        {
            _factory = () => task;

        }
        public TaskOr(Func<Task<T>> taskFactory)
        {
            _factory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
        }


        public static implicit operator TaskOr<T>(T value) => new TaskOr<T>(value);
        public static implicit operator TaskOr<T>(Task<T> task) => new TaskOr<T>(task);
        public static implicit operator TaskOr<T>(Func<Task<T>> asyncFunc) => new TaskOr<T>(asyncFunc);

        public async Task<T> AsTask()
        {
            return IsAsync() ? await _factory() : _value;
        }
        public object AsObject()
        {
            if(IsAsync())
                return _factory();
            return _value;
        }
        public bool IsAsync()
        {
            return _factory != null;
        }
        public override string ToString()
        {
            return IsAsync() ? "ASYNC" : _value?.ToString() ?? "null";
        }
        public object Value => _value;
    }
}
