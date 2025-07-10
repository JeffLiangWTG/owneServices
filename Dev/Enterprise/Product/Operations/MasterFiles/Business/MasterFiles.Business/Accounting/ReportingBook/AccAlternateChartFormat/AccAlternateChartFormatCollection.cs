using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartFormatDependentCollection : DependentBusinessObjectCollection<AccAlternateChartFormat, AccAlternateChart>
	{
		public AccAlternateChartFormatDependentCollection(AccAlternateChart alternateChart) : base(alternateChart)
		{
		}

		protected override string FkColumnName => AccAlternateChartFormatSchema.ANF_AAC_AlternateChart.Name;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			((AccAlternateChartFormat)child).ANF_Tier = (CargoWise.Types.ZShort)this.Count + 1;
		}
	}
	public class AccAlternateChartFormatCollection : BusinessObjectCollection<AccAlternateChartFormat>
	{
		public AccAlternateChartFormatCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
