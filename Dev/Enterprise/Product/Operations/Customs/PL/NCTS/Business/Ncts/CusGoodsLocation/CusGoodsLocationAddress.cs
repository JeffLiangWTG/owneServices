using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new JobDocAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

	protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

	NctsCommonMovementHeader DepartureMovementHeader => GoodsLocation?.DepartureMovementHeader;

	NctsCommonMovementHeader ArrivalMovementHeader => GoodsLocation?.ArrivalMovementHeader;

	ZBool IsPhase5DepartureMovement => GoodsLocation?.IsPhase5DepartureMovement ?? false;

	ZBool IsPhase5ArrivalMovement => GoodsLocation?.IsPhase5ArrivalMovement ?? false;

	new void MarkAsNeedingValidation()
	{
		if (IsPhase5DepartureMovement)
		{
			DepartureMovementHeader?.MarkAsNeedingValidation();
		}
		else if (IsPhase5ArrivalMovement)
		{
			ArrivalMovementHeader?.MarkAsNeedingValidation();
		}
	}

	public override ZBool E2_AddressOverride
	{
		get => base.E2_AddressOverride;
		set
		{
			base.E2_AddressOverride = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZGuid E2_OA_Address
	{
		get => base.E2_OA_Address;
		set
		{
			base.E2_OA_Address = value;
			MarkAsNeedingValidation();
		}
	}

	[MaxLength(70)]
	public override ZString E2_Contact
	{
		get => base.E2_Contact;
		set
		{
			base.E2_Contact = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_GovRegNum
	{
		get => base.E2_GovRegNum;
		set
		{
			base.E2_GovRegNum = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_ValidationStatus
	{
		get => base.E2_ValidationStatus;
		set
		{
			base.E2_ValidationStatus = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_RN_NKCountryCode
	{
		get => base.E2_RN_NKCountryCode;
		set
		{
			base.E2_RN_NKCountryCode = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Phone
	{
		get => base.E2_Phone;
		set
		{
			base.E2_Phone = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Fax
	{
		get => base.E2_Fax;
		set
		{
			base.E2_Fax = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Mobile
	{
		get => base.E2_Mobile;
		set
		{
			base.E2_Mobile = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Postcode
	{
		get => base.E2_Postcode;
		set
		{
			base.E2_Postcode = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_CompanyName
	{
		get => base.E2_CompanyName;
		set
		{
			base.E2_CompanyName = value;
			MarkAsNeedingValidation();
		}
	}

	[MaxLength(35)]
	public override ZString E2_City
	{
		get => base.E2_City;
		set
		{
			base.E2_City = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Address2
	{
		get => base.E2_Address2;
		set
		{
			base.E2_Address2 = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Address1
	{
		get => base.E2_Address1;
		set
		{
			base.E2_Address1 = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_GovRegNumType
	{
		get => base.E2_GovRegNumType;
		set
		{
			base.E2_GovRegNumType = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZGeography E2_GeoLocation
	{
		get => base.E2_GeoLocation;
		set
		{
			base.E2_GeoLocation = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZBool E2_IsResidential
	{
		get => base.E2_IsResidential;
		set
		{
			base.E2_IsResidential = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_ScreeningStatus
	{
		get => base.E2_ScreeningStatus;
		set
		{
			base.E2_ScreeningStatus = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_Email
	{
		get => base.E2_Email;
		set
		{
			base.E2_Email = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_AddressType
	{
		get => base.E2_AddressType;
		set
		{
			base.E2_AddressType = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_AddressMap
	{
		get => base.E2_AddressMap;
		set
		{
			base.E2_AddressMap = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_AdditionalAddressInformation
	{
		get => base.E2_AdditionalAddressInformation;
		set
		{
			base.E2_AdditionalAddressInformation = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZString E2_ParentTableCode
	{
		get => base.E2_ParentTableCode;
		set
		{
			base.E2_ParentTableCode = value;
			MarkAsNeedingValidation();
		}
	}

	public override ZGuid E2_ParentID
	{
		get => base.E2_ParentID;
		set
		{
			base.E2_ParentID = value;
			MarkAsNeedingValidation();
		}
	}
}
