using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLineFetchStrategy : PackLineFetchStrategy
	{
		public CFSPackLineFetchStrategy(CFSPackLine packLine)
			: base(packLine)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName == nameof(CFSPackLine.JL_OutturnUndelivered)))
			{
				Factory.AddFetchHint(JobTransportLegPackLineDivotSchema.J8_JL, BusinessObject.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
