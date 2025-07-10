using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(InstructionConfiguration))]
sealed class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
{
	public void TestEntryInstructionValidationDecider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertNull(configuration.GetValidationDecider(entryInstruction));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<UCC6ExportEntryInstructionValidationDecider>(configuration.GetValidationDecider(entryInstruction));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertNull(configuration.GetValidationDecider(entryInstruction));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertNull(configuration.GetValidationDecider(entryInstruction));
		}
	}

	public override void TestFiscalReferencesSupport()
	{
		var declaration = CreateDeclaration();
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("IsUCC6 IMP", true, configuration.FiscalReferencesSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertEquals("IsUCC6 EXP", false, configuration.FiscalReferencesSupport(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				AssertEquals("!IsUCC6", false, configuration.FiscalReferencesSupport(declaration));
			}
		});
	}

	public override void TestFiscalReferencesSupportOnCPC42And63Only() => AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));

	public override void TestAuthorisationsSupport() => AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));

	public override void TestAdditionalSupplyChainActorSupport()
	{
		var declaration = CreateDeclaration();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("Export", true, configuration.AdditionalSupplyChainActorSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Import", true, configuration.AdditionalSupplyChainActorSupport(declaration));
		});
	}

	public override void TestGuaranteesSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(false, configuration.GuaranteesSupport(declaration, entryInstruction));
	}

	public override void TestSealsSupport()
	{
		CombineAssertions(() =>
		{
			var declaration = CreateDeclaration();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("SealsSupport", false, configuration.SealsSupport(declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("SealsSupport", false, configuration.SealsSupport(declaration));
		});
	}

	public override void TestUseEoriForAuthorisationReference() => AssertEquals(false, configuration.UseEoriForAuthorisationReference);

	public override void TestAdditionalInfosSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
	}

	public override void TestSupportingDocumentsSupport() => AssertEquals(true, configuration.SupportingDocumentsSupport(CreateDeclaration()));

	public override void TestPreviousDocumentsSupport() => AssertEquals(true, configuration.PreviousDocumentsSupport(CreateDeclaration()));

	public override void TestRequestedDocumentsSupport() => AssertEquals(false, configuration.RequestedDocumentsSupport(CreateDeclaration()));

	public override void TestSpecialProceduresSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals(false, configuration.SpecialProceduresSupport(declaration, entryInstruction));
			entryInstruction.CEI_Style = "H1";
			AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
			entryInstruction.CEI_Style = "H3";
			AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
			entryInstruction.CEI_Style = "H4";
			AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
		});
	}

	public void TestGetCusAuthorizationUsageValidationDecider()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertType<UCC6ImportCusAuthorizationUsageValidationDecider>("IsUCC6 IMP", configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction));
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertType<UCC6ExportCusAuthorizationUsageValidationDecider>("IsUCC6 EXP", configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertNull("!IsUCC6", configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction));
		}
	}
}
