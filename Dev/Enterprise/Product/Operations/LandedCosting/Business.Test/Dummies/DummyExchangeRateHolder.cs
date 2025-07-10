using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class DummyExchangeRateHolder : DummyBusinessObject, ILandedCostExchangeRateHolder
	{
		public DummyExchangeRateHolder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString CurrencyCodeExposed;
		public ZString CurrencyCode => CurrencyCodeExposed;

		public ZString ReferenceNumberExposed;
		public ZString ReferenceNumber => ReferenceNumberExposed;

		public ZDecimal LandedCostExchangeRateExposed;
		public ZDecimal LandedCostExchangeRate
		{
			get => LandedCostExchangeRateExposed;
			set => LandedCostExchangeRateExposed = value;
		}

		public ZDecimal LandedCostExchangeRateDefaultExposed;
		public ZDecimal LandedCostExchangeRateDefault => LandedCostExchangeRateDefaultExposed;

		public ZGuid CompanyPK
		{
			get => companyPK.IsEmpty ? GlbCompany.CurrentCompany.PK : companyPK;
			set => companyPK = value;
		}
		ZGuid companyPK;
	}
}
