using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPollingTransactionTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(CusPollingTransaction);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string applicationCode = row[CusPollingTransaction.Schema.CPT_ApplicationCode].ToString().Trim();
			return GetTypeForApplicationCode(applicationCode);
		}

		public override Type GetTypeForNew()
		{
			return typeof(CusPollingTransaction);
		}

		Type GetTypeForApplicationCode(string applicationCode)
		{
			switch (applicationCode)
			{
				case CusPollingTransaction.ApplicationCodes.KRCustoms:
					return ObjectFactory.GetType<Integration.Customs.KR.ICusPollingTransaction>();
				default:
					return typeof(CusPollingTransaction);
			}
		}
	}
}
