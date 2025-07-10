using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	using CargoWise.Data.Testing;
	using NUnit.Framework;

	[TestedType(typeof(CusEntryHeader))]
	public class CusEntryHeaderTest : ECIWriteOff.Testing.CusEntryHeaderTest<CusEntryHeader>
	{
		public override void TestLastCustomsStatusIsImpediment()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader);
			JobDeclaration declaration1 = manifestCreator.AddDeclaration();
			JobDeclaration declaration2 = manifestCreator.AddDeclaration();
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", false, entryHeader.LastCustomsStatusIsImpediment);
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", true, entryHeader.LastCustomsStatusIsImpediment);
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", false, entryHeader.LastCustomsStatusIsImpediment);
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", true, entryHeader.LastCustomsStatusIsImpediment);
		}

		public void TestLoad()
		{
			var entryHeader1 = Factory.New<CusEntryHeader>();
			entryHeader1.CH_BGMReference = "101010";
			entryHeader1.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			entryHeader1.CH_IsActive = false;

			var entryHeader2 = Factory.New<CusEntryHeader>();
			entryHeader2.CH_BGMReference = "101010";
			entryHeader2.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			entryHeader2.CH_IsActive = true;

			var entryHeader3 = Factory.New<CusEntryHeader>();
			entryHeader3.CH_BGMReference = "101010";
			entryHeader3.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			entryHeader3.CH_IsActive = false;

			var entryHeader4 = Factory.New<CusEntryHeader>();
			entryHeader4.CH_BGMReference = "222222";
			entryHeader4.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			entryHeader4.CH_IsActive = true;

			var resultingEntryHeader = CusEntryHeader.Load(Factory, "101010");
			AssertEquals("ResultingEntryHeader.PK == EntryHeader2.PK", entryHeader2.PK, resultingEntryHeader.PK);
		}

		public void TestDeclarationsChangedToFormalAreNotIncluded()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2020, 08, 24), new ZDateTime(2020, 08, 25));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2020, 08, 24), new ZDateTime(2020, 08, 25));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2020, 08, 24), new ZDateTime(2020, 08, 25));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2020, 08, 24), new ZDateTime(2020, 08, 25));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2020, 08, 24), new ZDateTime(2020, 08, 25));
			Factory.Save();

			var manifestEntryHeader = Factory.New<CusEntryHeader>();
			manifestEntryHeader.Declarations.Add(declaration1);
			manifestEntryHeader.Declarations.Add(declaration2);
			manifestEntryHeader.Declarations.Add(declaration3);
			manifestEntryHeader.Declarations.Add(declaration4);
			manifestEntryHeader.Declarations.Add(declaration5);
			AssertEquals("All declarations are valid for the manifest", 5, manifestEntryHeader.Declarations.Count);

			declaration3.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration4.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Factory.Save();

			var manifestReloaded = new BusinessObjectFactory().Load<CusEntryHeader>(manifestEntryHeader.PK);
			AssertEquals("Only 3 declarations are now valid for the write-off manifest", 3, manifestReloaded.Declarations.Count);
		}

		public void TestOnSavingSetsUpManifestForAllDeclarationsInCollection()
		{
			OrgHeader carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			JobDeclaration declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			ECIWriteOff.CusEntryHeader entryHeader1 = declaration1.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			JobDeclaration declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			ECIWriteOff.CusEntryHeader entryHeader2 = declaration2.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			Factory.Save();

			ZString savedDeclarationReference1 = declaration1.JE_DeclarationReference;
			ZString savedDeclarationReference2 = declaration2.JE_DeclarationReference;
			ZString nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.Declarations.Add(declaration1);
			entryHeader.Declarations.Add(declaration2);
			Factory.Save();

			AssertEquals("EntryHeader.CH_BGMReference", nextManifestReference, entryHeader.CH_BGMReference);
			AssertEquals("EntryHeader.CH_JE", declaration1.PK, entryHeader.CH_JE);
			AssertEquals("EntryHeader.IsActive", true, entryHeader.IsActive);

			AssertEquals("EntryHeader1.CH_BGMReference", savedDeclarationReference1, entryHeader1.CH_BGMReference);
			AssertEquals("EntryHeader1.CH_JE", declaration1.PK, entryHeader1.CH_JE);
			AssertEquals("EntryHeader1.IsActive", false, entryHeader1.IsActive);

			AssertEquals("EntryHeader2.CH_BGMReference", savedDeclarationReference2, entryHeader2.CH_BGMReference);
			AssertEquals("EntryHeader2.CH_JE", declaration2.PK, entryHeader2.CH_JE);
			AssertEquals("EntryHeader2.IsActive", false, entryHeader2.IsActive);

			AssertEquals("Declaration1.JE_DeclarationReference", nextManifestReference + "-1", declaration1.JE_DeclarationReference);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ManifestedReadyToSend, declaration1.JE_EntryStatus);
			AssertEquals("Declaration1.CusEntryHeader.PK == EntryHeader.PK", entryHeader.PK, declaration1.CusEntryHeader.PK);
			AssertEquals("Declaration1.CustomsEntryHeaders.Contains(EntryHeader)", true, declaration1.CustomsEntryHeaders.Contains(entryHeader));
			AssertEquals("Declaration1.CustomsEntryHeaders.Contains(EntryHeader1)", true, declaration1.CustomsEntryHeaders.Contains(entryHeader1));

			AssertEquals("Declaration2.JE_DeclarationReference", nextManifestReference + "-2", declaration2.JE_DeclarationReference);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ManifestedReadyToSend, declaration2.JE_EntryStatus);
			AssertEquals("Declaration2.CusEntryHeader.PK == EntryHeader.PK", entryHeader.PK, declaration2.CusEntryHeader.PK);
			AssertEquals("Declaration2.CustomsEntryHeaders.Contains(EntryHeader)", true, declaration2.CustomsEntryHeaders.Contains(entryHeader));
			AssertEquals("Declaration2.CustomsEntryHeaders.Contains(EntryHeader2)", true, declaration2.CustomsEntryHeaders.Contains(entryHeader2));
		}

		public void TestManifestLinkOnDeclarationsDeterminesCorrectPadding()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			Factory.Save();

			ZString nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);
			var entryHeader = Factory.New<CusEntryHeader>();

			entryHeader.Declarations.Add(declaration1);
			entryHeader.Declarations.Add(declaration2);
			for (int i = 0; i < 12; i++)
			{
				entryHeader.Declarations.AddNew();
			}

			var declaration15 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			entryHeader.Declarations.Add(declaration15);

			Factory.Save();
			AssertEquals("Declaration1.JE_DeclarationReference", nextManifestReference + "-01", declaration1.JE_DeclarationReference);
			AssertEquals("Declaration2.JE_DeclarationReference", nextManifestReference + "-02", declaration2.JE_DeclarationReference);
			AssertEquals("Declaration15.JE_DeclarationReference", nextManifestReference + "-15", declaration15.JE_DeclarationReference);
		}

		public void TestManifestLinkDeterminesCorrectSuffix()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();
			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			Factory.Save();

			ZString nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);
			var entryHeader = Factory.New<CusEntryHeader>();

			entryHeader.Declarations.Add(declaration1);
			entryHeader.Declarations.Add(declaration2);
			for (int i = 0; i < 103; i++)
			{
				entryHeader.Declarations.AddNew();
			}

			var declaration106 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			entryHeader.Declarations.Add(declaration106);

			Factory.Save();
			AssertEquals("Declaration1.JE_DeclarationReference", nextManifestReference + "-001", declaration1.JE_DeclarationReference);
			AssertEquals("Declaration2.JE_DeclarationReference", nextManifestReference + "-002", declaration2.JE_DeclarationReference);
			AssertEquals("Declaration106.JE_DeclarationReference", nextManifestReference + "-106", declaration106.JE_DeclarationReference);
		}

		public override void TestPackages()
		{
			JobDeclaration declaration1 = JobDeclaration.New(Factory);
			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration1.JE_DeclarationReference = "M00001001-1";
			declaration1.JE_TotalNoOfPacks = 12;

			JobDeclaration declaration2 = JobDeclaration.New(Factory);
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration2.JE_DeclarationReference = "M00001001-2";
			declaration2.JE_TotalNoOfPacks = 13;

			JobDeclaration declaration3 = JobDeclaration.New(Factory);
			declaration3.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration3.JE_DeclarationReference = "B00001002";
			declaration3.JE_TotalNoOfPacks = 14;

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "M00001001";

			AssertEquals("EntryHeader.Packages", 25, entryHeader.PackagesCount);
			AssertEquals("Declaration3.EntryHeader.Packages", 14, declaration3.CusEntryHeader.PackagesCount);
		}

		public override void TestSetDefaultValues()
		{
			AssertEquals("EntryHeader.CH_MessageType", CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest, EntryHeader.CH_MessageType);
			AssertEquals("EntryHeader.IsManifestEntry", true, EntryHeader.IsManifestEntry);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, EntryHeader.CH_EntryStatus);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryHeader = EntryHeader;
			return entryHeader;
		}

		protected override bool ShouldBeDeletedIfContainsNoValuableData(Declaration.CusEntryHeader inactiveEntryHeader) => !(inactiveEntryHeader is CusEntryHeader);

		protected new CusEntryHeader EntryHeader => base.EntryHeader;

		protected override Declaration.CusEntryHeader GetNewCusEntryHeader()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = Declaration.PK;
			return entryHeader;
		}

		JobDeclaration CreateTestDeclaration(ZString entryStatus,   // Filter
			ZString masterBill, ZString flightNo, ZGuid carrier, // Grouping
			ZString loading, ZString discharge,                  // Grouping
			ZDateTime aTD, ZDateTime aTA)                        // Grouping
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
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

		#endregion

	}

	public class CustomsEntryHeaderECIWriteOff_NoDuplicatedReferenceTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestBGMReferenceNoDuplicated()
		{
			var factory = new BusinessObjectFactory();
			var declaration1 = JobDeclaration.New(factory);
			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var ref1 = ZString.Empty;

			var connection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				connection.BeginTransaction();
				entryHeader1.OnSaving();
				ref1 = entryHeader1.CH_BGMReference;
			}
			finally
			{
				connection.RollbackTransaction();
			}

			var factory2 = new BusinessObjectFactory();
			var declaration2 = JobDeclaration.New(factory2);
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();

			factory2.Save();
			AssertEquals("CH_BGMReference should use the last number from number fountain as the transaction rolled back.", ref1, entryHeader2.CH_BGMReference);
			AssertEquals("Should not refresh CH_BGMReference untill factory saving.", ref1, entryHeader1.CH_BGMReference);

			declaration1.JE_DeclarationReference = "";
			factory.Save();
			AssertNotEquals("CH_BGMReference get a new number after another saving.", ref1, entryHeader1.CH_BGMReference);
		}
	}
}
