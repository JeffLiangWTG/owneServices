using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Soon to be used")]
	public class GlbHolidaySourceHistory : AutoGlbHolidaySourceHistory
	{
		public GlbHolidaySourceHistory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GHH_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}

		#region Properties

		[RelatedBusinessObject("HolidaySource")]
		public override ZGuid GHH_GHS_HolidaySource
		{
			get { return base.GHH_GHS_HolidaySource; }
			set { base.GHH_GHS_HolidaySource = value; }
		}

		#endregion
	}
}
