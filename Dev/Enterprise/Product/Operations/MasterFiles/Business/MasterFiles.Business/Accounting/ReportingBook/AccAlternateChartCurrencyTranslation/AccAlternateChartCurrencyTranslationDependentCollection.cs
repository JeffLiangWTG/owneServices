using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartCurrencyTranslationDependentCollection : DependentBusinessObjectCollection<AccAlternateChartCurrencyTranslation, AccAlternateChart>
	{
		public AccAlternateChartCurrencyTranslationDependentCollection(AccAlternateChart alternateChart) : base(alternateChart)
		{
		}

		protected override string FkColumnName => AccAlternateChartCurrencyTranslationSchema.ART_AAC_AlternateChart.Name;
	}
}
