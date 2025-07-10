using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlBalanceTransaction : AutoHrlBalanceTransaction
	{
		public HrlBalanceTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public override bool CanDelete => false;

		public override void Delete() => throw new NotSupportedException();
	}
}
