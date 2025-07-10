using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation
	, Integration.Customs.PL.INctsCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public const string BM = "BM";

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsDepartureMovementHeader DepartureMovementHeader => (NctsDepartureMovementHeader)base.DepartureMovementHeader;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public ZBool IsPhase5DepartureMovement => (Header?.IsPhase5 ?? false) && (Header?.IsDepartureMovement ?? false);

	public ZBool IsPhase5ArrivalMovement => (Header?.IsPhase5 ?? false) && (Header?.IsArrivalMovement ?? false);

	public override bool ContactPersonDataVisible => base.ContactPersonDataVisible && CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

	new void MarkAsNeedingValidation()
	{
		if (IsPhase5DepartureMovement)
		{
			DepartureMovementHeader?.MarkAsNeedingValidation();
			Address.MarkAsNeedingValidation();
		}
		else if (IsPhase5ArrivalMovement)
		{
			ArrivalMovementHeader?.MarkAsNeedingValidation();
			Address.MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			base.CGL_Qualifier = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_Type
	{
		get => base.CGL_Type;
		set
		{
			base.CGL_Type = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_ParentTableCode
	{
		get => base.CGL_ParentTableCode;
		set
		{
			base.CGL_ParentTableCode = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZGuid CGL_ParentID
	{
		get => base.CGL_ParentID;
		set
		{
			base.CGL_ParentID = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_LocationUse
	{
		get => base.CGL_LocationUse;
		set
		{
			base.CGL_LocationUse = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_AdditionalIdentifier
	{
		get => base.CGL_AdditionalIdentifier;
		set
		{
			base.CGL_AdditionalIdentifier = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString CGL_CustomsOffice
	{
		get => base.CGL_CustomsOffice;
		set
		{
			base.CGL_CustomsOffice = value;
			MarkAsNeedingValidation();
		}
	}
}
