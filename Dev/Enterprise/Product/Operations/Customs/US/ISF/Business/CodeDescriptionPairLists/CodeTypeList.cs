using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CodeTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CBPAssignedNumber = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			public const string EncryptedConsigneeNumber = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
			public const string DUNS = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			public const string DUNSPlus4 = OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4;
			public const string FIRMS = OrgCusCode.USACodeTypes.FIRMSCode;
			public const string IRS = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			public const string Passport = OrgCusCode.CodeTypes.PassportID;
			public const string SocialSecurity = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			public const string SCAC = OrgCusCode.CodeTypes.CarrierCode;
		}

		public static class Descriptions
		{
			public const string CBPAssignedNumber = "CBP Assigned Number";
			public const string EncryptedConsigneeNumber = "CBP encrypted Consignee ID";
			public const string DUNS = "DUNS Number";
			public const string DUNSPlus4 = "DUNS+4 Number";
			public const string FIRMS = "FIRMS Code";
			public const string IRS = "Employer Identification Number - IRS#";
			public const string Passport = "Passport Number (Required for Shipment Type Code = '03')";
			public const string SocialSecurity = "Social Security Number";
			public const string SCAC = "Standard Carrier Alpha Code (SCAC)";
		}
	}

	public class ImporterCodeTypeList : CodeTypeList
	{
		public ImporterCodeTypeList()
		{
			AddPair(Codes.CBPAssignedNumber, Descriptions.CBPAssignedNumber);
			AddPair(Codes.IRS, Descriptions.IRS);
			AddPair(Codes.Passport, Descriptions.Passport);
			AddPair(Codes.SocialSecurity, Descriptions.SocialSecurity);
			AddPair(Codes.SCAC, Descriptions.SCAC);
		}
	}

	public class ConsigneeCodeTypeList : CodeTypeList
	{
		public ConsigneeCodeTypeList()
		{
			AddPair(Codes.IRS, Descriptions.IRS);
			AddPair(Codes.SocialSecurity, Descriptions.SocialSecurity);
			AddPair(Codes.CBPAssignedNumber, Descriptions.CBPAssignedNumber);
			AddPair(Codes.EncryptedConsigneeNumber, Descriptions.EncryptedConsigneeNumber);
			AddPair(Codes.Passport, Descriptions.Passport);
		}

		public static ZString[] GetListInOrderOfImportance()
		{
			return new ZString[]
			{
				OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
				OrgCusCode.USACodeTypes.CBPAssignedNumber,
				OrgCusCode.USACodeTypes.SocialSecurityNumber,
				OrgCusCode.CodeTypes.PassportID,
				OrgCusCode.USACodeTypes.EncryptedConsigneeNumber
			};
		}
	}

	public class OtherPartiesCodeTypeList : CodeTypeList
	{
		public OtherPartiesCodeTypeList()
		{
			AddPair(Codes.DUNS, Descriptions.DUNS);
			AddPair(Codes.DUNSPlus4, Descriptions.DUNSPlus4);
		}

		public static bool IsDUNSType(ZString code)
		{
			return code == Codes.DUNS || code == Codes.DUNSPlus4;
		}
	}

	public class OtherUSPartiesCodeTypeList : OtherPartiesCodeTypeList
	{
		public OtherUSPartiesCodeTypeList()
		{
			AddPair(Codes.IRS, Descriptions.IRS);
			AddPair(Codes.SocialSecurity, Descriptions.SocialSecurity);
			AddPair(Codes.CBPAssignedNumber, Descriptions.CBPAssignedNumber);
			AddPair(Codes.EncryptedConsigneeNumber, Descriptions.EncryptedConsigneeNumber);
		}
	}

	public class ShipToPartyCodeTypeList : OtherPartiesCodeTypeList
	{
		public ShipToPartyCodeTypeList()
		{
			AddPair(Codes.FIRMS, Descriptions.FIRMS);
		}
	}
}
