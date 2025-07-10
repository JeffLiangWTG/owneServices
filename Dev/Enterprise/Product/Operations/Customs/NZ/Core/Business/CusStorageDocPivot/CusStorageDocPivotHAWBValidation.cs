using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business
{
	public class CusStorageDocPivotHAWBValidation : CusStorageDocPivotValidation
	{
		public CusStorageDocPivotHAWBValidation(Customs.Business.AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();

			var parent = Parent;
			var currentDocReference = parent.CSD_StorageDocReference;
			var header = ((CusHAWB)parent.Parent).MAWB;
			if (header.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == currentDocReference)
				|| header.ChildBills.Cast<CusHAWB>().Any(house => HasDocumentsWithReference(house, currentDocReference, parent.PK)))
			{
				parent.CSD_StorageDocReferenceInfo.AddError(DuplicateDocumentErrorMessage);
			}
		}

		bool HasDocumentsWithReference(CusHAWB house, ZGuid docReference, ZGuid excludedDocPK)
		{
			return house.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == docReference && doc.PK != excludedDocPK);
		}
	}
}
