﻿/*
  Copyright (C) 2007-2014 Jeroen Frijters

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
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.AccessControl;

using IKVM.Runtime.Vfs;

using Microsoft.Win32.SafeHandles;

namespace IKVM.Java.Externs.java.io
{

    static class FileDescriptor
    {

        private static Converter<int, int> fsync;

        public static Stream open(string name, FileMode mode, FileAccess access)
        {
            if (VfsTable.Default.IsPath(name))
            {
                return VfsTable.Default.Open(name, mode, access);
            }
            else if (mode == FileMode.Append)
            {
#if NETFRAMEWORK
            // this is the way to get atomic append behavior for all writes
            return new FileStream(name, mode, FileSystemRights.AppendData, FileShare.ReadWrite, 1, FileOptions.None);
#else
                // the above constructor does not exist in .net core
                // since the buffer size is 1 byte, it's always atomic
                // if the buffer size needs to be bigger, find a way for the atomic append
                return new FileStream(name, mode, access, FileShare.ReadWrite, 1, false);
#endif
            }
            else
            {
                return new FileStream(name, mode, access, FileShare.ReadWrite, 1, false);
            }
        }

        [SecuritySafeCritical]
        public static bool flushPosix(FileStream fs)
        {
            if (fsync == null)
            {
                ResolveFSync();
            }
            bool success = false;
            SafeFileHandle handle = fs.SafeFileHandle;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle.DangerousAddRef(ref success);
                return fsync(handle.DangerousGetHandle().ToInt32()) == 0;
            }
            finally
            {
                if (success)
                {
                    handle.DangerousRelease();
                }
            }
        }

        [SecurityCritical]
        private static void ResolveFSync()
        {
            // we don't want a build time dependency on this Mono assembly, so we use reflection
            Type type = Type.GetType("Mono.Unix.Native.Syscall, Mono.Posix, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
            if (type != null)
            {
                fsync = (Converter<int, int>)Delegate.CreateDelegate(typeof(Converter<int, int>), type, "fsync", false, false);
            }
            if (fsync == null)
            {
                fsync = DummyFSync;
            }
        }

        private static int DummyFSync(int fd)
        {
            return 0;
        }
    }

}