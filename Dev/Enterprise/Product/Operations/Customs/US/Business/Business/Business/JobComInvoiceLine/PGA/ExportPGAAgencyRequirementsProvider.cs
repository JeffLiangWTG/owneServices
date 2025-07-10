using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class ExportPGARequirementsProvider : PGAAgencyRequirementsProvider
	{
		protected ExportPGARequirementsProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override bool IsPGAReqirementRelevant
		{
			get { return true; }
		}

		public override ZBool CanDisclaim(string agencyCode)
		{
			return agencyCode == GovernmentAgencyProgramCodeList.Codes.TTB
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.DEA
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.EPA;
		}

		public override bool DoesMatchCertificationMode(string agencyCode)
		{
			return true;
		}

		public override ZPropertyInfo GetDisclaimReasonInfo(string agencyCode)
		{
			return null;
		}

		public override CodeDescriptionPairList GetDisclaimReasonList(string agencyCode)
		{
			return null;
		}

		public override CodeDescriptionPairList GetGovernmentAgencyProgramCodeList()
		{
			return Factory.GetCachedValue("ExportPGAAgencyProgramCodeList", delegate
			{
				var result = new CodeDescriptionPairList();

				foreach (var code in SupportedPGACodesInExport)
				{
					result.AddPair(code);
				}

				return result;
			});
		}

		IEnumerable<string> SupportedPGACodesInExport
		{
			get
			{
				yield return GovernmentAgencyProgramCodeList.Codes.AMS;
				yield return GovernmentAgencyProgramCodeList.Codes.EPA;
				yield return GovernmentAgencyProgramCodeList.NMFS;
				yield return GovernmentAgencyProgramCodeList.Codes.ATF;
				yield return GovernmentAgencyProgramCodeList.Codes.FWS;
				yield return GovernmentAgencyProgramCodeList.Codes.DEA;
				yield return GovernmentAgencyProgramCodeList.Codes.TTB;
			}
		}

		public override ZString GetRequirementDescription(string agencyCode)
		{
			return ZString.Empty;
		}

		public override bool IsPGA(string agencyCode)
		{
			return true;
		}

		public override void ValidateDisclaimReason(string agencyCode)
		{
		}
	}

	public class ExportPGAInvoiceLineRequirementsProvider : ExportPGARequirementsProvider
	{
		public ExportPGAInvoiceLineRequirementsProvider(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		public override ZPropertyInfo GetIndicatorInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return invoiceLine.US_AMSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					return invoiceLine.US_ATFIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return invoiceLine.US_FWSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return invoiceLine.US_DEAIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.EPA:
					return invoiceLine.US_PSTIndicatorInfo;
				case GovernmentAgencyProgramCodeList.NMFS:
					return invoiceLine.US_NMFSHMSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return invoiceLine.US_TTBIndInfo;
			}
			return null;
		}

		public override void ValidateIndicator(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					invoiceLine.AddInfoValidation.ValidateUS_ATFInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					invoiceLine.AddInfoValidation.ValidateUS_FWSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					invoiceLine.AddInfoValidation.ValidateUS_DEAInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.EPA:
					invoiceLine.AddInfoValidation.ValidateUS_PSTIndicator();
					break;
				case GovernmentAgencyProgramCodeList.NMFS:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSHMSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					invoiceLine.AddInfoValidation.ValidateUS_TTBInd();
					break;
			}
		}
	}

	public class ExportPGAProductRequirementsProvider : ExportPGARequirementsProvider
	{
		public ExportPGAProductRequirementsProvider(CusClassPartPivot pivot)
			: base(pivot.Factory)
		{
			this.pivot = pivot;
		}
		readonly CusClassPartPivot pivot;

		public override ZPropertyInfo GetIndicatorInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return pivot.CD_AMSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					return pivot.CD_ATFIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return pivot.CD_FWSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return pivot.CD_DEAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.EPA:
					return pivot.CD_PSTIndicatorInfo;
				case GovernmentAgencyProgramCodeList.NMFS:
					return pivot.CD_NMFSHMSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return pivot.CD_TTBIndicatorInfo;
			}

			return null;
		}

		public override void ValidateIndicator(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					pivot.Details.Validation.ValidateCD_AMSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					pivot.Details.Validation.ValidateCD_ATFIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					pivot.Details.Validation.ValidateCD_FWSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					pivot.Details.Validation.ValidateCD_DEAIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.EPA:
					pivot.Details.Validation.ValidateCD_PSTIndicator();
					break;
				case GovernmentAgencyProgramCodeList.NMFS:
					pivot.Details.Validation.ValidateCD_NMFSHMSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					pivot.Details.Validation.ValidateCD_TTBIndicator();
					break;
			}
		}
	}
}
