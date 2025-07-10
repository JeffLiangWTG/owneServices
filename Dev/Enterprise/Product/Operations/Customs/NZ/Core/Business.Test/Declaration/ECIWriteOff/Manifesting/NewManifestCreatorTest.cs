using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(NewManifestCreator))]
	public class NewManifestCreatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateManifestFrom()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZCHC", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF254", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));

			Factory.Save();

			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			var manifests = manifestCreator.PotentialManifests;
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			var nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);

			var manifestEntryHeader = manifestCreator.CreateManifestFrom(manifest);
			AssertNotNull("ManifestCreator.CreateManifestFrom(Manifest)", manifestEntryHeader);

			var newFactory = new BusinessObjectFactory();

			var manifestDeclaration1 = newFactory.Load<JobDeclaration>(declaration1.PK);
			var manifestEntryHeader1 = manifestDeclaration1.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			var manifestDeclaration2 = newFactory.Load<JobDeclaration>(declaration5.PK);
			var manifestEntryHeader2 = manifestDeclaration2.CusEntryHeader as ECIWriteOff.CusEntryHeader;

			AssertEquals("ManifestDeclaration1.JE_DeclarationReference", nextManifestReference + "-1", manifestDeclaration1.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration1.IsManifestDeclaration", true, manifestDeclaration1.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader1.CH_BGMReference", nextManifestReference, manifestEntryHeader1.CH_BGMReference);
			AssertEquals("ManifestEntryHeader1 is CusEntryHeader", true, manifestEntryHeader1 is CusEntryHeader);

			AssertEquals("ManifestDeclaration2.JE_DeclarationReference", nextManifestReference + "-2", manifestDeclaration2.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration2.IsManifestDeclaration", true, manifestDeclaration2.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader2.CH_BGMReference", nextManifestReference, manifestEntryHeader2.CH_BGMReference);
			AssertEquals("ManifestEntryHeader2 is CusEntryHeader", true, manifestEntryHeader2 is CusEntryHeader);

			AssertEquals("ManifestEntryHeader1.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader1.PK);
			AssertEquals("ManifestEntryHeader2.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader2.PK);
		}

		public void TestManifestsCreatedInOrder()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2019, 09, 12), new ZDateTime(2019, 09, 13));
			Factory.Save();

			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			var manifests = manifestCreator.PotentialManifests;
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2019, 09, 13));
			var nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);

			var manifestEntryHeader = manifestCreator.CreateManifestFrom(manifest);
			AssertNotNull("ManifestCreator.CreateManifestFrom(Manifest)", manifestEntryHeader);

			var newFactory = new BusinessObjectFactory();

			var manifestDeclaration1 = newFactory.Load<JobDeclaration>(declaration1.PK);
			var manifestEntryHeader1 = manifestDeclaration1.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			var manifestDeclaration2 = newFactory.Load<JobDeclaration>(declaration2.PK);
			var manifestEntryHeader2 = manifestDeclaration2.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			var manifestDeclaration3 = newFactory.Load<JobDeclaration>(declaration3.PK);
			var manifestEntryHeader3 = manifestDeclaration1.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			var manifestDeclaration4 = newFactory.Load<JobDeclaration>(declaration4.PK);
			var manifestEntryHeader4 = manifestDeclaration2.CusEntryHeader as ECIWriteOff.CusEntryHeader;
			var manifestDeclaration5 = newFactory.Load<JobDeclaration>(declaration5.PK);
			var manifestEntryHeader5 = manifestDeclaration2.CusEntryHeader as ECIWriteOff.CusEntryHeader;

			AssertEquals("ManifestDeclaration1.JE_DeclarationReference", nextManifestReference + "-1", manifestDeclaration1.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration1.IsManifestDeclaration", true, manifestDeclaration1.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader1.CH_BGMReference", nextManifestReference, manifestEntryHeader1.CH_BGMReference);
			AssertEquals("ManifestEntryHeader1 is CusEntryHeader", true, manifestEntryHeader1 is CusEntryHeader);

			AssertEquals("ManifestDeclaration2.JE_DeclarationReference", nextManifestReference + "-2", manifestDeclaration2.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration2.IsManifestDeclaration", true, manifestDeclaration2.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader2.CH_BGMReference", nextManifestReference, manifestEntryHeader2.CH_BGMReference);
			AssertEquals("ManifestEntryHeader2 is CusEntryHeader", true, manifestEntryHeader2 is CusEntryHeader);

			AssertEquals("ManifestDeclaration3.JE_DeclarationReference", nextManifestReference + "-3", manifestDeclaration3.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration3.IsManifestDeclaration", true, manifestDeclaration3.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader3.CH_BGMReference", nextManifestReference, manifestEntryHeader3.CH_BGMReference);
			AssertEquals("ManifestEntryHeader3 is CusEntryHeader", true, manifestEntryHeader3 is CusEntryHeader);

			AssertEquals("ManifestDeclaration4.JE_DeclarationReference", nextManifestReference + "-4", manifestDeclaration4.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration4.IsManifestDeclaration", true, manifestDeclaration4.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader4.CH_BGMReference", nextManifestReference, manifestEntryHeader4.CH_BGMReference);
			AssertEquals("ManifestEntryHeader4 is CusEntryHeader", true, manifestEntryHeader4 is CusEntryHeader);

			AssertEquals("ManifestDeclaration5.JE_DeclarationReference", nextManifestReference + "-5", manifestDeclaration5.JE_DeclarationReference);
			AssertEquals("ManifestDeclaration5.IsManifestDeclaration", true, manifestDeclaration5.IsECIManifestDeclarationReference);
			AssertEquals("ManifestEntryHeader5.CH_BGMReference", nextManifestReference, manifestEntryHeader5.CH_BGMReference);
			AssertEquals("ManifestEntryHeader5 is CusEntryHeader", true, manifestEntryHeader5 is CusEntryHeader);

			AssertEquals("ManifestEntryHeader1.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader1.PK);
			AssertEquals("ManifestEntryHeader2.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader2.PK);
			AssertEquals("ManifestEntryHeader3.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader3.PK);
			AssertEquals("ManifestEntryHeader4.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader4.PK);
			AssertEquals("ManifestEntryHeader5.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, manifestEntryHeader5.PK);
		}

		public void TestPotentialManifests()
		{
			AssertEquals("ManifestCreator.PotentialManifests.GetType()", typeof(PotentialManifestCollection), ManifestCreator.PotentialManifests.GetType());
		}

		public void TestCreatingManifestKeepsOriginalDeclarationReferencesAgainstAnInactiveEntryHeaderOnEachDeclaration()
		{
			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			AssertEquals("Touching CusEntryHeader to Create Record", true, declaration1.CusEntryHeader.IsActive);
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			AssertEquals("Touching CusEntryHeader to Create Record", true, declaration2.CusEntryHeader.IsActive);

			Factory.Save();

			var savedDeclarationReference1 = declaration1.JE_DeclarationReference;
			var savedDeclarationReference2 = declaration2.JE_DeclarationReference;

			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			var manifests = manifestCreator.PotentialManifests;
			manifests.Load();
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			var nextManifestReference = Env.NumberFountains.ECIWriteOffManifestReference.PeekPreliminaryFormatted(Factory);

			manifestCreator.CreateManifestFrom(manifest);

			var newFactory = new BusinessObjectFactory();
			var entryHeaders = new CusEntryHeaderCollection(newFactory);
			var filter = new ZQuery(CusEntryHeaderSchema.CH_JE, SQLComparisonOperator.Equal, declaration1.PK);
			filter.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_JE, SQLComparisonOperator.Equal, declaration2.PK);
			entryHeaders.Load(filter);

			bool foundManifestEntryHeader = false;
			bool foundNonManifestEntryHeader1 = false;
			bool foundNonManifestEntryHeader2 = false;
			foreach (CusEntryHeader entryHeader in entryHeaders)
			{
				if (entryHeader.IsManifestEntry)
				{
					foundManifestEntryHeader = true;
					AssertEquals("Manifest EntryHeader.CH_BGMReference", nextManifestReference, entryHeader.CH_BGMReference);
					AssertEquals("Manifest EntryHeader.IsActive", true, entryHeader.IsActive);
					AssertEquals("Manifest EntryHeader Parent is set to one of the Declarations", true, (entryHeader.CH_JE == declaration1.PK || entryHeader.CH_JE == declaration2.PK));
				}
				else if (entryHeader.CH_JE == declaration1.PK)
				{
					foundNonManifestEntryHeader1 = true;
					AssertEquals("NonManifest1 EntryHeader.CH_BGMReference", savedDeclarationReference1, entryHeader.CH_BGMReference);
					AssertEquals("NonManifest1 EntryHeader.IsActive", false, entryHeader.IsActive);
				}
				else if (entryHeader.CH_JE == declaration2.PK)
				{
					foundNonManifestEntryHeader2 = true;
					AssertEquals("NonManifest2 EntryHeader.CH_BGMReference", savedDeclarationReference2, entryHeader.CH_BGMReference);
					AssertEquals("NonManifest2 EntryHeader.IsActive", false, entryHeader.IsActive);
				}
				else
				{
					Fail("Unexpected CusEntryHeader record matching specified filter.");
				}
			}
			AssertEquals("FoundManifestEntryHeader", true, foundManifestEntryHeader);
			AssertEquals("FoundNonManifestEntryHeader1", true, foundNonManifestEntryHeader1);
			AssertEquals("FoundNonManifestEntryHeader2", true, foundNonManifestEntryHeader2);

			AssertEquals("EntryHeaders.Count", 3, entryHeaders.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => ManifestCreator;

		protected NewManifestCreator ManifestCreator => manifestCreator ?? (manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK));
		NewManifestCreator manifestCreator;
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
	}

	public class ECIWriteOffManifestReferenceNumberFountainTest : TransactionedTestCase
	{
		public void TestECIWriteOffManifestFountainPrefixHasntChanged()
		{
			AssertEquals("ECI WriteOff Manifesting will not work AT ALL if NumberFountains.ECIWriteOffManifestFountainPrefix is changed.", "M", NumberFountains.ECIManifestReferencePrefix);
		}

		public void TestGetNewECIWriteOffManifestReference()
		{
			// Lengths of the Manifest Ref are important as it's used as a natural key in a number of places.
			ZString manifestReference1 = Env.NumberFountains.ECIWriteOffManifestReference.GetNextFormatted(CargoWise.Data.Db.Connection);
			ZString manifestReference2 = Env.NumberFountains.ECIWriteOffManifestReference.GetNextFormatted(CargoWise.Data.Db.Connection);

			AssertEquals("ManifestReference1.Left(1)", NumberFountains.ECIManifestReferencePrefix, manifestReference1.Left(1));
			AssertEquals("ManifestReference1.Length", 9, manifestReference1.Length);
			AssertEquals("ManifestReference1.Substring(3).IsNumbersOnlyOrEmpty", true, manifestReference1.Substring(1).IsNumbersOnlyOrEmpty);

			AssertEquals("ManifestReference2.Left(3)", NumberFountains.ECIManifestReferencePrefix, manifestReference2.Left(1));
			AssertEquals("ManifestReference2.Length", 9, manifestReference2.Length);
			AssertEquals("ManifestReference2.Substring(3).IsNumbersOnlyOrEmpty", true, manifestReference2.Substring(1).IsNumbersOnlyOrEmpty);

			Assert("ManifestReference1 != ManifestReference2 (both ended up being " + manifestReference1 + ")", manifestReference1 != manifestReference2);
		}
		protected override void SetUp()
		{
			base.SetUp();
			CargoWise.Data.Db.Connection.BeginTransaction();
		}

		protected override void TearDown()
		{
			CargoWise.Data.Db.Connection.RollbackTransaction();
			base.TearDown();
		}
	}
}
