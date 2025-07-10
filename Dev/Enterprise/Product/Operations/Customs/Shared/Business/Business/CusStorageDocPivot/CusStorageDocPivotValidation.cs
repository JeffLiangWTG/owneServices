//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStorageDocPivotValidation
//
//    This class should be used for overriding validation in AutoCusStorageDocPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class CusStorageDocPivotValidation : AutoCusStorageDocPivotValidation
	{
		public CusStorageDocPivotValidation(AutoCusStorageDocPivot parent)
			: base(parent)
		{
		}

		protected new BaseCusStorageDocPivot Parent => (BaseCusStorageDocPivot)base.Parent;

		protected override void CheckCSD_DocType()
		{
			base.CheckCSD_DocType();
			MandatoryValidation.CheckEntered(Parent.CSD_DocTypeInfo);
			CheckDuplicates();
		}

		void CheckDuplicates()
		{
			var parentBo = Parent;
			var docType = parentBo.CSD_DocType;
			if (!docType.IsEmpty)
			{
				var query = new ZQuery(CusStorageDocPivotSchema.PK, SQLComparisonOperator.NotEqual, parentBo.PK);
				query.AddToFilter(CusStorageDocPivotSchema.CSD_DocType, docType);
				query.AddToFilter(CusStorageDocPivotSchema.CSD_StorageDocReference, parentBo.CSD_StorageDocReference);
				query.AddToFilter(CusStorageDocPivotSchema.CSD_ParentID, parentBo.CSD_ParentID);
				query.AddToFilter(CusStorageDocPivotSchema.CSD_ParentTableCode, parentBo.CSD_ParentTableCode);
				query.FetchOnlyFromLocalCache = !(parentBo.Parent?.IsInDatabase ?? true);
				if (parentBo.Factory.Exists(typeof(BaseCusStorageDocPivot), query))
				{
					parentBo.CSD_DocTypeInfo.AddError(DocTypeStorageDuplicatingMessage);
				}
			}
		}

		protected virtual string DocTypeStorageDuplicatingMessage => Res.GetString("db9476b1-8561-4265-befb-2d45e05a1b75", "The combination of eDoc, Document Type should be unique.");
	}
}
