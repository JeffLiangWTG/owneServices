using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(PotentialManifest))]
	class PotentialManifestTest : PersistentBusinessObjectTestCase
	{
		public void TestMessageTypeDescription()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			carrier.OH_FullName = "BetterDamnWorkThisTime";

			var declaration = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "08111111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("08111111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));
			AssertEquals("manifest.MessageTypeDescription", JobMessageTypeList.Descriptions.Import, manifest.MessageTypeDescription);
		}

		public void TestJE_FormattedMasterBillFormatsCorrectly()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			carrier.OH_FullName = "BetterDamnWorkThisTime";

			var declaration = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "08111111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("08111111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));
			AssertEquals("Manifest.JE_FormattedMasterBill", "081-11111111", manifest.JE_FormattedMasterBill);
		}

		public void TestShippingLineFullName()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			carrier.OH_FullName = "BetterDamnWorkThisTime";

			var declaration = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));
			AssertEquals("Manifest.ShippingLineFullName", carrier.OH_FullName, manifest.ShippingLineFullName);
		}

		public void TestGetDeclarationsForManifestWhenSomeManifestsAreNotReadyForManifesting()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 1, 0, 0), new ZDateTime(2005, 12, 13, 6, 0, 0));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.NotSentToCustoms, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 2, 0, 0), new ZDateTime(2005, 12, 13, 7, 0, 0));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.SentToCustoms, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 3, 0, 0), new ZDateTime(2005, 12, 13, 8, 0, 0));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 4, 0, 0), new ZDateTime(2005, 12, 13, 9, 0, 0));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ManifestedReadyToSend, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 5, 0, 0), new ZDateTime(2005, 12, 13, 10, 0, 0));

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			var manifestCreationFactory = new BusinessObjectFactory();
			var declarationsForManifest = manifest.GetDeclarationsForManifest(manifestCreationFactory, GlbCompany.CurrentCompany.PK);
			AssertEquals("DeclarationsForManifest.Contains(Declaration1.PK)", true, declarationsForManifest.Contains(declaration1.PK));
			AssertEquals("DeclarationsForManifest.Count", 1, declarationsForManifest.Count);
		}

		public void TestGetDeclarationsForManifestDoesntBarfOnEmptyDates()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", ZDateTime.Empty, ZDateTime.Empty);
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 2, 0, 0), new ZDateTime(2005, 12, 13, 7, 0, 0));

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", ZDateTime.Empty);

			var manifestCreationFactory = new BusinessObjectFactory();
			var declarationsForManifest = manifest.GetDeclarationsForManifest(manifestCreationFactory, GlbCompany.CurrentCompany.PK);
			AssertEquals("DeclarationsForManifest.Contains(Declaration1.PK)", true, declarationsForManifest.Contains(declaration1.PK));
			AssertEquals("DeclarationsForManifest.Count", 1, declarationsForManifest.Count);
		}

		public void TestGetDeclarationsForManifestDoesntBarfOnEmptyCarrier()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", ZGuid.Empty, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 2, 0, 0), new ZDateTime(2005, 12, 13, 7, 0, 0));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 2, 0, 0), new ZDateTime(2005, 12, 13, 7, 0, 0));
			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", ZGuid.Empty, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			var manifestCreationFactory = new BusinessObjectFactory();
			var declarationsForManifest = manifest.GetDeclarationsForManifest(manifestCreationFactory, GlbCompany.CurrentCompany.PK);
			AssertEquals("DeclarationsForManifest.Contains(Declaration1.PK)", true, declarationsForManifest.Contains(declaration1.PK));
			AssertEquals("DeclarationsForManifest.Count", 1, declarationsForManifest.Count);
		}

		public void TestGetDeclarationsForManifest()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 1, 0, 0), new ZDateTime(2005, 12, 13, 6, 0, 0));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 2, 0, 0), new ZDateTime(2005, 12, 13, 7, 0, 0));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZCHC", new ZDateTime(2005, 12, 12, 3, 0, 0), new ZDateTime(2005, 12, 13, 8, 0, 0));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF254", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 4, 0, 0), new ZDateTime(2005, 12, 13, 9, 0, 0));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12, 5, 0, 0), new ZDateTime(2005, 12, 13, 10, 0, 0));

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			var manifestCreationFactory = new BusinessObjectFactory();
			var declarationsForManifest = manifest.GetDeclarationsForManifest(manifestCreationFactory, GlbCompany.CurrentCompany.PK);
			AssertEquals("DeclarationsForManifest.Contains(Declaration1.PK)", true, declarationsForManifest.Contains(declaration1.PK));
			AssertEquals("DeclarationsForManifest.Contains(Declaration5.PK)", true, declarationsForManifest.Contains(declaration5.PK));
			AssertEquals("DeclarationsForManifest.Count", 2, declarationsForManifest.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			return manifests[0];
		}

		JobDeclaration CreateTestDeclaration(ZString entryStatus,   // Filter
			ZString masterBill, ZString flightNo, ZGuid carrier, // Grouping
			ZString loading, ZString discharge,                  // Grouping
			ZDateTime aTD, ZDateTime aTA)                        // Grouping
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_EntryStatus = entryStatus;
			declaration.JE_MasterBill = masterBill;
			declaration.JE_VoyageFlightNo = flightNo;
			declaration.JE_OH_ShippingLine = carrier;
			declaration.JE_RL_NKPortOfLoading = loading;
			declaration.JE_RL_NKPortOfArrival = discharge;
			declaration.JE_ExportDate = aTD;
			declaration.JE_DateOfArrival = aTA;
			return declaration;
		}

		protected override bool IsDeleteSupported() => false;
	}
}
