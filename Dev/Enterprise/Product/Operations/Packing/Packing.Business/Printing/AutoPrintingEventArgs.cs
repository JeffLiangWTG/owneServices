using System;

namespace Enterprise.Packing.Business
{
	public class AutoPrintingEventArgs : EventArgs
	{
		public AutoPrintingEventArgs(PkgPackage package)
		{
			Package = package;
		}

		public readonly PkgPackage Package;
	}
}
