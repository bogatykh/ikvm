﻿using System.Runtime.InteropServices;

using FluentAssertions;

using java.lang.management;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Tests.Java.com.sun.management
{

    [TestClass]
    public class OperatingSystemMXBeanTests
    {

        static readonly global::com.sun.management.OperatingSystemMXBean mbean = (global::com.sun.management.OperatingSystemMXBean)ManagementFactory.getOperatingSystemMXBean();

        [TestMethod]
        public void CanGetArch()
        {
            mbean.getArch().Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void CanGetAvailableProcessors()
        {
            mbean.getAvailableProcessors().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetProcessCpuLoad()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mbean.getProcessCpuLoad().Should().Be(-1);
        }

        [TestMethod]
        public void CanGetProcessCpuTime()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                mbean.getProcessCpuTime().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetSystemLoadAverage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mbean.getProcessCpuLoad().Should().Be(-1);
        }

        [TestMethod]
        public void CanGetTotalPhysicalMemorySize()
        {
            mbean.getTotalPhysicalMemorySize().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetFreePhysicalMemorySize()
        {
            mbean.getFreePhysicalMemorySize().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetTotalSwapSpaceSize()
        {
            mbean.getTotalSwapSpaceSize().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetFreeSwapSpaceSize()
        {
            mbean.getFreeSwapSpaceSize().Should().BeGreaterOrEqualTo(1);
        }

        [TestMethod]
        public void CanGetCommittedVirtualMemorySize()
        {
            mbean.getCommittedVirtualMemorySize().Should().BeGreaterOrEqualTo(1);
        }

    }

}
