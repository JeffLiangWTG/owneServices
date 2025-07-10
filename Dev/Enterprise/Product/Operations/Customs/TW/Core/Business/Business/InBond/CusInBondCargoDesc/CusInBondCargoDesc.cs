using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondCargoDesc : Customs.Business.CusInBondCargoDesc
	{
		public CusInBondCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type FeeTypeCore => typeof(Customs.Business.CusInBondFee);
	}
}
