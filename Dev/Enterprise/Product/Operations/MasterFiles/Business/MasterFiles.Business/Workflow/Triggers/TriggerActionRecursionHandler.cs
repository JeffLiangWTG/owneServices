using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Workflow.Triggers
{
	public sealed class TriggerActionRecursionHandler
	{
		#region Static Members
		public static IDisposable WithActionRecursionDetection(TriggerActionRecursionHandler handler = null)
		{
			if (threadStaticInstance == null)
			{
				threadStaticInstance = handler ?? new TriggerActionRecursionHandler();
				return new DisposableAction(() => threadStaticInstance = null);
			}
			else
			{
				return null;
			}
		}

		public static TriggerActionRecursionHandler Instance
		{
			get
			{
				var instance = threadStaticInstance;
				if (instance == null)
				{
					ErrorReporter.ReportOnce("aa87b5af-291e-4c83-87b6-3ac6693516f7", "Raising events without recursion handling.");
					return new TriggerActionRecursionHandler();
				}
				else
				{
					return instance;
				}
			}
		}

		[ThreadStatic]
		static TriggerActionRecursionHandler threadStaticInstance;
		#endregion

		#region API
		internal bool IsDuplicateAction(ZGuid identifier) => !ranActions.Add(identifier);

		readonly HashSet<ZGuid> ranActions = new HashSet<ZGuid>();
		#endregion
	}
}
