using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCRuleSecondaryTariffException : AutoUSCRuleSecondaryTariffException
	{
		public USCRuleSecondaryTariffException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoUSCRuleSecondaryTariffException.Schema
		{
			public const string FormattedTariff = "FormattedTariff";
		}

		#region New Properties/Methods

		[List(nameof(Lookups) + "." + nameof(USCRuleSecondaryTariffExceptionLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariff
		{
			get { return new TariffFormatter().DisplayFormat(U4_Tariff); }
			set
			{
				U4_Tariff = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariff();
				FormattedTariffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffInfo
		{
			get { return GetZPropertyInfo(Schema.FormattedTariff); }
		}

		internal bool HasBroaderTariffRangesThan(USCRuleSecondaryTariffException passedException)
		{
			return passedException.U4_Tariff.StartsWith(U4_Tariff);
		}

		public bool Applies(ZString tariffNumber, ZDateTime dateOfException)
		{
			return tariffNumber.StartsWith(U4_Tariff) &&
					(U4_DateFrom.IsEmpty || U4_DateFrom <= dateOfException) &&
					(U4_DateTo.IsEmpty || U4_DateTo >= dateOfException);
		}

		#endregion

		#region Related Objects

		[RelatedBusinessObject("SecondaryTariffRule")]
		public override ZGuid U4_U3
		{
			get { return base.U4_U3; }
			set { base.U4_U3 = value; }
		}

		public USCRuleSecondaryTariff SecondaryTariffRule
		{
			get { return Factory.Load<USCRuleSecondaryTariff>(U4_U3); }
		}

		#endregion
	}
}
