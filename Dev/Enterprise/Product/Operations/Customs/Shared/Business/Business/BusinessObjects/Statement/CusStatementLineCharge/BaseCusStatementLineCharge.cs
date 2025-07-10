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
	[SingleObjectAroundARow]
	public abstract class BaseCusStatementLineCharge : AutoCusStatementLineCharge, Integration.Customs.Shared.IBaseCusStatementLineCharge
	{
		protected BaseCusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BaseCusStatementLine StatementLine => Factory.Load<BaseCusStatementLine>(B4_B3);

		public static readonly BaseCusStatementLineChargeTypeDecider TypeDecider = new BaseCusStatementLineChargeTypeDecider();
	}

	public class BaseCusStatementLineChargeTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetCusStatementLineChargeCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusStatementLineCharge>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusStatementLineCharge>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusStatementLineCharge>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusStatementLineCharge>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusStatementLineCharge>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry
		{
			get { return ObjectFactory.GetType<Integration.Customs.US.ICusStatementLineCharge>(); }
		}

		protected ZString GetCusStatementLineChargeCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var linePK = (row != null) ? new ZGuid(row[CusStatementLineChargeSchema.B4_B3.Name]) : ZGuid.Invalid;
			var line = (linePK.IsValid) ? factory.Load<BaseCusStatementLine>(linePK) : null;
			var header = line?.StatementHeader;
			return header != null ? header.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
