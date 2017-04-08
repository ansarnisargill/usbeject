#if NET20 || NET35
using System;

namespace UsbEject.Library
{
    internal sealed class Lazy<T>
    {
        private readonly Func<T> createValue;
        private readonly object _lock = new object();
        private bool isValueCreated;
        private T value;

        public Lazy(Func<T> createValue)
        {
            if (createValue == null)
                throw new ArgumentNullException(nameof(createValue));

            this.createValue = createValue;
        }

        public T Value
        {
            get
            {
                if (!isValueCreated)
                {
                    lock (_lock)
                    {
                        if (!isValueCreated)
                        {
                            value = createValue();
                            isValueCreated = true;
                        }
                    }
                }
                return value;
            }
        }

        public bool IsValueCreated
        {
            get { return isValueCreated; }
        }
    }
}
#endif