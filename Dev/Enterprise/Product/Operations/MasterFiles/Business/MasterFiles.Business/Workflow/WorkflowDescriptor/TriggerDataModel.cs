using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerDataModel
	{
		public TriggerDataModel(IBaseTrigger trigger, IBusiness parent)
		{
			if (trigger is IUniversalTemplateTrigger universalTrigger)
			{
				previousEventDate = Lazy.Create(() =>
				{
					var jobVersion = universalTrigger.GetOrCreateJobVersionOfTrigger(parent, false);
					return ((IBaseTrigger)jobVersion)?.ActualDate ?? ZDateTime.Empty;
				});
			}
			else
			{
				previousEventDate = Lazy.Create(() => trigger.ActualDate);
			}
		}

		public TriggerDataModel(Lazy<ZDateTime> previousEventDate)
		{
			this.previousEventDate = previousEventDate;
		}

		readonly Lazy<ZDateTime> previousEventDate;

		#region Properties

		public ZDateTime PreviousEventDate => previousEventDate.Value;

		#endregion
	}
}
