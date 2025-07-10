using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusOutturnTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result;

			var applicationCode = (string)row[CusOutturn.Schema.C5_ApplicationCode];
			if (!string.IsNullOrWhiteSpace(applicationCode))
			{
				result = GetTypeByApplicationCode(applicationCode);
			}
			else
			{
				result = GetTypeByCountryAndParent(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, (string)row[CusOutturn.Schema.C5_ParentTableCode]);
			}

			if (result == null)
			{
				result = defaultType;
			}

			return result;
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeByCountryAndParent(countryCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parentTableCode: ZString.Empty) ?? defaultType;
		}

		public override Type GetTypeForNew()
		{
			return GetTypeForBinding();
		}

		Type GetTypeByCountryAndParent(ZString countryCode, ZString parentTableCode)
		{
			Type result = null;
			if (countryCode == Core.Constants.CountryCodes.Australia)
			{
				result = ObjectFactory.GetType<Integration.Customs.AU.ICusOutturn>();
			}
			else if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				result = ObjectFactory.GetType<Integration.Customs.ZA.ICusOutturn>();
			}
			else if (
				parentTableCode == JobComInvoiceLineSchema.Constants.Prefix
				&& ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode))
			)
			{
				result = ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>();
			}
			else if (countryCode == Core.Constants.CountryCodes.UnitedKingdom && parentTableCode == CusHAWBSchema.Constants.Prefix)
			{
				result = ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusOutturn>();
			}

			return result;
		}

		Type GetTypeByApplicationCode(ZString applicationCode)
		{
			Type result = null;
			switch (applicationCode)
			{
				case CusOutturnApplicationCodeList.Codes.CMR:
					result = ObjectFactory.GetType<Integration.Customs.AU.ICusOutturn>();
					break;
				case CusOutturnApplicationCodeList.Codes.ZAC:
					result = ObjectFactory.GetType<Integration.Customs.ZA.ICusOutturn>();
					break;
				case CusOutturnApplicationCodeList.Codes.EMC:
					result = ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>();
					break;
				case CusOutturnApplicationCodeList.Codes.CUK:
					result = ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusOutturn>();
					break;
			}
			return result;
		}

		readonly Type defaultType = typeof(CusOutturn);
	}
}
