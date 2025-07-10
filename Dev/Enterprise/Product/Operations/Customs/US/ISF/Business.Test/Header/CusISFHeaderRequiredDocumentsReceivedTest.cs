using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderRequiredDocumentsReceivedTest : JobRequiredDocumentsReceivedTest<CusISFHeader>
	{
		protected override void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			pk = header.PK;
			requiredDocumentsParent = header;
			logsParent = header;
		}

		protected override void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var header = factory.Load<CusISFHeader>(pk);
			requiredDocumentsParent = header;
			logsParent = header;
		}

		protected override void ModifyParent(CusISFHeader parent)
		{
			parent.BF_CustomsReference = "aaaa";
		}

		protected override void CreateCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForUSAAndNoOrigin();
		}
	}
}
