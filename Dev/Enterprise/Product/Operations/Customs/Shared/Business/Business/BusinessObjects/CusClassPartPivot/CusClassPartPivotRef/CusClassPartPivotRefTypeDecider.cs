using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotRefTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusClassPartPivotRef);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;
			if (row != null)
			{
				var pivotPK = new ZGuid(row[CusClassPartPivotRef.Schema.CIR_CI]);
				var pivot = pivotPK.IsValid ? factory.Load<BaseCusClassPartPivot>(pivotPK) : null;
				if (pivot != null)
				{
					var typeCode = new ZString(row[CusClassPartPivotRefSchema.CIR_ReferenceType.Name]);
					type = GetTypeForLoad(typeCode, pivot);
					if (type == null)
					{
						var countryCode = GetCountryCode(pivot);
						type = GetTypeForCountryCode(countryCode);
					}
				}
			}
			return type;
		}

		ZString GetCountryCode(BaseCusClassPartPivot pivot)
		{
			return pivot != null ? pivot.CI_RN_NKCountry : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		Type GetTypeForLoad(ZString type, BaseCusClassPartPivot pivot)
		{
			Type result = null;

			if (!type.IsEmpty)
			{
				(pivot as ICusClassPartPivotRefTypeSupporter)?.GetCusClassPartPivotRefTypes()?.TryGetValue(type, out result);
			}
			return result;
		}
	}
}
