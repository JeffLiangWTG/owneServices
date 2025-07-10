using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business
{
	public class PermitCountrySpecificInstruction : Customs.Business.PermitCountrySpecificInstruction
	{
		public PermitCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Lists
		public override Customs.Business.PermitTypeList GetTypeList()
		{
			return Factory.GetCachedValue<PermitTypeList>();
		}

		public override Customs.Business.PermitTransactionTypeList GetTransactionTypeList(ZString permitType, ZString permitSubType)
		{
			if (permitType == PermitTypeList.Codes.FTZ)
			{
				var key = ZString.Format("PermitTransactionTypeList_{0}_{1}", permitType, permitSubType);
				return Factory.GetCachedValue(key, () =>
				{
					var result = new Customs.Business.PermitTransactionTypeList();
					result.AddPair(PermitTransactionTypeList.Codes.FTZ, PermitTransactionTypeList.Descriptions.FTZ);
					return result;
				});
			}
			else
			{
				return base.GetTransactionTypeList(permitType, permitSubType);
			}
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType)
		{
			if (permitType == PermitTypeList.Codes.FTZ)
			{
				var key = ZString.Format("PermitRuleCodeList_{0}_{1}", permitType, permitSubType);
				return Factory.GetCachedValue(key, () =>
				{
					var result = new Customs.Business.PermitRuleCodeList();
					result.AddPair(USPermitRuleCodeList.Codes.COO, USPermitRuleCodeList.Descriptions.COO);
					result.AddPair(USPermitRuleCodeList.Codes.PRD, USPermitRuleCodeList.Descriptions.PRD);
					result.AddPair(USPermitRuleCodeList.Codes.TAR, USPermitRuleCodeList.Descriptions.TAR);
					result.AddPair(USPermitRuleCodeList.Codes.ZST, USPermitRuleCodeList.Descriptions.ZST);
					result.AddPair(USPermitRuleCodeList.Codes.FRM, USPermitRuleCodeList.Descriptions.FRM);
					return result;
				});
			}
			else
			{
				return base.GetRuleCodeList(permitType, permitSubType);
			}
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule()
		{
			return Factory.GetCachedValue<USPermitRuleCodeList>();
		}

		public override ICollection GetLookupList(Customs.Business.SharedCusPermitHeader permitHeader, ZString ruleCode)
		{
			switch (ruleCode)
			{
				case USPermitRuleCodeList.Codes.COO:
					return new USCCountryCollection(Factory);
				case USPermitRuleCodeList.Codes.ZST:
					return Factory.GetCachedValue<ZoneStatusList>();
				case USPermitRuleCodeList.Codes.FRM:
					var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
					return result;
				default:
					return null;
			}
		}

		#endregion

		public override Customs.Business.AppliesToIndicator GetAppliesToIndicator(ZString permitType, ZString permitSubType)
		{
			if (permitType == PermitTypeList.Codes.FTZ)
			{
				return Customs.Business.AppliesToIndicator.Optional;
			}
			else
			{
				return base.GetAppliesToIndicator(permitType, permitSubType);
			}
		}

		#region CusPermitRule
		public override Customs.Business.PermitMatchingType GetMatchingType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case USPermitRuleCodeList.Codes.ZST:
					return Customs.Business.PermitMatchingType.SingleValue;
				case USPermitRuleCodeList.Codes.PRD:
					return Customs.Business.PermitMatchingType.SingleValue;
				case USPermitRuleCodeList.Codes.FRM:
					return Customs.Business.PermitMatchingType.SingleValue;
				default:
					return base.GetMatchingType(ruleCode);
			}
		}

		public override ZString GetValueFromFieldType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case USPermitRuleCodeList.Codes.ZST:
					return nameof(FieldType.TextDropEdit);
				case USPermitRuleCodeList.Codes.FRM:
					return nameof(FieldType.TextCodeFindBox);
				default:
					return base.GetValueFromFieldType(ruleCode);
			}
		}
		#endregion
	}
}
