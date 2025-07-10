using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInbondBillAddRef : AutoCusInbondBillAddRef
	{
		protected CusInbondBillAddRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusInbondBillAddRefTypeDecider TypeDecider = new CusInbondBillAddRefTypeDecider();

		#endregion
	}
}
