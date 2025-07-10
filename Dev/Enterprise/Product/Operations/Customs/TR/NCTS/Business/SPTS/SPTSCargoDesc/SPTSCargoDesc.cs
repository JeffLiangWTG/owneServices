using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSCargoDesc : Customs.Business.CusInBondCargoDesc
	{
		public SPTSCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type FeeTypeCore => typeof(Customs.Business.CusInBondFee);
	}
}
