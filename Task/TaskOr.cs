using System;
using System.Threading.Tasks;

namespace ShortCodeRenderer.Task
{
    public class TaskOr<T>

    {
        private readonly T _value;
        private readonly Task<T> _task;
        private readonly bool _isAsync;

        private TaskOr(T value)
        {
            _value = value;
            _isAsync = false;
        }

        private TaskOr(Task<T> task)
        {
            _task = task;
            _isAsync = true;
        }

        public static implicit operator TaskOr<T>(T value) => new TaskOr<T>(value);
        public static implicit operator TaskOr<T>(Task<T> task) => new TaskOr<T>(task);

        public async Task<T> AsTask()
        {
            return _isAsync ? await _task : _value;
        }
        public object AsObject()
        {
            if(_isAsync)
                return _task;
            return _value;
        }
        public bool IsAsync()
        {
            return _isAsync;
        }
        public override string ToString()
        {
            return _isAsync ? _task.ToString() : _value?.ToString() ?? "null";
        }
        public object Value => _value;
    }
}
