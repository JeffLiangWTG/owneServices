using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeDataCollection : Customs.Business.CusCodeDataCollection<ETradeData>
	{
		public ETradeDataCollection(BusinessObject master, ZString code)
			: base(master, code)
		{ }
	}
}
