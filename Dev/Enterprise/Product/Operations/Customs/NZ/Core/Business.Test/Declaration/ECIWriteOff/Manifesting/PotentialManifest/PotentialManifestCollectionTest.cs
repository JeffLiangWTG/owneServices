using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	public class PotentialManifestCollectionTest : TestCaseWithFactory
	{
		public void TestFind()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = "NZAKL";
			newBranch.GB_Code = "~NZ";
			Factory.Save();

			var carrier = OrgHeader.New(Factory);
			carrier.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZCHC", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF254", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			var declaration5 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			JobDeclaration dec6WrongComp = null;
			using (DisposableEnvironment.ForBranch(newBranch.PK.ToGuid()))
			{
				dec6WrongComp = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13));
			}

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			AssertCollectionNotContains(dec6WrongComp, manifests);
			var manifest = manifests.Find("081-11111111", "QF253", carrier.PK, JobMessageTypeList.Codes.Import, "NZAKL", new ZDateTime(2005, 12, 13));

			AssertEquals("Manifest.JE_MasterBill", "081-11111111", manifest.JE_MasterBill);
			AssertEquals("Manifest.JE_VoyageFlightNo", "QF253", manifest.JE_VoyageFlightNo);
			AssertEquals("Manifest.JE_OH_ShippingLine", carrier.PK, manifest.JE_OH_ShippingLine);
			AssertEquals("Manifest.MessageType", JobMessageTypeList.Codes.Import, manifest.JE_MessageType);
			AssertEquals("Manifest.BarrierPort", "NZAKL", manifest.BarrierPort);
			AssertEquals("Manifest.BarrierDate", new ZDateTime(2005, 12, 13), manifest.BarrierDate);
			AssertEquals("Manifest.DeclarationCount", 2, manifest.DeclarationCount);
		}

		public void TestCollectionLoadsDeclarationsGroupedNicely()
		{
			var carrier1 = OrgHeader.New(Factory);
			carrier1.FillWithValidTestData();
			var carrier2 = OrgHeader.New(Factory);
			carrier2.FillWithValidTestData();

			var declaration1 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier1.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13, 9, 11, 0));
			var declaration2 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111111", "QF253", carrier1.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 12), new ZDateTime(2005, 12, 13, 11, 9, 0));
			var declaration3 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.ReadyForManifesting, "081-11111122", "QF253", carrier1.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 13), new ZDateTime(2005, 12, 14, 0, 9, 11));
			var declaration4 = CreateTestDeclaration(LowValueConsignmentStatusList.Codes.NotSentToCustoms, "081-11111133", "QF253", carrier1.PK, "USLAX", "NZAKL", new ZDateTime(2005, 12, 13), new ZDateTime(2005, 12, 14, 9, 0, 11));

			Factory.Save();

			var manifests = new PotentialManifestCollection(Factory, GlbCompany.CurrentCompany.PK);
			manifests.Load();
			AssertEquals("Manifests.Count", 2, manifests.Count);

			AssertCollectionContents(manifests, declaration1, 2);
			AssertCollectionContents(manifests, declaration3, 1);
			AssertCollectionContents(manifests, declaration4, 0);
		}

		void AssertCollectionContents(PotentialManifestCollection manifests, JobDeclaration declaration, int expectedCountOfMatchingDecs)
		{
			int actualCountOfMatchingDecs = 0;
			foreach (PotentialManifest manifest in manifests)
			{
				if (manifest.JE_MasterBill == declaration.JE_MasterBill
					&& manifest.JE_VoyageFlightNo == declaration.JE_VoyageFlightNo
					&& manifest.JE_OH_ShippingLine == declaration.JE_OH_ShippingLine
					&& manifest.JE_MessageType == declaration.JE_MessageType
					&& manifest.BarrierPort == declaration.BarrierPort
					&& manifest.BarrierDate == declaration.BarrierDate.Date)
				{
					actualCountOfMatchingDecs += manifest.DeclarationCount;
				}
			}
			AssertEquals("Count of Declarations Matching the Specified Key Declaration", expectedCountOfMatchingDecs, actualCountOfMatchingDecs);
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
	}
}
