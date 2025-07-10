using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryRequiredDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRT_ReferenceType_List()
		{
			RefDocType docTypeSCL = Factory.New<RefDocType>();
			docTypeSCL.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;

			RefDocType docTypeCSR = Factory.New<RefDocType>();
			docTypeCSR.RT_ReferenceType = Constants.ReferenceTypes.ClientSupplierRelationship;

			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			AssertCollectionContains("RT_ReferenceType_List should contain docTypeSCL", docTypeSCL, requiredDocument.Lookups.RT_ReferenceType_List);
			AssertCollectionNotContains("RT_ReferenceType_List should NOT contain docTypeCSR", docTypeCSR, requiredDocument.Lookups.RT_ReferenceType_List);

			RefDocType docTypeINV = Factory.New<RefDocType>();
			docTypeINV.RT_ReferenceType = Constants.ReferenceTypes.All;
			docTypeINV.RT_DocType = Constants.RefDocTypes.Invoice;
			AssertCollectionContains("RT_ReferenceType_List should contain docTypeINV", docTypeINV, requiredDocument.Lookups.RT_ReferenceType_List);

			RefDocType docTypePRV = Factory.New<RefDocType>();
			docTypePRV.RT_ReferenceType = Constants.ReferenceTypes.All;
			docTypePRV.RT_DocType = Constants.RefDocTypes.InternallyCreatedPrivateDocument;
			AssertCollectionNotContains("RT_ReferenceType_List should NOT contain docTypePRV", docTypePRV, requiredDocument.Lookups.RT_ReferenceType_List);

			RefDocType docTypePUB = Factory.New<RefDocType>();
			docTypePUB.RT_ReferenceType = Constants.ReferenceTypes.All;
			docTypePUB.RT_DocType = Constants.RefDocTypes.InternallyCreatedPublicDocument;
			AssertCollectionNotContains("RT_ReferenceType_List should NOT contain docTypePUB", docTypePUB, requiredDocument.Lookups.RT_ReferenceType_List);
		}

		public void TestTransportMode_List()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			AssertEquals("Should only contain 7", 7, requiredDocument.Lookups.TransportMode_List.Count);
			AssertEquals("ALL", Constants.TransportModes.All, requiredDocument.Lookups.TransportMode_List[0].Code);
			AssertEquals("Sea", Constants.TransportModes.Sea, requiredDocument.Lookups.TransportMode_List[1].Code);
			AssertEquals("Air", Constants.TransportModes.Air, requiredDocument.Lookups.TransportMode_List[2].Code);
			AssertEquals("FCL", Constants.ContainerModes.FCL, requiredDocument.Lookups.TransportMode_List[3].Code);
			AssertEquals("LCL", Constants.ContainerModes.LCL, requiredDocument.Lookups.TransportMode_List[4].Code);
			AssertEquals("Rail", Constants.TransportModes.Rail, requiredDocument.Lookups.TransportMode_List[5].Code);
			AssertEquals("Road", Constants.TransportModes.Road, requiredDocument.Lookups.TransportMode_List[6].Code);
		}

		public void TestDocUsage_List()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			AssertEquals("Should only contain 5", 5, requiredDocument.Lookups.DocUsage_List.Count);
			AssertEquals("All", JobRequiredDocument.DocUsage.All, requiredDocument.Lookups.DocUsage_List[0].Code);
			AssertEquals("Export", JobRequiredDocument.DocUsage.Export, requiredDocument.Lookups.DocUsage_List[1].Code);
			AssertEquals("Import", JobRequiredDocument.DocUsage.Import, requiredDocument.Lookups.DocUsage_List[2].Code);
			AssertEquals("Both", JobRequiredDocument.DocUsage.Both, requiredDocument.Lookups.DocUsage_List[3].Code);
			AssertEquals("Domestic", JobRequiredDocument.DocUsage.Domestic, requiredDocument.Lookups.DocUsage_List[4].Code);
		}
	}
}
