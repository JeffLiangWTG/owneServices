using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusStorageDocPivotUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		readonly BaseCusStorageDocPivot pivot;
		readonly BusinessObject parent;

		public CusStorageDocPivotUniqueIndexFailureHandler(BaseCusStorageDocPivot pivot)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
			parent = pivot.Parent;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return CusStorageDocPivotSchema.Constants.Indexes.NR_UC__CSD_ParentTableCode_CSD_ParentID_CSD_DocType_CSD_StorageDocReference;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var existingItem = GetExistingItem();
			if (existingItem != null)
			{
				var message = GenerateMessage();
				pivot.Delete();
				Reload();
				notifier.ReportInformation(message, Res.GetString("188d78a3-3e76-46cd-8e8e-84cda322b6b9", "Save Error"));
			}
		}

		void Reload()
		{
			if (parent is ICusStorageDocPivotTypeSupporter p)
			{
				p.ReloadCollection();
			}
		}

		string GenerateMessage()
		{
			var fileName = pivot.FileName;
			return Res.GetString("03c76f6c-c4a1-44ab-83e9-5cf26259cf92",
				"The type '{0}' eDoc ({1}) has already been linked to {2} by another user. Your changes have been merged, please review your changes and save again.",
				pivot.CSD_DocType,
				fileName.IsEmpty ? pivot.CSD_StorageDocReference.ToString() : (string)fileName,
				(parent as ICusStorageDocPivotTypeSupporter)?.HumanReadableName ?? parent?.HumanReadableName ?? ZString.Empty);
		}

		BaseCusStorageDocPivot GetExistingItem()
		{
			var query = new ZDBOnlyQuery(typeof(BaseCusStorageDocPivot));
			query.AddToFilter(CusStorageDocPivotSchema.PK, SQLComparisonOperator.NotEqual, pivot.PK);
			query.AddToFilter(CusStorageDocPivotSchema.CSD_DocType, pivot.CSD_DocType);
			query.AddToFilter(CusStorageDocPivotSchema.CSD_StorageDocReference, pivot.CSD_StorageDocReference);
			query.AddToFilter(CusStorageDocPivotSchema.CSD_ParentID, pivot.CSD_ParentID);
			query.AddToFilter(CusStorageDocPivotSchema.CSD_ParentTableCode, pivot.CSD_ParentTableCode);

			return pivot.Factory.LoadTop1<BaseCusStorageDocPivot>(query);
		}
	}
}
