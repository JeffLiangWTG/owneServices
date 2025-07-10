using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxRate : AutoAccOrgTaxRate
	{
		public AccOrgTaxRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Tax Rate

		[DecimalPlaces(9)]
		[ResourceStringData("b1e93d7f-4fe5-4f88-a4c6-5b556eca601b", Caption = "Rate (%)")]
		public ZDecimal Rate => OTR_RateDenominator != 0 ? (new ZDecimal(OTR_RateNumerator) / OTR_RateDenominator) : 0;

		public ZPropertyInfo RateInfo => GetZPropertyInfo(nameof(Rate));

		#endregion

		[List("Lookups.Source")]
		public override ZString OTR_Source
		{
			get => base.OTR_Source;
			set
			{
				base.OTR_Source = value;
				Validation.ValidateOTR_StartDate();
			}
		}

		public override ZDate OTR_EndDate
		{
			get => base.OTR_EndDate;
			set
			{
				base.OTR_EndDate = value;
				Validation.ValidateOTR_StartDate();
			}
		}

		public override ZDate OTR_StartDate
		{
			get => base.OTR_StartDate;
			set
			{
				base.OTR_StartDate = value;
				Validation.ValidateOTR_EndDate();
			}
		}

		public override ZGuid OTR_OTC
		{
			get => base.OTR_OTC;
			set
			{
				base.OTR_OTC = value;
				Validation.ValidateOTR_StartDate();
			}
		}

		public override ZInt OTR_RateDenominator
		{
			get => base.OTR_RateDenominator;
			set
			{
				bool hasChanged = OTR_RateDenominator != value;
				base.OTR_RateDenominator = value;

				if (hasChanged)
				{
					Validation.Validate_Rate();
				}
			}
		}

		public override ZInt OTR_RateNumerator
		{
			get => base.OTR_RateNumerator;
			set
			{
				bool hasChanged = OTR_RateNumerator != value;
				base.OTR_RateNumerator = value;

				if (hasChanged)
				{
					Validation.Validate_Rate();
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
