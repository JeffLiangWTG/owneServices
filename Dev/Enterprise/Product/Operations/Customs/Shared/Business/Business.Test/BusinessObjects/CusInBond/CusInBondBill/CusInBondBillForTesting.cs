using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondBillForTesting : CusInBondBill
	{
		public CusInBondBillForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementDetailType => throw new NotImplementedException();
	}
}
