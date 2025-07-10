using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondHeaderForTesting : CusInBondHeader
	{
		public CusInBondHeaderForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type BillTypeCore => throw new NotImplementedException();

		protected override Type MovementHeaderTypeCore => throw new NotImplementedException();

		protected override CusInBondMoveHeaderCollection GetMovementHeaders() => throw new NotImplementedException();

		protected override ICusInBondBillCollection GetNewBillsCollection() => new CusInBondBillCollectionForTesting(this);
	}
}
