using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlBalanceAffectingLog : AutoHrlBalanceAffectingLog
	{
		public HrlBalanceAffectingLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete() => throw new NotSupportedException();
	}
}
