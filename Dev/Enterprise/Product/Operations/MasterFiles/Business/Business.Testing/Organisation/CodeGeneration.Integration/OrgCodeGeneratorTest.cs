using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgCodeGeneratorTest : TestCaseWithFactory
	{
		CargoWise.Organizations.CodeGeneration.OrgCodeGenerator generator;
		OrgHeader organization;
		RefUNLOCO unloco;

		protected CargoWise.Organizations.CodeGeneration.OrgCodeGenerator Generator
		{
			get { return generator ?? (generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters())); }
		}

		protected OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					organization.OH_FullName = "Test Org";
					organization.OH_RL_NKClosestPort = Unloco.RL_Code;
				}
				return organization;
			}
		}

		protected RefUNLOCO Unloco
		{
			get
			{
				if (unloco == null)
				{
					unloco = Factory.New<RefUNLOCO>();
					RefCountry country = Factory.New<RefCountry>();
					country.RN_Code = "ZX";
					unloco.RL_Code = "ZXZZZ";
					unloco.RL_RN_NKCountryCode = country.Code;
				}
				return unloco;
			}
		}

		string GetCode()
		{
			return GetCode(Organization);
		}

		string GetCode(OrgHeader organization)
		{
			return Generator.GenerateCode(new OrgHeaderOrgCodeInfo(organization), new OrgCodeDbProxy(organization.Factory)).GetProposedCode();
		}

		protected void SetUnlocoCode(string value)
		{
			Unloco.RL_Code = value;
			Organization.OH_RL_NKClosestPort = value;
		}

		public void TestDoesNotRemoveSomeChars()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 9;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters());

			string newCode = "ABC_DEF";
			Organization.OH_FullName = newCode;
			AssertEquals(newCode, generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
		}

		public void TestRemoveAccentsOnOrgName()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 9;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters());

			Organization.OH_FullName = "ÉÂÊÎÔÛÀÈÙ";
			AssertEquals("EAEIOUAEU", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_FullName = "ËÏÜŸÇÄÖÜÅ";
			AssertEquals("EIUYCAOUA", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_FullName = "éâêîôûàèù";
			AssertEquals("EAEIOUAEU", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_FullName = "ëïüÿçäöüå";
			AssertEquals("EIUYCAOUA", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
		}

		public void TestTransformsLettersIntoLatin()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 9;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters());

			//Danish
			Organization.OH_FullName = "ÆØHELLO";
			AssertEquals("AEOHELLO", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			//German
			Organization.OH_FullName = "GROß";
			AssertEquals("GROSS", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			//French
			Organization.OH_FullName = "ŒHELLO";
			AssertEquals("OEHELLO", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			//Russian
			Organization.OH_FullName = "ЁЖ";
			AssertEquals("EZH", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			//Ukrainian
			Organization.OH_FullName = "ҐАВА";
			AssertEquals("GAVA", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			//Greek
			Organization.OH_FullName = "Πυθαγόρας";
			AssertEquals("PYTAGORAS", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
		}

		public void TestAlgorithmAndOrgTypes()
		{
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Payables, Organization.OH_IsCreditorInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Receivables, Organization.OH_IsDebtorInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Broker, Organization.OH_IsBrokerInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Competitor, Organization.OH_IsCompetitorInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Consignee, Organization.OH_IsConsigneeInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Consignor, Organization.OH_IsConsignorInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Forwarder, Organization.OH_IsForwarderInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Services, Organization.OH_IsMiscFreightServicesInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Sales, Organization.OH_IsSalesLeadInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Carrier, Organization.OH_IsShippingProviderInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.TransportClient, Organization.OH_IsTransportClientInfo);
			TestAlgorithmAndOrgTypes(OrgCodeOrgTypeDescription.Warehouse, Organization.OH_IsWarehouseClientInfo);
		}

		void TestAlgorithmAndOrgTypes(string selectedOrgType, ZPropertyInfo propertyInfo)
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.SelectableOrgTypes[selectedOrgType].Selected = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters());

			propertyInfo.Value = ZBool.True;
			AssertEquals("Generated Code", "TES", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
			AssertEquals("Algorithm's Second Name Order", (ZByte)0, generator.GetAlgorithm(new OrgHeaderOrgCodeInfo(Organization)).Elements.First(e => e.Description == OrgCodeElementDescription.SecondName).Order);

			propertyInfo.Value = ZBool.False;
			AssertEquals("Generated Code", "TESORGZZZ", generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
			AssertEquals("Algorithm's Second Name Order", (ZByte)2, generator.GetAlgorithm(new OrgHeaderOrgCodeInfo(Organization)).Elements.First(e => e.Description == OrgCodeElementDescription.SecondName).Order);
		}

		public void TestCodeSpecificUniqueNumber()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 5;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			AssertEquals("Generated Code", "TEORGZX001", GetCode());

			Factory.Save();
			AssertEquals("Saved Org Code", "TEORGZX001", Organization.OH_Code);
			AssertEquals("Generated Code for existing org", "TEORGZX001", GetCode(Organization));
			var anotherOrg = Factory.New<OrgHeader>();
			anotherOrg.OH_RL_NKClosestPort = Unloco.RL_Code;
			anotherOrg.OH_FullName = Organization.OH_FullName;
			AssertEquals("Generated Code new org", "TEORGZX002", GetCode(anotherOrg));
		}

		public void TestDuplicateCodes()
		{
			string code = GetCode();
			AssertEquals("Generated Code", "TESORGZZZ", GetCode());
			Organization.OH_Code = code;

			OrgHeader anotherOrganization = Factory.New<OrgHeader>();
			anotherOrganization.OH_RL_NKClosestPort = Unloco.RL_Code;
			anotherOrganization.OH_FullName = Organization.OH_FullName;
			AssertEquals("Generated Code", "TESORGZZZ1", GetCode(anotherOrganization));
		}

		/// <summary>
		/// This is to keep compatibility with the old behavior.
		/// </summary>
		public void TestEmptyCodeIsGeneratedIfNameOrUnlocoIsEmpty()
		{
			Organization.OH_FullName = "";
			Organization.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Generated Code", "", Generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_FullName = "ABC";
			Organization.OH_RL_NKClosestPort = "";
			AssertEquals("Generated Code", "", Generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
		}

		public void TestGenerateCodeOnlyIfAlgorithmTypeApplies()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Broker].Selected = true;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			IOrgCode code;

			Organization.OH_IsBroker = true;
			AssertEquals("GenerateCodeOnlyIfAlgorithmTypeApplies()", true, Generator.GenerateCodeOnlyIfAlgorithmTypeApplies(OrgCodeAlgorithmType.Override, new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory), out code));
			AssertEquals("Generated Code", "TES", code.GetProposedCode());

			AssertEquals("GenerateCodeOnlyIfAlgorithmTypeApplies()", false, Generator.GenerateCodeOnlyIfAlgorithmTypeApplies(OrgCodeAlgorithmType.Default, new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory), out code));
			AssertNull("Generated Code", code);

			Organization.OH_IsBroker = false;
			AssertEquals("GenerateCodeOnlyIfAlgorithmTypeApplies()", false, Generator.GenerateCodeOnlyIfAlgorithmTypeApplies(OrgCodeAlgorithmType.Override, new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory), out code));
			AssertNull("Generated Code", code);

			AssertEquals("GenerateCodeOnlyIfAlgorithmTypeApplies()", true, Generator.GenerateCodeOnlyIfAlgorithmTypeApplies(OrgCodeAlgorithmType.Default, new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory), out code));
			AssertEquals("Generated Code", "TESORGZZZ", code.GetProposedCode());
		}

		public void TestGlobalAndNationalAccount()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 2;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 4;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			Organization.OH_IsGlobalAccount = true;
			AssertEquals("Generated Code", "TE_WW", Generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_IsGlobalAccount = false;
			AssertEquals("Generated Code", "TEZXZZZZXZZZ", Generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());

			Organization.OH_IsNationalAccount = true;
			AssertEquals("Generated Code", "TE_ZX", Generator.GenerateCode(new OrgHeaderOrgCodeInfo(Organization), new OrgCodeDbProxy(Organization.Factory)).GetProposedCode());
		}

		public void TestGloballySpecificUniqueNumber()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 2;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 2;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			AssertEquals("Generated Code", "ZZZTEZXZZZ01", GetCode());

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			try
			{
				Env.NumberFountains.OrgCodeNumberFountain.SetNext(Factory, 10);
				AssertEquals("Generated Code", "ZZZTEZXZZZ10", GetCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestIataCode()
		{
			AssertEquals("Generated Code", "TESORGZZZ", GetCode());
			Unloco.RL_IATA = "ABC";
			AssertEquals("Generated Code", "TESORGABC", GetCode());
		}

		public void TestNameElements()
		{
			Organization.OH_FullName = "Apple Bon Strawberry";

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 1;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 5;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 2;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			AssertEquals("Generated Code", "APPBONZZZST", GetCode());

			Organization.OH_IsNationalAccount = true;
			AssertEquals("Generated Code", "ABONSTRA_ZX", GetCode());
		}

		public void TestExcludingCityAndCountry()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "KOJI China ABC Pty Ltd";

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Generated Code", "KOJCHISYD", GetCode(org));

			org.OH_RL_NKClosestPort = "CNSHA";
			AssertEquals("Generated Code", "KOJCHISHA", GetCode(org));

			org.OH_FullName = "KOJI Shanghai ABC Pty Ltd";

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Generated Code", "KOJSHASYD", GetCode(org));

			org.OH_RL_NKClosestPort = "CNSHA";
			AssertEquals("Generated Code", "KOJSHASHA", GetCode(org));
		}
	}
}
