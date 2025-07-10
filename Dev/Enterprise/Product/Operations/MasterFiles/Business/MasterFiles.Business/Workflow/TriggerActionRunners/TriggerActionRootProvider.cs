using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	class TriggerActionRootProvider : ITriggerActionRootProvider
	{
		public IBusiness[] GetRoots(IProcessTaskNotification action, BusinessObject parent, IStmALog @event)
		{
			var result = ((IDynamicRootProvider)action).AugmentedRoots(parent)
				.Cast<IBusiness>();

			if (@event != null)
			{
				result = result
					.Append(@event)
					.Append((IBusiness)Activator.CreateInstance(typeof(TriggeringEventPropertyProvider<>).MakeGenericType(@event.GetType()), @event));
			}

			return result.ToArray();
		}
	}

	public class TriggeringEventPropertyProvider<TStmALog> : NonPersistentBusinessObject, IAllowMacroAccessToAllPublicProperties
		where TStmALog : class, IStmALog
	{
		public TStmALog TriggeringEvent { get; }

		public TriggeringEventPropertyProvider(TStmALog log)
		{
			TriggeringEvent = log;
		}
	}
}
