using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSMoveLineItem : Customs.Business.CusInBondMoveLineItem
	{
		public SPTSMoveLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("MoveDetail")]
		public override ZGuid BI_B9 { get => base.BI_B9; set => base.BI_B9 = value; }

		public SPTSMoveDetail MoveDetail => Factory.Load<SPTSMoveDetail>(BI_B9);
	}
}
