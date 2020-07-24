// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

#if NET20 || NET35
using System;

namespace UsbEject
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