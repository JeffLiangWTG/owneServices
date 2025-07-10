using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGADisclaimOption : NonPersistentBusinessObject
	{
		public CusUSLVItemPGADisclaimOption(BusinessObjectFactory factory, string agencyCode)
			: base(factory)
		{
			AgencyCode = agencyCode;
			PGADisclaimReasonList = GetLargestPGADisclaimReasonList(agencyCode);
		}

		public ZString AgencyCode { get; }

		[ReadOnly(true)]
		public ZString AgencyCodeWithDescription
		{
			get; set;
		}

		public CodeDescriptionPairList PGADisclaimReasonList { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		CodeDescriptionPairList GetLargestPGADisclaimReasonList(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AQ1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FS3, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AM1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AM8, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FW1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AL1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EP1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EP5, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EP3, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes._370:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EP7, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM3, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.OM1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM5, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.DT1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.TB3, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.CP1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.DE1, Factory, true, EntryTypeList.Codes.LowValue);
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return US.Business.PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EH2, Factory, true, EntryTypeList.Codes.LowValue);
				default:
					return new PGADisclaimReasonList();
			}
		}

		[List(nameof(PGADisclaimReasonList))]
		[MaxLength(1)]
		public ZString DisclaimReason
		{
			get => disclaimReason;
			set
			{
				var oldValue = disclaimReason;
				if (oldValue != value)
				{
					CheckMaximumLength(DisclaimReasonInfo, value);
					SetNonPersistentPropertyValue(DisclaimReasonInfo, ref disclaimReason, value);
				}
			}
		}

		ZString disclaimReason;

		public ZPropertyInfo DisclaimReasonInfo => GetZPropertyInfo(nameof(DisclaimReason));
	}
}
