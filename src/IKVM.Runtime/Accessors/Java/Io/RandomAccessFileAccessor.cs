﻿namespace IKVM.Runtime.Accessors.Java.Io
{

#if FIRST_PASS == false && EXPORTER == false && IMPORTER == false

    /// <summary>
    /// Provides runtime access to the 'java.io.RandomAccessFile' type.
    /// </summary>
    internal sealed class RandomAccessFileAccessor : Accessor<object>
    {

        FieldAccessor<object, object> fd;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resolver"></param>
        public RandomAccessFileAccessor(AccessorTypeResolver resolver) :
            base(resolver, "java.io.RandomAccessFile")
        {

        }

        /// <summary>
        /// Gets the value of the 'fd' field.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public object GetFd(object self) => GetField(ref fd, nameof(fd)).GetValue(self);

        /// <summary>
        /// Sets the value of the 'fd' field.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetFd(object self, object value) => GetField(ref fd, nameof(fd)).SetValue(self, value);

    }

#endif

}
