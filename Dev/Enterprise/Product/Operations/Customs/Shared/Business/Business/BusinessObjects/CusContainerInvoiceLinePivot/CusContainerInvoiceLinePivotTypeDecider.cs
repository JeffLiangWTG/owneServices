using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainerInvoiceLinePivotTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia,    () => ObjectFactory.GetType<Integration.Customs.AU.ICusContainerInvoiceLinePivot>()),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand,   () => ObjectFactory.GetType<Integration.Customs.NZ.ICusContainerInvoiceLinePivot>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () => ObjectFactory.GetType<Integration.Customs.US.ICusContainerInvoiceLinePivot>())
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusContainerInvoiceLinePivot);

		protected ZString GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var containerPK = (row != null) ? new ZGuid(row[CusContainerInvoiceLinePivotSchema.C2_CO.Name]) : ZGuid.Invalid;
			var container = (containerPK.IsValid) ? factory.Load<BaseCusContainer>(containerPK) : null;
			var containerDecl = container?.Declaration;
			return containerDecl != null ? containerDecl.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
