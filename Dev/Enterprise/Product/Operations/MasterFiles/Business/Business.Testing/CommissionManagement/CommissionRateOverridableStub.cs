using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionRateOverridableStub : ICommissionRateOverridable
	{
		public CommissionRateOverridableStub(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		public GlbCompany Company
		{
			get;
			set;
		}

		public ZString CommissionType
		{
			get;
			set;
		}

		public ZDecimal CommissionPercentage
		{
			get;
			set;
		}

		public ZDecimal CommissionAmount
		{
			get;
			set;
		}

		public ZString CommissionCurrency
		{
			get;
			set;
		}

		public ZString CommissionPeriod
		{
			get;
			set;
		}
	}
}
