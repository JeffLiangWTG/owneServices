using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	class SharedPartSynchronisationManager : IDataRefreshBusSubscriber, IService
	{
		public SharedPartSynchronisationManager(BusinessObjectFactory factory)
		{
			Factory = factory;
			list = new List<IDataRefreshBusSubscriber>();
		}

		readonly List<IDataRefreshBusSubscriber> list;

		public void StartManaging(CusISFLinePartSynchronisationManager manager)
		{
			if (list.Count == 0)
			{
				DataRefreshManager refreshManager = new DataRefreshManager();
				refreshManager.StartManaging(OrgSupplierPartSchema.Constants.TableName, this);
				refreshManager.StartManaging(OrgPartRelationSchema.Constants.TableName, this);
				refreshManager.StartManaging(OrgPartUnitSchema.Constants.TableName, this);
				refreshManager.StartManaging(CusClassPartPivotSchema.Constants.TableName, this);
			}
			list.Add(manager);
		}

		public void StopManaging(CusISFLinePartSynchronisationManager manager)
		{
			list.Remove(manager);
			if (list.Count == 0)
			{
				DataRefreshManager refreshManager = new DataRefreshManager();
				refreshManager.StopManaging(OrgSupplierPartSchema.Constants.TableName, this);
				refreshManager.StopManaging(OrgPartRelationSchema.Constants.TableName, this);
				refreshManager.StopManaging(OrgPartUnitSchema.Constants.TableName, this);
				refreshManager.StopManaging(CusClassPartPivotSchema.Constants.TableName, this);
			}
		}

		#region IDataRefreshBusSubscriber Members

		public BusinessObjectFactory Factory
		{
			get;
			private set;
		}

		public void UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			foreach (IDataRefreshBusSubscriber subscriber in list.ToArray())
			{
				subscriber.UpdatedByDataRefresh(publishedObjects);
			}
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		#endregion
	}
}
