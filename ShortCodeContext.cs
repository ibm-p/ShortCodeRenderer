using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer
{
    public class ShortCodeContext
    {
        private readonly Dictionary<Type, object> _services =  new Dictionary<Type, object>();

        public ShortCodeContext Register<T>(T instance)
        {
            _services[typeof(T)] = instance;
            return this;
        }

        public T Resolve<T>()
        {
            return (T)_services[typeof(T)];
        }
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
        public ShortCodeContext Unregister<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
            }
            return this;
        }
        public void Clear()
        {
            _services.Clear();
        }
        public static ShortCodeContext Create()
        {
            return new ShortCodeContext();
        }
    }
}
