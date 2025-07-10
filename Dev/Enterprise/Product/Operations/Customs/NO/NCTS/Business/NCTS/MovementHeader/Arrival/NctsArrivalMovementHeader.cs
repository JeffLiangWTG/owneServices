using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NO.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader,
	Integration.Customs.NO.IArrivalMovementHeader
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsArrivalMovementHeader.Schema
	{
		public const string GoodsRegistrationNumber = "GoodsRegistrationNumber";
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

	protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

	#region Goods Registration Number

	[MaxLength(Enterprise.Customs.Common.CusEntryNumber.Schema.CE_EntryNumMaxLength)]
	[ResourceStringData("F2EFAAE8-EF57-4CC4-BCBD-F5A20E0E098B", Caption = "Goods Registration Number", ShortCaption = "Goods Reg. Num.", FullDescription = "Reference ID for the customs clearance of this goods. Normally the goods-number but it may be others (such as customs approval no).")]
	public ZString GoodsRegistrationNumber
	{
		get
		{
			if (grnEntryNumber is null)
			{
				grnEntryNumber = CusEntryNumber.Load(Header, CusEntryNumberTypes.Norway.GoodsNumber, CountryCode);
			}

			return grnEntryNumber?.CE_EntryNum ?? ZString.Empty;
		}
		set
		{
			value = value.TrimEndSpaceTab();
			var isGRNFieldNull = grnEntryNumber is null;
			var oldValue = isGRNFieldNull ? ZString.Empty : grnEntryNumber.CE_EntryNum;
			if (oldValue != value)
			{
				CheckMaximumLength(GoodsRegistrationNumberInfo, value);

				if (isGRNFieldNull || grnEntryNumber.IsDeleted)
				{
					grnEntryNumber = CusEntryNumber.LoadOrCreate(Header, CusEntryNumberTypes.Norway.GoodsNumber, CountryCode);
					grnEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
					grnEntryNumber.CE_EntryIsSystemGenerated = true;
				}

				grnEntryNumber.CE_EntryNum = value;

				RegisterEditableChildObject(grnEntryNumber);

				if (!IsValidationSuspended)
				{
					Validation.ValidateGoodsRegistrationNumber();
				}
			}

			GoodsRegistrationNumberInfo.RefreshBinding(oldValue);
		}
	}
	CusEntryNumber grnEntryNumber;

	public ZPropertyInfo GoodsRegistrationNumberInfo => GetZPropertyInfo(Schema.GoodsRegistrationNumber);

	#endregion
}
