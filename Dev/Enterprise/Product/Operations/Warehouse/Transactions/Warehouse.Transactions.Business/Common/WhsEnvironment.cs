using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsEnvironment
	{
		#region IsRF

		public static bool IsRF
		{
			get { return Globals.IsWeb && !Globals.IsUserInteractive; }
			#region Testing
#if DEBUG
			set
			{
				if (Globals.IsTest)
				{
					Globals.IsWeb = value;
					Globals.IsUserInteractive = !value;
				}
				else
				{
					throw new InvalidOperationException("IsRF should not be set outside of tests.");
				}
			}
#endif
			#endregion
		}

		#endregion

		#region IsWebTracker

		public static bool IsWebTracker
		{
			get { return Globals.IsWeb && Globals.IsUserInteractive; }
			#region Testing
#if DEBUG
			set
			{
				if (Globals.IsTest)
				{
					Globals.IsWeb = value;
					Globals.IsUserInteractive = true; // when setting to false we want to default to CargowiseOne and not Service Tasks / Batch Processor
				}
				else
				{
					throw new InvalidOperationException("IsWebTracker should not be set outside of tests.");
				}
			}
#endif
			#endregion
		}

		#endregion
	}
}
