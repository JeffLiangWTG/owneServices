using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class DeclarationsCreatedCancelledBusinessObjectFactory : BusinessObjectFactory
	{
		public DeclarationsCreatedCancelledBusinessObjectFactory()
		{
		}

		public DeclarationsCreatedCancelledBusinessObjectFactory(CargoWise.Data.DbConnection connection)
			: base(connection)
		{
		}

		public override BusinessObject New(Type bizOType)
		{
			BusinessObject result = base.New(bizOType);
			if (result is BaseJobDeclaration)
			{
				((BaseJobDeclaration)result).JE_IsCancelled = true;
				result.HasChanges = false;
			}

			if (result is CommonShipment)
			{
				((CommonShipment)result).JS_IsCancelled = true;
				result.HasChanges = false;
			}

			return result;
		}
	}
}
