using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class JobStorageTypeDecider : TypeDecider
	{
		#region GetTypeForNew

		public override Type GetTypeForNew()
		{
			return typeof(JobStorage);
		}

		#endregion

		#region GetTypeForBinding

		public override Type GetTypeForBinding()
		{
			return typeof(JobStorage);
		}

		#endregion

		#region GetTypeForLoad

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return (string)row[JobStorageSchema.Constants.ET_StorageType] switch
			{
				RatingConstants.RateCategory.WHS => ObjectFactory.GetType<IWhsInvoice>(),
				RatingConstants.RateCategory.CYD => ObjectFactory.GetType<IPeriodicInvoicing>(),
				_ => typeof(JobStorage)
			};
		}

		#endregion
	}
}

