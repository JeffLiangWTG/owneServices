using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class CusInBondFee : AutoCusInBondFee
	{
		protected CusInBondFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusInBondFeeTypeDecider TypeDecider = new CusInBondFeeTypeDecider();
	}
}
