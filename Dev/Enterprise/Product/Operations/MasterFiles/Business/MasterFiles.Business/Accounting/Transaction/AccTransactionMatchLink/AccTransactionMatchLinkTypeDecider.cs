using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionMatchLinkTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return ObjectFactory.GetType<Integration.IAccTransactionMatchLink>();
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return ObjectFactory.GetType<Integration.IAccTransactionMatchLink>();
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.IAccTransactionMatchLink>();
		}
	}
}
