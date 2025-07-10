using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS;

public class NonPersistentNctsUnloadingRemarkCollection : NonPersistentBusinessObjectCollection<NonPersistentNctsUnloadingRemark>
{
	public NonPersistentNctsUnloadingRemarkCollection(NctsArrivalMovementHeader movementHeader) : base(movementHeader.Factory)
	{
		this.movementHeader = movementHeader;
		PopulateNonPersistentNctsUnloadingRemarkCollection();
	}
	readonly NctsArrivalMovementHeader movementHeader;

	void PopulateNonPersistentNctsUnloadingRemarkCollection()
	{
		var pattern = @"\<([^\<\>]+)\>";
		String[] unloadingRemarkArray = Regex.Split(movementHeader.BM_UnloadingRemarks, pattern);
		if (!unloadingRemarkArray.IsNullOrEmpty() && unloadingRemarkArray.Length > 0)
		{
			foreach (var unloadingRemarkString in unloadingRemarkArray)
			{
				String[] unloadingRemarkSplit = unloadingRemarkString.Split(';');
				if (!unloadingRemarkSplit.IsNullOrEmpty() && unloadingRemarkSplit.Length % 4 == 0)
				{
					var unloadingRemark = this.AddNew();
					using (unloadingRemark.SuspendSettingHasChanges())
					{
						unloadingRemark.ItemNumber = ZShort.Parse(unloadingRemarkSplit[0]);
						unloadingRemark.EoriNumber = unloadingRemarkSplit[1];
						unloadingRemark.Code = unloadingRemarkSplit[2];
						unloadingRemark.Number = unloadingRemarkSplit[3];
					}
				}
			}
		}
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => new NonPersistentNctsUnloadingRemark(movementHeader);
}
