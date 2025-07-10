using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	[SuppressMessage("Microsoft.Usage", "CA1060:Move P/Invokes to NativeMethods class")]
	[SuppressMessage("Microsoft.Usage", "CA2216:Disposable types should declare finalizer")]
	[SuppressMessage("Microsoft.Usage", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	public class Job : IDisposable
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr CreateJobObject(IntPtr a, string lpName);

		[DllImport("kernel32.dll")]
		static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		IntPtr handle;
		bool disposed;
		const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

		public Job()
		{
			handle = CreateJobObject(IntPtr.Zero, null);

			var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
			{
				LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
			};

			var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
			{
				BasicLimitInformation = info
			};

			var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
			var extendedInfoPtr = Marshal.AllocHGlobal(length);
			Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

			if (!SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
			{
				throw new NotSupportedException($"Unable to set information.  Error: {Marshal.GetLastWin32Error()}");
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{ }

			Close(out var _);
			disposed = true;
		}

		public virtual bool Close(out int errorCode)
		{
			var result = CloseHandle(handle);
			errorCode = result ? 0 : Marshal.GetLastWin32Error();
			return result;
		}

		public bool AddProcess(IntPtr processHandle, out int errorCode)
		{
			var result = AssignProcessToJobObject(handle, processHandle);
			errorCode = result ? 0 : Marshal.GetLastWin32Error();
			return result;
		}
	}

	#region Helper classes

	[SuppressMessage("Microsoft.Usage", "CA1815:Override equals and operator equals on value types")]
	[StructLayout(LayoutKind.Sequential)]
	struct IO_COUNTERS
	{
		public ulong ReadOperationCount;
		public ulong WriteOperationCount;
		public ulong OtherOperationCount;
		public ulong ReadTransferCount;
		public ulong WriteTransferCount;
		public ulong OtherTransferCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_BASIC_LIMIT_INFORMATION
	{
		public long PerProcessUserTimeLimit;
		public long PerJobUserTimeLimit;
		public uint LimitFlags;
		public UIntPtr MinimumWorkingSetSize;
		public UIntPtr MaximumWorkingSetSize;
		public uint ActiveProcessLimit;
		public UIntPtr Affinity;
		public uint PriorityClass;
		public uint SchedulingClass;
	}

	[SuppressMessage("Microsoft.Usage", "CA1815:Override equals and operator equals on value types")]
	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_ATTRIBUTES
	{
		public uint nLength;
		public IntPtr lpSecurityDescriptor;
		public int bInheritHandle;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
	{
		public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
		public IO_COUNTERS IoInfo;
		public UIntPtr ProcessMemoryLimit;
		public UIntPtr JobMemoryLimit;
		public UIntPtr PeakProcessMemoryUsed;
		public UIntPtr PeakJobMemoryUsed;
	}

	public enum JobObjectInfoType
	{
		None = 0,
		BasicLimitInformation = 2,
		BasicUIRestrictions = 4,
		SecurityLimitInformation = 5,
		EndOfJobTimeInformation = 6,
		AssociateCompletionPortInformation = 7,
		ExtendedLimitInformation = 9,
		GroupInformation = 11
	}

	#endregion

}
