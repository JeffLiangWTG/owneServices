using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<ICharge>();
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeForNew();
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForNew();
		}
	}
}
