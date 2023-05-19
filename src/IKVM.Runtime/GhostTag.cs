﻿/*
  Copyright (C) 2002-2015 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Runtime.CompilerServices;
using System.Threading;

using IKVM.Attributes;
using IKVM.Internal;

#if IMPORTER || EXPORTER
using IKVM.Reflection;
using IKVM.Reflection.Emit;

using Type = IKVM.Reflection.Type;
#else
#endif

namespace IKVM.Runtime
{

    static class GhostTag
    {

        static volatile ConditionalWeakTable<object, TypeWrapper> dict;

        internal static void SetTag(object obj, RuntimeTypeHandle typeHandle)
        {
#if FIRST_PASS || IMPORTER
            throw new NotImplementedException();
#else
            SetTag(obj, ClassLoaderWrapper.GetWrapperFromType(Type.GetTypeFromHandle(typeHandle)));
#endif
        }

        internal static void SetTag(object obj, TypeWrapper wrapper)
        {
            if (dict == null)
            {
                ConditionalWeakTable<object, TypeWrapper> newDict = new ConditionalWeakTable<object, TypeWrapper>();
                Interlocked.CompareExchange(ref dict, newDict, null);
            }
            dict.Add(obj, wrapper);
        }

        internal static TypeWrapper GetTag(object obj)
        {
            if (dict != null)
            {
                TypeWrapper tw;
                dict.TryGetValue(obj, out tw);
                return tw;
            }
            return null;
        }

        // this method is called from <GhostType>.IsInstanceArray()
        internal static bool IsGhostArrayInstance(object obj, RuntimeTypeHandle typeHandle, int rank)
        {
#if FIRST_PASS || IMPORTER
            throw new NotImplementedException();
#else
            TypeWrapper tw1 = GhostTag.GetTag(obj);
            if (tw1 != null)
            {
                TypeWrapper tw2 = ClassLoaderWrapper.GetWrapperFromType(Type.GetTypeFromHandle(typeHandle)).MakeArrayType(rank);
                return tw1.IsAssignableTo(tw2);
            }
            return false;
#endif
        }

        // this method is called from <GhostType>.CastArray()
        [HideFromJava]
        internal static void ThrowClassCastException(object obj, RuntimeTypeHandle typeHandle, int rank)
        {
#if FIRST_PASS || IMPORTER
            throw new NotImplementedException();
#else
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(global::ikvm.runtime.Util.getClassFromObject(obj).getName()).Append(" cannot be cast to ")
                .Append('[', rank).Append('L').Append(global::ikvm.runtime.Util.getClassFromTypeHandle(typeHandle).getName()).Append(';');
            throw new global::java.lang.ClassCastException(sb.ToString());
#endif
        }
    }

}
