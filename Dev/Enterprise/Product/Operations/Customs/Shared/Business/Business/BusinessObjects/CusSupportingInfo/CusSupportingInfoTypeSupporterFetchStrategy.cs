using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusSupportingInfoTypeSupporterFetchStrategy : IBusinessObjectFetchStrategy
	{
		public CusSupportingInfoTypeSupporterFetchStrategy(Integration.Customs.ICusSupportingInfoTypeSupporter supporter)
		{
			this.supporter = Argument.NotNull(supporter, nameof(supporter));
		}

		#region FetchForLoad

		public void FetchForLoad()
		{
			FetchForLoadCore();
		}

		protected virtual void FetchForLoadCore()
		{
		}

		#endregion

		#region FetchForLoadChildEditableObjects

		public void FetchForLoadChildEditableObjects()
		{
			FetchForLoadChildEditableObjectsCore();
		}

		protected virtual void FetchForLoadChildEditableObjectsCore()
		{
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, supporter.PK);
			var dictionary = supporter.GetCusSupportingInfoTypes();
			if (dictionary != null)
			{
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, supporter.PK);
				foreach (var pair in dictionary)
				{
					Factory.AddFetchHint(pair.Value, new ZQuery(CusSupportingInfoSchema.CSI_Type, pair.Key), query);
				}
			}
		}

		#endregion

		#region FetchForFactorySave

		public void FetchForFactorySave()
		{
			FetchForFactorySaveCore();
		}

		protected virtual void FetchForFactorySaveCore()
		{
		}

		#endregion

		#region FetchForFactorySaveBeforeTransaction

		public void FetchForFactorySaveBeforeTransaction()
		{
		}

		#endregion

		#region FetchForValidate

		public void FetchForValidate()
		{
			FetchForValidateCore();
		}

		protected virtual void FetchForValidateCore()
		{
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, supporter.PK);
			var dictionary = supporter.GetCusSupportingInfoTypes();
			if (dictionary != null)
			{
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, supporter.PK);
				foreach (var pair in dictionary)
				{
					Factory.AddFetchHint(pair.Value, new ZQuery(CusSupportingInfoSchema.CSI_Type, pair.Key), query);
				}
			}
		}

		#endregion

		#region FetchForView
		public void FetchForView(TableColumn[] columns)
		{
			FetchForViewCore(columns);
		}

		protected virtual void FetchForViewCore(TableColumn[] columns)
		{
		}
		#endregion

		#region FetchForDelete

		public void FetchForDelete()
		{
			FetchForDeleteCore();
		}

		protected virtual void FetchForDeleteCore()
		{
		}

		#endregion

		#region FetchForBind

		public void FetchForBind()
		{
			FetchForBindCore();
		}

		protected virtual void FetchForBindCore()
		{
		}

		#endregion

		readonly protected Integration.Customs.ICusSupportingInfoTypeSupporter supporter;

		protected BusinessObjectFactory Factory => supporter.Factory;
	}
}
