using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxRate : AutoAccTaxRate
	{
		public TaxFrameworkAccTaxRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		protected override AccTaxRateValidation GetNewValidation()
		{
			return new TaxFrameworkAccTaxRateValidation(this);
		}

		[List("Lookups.TaxSystems")]
		public override ZString AT_TaxSystemCode { get => base.AT_TaxSystemCode; set => base.AT_TaxSystemCode = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AT_Type = AccTaxRate.Types.NotReportable;
		}

		[ReadOnly(true)]
		public override ZString AT_Type { get => base.AT_Type; set => base.AT_Type = value; }

		public new TaxFrameworkAccTaxRateLookups Lookups
		{
			get { return (TaxFrameworkAccTaxRateLookups)base.Lookups; }
		}

		protected override AccTaxRateLookups GetNewLookups()
		{
			return new TaxFrameworkAccTaxRateLookups(this);
		}

		[List("Lookups.RateSources")]
		public override ZString AT_RateSource { get => base.AT_RateSource; set => base.AT_RateSource = value; }
	}
}