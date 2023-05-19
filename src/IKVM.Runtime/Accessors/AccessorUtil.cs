﻿using System;
using System.Threading;

namespace IKVM.Runtime.Accessors
{

    static class AccessorUtil
    {

        /// <summary>
        /// Gets the value at the given location or initializes it if null.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static T LazyGet<T>(ref T location, Func<T> create)
            where T : class
        {
            if (create is null)
                throw new ArgumentNullException(nameof(create));

            if (location == null)
                Interlocked.CompareExchange(ref location, create(), null);

            return location;
        }

    }

}
