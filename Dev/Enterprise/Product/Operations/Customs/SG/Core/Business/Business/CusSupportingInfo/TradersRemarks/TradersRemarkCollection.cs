using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class TradersRemarkCollection : CusSupportingInfoCollection<TradersRemark>
	{
		public TradersRemarkCollection(JobDeclaration declaration)
			: base(declaration, CusSupportingInfoTypeList.Codes.TradersRemarks)
		{
			MaxCountValidationEnable(5);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();
			filter.OrderBy = CusSupportingInfoSchema.Constants.CSI_LineNo + OrderByClause.Ascending;
			return filter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var tradersRemark = (TradersRemark)child;
			tradersRemark.CSI_LineNo = this.Count > 0 ? this.Cast<TradersRemark>().Max(x => x.CSI_LineNo) + 1 : 1;
		}
	}
}
