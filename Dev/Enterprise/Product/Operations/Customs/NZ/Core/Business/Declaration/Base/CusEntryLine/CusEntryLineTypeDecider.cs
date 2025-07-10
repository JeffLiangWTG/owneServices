using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryLineTypeDecider : TypeDecider
	{
		public CusEntryLineTypeDecider()
		{
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return typeof(CusEntryLine);
		}

		public override Type GetTypeForNew()
		{
			return typeof(CusEntryLine);
		}
	}
}
