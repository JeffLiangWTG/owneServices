using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveLineItemTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var parentPk = (row != null) ? new ZGuid(row[CusInBondMoveLineItem.Schema.BI_B9]) : ZGuid.Empty;
			return parentPk.IsValid ? factory.Load<CusInBondMoveDetail>(parentPk)?.MoveLineItemType : null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
