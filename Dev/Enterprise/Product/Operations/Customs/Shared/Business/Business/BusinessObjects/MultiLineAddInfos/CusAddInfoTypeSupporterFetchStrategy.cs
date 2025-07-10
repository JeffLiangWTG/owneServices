using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusAddInfoTypeSupporterFetchStrategy : ICusAddInfoTypeSupporterFetchStrategy
	{
		public CusAddInfoTypeSupporterFetchStrategy(ICusAddInfoTypeSupporter supporter)
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
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, supporter.PK);
			var dictionary = supporter.GetCusAddInfoTypes();
			if (dictionary != null)
			{
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, supporter.PK);
				foreach (var pair in dictionary)
				{
					Factory.AddFetchHint(pair.Value, new ZQuery(CusAddInfoSchema.B7_Type, pair.Key), query);
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
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, supporter.PK);
			var dictionary = supporter.GetCusAddInfoTypes();
			if (dictionary != null)
			{
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, supporter.PK);
				foreach (var pair in dictionary)
				{
					Factory.AddFetchHint(pair.Value, new ZQuery(CusAddInfoSchema.B7_Type, pair.Key), query);
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

		readonly protected ICusAddInfoTypeSupporter supporter;

		protected BusinessObjectFactory Factory => supporter.Factory;
	}
}
