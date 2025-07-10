using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business
{
	public class CusStorageDocPivotMAWBValidation : CusStorageDocPivotValidation
	{
		public CusStorageDocPivotMAWBValidation(Customs.Business.AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();

			var parent = Parent;
			var currentDocReference = parent.CSD_StorageDocReference;
			var header = (CusMAWB)parent.Parent;
			if (header.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == currentDocReference && doc.PK != parent.PK)
				|| header.ChildBills.Cast<CusHAWB>().Any(house => HasDocumentsWithReference(house, currentDocReference)))
			{
				parent.CSD_StorageDocReferenceInfo.AddError(DuplicateDocumentErrorMessage);
			}
		}

		bool HasDocumentsWithReference(CusHAWB house, ZGuid docReference)
		{
			return house.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == docReference);
		}
	}
}
