using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business
{
	public class CusStorageDocPivotSCAHouseValidation : CusStorageDocPivotValidation
	{
		public CusStorageDocPivotSCAHouseValidation(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();

			var parent = Parent;
			var currentDocReference = parent.CSD_StorageDocReference;
			var header = ((CusSCAHouse)parent.Parent).OceanBill;
			if (header.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == currentDocReference)
				|| header.HouseBills.Cast<CusSCAHouse>().Any(house => HasDocumentsWithReference(house, currentDocReference, parent.PK)))
			{
				parent.CSD_StorageDocReferenceInfo.AddError(DuplicateDocumentErrorMessage);
			}
		}

		bool HasDocumentsWithReference(CusSCAHouse house, ZGuid docReference, ZGuid excludedDocPK)
		{
			return house.EDocPivotCollection.Cast<CusStorageDocPivot>().Any(doc => doc.CSD_StorageDocReference == docReference && doc.PK != excludedDocPK);
		}
	}
}
