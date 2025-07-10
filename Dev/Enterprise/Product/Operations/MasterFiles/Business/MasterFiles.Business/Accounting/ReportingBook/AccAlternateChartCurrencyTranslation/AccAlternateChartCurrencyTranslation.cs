using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccAlternateChart), "AccAlternateChartCurrencyTranslations")]
	public class AccAlternateChartCurrencyTranslation : AutoAccAlternateChartCurrencyTranslation
	{
		public AccAlternateChartCurrencyTranslation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(3)]
		[List("Lookups.AccounTypeList")]
		public override ZString ART_Type { get => base.ART_Type; set => base.ART_Type = value; }

		[MaxLength(3)]
		[List("Lookups.CurrencyTranslationLevelList")]
		public override ZString ART_CurrencyTranslationLevel { get => base.ART_CurrencyTranslationLevel; set => base.ART_CurrencyTranslationLevel = value; }

		[MaxLength(3)]
		[List("Lookups.ExRateTypeList")]
		public override ZString ART_ExRateType { get => base.ART_ExRateType; set => base.ART_ExRateType = value; }

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			this.ART_CurrencyTranslationLevel = AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal;
			this.ART_ExRateType = ExchangeRateTypes.Code.BuyRate;
		}

#endif
	}
}
