using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusAttributeFilterTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetClassPartPivotCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusAttributeFilter>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry
		{
			get { return typeof(CusAttributeFilter); }
		}

		ZString GetClassPartPivotCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var parentPK = (row != null) ? new ZGuid(row[CusAttributeFilterSchema.BG_CI.Name]) : ZGuid.Empty;
			if (parentPK.IsValid)
			{
				var pivot = factory.Load<BaseCusClassPartPivot>(parentPK);
				if (pivot != null)
				{
					result = pivot.CI_RN_NKCountry;
					if (result.IsEmpty)
					{
						var classification = pivot.Classification;
						if (classification != null)
						{
							result = classification.CC_RN_NKCountryCode;
						}
					}
				}
			}
			return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
		}
	}
}
