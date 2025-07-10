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
	public abstract class PGAAgencyRequirementsProvider
	{
		protected PGAAgencyRequirementsProvider(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public readonly BusinessObjectFactory Factory;

		public abstract bool DoesMatchCertificationMode(string agencyCode);
		public abstract bool IsPGA(string agencyCode);
		public abstract bool IsPGAReqirementRelevant { get; }

		public abstract ZString GetRequirementDescription(string agencyCode);

		public abstract ZPropertyInfo GetIndicatorInfo(string agencyCode);
		public abstract void ValidateIndicator(string agencyCode);

		public abstract ZPropertyInfo GetDisclaimReasonInfo(string agencyCode);
		public abstract void ValidateDisclaimReason(string agencyCode);
		public abstract CodeDescriptionPairList GetGovernmentAgencyProgramCodeList();

		public abstract CodeDescriptionPairList GetDisclaimReasonList(string agencyCode);

		public virtual ZBool CanDisclaim(string agencyCode)
		{
			return GovernmentAgencyProgramCodeList.CanDisclaim(agencyCode);
		}
	}

	class ProductPGAgencyRequirementProvider : PGAAgencyRequirementsProvider
	{
		public ProductPGAgencyRequirementProvider(CusClassPartPivot pivot)
			: base(pivot.Factory)
		{
			this.pivot = pivot;
		}

		readonly CusClassPartPivot pivot;

		public override bool DoesMatchCertificationMode(string agencyCode)
		{
			return true;
		}

		public override bool IsPGA(string agencyCode)
		{
			bool result = true;
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.DOT:
					result = false;
					break;
			}
			return result;
		}

		public override bool IsPGAReqirementRelevant
		{
			get { return true; }
		}

		public override ZString GetRequirementDescription(string agencyCode)
		{
			var result = ZString.Empty;
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = pivot.US_APHISRequirementDesc;
					break;

				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = pivot.US_ACEFDARequirementDesc;
					break;

				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = pivot.US_FSISRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = pivot.US_AMSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = pivot.US_NOPRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = pivot.US_FWSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = pivot.US_LaceyRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = pivot.US_ODSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = pivot.US_PSTRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = pivot.US_HFCRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = pivot.US_VNERequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = pivot.US_NMFS370RequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = pivot.US_TSCARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = pivot.US_NMFSAMRRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = pivot.US_OMCRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = pivot.US_NMFSHMSRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					result = pivot.US_NMFSSIMPRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = pivot.US_NHTSARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = pivot.US_TTBRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = pivot.US_CPSCRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = pivot.US_DEARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					result = pivot.US_NMFSCOARequirementDesc;
					break;
			}

			return result == OGARequirementCalculator.NoRequirement ? ZString.Empty : result;
		}

		public override ZPropertyInfo GetIndicatorInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return pivot.CD_APHISIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return pivot.CD_ACEFDAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return pivot.CD_FSISIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return pivot.CD_FWSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return pivot.CD_LaceyActIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes._370:
					return pivot.CD_NMFS370IndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return pivot.CD_NMFSAMRIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					return pivot.CD_DDTCIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return pivot.CD_NMFSHMSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					return pivot.CD_NMFSSIMPIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return pivot.CD_ODSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return pivot.CD_PSTIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return pivot.CD_HFCIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return pivot.CD_VNEIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return pivot.CD_TSCAClaimIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return pivot.CD_TTBIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return pivot.CD_OMCIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return pivot.CD_AMSIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return pivot.CD_NOPIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return pivot.CD_NHTSAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					return pivot.CD_ATFIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return pivot.CD_CPSCIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return pivot.CD_DEAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					return pivot.CD_NMFSCOAIndicatorInfo;
			}
			return null;
		}

		public override void ValidateIndicator(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					pivot.Details.Validation.ValidateCD_APHISIndicator();
					break;

				case GovernmentAgencyProgramCodeList.Codes.FDA:
					pivot.Details.Validation.ValidateCD_ACEFDAIndicator();
					break;

				case GovernmentAgencyProgramCodeList.Codes.VNE:
					pivot.Details.Validation.ValidateCD_VNEIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					pivot.Details.Validation.ValidateCD_OMCIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					pivot.Details.Validation.ValidateCD_NMFS370Indicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					pivot.Details.Validation.ValidateCD_NMFSAMRIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					pivot.Details.Validation.ValidateCD_DDTCIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					pivot.Details.Validation.ValidateCD_NMFSHMSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					pivot.Details.Validation.ValidateCD_NMFSSIMPIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					pivot.Details.Validation.ValidateCD_NMFSCOAIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					pivot.Details.Validation.ValidateCD_ODSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					pivot.Details.Validation.ValidateCD_FSISIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					pivot.Details.Validation.ValidateCD_FWSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					pivot.Details.Validation.ValidateCD_PSTIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					pivot.Details.Validation.ValidateCD_HFCIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					pivot.Details.Validation.ValidateCD_LaceyActIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					pivot.Details.Validation.ValidateCD_AMSIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					pivot.Details.Validation.ValidateCD_NOPIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					pivot.Details.Validation.ValidateCD_TSCAClaimIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					pivot.Details.Validation.ValidateCD_TTBIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					pivot.Details.Validation.ValidateCD_NHTSAIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					pivot.Details.Validation.ValidateCD_ATFIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					pivot.Details.Validation.ValidateCD_CPSCIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					pivot.Details.Validation.ValidateCD_DEAIndicator();
					break;
			}
		}

		public override ZPropertyInfo GetDisclaimReasonInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return pivot.CD_APHISDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return pivot.CD_FSISDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return pivot.CD_FWSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return pivot.CD_ODSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return pivot.CD_PSTDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return pivot.CD_HFCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return pivot.CD_OMCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return pivot.CD_VNEDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return pivot.CD_TSCADisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return pivot.CD_AMSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return pivot.CD_NOPDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return pivot.CD_NHTSADisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return pivot.CD_LaceyActDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes._370:
					return pivot.CD_NMFS370DisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return pivot.CD_NMFSAMRDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return pivot.CD_NMFSHMSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return pivot.CD_ACEFDADisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return pivot.CD_TTBDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return pivot.CD_CPSCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return pivot.CD_DEADisclaimReasonInfo;
			}
			return null;
		}

		public override void ValidateDisclaimReason(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					pivot.Details.Validation.ValidateCD_APHISDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					pivot.Details.Validation.ValidateCD_NMFSHMSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					pivot.Details.Validation.ValidateCD_NMFSAMRDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					pivot.Details.Validation.ValidateCD_NMFS370DisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					pivot.Details.Validation.ValidateCD_VNEDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					pivot.Details.Validation.ValidateCD_ODSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					pivot.Details.Validation.ValidateCD_FSISDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					pivot.Details.Validation.ValidateCD_FWSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					pivot.Details.Validation.ValidateCD_PSTDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					pivot.Details.Validation.ValidateCD_HFCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					pivot.Details.Validation.ValidateCD_TSCADisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					pivot.Details.Validation.ValidateCD_OMCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					pivot.Details.Validation.ValidateCD_AMSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					pivot.Details.Validation.ValidateCD_NOPDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					pivot.Details.Validation.ValidateCD_NHTSADisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					pivot.Details.Validation.ValidateCD_LaceyActDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					pivot.Details.Validation.ValidateCD_ACEFDADisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					pivot.Details.Validation.ValidateCD_TTBDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					pivot.Details.Validation.ValidateCD_CPSCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					pivot.Details.Validation.ValidateCD_DEADisclaimReason();
					break;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override CodeDescriptionPairList GetDisclaimReasonList(string agencyCode)
		{
			var result = new CodeDescriptionPairList();
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = pivot.Lookups.APHISDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = pivot.Lookups.NMFS370DisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = pivot.Lookups.NMFSAMRDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = pivot.Lookups.NMFSHMSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = pivot.Lookups.VNEDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = pivot.Lookups.ODSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = pivot.Lookups.FSISDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = pivot.Lookups.FWSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = pivot.Lookups.PSTDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = pivot.Lookups.HFCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = pivot.Lookups.TSCADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = pivot.Lookups.OMCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = pivot.Lookups.AMSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = pivot.Lookups.NOPDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = pivot.Lookups.NHTSADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = pivot.Lookups.LaceyDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = pivot.Lookups.FDADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = pivot.Lookups.TTBDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = pivot.Lookups.CPSCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = pivot.Lookups.DEADisclaimReasonList;
					break;
			}
			return result;
		}

		public override CodeDescriptionPairList GetGovernmentAgencyProgramCodeList()
		{
			var result = new CodeDescriptionPairList();
			var list = Factory.GetCachedValue<GovernmentAgencyProgramCodeList>();
			result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.DOT);
			result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.FCC);
			result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.FDA);

			foreach (string code in SupportedPGACodesInProduct)
			{
				result.AddPair(code, list.GetDescriptionFromCode(code));
			}

			return result;
		}

		static IEnumerable<string> SupportedPGACodesInProduct
		{
			get
			{
				yield return GovernmentAgencyProgramCodeList.Codes.Lacey;
				yield return GovernmentAgencyProgramCodeList.Codes.FDA;
				yield return GovernmentAgencyProgramCodeList.Codes.NHTSA;
				yield return GovernmentAgencyProgramCodeList.Codes.ODS;
				yield return GovernmentAgencyProgramCodeList.Codes.PST;
				yield return GovernmentAgencyProgramCodeList.Codes.HFC;
				yield return GovernmentAgencyProgramCodeList.Codes.TSCA;
				yield return GovernmentAgencyProgramCodeList.Codes.VNE;
				yield return GovernmentAgencyProgramCodeList.Codes.ATF;
				yield return GovernmentAgencyProgramCodeList.Codes.OMC;
				yield return GovernmentAgencyProgramCodeList.Codes.TTB;
				yield return GovernmentAgencyProgramCodeList.Codes.CPSC;
				yield return GovernmentAgencyProgramCodeList.Codes.DEA;
				yield return GovernmentAgencyProgramCodeList.Codes.APHIS;
				yield return GovernmentAgencyProgramCodeList.Codes.AMS;
				yield return GovernmentAgencyProgramCodeList.Codes.NOP;
				yield return GovernmentAgencyProgramCodeList.Codes.DDTC;
				yield return GovernmentAgencyProgramCodeList.Codes.FWS;
				yield return GovernmentAgencyProgramCodeList.Codes._370;
				yield return GovernmentAgencyProgramCodeList.Codes.AMR;
				yield return GovernmentAgencyProgramCodeList.Codes.HMS;
				yield return GovernmentAgencyProgramCodeList.Codes.SIMP;
				yield return GovernmentAgencyProgramCodeList.Codes.COA;
			}
		}
	}

	class InvoiceLinePGAAgencyRequirementsProvider : PGAAgencyRequirementsProvider
	{
		public InvoiceLinePGAAgencyRequirementsProvider(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public override bool DoesMatchCertificationMode(string agencyCode)
		{
			return invoiceLine.DoesMatchCertificationMode(agencyCode);
		}

		public override bool IsPGA(string agencyCode)
		{
			return invoiceLine.IsPGA(agencyCode);
		}

		public override bool IsPGAReqirementRelevant
		{
			get
			{
				return (invoiceLine.Declaration != null && invoiceLine.Declaration.IsACE);
			}
		}

		public override ZString GetRequirementDescription(string agencyCode)
		{
			var result = ZString.Empty;

			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = invoiceLine.US_APHISReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DOT:
					result = invoiceLine.US_DOTRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = invoiceLine.US_OMCReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = invoiceLine.JI_FDARequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = invoiceLine.US_FSISReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = invoiceLine.US_FWSReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = invoiceLine.US_LaceyRequirementDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = invoiceLine.US_ODSReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = invoiceLine.US_PSTReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = invoiceLine.US_HFCReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = invoiceLine.US_VNEReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = invoiceLine.US_NMFS370ReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = invoiceLine.US_TSCAReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = invoiceLine.US_NMFSAMRReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = invoiceLine.US_NMFSHMSReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					result = invoiceLine.US_NMFSSIMReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = invoiceLine.US_NHTSAReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = invoiceLine.US_TTBReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = invoiceLine.US_AMSReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = invoiceLine.US_NOPReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = invoiceLine.US_CPSCReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = invoiceLine.US_DEAReqDesc;
					break;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					result = invoiceLine.US_NMFSCOAReqDesc;
					break;
			}

			return result == OGARequirementCalculator.NoRequirement ? ZString.Empty : result;
		}

		public override ZPropertyInfo GetIndicatorInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return invoiceLine.US_APHISIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.DOT:
					return invoiceLine.US_DOTIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FCC:
					return invoiceLine.US_FCCIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return invoiceLine.US_FDAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return invoiceLine.US_OMCIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return invoiceLine.US_FSISIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return invoiceLine.US_FWSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return invoiceLine.US_LaceyIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes._370:
					return invoiceLine.US_NMFS370IndInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return invoiceLine.US_NMFSAMRIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					return invoiceLine.US_DDTCIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return invoiceLine.US_NMFSHMSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					return invoiceLine.US_NMFSSIMPIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return invoiceLine.US_ODSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return invoiceLine.US_PSTIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return invoiceLine.US_HFCIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return invoiceLine.US_VNEIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return invoiceLine.US_TSCAIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return invoiceLine.US_TTBIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return invoiceLine.US_AMSIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return invoiceLine.US_NOPIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return invoiceLine.US_NHTSAIndicatorInfo;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					return invoiceLine.US_ATFIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return invoiceLine.US_CPSCIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return invoiceLine.US_DEAIndInfo;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					return invoiceLine.US_NMFSCOAIndInfo;
			}
			return null;
		}

		public override void ValidateIndicator(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					invoiceLine.AddInfoValidation.ValidateUS_APHISInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					invoiceLine.AddInfoValidation.ValidateUS_FDAIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FCC:
					invoiceLine.AddInfoValidation.ValidateUS_FCCIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					invoiceLine.AddInfoValidation.ValidateUS_OMCInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DOT:
					invoiceLine.AddInfoValidation.ValidateUS_DOTIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					invoiceLine.AddInfoValidation.ValidateUS_VNEInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					invoiceLine.AddInfoValidation.ValidateUS_NMFS370Ind();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSAMRInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DDTC:
					invoiceLine.AddInfoValidation.ValidateUS_DDTCInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSHMSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.SIMP:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSSIMPInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					invoiceLine.AddInfoValidation.ValidateUS_ODSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					invoiceLine.AddInfoValidation.ValidateUS_FWSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					invoiceLine.AddInfoValidation.ValidateUS_PSTIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					invoiceLine.AddInfoValidation.ValidateUS_HFCInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					invoiceLine.AddInfoValidation.ValidateUS_LaceyIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					invoiceLine.AddInfoValidation.ValidateUS_TSCAInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					invoiceLine.AddInfoValidation.ValidateUS_TTBInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					invoiceLine.AddInfoValidation.ValidateUS_NHTSAIndicator();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ATF:
					invoiceLine.AddInfoValidation.ValidateUS_ATFInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					invoiceLine.AddInfoValidation.ValidateUS_CPSCInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					invoiceLine.AddInfoValidation.ValidateUS_DEAInd();
					break;
				case GovernmentAgencyProgramCodeList.Codes.COA:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSCOAInd();
					break;
			}
		}

		public override ZPropertyInfo GetDisclaimReasonInfo(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					return invoiceLine.US_APHISDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					return invoiceLine.US_FSISDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					return invoiceLine.US_FWSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return invoiceLine.US_ODSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					return invoiceLine.US_OMCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					return invoiceLine.US_PSTDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return invoiceLine.US_HFCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return invoiceLine.US_VNEDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					return invoiceLine.US_TSCADisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					return invoiceLine.US_AMSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					return invoiceLine.US_NOPDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					return invoiceLine.US_NHTDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return invoiceLine.US_LaceyDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return invoiceLine.US_FDADisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes._370:
					return invoiceLine.US_NMFS370DisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					return invoiceLine.US_NMFSAMRDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					return invoiceLine.US_NMFSHMSDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					return invoiceLine.US_TTBDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					return invoiceLine.US_CPSCDisclaimReasonInfo;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					return invoiceLine.US_DEADisclaimReasonInfo;
			}
			return null;
		}

		public override void ValidateDisclaimReason(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					invoiceLine.AddInfoValidation.ValidateUS_APHISDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					invoiceLine.AddInfoValidation.ValidateUS_NMFS370DisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSAMRDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					invoiceLine.AddInfoValidation.ValidateUS_NMFSHMSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					invoiceLine.AddInfoValidation.ValidateUS_VNEDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					invoiceLine.AddInfoValidation.ValidateUS_OMCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					invoiceLine.AddInfoValidation.ValidateUS_ODSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					invoiceLine.AddInfoValidation.ValidateUS_FSISDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					invoiceLine.AddInfoValidation.ValidateUS_FWSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					invoiceLine.AddInfoValidation.ValidateUS_PSTDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					invoiceLine.AddInfoValidation.ValidateUS_HFCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					invoiceLine.AddInfoValidation.ValidateUS_TSCADisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					invoiceLine.AddInfoValidation.ValidateUS_AMSDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					invoiceLine.AddInfoValidation.ValidateUS_NOPDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					invoiceLine.AddInfoValidation.ValidateUS_NHTDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					invoiceLine.AddInfoValidation.ValidateUS_LaceyDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					invoiceLine.AddInfoValidation.ValidateUS_FDADisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					invoiceLine.AddInfoValidation.ValidateUS_TTBDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					invoiceLine.AddInfoValidation.ValidateUS_CPSCDisclaimReason();
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					invoiceLine.AddInfoValidation.ValidateUS_DEADisclaimReason();
					break;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override CodeDescriptionPairList GetDisclaimReasonList(string agencyCode)
		{
			var result = new CodeDescriptionPairList();
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.APHIS:
					result = invoiceLine.AddInfoLookups.APHISDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes._370:
					result = invoiceLine.AddInfoLookups.NMFS370DisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMR:
					result = invoiceLine.AddInfoLookups.NMFSAMRDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HMS:
					result = invoiceLine.AddInfoLookups.NMFSHMSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					result = invoiceLine.AddInfoLookups.VNEDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.OMC:
					result = invoiceLine.AddInfoLookups.OMCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					result = invoiceLine.AddInfoLookups.ODSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FSIS:
					result = invoiceLine.AddInfoLookups.FSISDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FWS:
					result = invoiceLine.AddInfoLookups.FWSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.PST:
					result = invoiceLine.AddInfoLookups.PSTDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					result = invoiceLine.AddInfoLookups.HFCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TSCA:
					result = invoiceLine.AddInfoLookups.TSCADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.AMS:
					result = invoiceLine.AddInfoLookups.AMSDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NOP:
					result = invoiceLine.AddInfoLookups.NOPDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.NHTSA:
					result = invoiceLine.AddInfoLookups.NHTSADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					result = invoiceLine.AddInfoLookups.LaceyDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.FDA:
					result = invoiceLine.AddInfoLookups.FDADisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.TTB:
					result = invoiceLine.AddInfoLookups.TTBDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.CPSC:
					result = invoiceLine.AddInfoLookups.CPSCDisclaimReasonList;
					break;
				case GovernmentAgencyProgramCodeList.Codes.DEA:
					result = invoiceLine.AddInfoLookups.DEADisclaimReasonList;
					break;
			}
			return result;
		}

		public override CodeDescriptionPairList GetGovernmentAgencyProgramCodeList()
		{
			return Factory.GetCachedValue("GovernmentAgencyProgramCodeList", delegate
			{
				var result = new GovernmentAgencyProgramCodeList();
				result.RemoveCode(GovernmentAgencyProgramCodeList.Codes.EPA);
				return result;
			});
		}
	}
}
