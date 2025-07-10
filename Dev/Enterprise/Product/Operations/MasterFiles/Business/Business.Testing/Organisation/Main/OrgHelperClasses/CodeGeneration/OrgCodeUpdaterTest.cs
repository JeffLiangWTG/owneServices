using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCodeUpdaterTest : TestCaseWithFactory
	{
		OrgHeader existingOrg;
		OrgHeader org1;
		OrgHeader org2;
		MockOrgCodeUpdater updater;

		MockOrgCodeUpdater Updater
		{
			get { return updater ?? (updater = new MockOrgCodeUpdater(Factory)); }
		}

		void CreateTestData()
		{
			existingOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org1 = Updater.Orgs.AddNew();
			org2 = Updater.Orgs.AddNew();

			org1.OH_FullName = "Test org";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.OH_Code = "FIRSTCODE";
			org1.MainAddress.OA_Address1 = "Test address";

			org2.OH_FullName = "Another test org";
			org2.OH_RL_NKClosestPort = "AUMEL";
			org2.OH_Code = "SECONDCODE";
			org2.MainAddress.OA_Address1 = "Test address 2";

			Factory.Save();
		}

		public static ZString GetOrgCodeFromDB(OrgHeader org)
		{
			return new BusinessObjectFactory().Load<OrgHeader>(org.PK).OH_Code;
		}

		void SetAlgorithmWithSpecificUniqueNumber()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 5;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
		}

		public void TestClashingCodes()
		{
			OrgHeader originalOrg = Updater.Orgs.AddNew();
			originalOrg.OH_FullName = "Test original org";
			originalOrg.OH_RL_NKClosestPort = "ADALV";
			originalOrg.MainAddress.OA_Address1 = "Test address 1";

			OrgHeader clashingOrg = Updater.Orgs.AddNew();
			clashingOrg.OH_FullName = "Test original org";
			clashingOrg.OH_RL_NKClosestPort = "ADALV";
			clashingOrg.MainAddress.OA_Address1 = "Test address 1";

			Factory.Save();

			Updater.UpdateOrgCodes();
			AssertEquals("Code of first org should be unnumbered.", "TESORIALV", GetOrgCodeFromDB(originalOrg));
			AssertEquals("Code of clashing org should be altered to ensure uniqueness.", "TESORIALV1", GetOrgCodeFromDB(clashingOrg));
		}

		public void TestCodeClashingWithPreExistingCode()
		{
			OrgHeader originalOrg = Updater.Orgs.AddNew();
			originalOrg.OH_FullName = "Test original org";
			originalOrg.OH_RL_NKClosestPort = "ADALV";
			originalOrg.MainAddress.OA_Address1 = "Test address 1";

			OrgHeader problemOrg = Updater.Orgs.AddNew();
			problemOrg.OH_FullName = "Test original org";
			problemOrg.OH_RL_NKClosestPort = "ADALV";
			problemOrg.MainAddress.OA_Address1 = "Test address 1";
			problemOrg.OH_Code = originalOrg.OH_Code + "10";

			OrgHeader clashingOrg = Updater.Orgs.AddNew();
			clashingOrg.OH_FullName = "Test original org";
			clashingOrg.OH_RL_NKClosestPort = "ADALV";
			clashingOrg.MainAddress.OA_Address1 = "Test address 1";
			clashingOrg.OH_Code = "XXXXXXXXX";

			AssertEquals("First org code", "TESORIALV", originalOrg.OH_Code);
			AssertEquals("Second org code", "TESORIALV10", problemOrg.OH_Code);
			AssertEquals("Clashing org code", "XXXXXXXXX", clashingOrg.OH_Code);
			Factory.Save();
			AssertEquals("First org code", "TESORIALV", originalOrg.OH_Code);
			AssertEquals("Second org code", "TESORIALV1", problemOrg.OH_Code);
			AssertEquals("Clashing org code", "XXXXXXXXX", clashingOrg.OH_Code);

			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "TESORIALV", GetOrgCodeFromDB(originalOrg));
			AssertEquals("Second org code", "TESORIALV1", GetOrgCodeFromDB(problemOrg));
			AssertEquals("Clashing org code", "TESORIALV2", GetOrgCodeFromDB(clashingOrg));
		}

		public void TestRegenerateCodeWithPreExistingCodeWithSpecificUniqueNumberDigsMixedCharacters()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_FullName = "Test original org";
			orgHeader1.OH_RL_NKClosestPort = "ADALV";
			orgHeader1.OH_Code = "TESALVX";
			orgHeader1.MainAddress.OA_Address1 = "Test address 1";

			var orgHeader2 = Updater.Orgs.AddNew();
			orgHeader2.OH_FullName = "Test original org";
			orgHeader2.OH_RL_NKClosestPort = "ADALV";
			orgHeader2.OH_Code = "TESALVXX";
			orgHeader2.MainAddress.OA_Address1 = "Test address 1";

			var orgHeader3 = Updater.Orgs.AddNew();
			orgHeader3.OH_FullName = "Test original org";
			orgHeader3.OH_RL_NKClosestPort = "ADALV";
			orgHeader3.OH_Code = "TESALVXXX";
			orgHeader3.MainAddress.OA_Address1 = "Test address 1";

			var orgHeaderMax = Updater.Orgs.AddNew();
			orgHeaderMax.OH_FullName = "Test original org";
			orgHeaderMax.OH_RL_NKClosestPort = "ADALV";
			orgHeaderMax.OH_Code = "TESALV100";
			orgHeaderMax.MainAddress.OA_Address1 = "Test address 1";

			Factory.Save();

			AssertEquals("First org code", "TESALVX", orgHeader1.OH_Code);
			AssertEquals("Second org code", "TESALVXX", orgHeader2.OH_Code);
			AssertEquals("Third org code", "TESALVXXX", orgHeader3.OH_Code);
			AssertEquals("Max org code", "TESALV100", orgHeaderMax.OH_Code);

			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "TESALV101", GetOrgCodeFromDB(orgHeader1));
			AssertEquals("Second org code", "TESALV102", GetOrgCodeFromDB(orgHeader2));
			AssertEquals("Third org code", "TESALV103", GetOrgCodeFromDB(orgHeader3));
			AssertEquals("Fourth org code", "TESALV100", GetOrgCodeFromDB(orgHeaderMax));
		}

		public void TestRegenerateCodeWithPreExistingCodeWithGloballyUniqueNumberDigsMixedCharacters()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.Registry.CanUserEditOrganisationCode = true;

			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_FullName = "Test original org";
			orgHeader1.MainAddress.OA_Address1 = "Test address 1";
			orgHeader1.OH_Code = "TES_A";

			var orgHeader2 = Updater.Orgs.AddNew();
			orgHeader2.OH_FullName = "Test original org";
			orgHeader2.MainAddress.OA_Address1 = "Test address 1";
			orgHeader2.OH_Code = "TEST_B";

			var orgHeader3 = Updater.Orgs.AddNew();
			orgHeader3.OH_FullName = "Test original org";
			orgHeader3.MainAddress.OA_Address1 = "Test address 1";
			orgHeader3.OH_Code = "TEST_C";

			var orgHeader4 = Updater.Orgs.AddNew();
			orgHeader4.OH_FullName = "Street original org";
			orgHeader4.MainAddress.OA_Address1 = "Test address 1";
			orgHeader4.OH_Code = "TEST_D";

			Factory.Save();

			AssertEquals("First org code", "TES_A", orgHeader1.OH_Code);
			AssertEquals("Second org code", "TEST_B", orgHeader2.OH_Code);
			AssertEquals("Third org code", "TEST_C", orgHeader3.OH_Code);
			AssertEquals("Fourth org code", "TEST_D", orgHeader4.OH_Code);

			((IDbConnected)Factory).Connection.BeginTransaction();
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(((IDbConnected)Factory).Connection, 1);
			try
			{
				Updater.UpdateOrgCodes();

				AssertEquals("First org code", "TES001", GetOrgCodeFromDB(orgHeader1));
				AssertEquals("Second org code", "TES002", GetOrgCodeFromDB(orgHeader2));
				AssertEquals("Third org code", "TES003", GetOrgCodeFromDB(orgHeader3));
				AssertEquals("Fourth org code", "STR004", GetOrgCodeFromDB(orgHeader4));
			}
			finally
			{
				((IDbConnected)Factory).Connection.RollbackTransaction();
			}
		}

		public void TestRegenerateCodeWithPreExistingMaxCodeWithSpecificUniqueNumber8Digs()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 8;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_Code = "test1";
			var orgHeader2 = Updater.Orgs.AddNew();
			orgHeader2.OH_Code = "test2";
			var orgHeader3 = updater.Orgs.AddNew();
			orgHeader3.OH_Code = "test3";
			var maxOrgHeader = updater.Orgs.AddNew();
			maxOrgHeader.OH_Code = "90000000";

			Factory.Save();

			AssertEquals("First org code", "test1", orgHeader1.OH_Code);
			AssertEquals("Second org code", "test2", orgHeader2.OH_Code);
			AssertEquals("Second org code", "test3", orgHeader3.OH_Code);
			AssertEquals("Third org code", "90000000", maxOrgHeader.OH_Code);

			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "90000001", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader1));
			AssertEquals("Second org code", "90000002", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader2));
			AssertEquals("Third org code", "90000003", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader3));
			AssertEquals("Third org code", "90000000", OrgCodeUpdaterTest.GetOrgCodeFromDB(maxOrgHeader));
		}

		public void TestRegenerateCodeWithoutPreExistingMaxCodeWithSpecificUniqueNumber8Digs()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 8;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_Code = "test1";
			var orgHeader2 = Updater.Orgs.AddNew();
			orgHeader2.OH_Code = "test2";
			var orgHeader3 = updater.Orgs.AddNew();
			orgHeader3.OH_Code = "test3";

			Factory.Save();

			AssertEquals("First org code", "test1", orgHeader1.OH_Code);
			AssertEquals("Second org code", "test2", orgHeader2.OH_Code);
			AssertEquals("Second org code", "test3", orgHeader3.OH_Code);

			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "00000001", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader1));
			AssertEquals("Second org code", "00000002", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader2));
			AssertEquals("Third org code", "00000003", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader3));
		}

		public void TestRegenerateCodeWithPreExistingCodeWithGloballyUniqueNumber8Digs()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 8;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_Code = "10000000";
			var orgHeader2 = Updater.Orgs.AddNew();
			orgHeader2.OH_Code = "20000000";
			var orgHeader3 = Updater.Orgs.AddNew();
			orgHeader3.OH_Code = "90000000";

			Factory.Save();

			AssertEquals("First org code", "10000000", orgHeader1.OH_Code);
			AssertEquals("Second org code", "20000000", orgHeader2.OH_Code);
			AssertEquals("Third org code", "90000000", orgHeader3.OH_Code);

			((IDbConnected)Factory).Connection.BeginTransaction();
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(((IDbConnected)Factory).Connection, 70000000);
			try
			{
				Updater.UpdateOrgCodes();

				AssertEquals("First org code", "10000000", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader1));
				AssertEquals("Second org code", "20000000", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader2));
				AssertEquals("Third org code", "70000000", OrgCodeUpdaterTest.GetOrgCodeFromDB(orgHeader3));
			}
			finally
			{
				((IDbConnected)Factory).Connection.RollbackTransaction();
			}
		}

		public void TestUpdateOrgCodesShouldNotChangeWhenNumberFountainException()
		{
			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			Env.Registry.CanUserEditOrganisationCode = true;
			var orgHeader1 = Updater.Orgs.AddNew();
			orgHeader1.OH_FullName = "Test original org";
			orgHeader1.MainAddress.OA_Address1 = "Test address 1";
			orgHeader1.OH_Code = "7000";
			Factory.Save();

			((IDbConnected)Factory).Connection.BeginTransaction();
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(((IDbConnected)Factory).Connection, 1);
			try
			{
				var orgCodeInfo = Updater.LoadAllOrgsBase()[0];
				var mockOrgCodeDbProxy = new Mock<IOrgCodeDbProxy>();
				mockOrgCodeDbProxy.Setup(x => x.NumberFountainGetNext()).Throws(new Exception());
				var generator = new OrgCodeGenerator();
				var orgCode = generator.GenerateCode(orgCodeInfo, Factory);

				AssertExceptionThrown<Exception>(() => Updater.UpdateSingleOrgCode(orgCodeInfo, mockOrgCodeDbProxy.Object, orgCode, new Dictionary<string, IOrgCodeInfo>(), new List<ZGuid>()));
				AssertEquals("First org code", "7000", GetOrgCodeFromDB(orgHeader1));
			}
			finally
			{
				((IDbConnected)Factory).Connection.RollbackTransaction();
			}
		}

		public void TestCodeUpdaterWithNewDuplicates()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			org1 = Updater.Orgs.AddNew();
			org1.OH_FullName = "Fake org";
			org1.OH_RL_NKClosestPort = "AUBNE";

			org2 = Updater.Orgs.AddNew();
			org2.OH_FullName = "Fake org";
			org2.OH_RL_NKClosestPort = "AUBNE";
			org2.OH_Code = "SECONDCODE";

			OrgHeader org3 = Updater.Orgs.AddNew();
			org3.OH_FullName = "Fake org";
			org3.OH_RL_NKClosestPort = "AUBNE";
			org3.OH_Code = "THIRDCODE";

			Factory.Save();

			AssertEquals("First org code", "FAKORGBNE", org1.OH_Code);
			AssertEquals("Second org code", "SECONDCODE", org2.OH_Code);
			AssertEquals("Second org code", "THIRDCODE", org3.OH_Code);

			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "FAKORGBNE", GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "FAKORGBNE1", GetOrgCodeFromDB(org2));
			AssertEquals("Second org code", "FAKORGBNE2", GetOrgCodeFromDB(org3));
		}

		public void TestCodeUpdaterWithNewDuplicates_CodeSpecificUniqueNumber()
		{
			// it is important that STORMA/B/C/D.Length < STORMCMI3.Length
			// and CodeSpecificUniqueNumber is used in the code

			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm oca = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oca);

			org1 = Updater.Orgs.AddNew();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			org1.OH_Code = "STORMCMI3000";

			org2 = Updater.Orgs.AddNew();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			org2.OH_Code = "STORMB";

			OrgHeader org3 = Updater.Orgs.AddNew();
			org3.OH_FullName = "Stormcloaks";
			org3.OH_RL_NKClosestPort = "JPMI3";
			org3.OH_Code = "STORMCMI3001";

			OrgHeader org4 = Updater.Orgs.AddNew();
			org4.OH_FullName = "Stormcloaks";
			org4.OH_RL_NKClosestPort = "JPMI3";
			org4.OH_Code = "STORMD";

			Factory.Save();

			Updater.UpdateOrgCodes();

			AssertEquals("STORMCMI3000", GetOrgCodeFromDB(org1));
			AssertEquals("STORMCMI3002", GetOrgCodeFromDB(org2));
			AssertEquals("STORMCMI3001", GetOrgCodeFromDB(org3));
			AssertEquals("STORMCMI3003", GetOrgCodeFromDB(org4));
		}

		public void TestCodeUpdaterWithNewDuplicates_OverflowingCode()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 9;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 2;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			org1 = Updater.Orgs.AddNew();
			org2 = Updater.Orgs.AddNew();

			org1.OH_FullName = "ABCDEFGHI";
			org1.OH_RL_NKClosestPort = "AUBNE";
			org1.OH_Code = "FIRSTCODE";

			org2.OH_FullName = "ABCDEFGHI";
			org2.OH_RL_NKClosestPort = "AUBNE";
			org2.OH_Code = "SECONDCODE";

			Factory.Save();

			AssertEquals("First org code", "FIRSTCODE", org1.OH_Code);
			AssertEquals("Second org code", "SECONDCODE", org2.OH_Code);

			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ABCDEFGHIBNE", GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ABCDEFGHIBN1", GetOrgCodeFromDB(org2));
		}

		public void TestDifferentAlgorithms()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Broker].Selected = true;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader org1 = Updater.Orgs.AddNew();
			org1.OH_IsBroker = true;
			org1.OH_FullName = "Test original org";
			org1.OH_RL_NKClosestPort = "ADALV";
			org1.OH_Code = "FIRSTCODE";

			OrgHeader org2 = Updater.Orgs.AddNew();
			org2.OH_IsBroker = false;
			org2.OH_FullName = "Test original org";
			org2.OH_RL_NKClosestPort = "ADALV";
			org2.OH_Code = "SECONDCODE";

			OrgHeader org3 = Updater.Orgs.AddNew();
			org3.OH_IsBroker = true;
			org3.OH_FullName = "Test original org";
			org3.OH_RL_NKClosestPort = "ADALV";
			org3.OH_Code = "THIRDCODE";

			Factory.Save();

			Updater.UpdateOrgCodes(OrgCodeAlgorithmType.Override);
			AssertEquals("First org code", "TES", GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "SECONDCODE", GetOrgCodeFromDB(org2));
			AssertEquals("Third org code", "TES1", GetOrgCodeFromDB(org3));

			Updater.UpdateOrgCodes(OrgCodeAlgorithmType.Default);
			AssertEquals("First org code", "TES", GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "TESORIALV", GetOrgCodeFromDB(org2));
			AssertEquals("Third org code", "TES1", GetOrgCodeFromDB(org3));

			algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Broker].Selected = true;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			updater = new MockOrgCodeUpdater(newFactory);
			Updater.Orgs.Add(newFactory.Load<OrgHeader>(org1.PK));
			Updater.Orgs.Add(newFactory.Load<OrgHeader>(org2.PK));
			Updater.Orgs.Add(newFactory.Load<OrgHeader>(org3.PK));

			Updater.UpdateOrgCodes(OrgCodeAlgorithmType.Override);
			AssertEquals("First org code", "TESORIALV1", GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "TESORIALV", GetOrgCodeFromDB(org2));
			AssertEquals("Third org code", "TESORIALV2", GetOrgCodeFromDB(org3));
		}

		/// <summary>
		/// Simulates when, after the updater has started,
		/// another user has updated or added a new org with a code 
		/// that clashes with an updated code.
		/// </summary>
		public void TestOrgCodeChangeDuringUpdate()
		{
			CreateTestData();

			ZDateTime startTime = ZDateTime.Now.AddSeconds(-1);
			Updater.StartTimeOverride = startTime;

			OrgCodeGenerator generator = new OrgCodeGenerator();
			ZString code = generator.GenerateCode(org1).GetProposedCode();

			existingOrg.OH_Code = code;
			Factory.Save();

			Updater.UpdateOrgCodes();
			AssertEquals(code + 1, GetOrgCodeFromDB(org1));
		}

		[StressTest]
		public void TestLoadAllOrgs()
		{
			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load();
			IOrgCodeInfo[] allOrgs = Updater.LoadAllOrgsBase();
			AssertEquals("LoadAllOrgs().Length", collection.Count, allOrgs.Length);
		}

		public void TestUpdateOrgCodes()
		{
			CreateTestData();
			Updater.UpdateOrgCodes();
			OrgCodeGenerator generator = new OrgCodeGenerator();
			AssertEquals("Code should be updated.", generator.GenerateCode(org1).GetProposedCode(), GetOrgCodeFromDB(org1));
			AssertEquals("Code should be updated.", generator.GenerateCode(org2).GetProposedCode(), GetOrgCodeFromDB(org2));
		}

		public void TestUpdateOrgCodesWithSpecificUniqueNumberWithoutExistingMaxOrgCode()
		{
			SetAlgorithmWithSpecificUniqueNumber();
			CreateTestData();

			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG00002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));

			org1.OH_Code = "ORG00001";
			org2.OH_Code = "ORG00002";

			// If only the number at the end of the code has changed, there is no need to update.
			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG00002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));
			AssertEquals("There should be no pending transactions.", 1, Db.Connection.AppTransactionCount);
		}

		public void TestUpdateOrgCodesWithSpecificUniqueNumberWithExistingMaxOrgCode()
		{
			SetAlgorithmWithSpecificUniqueNumber();
			CreateTestData();
			var orgWithMaxOhCode = Updater.Orgs.AddNew();
			orgWithMaxOhCode.OH_Code = "Org10000";
			Factory.Save();

			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG10001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG10002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));

			var anotherOrgWithMaxOhCode = Updater.Orgs.AddNew();
			anotherOrgWithMaxOhCode.OH_Code = "Org20000";
			Factory.Save();

			// If only the number at the end of the code has changed, there is no need to update.
			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG10001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG10002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));
			AssertEquals("There should be no pending transactions.", 1, Db.Connection.AppTransactionCount);
		}

		public void TestUpdateOrgCodesWithUniqueNumber()
		{
			SetAlgorithmWithSpecificUniqueNumber();
			CreateTestData();

			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG00002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));

			org1.OH_Code = "ORG00001";
			org2.OH_Code = "ORG00002";

			// If only the number at the end of the code has changed, there is no need to update.
			Updater.UpdateOrgCodes();
			AssertEquals("First org code", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG00002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));
			AssertEquals("There should be no pending transactions.", 1, Db.Connection.AppTransactionCount);
		}

		public void TestUpdateOrgCodesWithUniqueNumberWithClashing()
		{
			SetAlgorithmWithSpecificUniqueNumber();
			CreateTestData();
			OrgHeader org3 = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();

			org3.OH_FullName = "Test org";
			org3.OH_RL_NKClosestPort = "UAIEV";
			org3.OH_Code = "ORG00001";
			org3.MainAddress.OA_Address1 = "Test address other";
			org3.Factory.Save();

			AssertEquals("should not be changed", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org3));

			Updater.Orgs.Add(org3);
			Updater.UpdateOrgCodes();

			AssertEquals("First org code", "ORG00002", OrgCodeUpdaterTest.GetOrgCodeFromDB(org1));
			AssertEquals("Second org code", "ORG00003", OrgCodeUpdaterTest.GetOrgCodeFromDB(org2));
			AssertEquals("should not be changed", "ORG00001", OrgCodeUpdaterTest.GetOrgCodeFromDB(org3));
		}

		#region MockOrgCodeUpdater Class

		class MockOrgCodeUpdater : OrgCodeUpdater
		{
			readonly OrgHeaderCollection orgs;
			ZDateTime? startTimeOverride;

			public MockOrgCodeUpdater(BusinessObjectFactory factory)
				: base(factory, 1)
			{
				this.orgs = new OrgHeaderCollection(factory);
			}

			public OrgHeaderCollection Orgs
			{
				get { return orgs; }
			}

			public ZDateTime? StartTimeOverride
			{
				get { return startTimeOverride; }
				set { startTimeOverride = value; }
			}

			protected override DateTime GetStartTime()
			{
				return (StartTimeOverride == null) ? base.GetStartTime() : StartTimeOverride.Value.ToDateTime();
			}

			protected override IOrgCodeInfo[] LoadAllOrgs()
			{
				IOrgCodeInfo[] result = new IOrgCodeInfo[Orgs.Count];
				for (int i = 0; i < Orgs.Count; i++)
				{
					result[i] = new OrgHeaderOrgCodeInfo(Orgs[i]);
				}
				return result;
			}

			public IOrgCodeInfo[] LoadAllOrgsBase()
			{
				return base.LoadAllOrgs();
			}

			public void UpdateOrgCodes()
			{
				UpdateOrgCodes(OrgCodeAlgorithmType.Default);
			}
		}

		#endregion
	}
}
