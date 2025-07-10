using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
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

		public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
		{
			return Factory.GetCachedValue(string.Format("ZA_PermitCountrySpecificInstruction_SubTypeList_{0}", typeCode), () =>
			{
				switch (typeCode)
				{
					case PermitTypeList.Codes.RCC:
					case PermitTypeList.Codes.PRC:
					case PermitTypeList.Codes.VALA:
						return new PermitSubTypeList();
					default:
						return new Customs.Business.PermitSubTypeList();
				}
			});
		}

		public override PermitQtyValIndicatorList GetQtyValIndicatorList(ZString permitType, ZString permitSubType)
		{
			switch (permitType)
			{
				case PermitTypeList.Codes.RCC:
					var rccQtyValList = new PermitQtyValIndicatorList();
					rccQtyValList.RemoveCode(Customs.Business.PermitQtyValIndicatorList.Codes.QTY);
					rccQtyValList.RemoveCode(Customs.Business.PermitQtyValIndicatorList.Codes.BTH);
					return rccQtyValList;
				case PermitTypeList.Codes.REB:
					return Factory.GetCachedValue<EmptyPermitQtyValIndicatorList>();
				default:
					return Factory.GetCachedValue<PermitQtyValIndicatorList>();
			}
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule()
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		public override ICollection GetPermitNumberCollection(SharedCusPermitHeader permitHeader)
		{
			if (permitHeader.CPH_Type == PermitTypeList.Codes.REB)
			{
				var result = new Universal.TariffViewCollection(permitHeader.Factory, permitHeader.CPH_RN_NKCountryCode, ZDateTime.Now, new ZString[]
				{
					UniversalReferenceConstants.RefCusTariffTypes.StartsWith3,
					UniversalReferenceConstants.RefCusTariffTypes.StartsWith4
				});
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffCode, "Property", permitHeader.CPH_Number));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.DefaultLanguageDescription, "Property", ZString.Empty));
				return result;
			}

			return base.GetPermitNumberCollection(permitHeader);
		}

		#endregion

		public override string GetCustomLabelForPermitNumber(ZString permitType)
		{
			return permitType == PermitTypeList.Codes.REB ? Res.GetString("CusPermitForm|436A9E13-DC9E-4B7E-8DA0-800DDA3D52AC", "Rebate Code")
				: base.GetCustomLabelForPermitNumber(permitType);
		}
	}
}
