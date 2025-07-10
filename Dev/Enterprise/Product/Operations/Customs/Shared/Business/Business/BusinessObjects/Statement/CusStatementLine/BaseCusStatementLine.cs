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
	public abstract class BaseCusStatementLine : AutoCusStatementLine, Integration.Customs.Shared.IBaseCusStatementLine
	{
		protected BaseCusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BaseCusStatementHeader StatementHeader => Factory.Load<BaseCusStatementHeader>(B3_B2);

		public static readonly BaseCusStatementLineTypeDecider TypeDecider = new BaseCusStatementLineTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusStatementLineFetchHint(this);
	}

	public class BaseCusStatementLineTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusStatementLineCountryCode(row, factory));

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusStatementLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusStatementLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusStatementLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusStatementLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusStatementLine>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.US.ICusStatementLine>();

		protected ZString GetCusStatementLineCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = (row != null) ? new ZGuid(row[CusStatementLineSchema.B3_B2.Name]) : ZGuid.Invalid;
			var header = (headerPK.IsValid) ? factory.Load<BaseCusStatementHeader>(headerPK) : null;
			return header != null ? header.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
