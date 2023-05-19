﻿using System;
using System.Collections;

namespace IKVM.Runtime.Accessors.Ikvm.Util
{

#if FIRST_PASS == false && EXPORTER == false && IMPORTER == false

    class EnumeratorIteratorAccessor : Accessor<object>
    {

        MethodAccessor<Func<IEnumerator, object>> init;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resolver"></param>
        public EnumeratorIteratorAccessor(AccessorTypeResolver resolver) :
            base(resolver, "ikvm.util.EnumeratorIterator")
        {

        }

        /// <summary>
        /// Gets the value of the 'enumerator' field.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public object Init(IEnumerator enumerator) => GetConstructor(ref init, typeof(IEnumerator)).Invoker(enumerator);

    }

#endif

}