using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInBondMoveLineItem : AutoCusInBondMoveLineItem, Integration.Customs.ICusInBondMoveLineItem
	{
		protected CusInBondMoveLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new CusInBondMoveLineItemTypeDecider();

		#endregion
	}
}
