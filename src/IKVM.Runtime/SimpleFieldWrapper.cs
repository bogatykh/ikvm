﻿/*
  Copyright (C) 2002-2014 Jeroen Frijters

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
using System.Diagnostics;

using IKVM.Runtime;

#if IMPORTER || EXPORTER
using IKVM.Reflection;
using IKVM.Reflection.Emit;

using Type = IKVM.Reflection.Type;
#else
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

#endif

#if IMPORTER
using IKVM.Tools.Importer;
#endif

namespace IKVM.Internal
{

    /// <summary>
    /// Field wrapper implementation for standard fields.
    /// </summary>
    sealed class SimpleFieldWrapper : FieldWrapper
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="fieldType"></param>
        /// <param name="fi"></param>
        /// <param name="name"></param>
        /// <param name="sig"></param>
        /// <param name="modifiers"></param>
        internal SimpleFieldWrapper(TypeWrapper declaringType, TypeWrapper fieldType, FieldInfo fi, string name, string sig, ExModifiers modifiers) :
            base(declaringType, fieldType, name, sig, modifiers, fi)
        {
            Debug.Assert(!(fieldType == PrimitiveTypeWrapper.DOUBLE || fieldType == PrimitiveTypeWrapper.LONG) || !IsVolatile);
        }

#if !IMPORTER && !EXPORTER && !FIRST_PASS

        internal override object GetValue(object obj)
        {
            return GetField().GetValue(obj);
        }

        internal override void SetValue(object obj, object value)
        {
            GetField().SetValue(obj, value);
        }

#endif

#if EMITTERS

        protected override void EmitGetImpl(CodeEmitter il)
        {
            var fi = GetField();

            if (IsStatic == false && DeclaringType.IsNonPrimitiveValueType)
                il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);

            // conduct load operation with volatile tag if field is volatile
            if (IsVolatile)
            {
                il.EmitMemoryBarrier();
                il.Emit(OpCodes.Volatile);
            }

            if (IsStatic)
                il.Emit(OpCodes.Ldsfld, fi);
            else
                il.Emit(OpCodes.Ldfld, fi);
        }

        protected override void EmitSetImpl(CodeEmitter il)
        {
            var fi = GetField();

            if (IsStatic == false && DeclaringType.IsNonPrimitiveValueType)
            {
                var value = il.DeclareLocal(FieldTypeWrapper.TypeAsLocalOrStackType);
                il.Emit(OpCodes.Stloc, value);
                il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);
                il.Emit(OpCodes.Ldloc, value);
            }

            // conduct store operation with volatile tag if field is volatile
            if (IsVolatile)
                il.Emit(OpCodes.Volatile);

            if (IsStatic)
                il.Emit(OpCodes.Stsfld, fi);
            else
                il.Emit(OpCodes.Stfld, fi);

            if (IsVolatile)
                il.EmitMemoryBarrier();
        }

        protected override void EmitUnsafeGetImpl(CodeEmitter il)
        {
            var fi = GetField();

            if (IsStatic)
            {
                if (IsFinal)
                {
                    // perform an indirect load to prevent the JIT from caching the value
                    il.Emit(OpCodes.Ldsflda, fi);
                    FieldTypeWrapper.EmitLdind(il);
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, fi);
                }
            }
            else
            {
                if (DeclaringType.IsNonPrimitiveValueType)
                    il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);

                il.Emit(OpCodes.Ldfld, fi);
            }
        }

        protected override void EmitUnsafeSetImpl(CodeEmitter il)
        {
            var fi = GetField();

            if (IsStatic == false && DeclaringType.IsNonPrimitiveValueType)
            {
                var value = il.DeclareLocal(FieldTypeWrapper.TypeAsLocalOrStackType);
                il.Emit(OpCodes.Stloc, value);
                il.Emit(OpCodes.Unbox, DeclaringType.TypeAsTBD);
                il.Emit(OpCodes.Ldloc, value);
            }

            if (IsStatic)
            {
                il.Emit(OpCodes.Stsfld, fi);
            }
            else
            {
                il.Emit(OpCodes.Stfld, fi);
            }
        }

        protected override void EmitUnsafeVolatileGetImpl(CodeEmitter il)
        {
            var fi = GetField();

            if (IsStatic)
            {
                il.Emit(OpCodes.Ldsflda, fi);
            }
            else
            {
                if (DeclaringType.IsNonPrimitiveValueType)
                    il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);

                il.Emit(OpCodes.Ldflda, fi);
            }


            if (FieldTypeWrapper == PrimitiveTypeWrapper.BOOLEAN)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadBoolean);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.BYTE)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadByte);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.CHAR)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadChar);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.SHORT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadShort);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.INT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadInt);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.LONG)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadLong);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.FLOAT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadFloat);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.DOUBLE)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileReadDouble);
            else
            {
                il.EmitMemoryBarrier();
                il.Emit(OpCodes.Volatile);
                FieldTypeWrapper.EmitLdind(il);
            }
        }

        protected override void EmitUnsafeVolatileSetImpl(CodeEmitter il)
        {
            var fi = GetField();

            var value = il.DeclareLocal(FieldTypeWrapper.TypeAsLocalOrStackType);
            il.Emit(OpCodes.Stloc, value);

            if (IsStatic)
            {
                il.Emit(OpCodes.Ldsflda, fi);
            }
            else
            {
                if (DeclaringType.IsNonPrimitiveValueType)
                    il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);

                il.Emit(OpCodes.Ldflda, fi);
            }

            il.Emit(OpCodes.Ldloc, value);

            if (FieldTypeWrapper == PrimitiveTypeWrapper.BOOLEAN)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteBoolean);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.BYTE)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteByte);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.CHAR)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteChar);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.SHORT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteShort);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.INT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteInt);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.LONG)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteLong);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.FLOAT)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteFloat);
            else if (FieldTypeWrapper == PrimitiveTypeWrapper.DOUBLE)
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.VolatileWriteDouble);
            else
            {
                il.Emit(OpCodes.Volatile);
                FieldTypeWrapper.EmitStind(il);
                il.EmitMemoryBarrier();
            }
        }

        protected override void EmitUnsafeCompareAndSwapImpl(CodeEmitter il)
        {
            var fi = GetField();

            var update = il.AllocTempLocal(FieldTypeWrapper.TypeAsLocalOrStackType);
            var expect = il.AllocTempLocal(FieldTypeWrapper.TypeAsLocalOrStackType);

            il.Emit(OpCodes.Stloc, update);
            il.Emit(OpCodes.Stloc, expect);

            if (IsStatic)
            {
                il.Emit(OpCodes.Ldsflda, fi);
            }
            else
            {
                if (DeclaringType.IsNonPrimitiveValueType)
                    il.Emit(OpCodes.Unbox, DeclaringType.TypeAsLocalOrStackType);

                il.Emit(OpCodes.Ldflda, fi);
            }

            il.Emit(OpCodes.Ldloc, expect);
            il.Emit(OpCodes.Ldloc, update);

            if (FieldTypeWrapper.IsPrimitive)
            {
                if (FieldTypeWrapper == PrimitiveTypeWrapper.INT)
                {
                    il.Emit(OpCodes.Call, ByteCodeHelperMethods.CompareAndSwapInt);
                }
                else if (FieldTypeWrapper == PrimitiveTypeWrapper.LONG)
                {
                    il.Emit(OpCodes.Call, ByteCodeHelperMethods.CompareAndSwapLong);
                }
                else if (FieldTypeWrapper == PrimitiveTypeWrapper.DOUBLE)
                {
                    il.Emit(OpCodes.Call, ByteCodeHelperMethods.CompareAndSwapDouble);
                }
                else
                {
                    throw new InternalException();
                }
            }
            else
            {
                il.Emit(OpCodes.Call, ByteCodeHelperMethods.CompareAndSwapObject);
            }

            il.ReleaseTempLocal(expect);
            il.ReleaseTempLocal(update);
        }

#endif

    }

}
