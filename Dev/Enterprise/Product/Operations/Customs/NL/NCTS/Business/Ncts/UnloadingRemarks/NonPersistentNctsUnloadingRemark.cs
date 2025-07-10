using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NonPersistentNctsUnloadingRemark : AutoNonPersistentNctsUnloadingRemark
{
	public NonPersistentNctsUnloadingRemark(BusinessObjectFactory factory) : base(factory)
	{
	}

	public NonPersistentNctsUnloadingRemark(NctsArrivalMovementHeader movementHeader) : base(movementHeader.Factory)
	{
	}

	public ZString ToFormattedString() => $"<{(ItemNumber == 0 ? ItemNumber.ToString("D2") : ItemNumber)};{EoriNumber};{Code};{Number}>";

	[List(nameof(Lookups) + "." + nameof(NonPersistentNctsUnloadingRemarkLookups.CodeList))]
	public override ZString Code { get => base.Code; set => base.Code = value; }

	#region Lookups

	public NonPersistentNctsUnloadingRemarkLookups Lookups
	{
		get
		{
			if (fLookups == null || !IsLookupsCachedInBase)
			{
				fLookups = GetNewLookups();
			}

			return fLookups;
		}
	}

	protected virtual NonPersistentNctsUnloadingRemarkLookups GetNewLookups()
	{
		return new NonPersistentNctsUnloadingRemarkLookups(this);
	}

	NonPersistentNctsUnloadingRemarkLookups fLookups;

	#endregion
}
