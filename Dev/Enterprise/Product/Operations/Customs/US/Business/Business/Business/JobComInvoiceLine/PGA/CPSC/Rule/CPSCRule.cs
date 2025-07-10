using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class CPSCRule : CusAddInfo<CPSCRuleAddInfo>, ICPSCRulesAndLabs, ICusAddInfoTypeSupporter
	{
		public CPSCRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<CPSCRuleAddInfo>.Schema
		{
			public const string US_CPSCAccreditedLabID = USCPSCRuleAddInfoSchema.Constants.US_CPSCAccreditedLabID;
			public const string US_OA_SafetyTestLocationAddress = USCPSCRuleAddInfoSchema.Constants.US_OA_SafetyTestLocationAddress;
			public const string US_RuleCodes = USCPSCRuleAddInfoSchema.Constants.US_RuleCodes;
			public const string SafetyTestLocationOrgPK = "SafetyTestLocationOrgPK";
			public const string US_PreviousInspectionDate = USCPSCRuleAddInfoSchema.Constants.US_PreviousInspectionDate;
		}

		#endregion

		#region AddInfo Properties

		public CPSCHeader Header
		{
			get { return Factory.Load<CPSCHeader>(B7_ParentID); }
		}

		public ZString US_CPSCAccreditedLabID
		{
			get { return AddInfo.US_CPSCAccreditedLabID; }
			set { AddInfo.US_CPSCAccreditedLabID = value; }
		}

		public ZPropertyInfo US_CPSCAccreditedLabIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CPSCAccreditedLabID, x => AddInfo.US_CPSCAccreditedLabIDInfo); }
		}

		public ZString US_RuleCodes
		{
			get { return AddInfo.US_RuleCodes; }
			set { AddInfo.US_RuleCodes = value; }
		}

		internal ZString RuleCodesForMessage { get; set; }

		public ZPropertyInfo US_RuleCodesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RuleCodes, x => AddInfo.US_RuleCodesInfo); }
		}

		public ZDateTime US_PreviousInspectionDate
		{
			get { return AddInfo.US_PreviousInspectionDate; }
			set { AddInfo.US_PreviousInspectionDate = value; }
		}

		public ZPropertyInfo US_PreviousInspectionDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PreviousInspectionDate, x => AddInfo.US_PreviousInspectionDateInfo); }
		}

		#endregion

		#region US_OA_SafetyTestLocationAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_SafetyTestLocationAddress_ZAddress
		{
			get
			{
				if (safetyTestLocationAddress_ZAddress == null)
				{
					safetyTestLocationAddress_ZAddress = GetNewUS_SafetyTestLocationAddress_ZAddress();
					safetyTestLocationAddress_ZAddress.IsOrgVisible = true;
					safetyTestLocationAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return safetyTestLocationAddress_ZAddress;
			}
		}
		ZAddress safetyTestLocationAddress_ZAddress;

		public void RefreshUS_SafetyTestLocationAddress_ZAddress()
		{
			safetyTestLocationAddress_ZAddress = null;
		}

		protected ZAddress GetNewUS_SafetyTestLocationAddress_ZAddress()
		{
			return new ZAddress(US_OA_SafetyTestLocationAddressInfo);
		}

		[List(nameof(US_SafetyTestLocationAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_SafetyTestLocationAddress
		{
			get { return AddInfo.US_OA_SafetyTestLocationAddress; }
			set
			{
				var hasChange = US_OA_SafetyTestLocationAddress != value;
				AddInfo.US_OA_SafetyTestLocationAddress = value;
				if (hasChange && !IsCopying)
				{
					var labNumber = OrgHeaderWrapper.GetCustomsCodeFromAddress(SafetyTestLocationAddress, OrgCusCode.USACodeTypes.CPSCAccreditedLabId);
					US_CPSCAccreditedLabID = labNumber.Left(AutoUSCPSCRuleAddInfo.Schema.US_CPSCAccreditedLabIDMaxLength);
				}
			}
		}

		public ZPropertyInfo US_OA_SafetyTestLocationAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_SafetyTestLocationAddress, x => AddInfo.US_OA_SafetyTestLocationAddressInfo); }
		}

		public OrgAddress SafetyTestLocationAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_SafetyTestLocationAddress); }
		}

		internal OrgHeaderWrapper SafetyTestLocationWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (SafetyTestLocationAddress != null)
				{
					result = OrgHeaderWrapper.New(SafetyTestLocationAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCRuleAddInfoLookups.Organizations))]
		public ZGuid SafetyTestLocationOrgPK
		{
			get { return US_SafetyTestLocationAddress_ZAddress.OrgPK; }
			set { US_SafetyTestLocationAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SafetyTestLocationOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SafetyTestLocationOrgPK, x => US_SafetyTestLocationAddress_ZAddress.OrgPKInfo); }
		}

		bool AllFieldIsEmpty => GetUsedFieldsInfos().All(x => x.Value.IsEmpty);

		IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return US_CPSCAccreditedLabIDInfo;
			yield return US_OA_SafetyTestLocationAddressInfo;
			yield return US_RuleCodesInfo;
			yield return SafetyTestLocationOrgPKInfo;
			yield return US_PreviousInspectionDateInfo;
		}

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (AllFieldIsEmpty)
			{
				Delete();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "CPSC Rule"; }
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (CPSCRule)base.CloneInternal(args);
			return result;
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USCPSCReport, typeof(CPSCReport));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USCPSCRuleAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USCPSCRuleAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		CPSCRuleAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CPSCRuleAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		CPSCRuleAddInfo fAddInfo;

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		[ChildEditable(true)]
		public CPSCReportCollection ReportAndLabs
		{
			get
			{
				if (reportAndLabs == null)
				{
					reportAndLabs = new CPSCReportCollection(this);
					reportAndLabs.Load();
					RegisterEditableChildObject(reportAndLabs);
				}
				return reportAndLabs;
			}
		}
		CPSCReportCollection reportAndLabs;

		#endregion

		#region ICPSCRulesAndLabs

		IPGAContactDetails ICPSCRulesAndLabs.SafetyTestLocation
		{
			get { return SafetyTestLocationWrapper; }
		}

		ZString ICPSCRulesAndLabs.CPSCAccreditedLabID
		{
			get { return US_CPSCAccreditedLabID; }
		}

		ZDateTime ICPSCRulesAndLabs.PreviousInspectionDate
		{
			get { return US_PreviousInspectionDate; }
		}

		ZString ICPSCRulesAndLabs.RuleCodes
		{
			get
			{
				var result = new ZStringBuilder();
				var relatedRules = Header.RuleAndLabs.Cast<CPSCRule>().Where(x => x.US_OA_SafetyTestLocationAddress == US_OA_SafetyTestLocationAddress && x.US_CPSCAccreditedLabID == US_CPSCAccreditedLabID).OrderBy(x => x.US_RuleCodes);
				foreach (var relatedRule in relatedRules)
				{
					result.AppendIfNotEmpty(relatedRule.US_RuleCodes);
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		CPSCReportCollection ICPSCRulesAndLabs.ReportAndLabs
		{
			get { return ReportAndLabs; }
		}

		#endregion

		public RuleCodeMultipleCodeCollection AdditionalRuleCodes
		{
			get { return new RuleCodeMultipleCodeCollection(this.US_RuleCodes, Factory); }
		}
	}
}
