using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5167MessageSendingObject))]
	sealed class N5167MessageSendingObject_DateProvider_Tests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new N5167MessageSendingObject(entryHeader);
		}

		[ExpectNoExceptions]
		public void TestData()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			messageSending.Header.Declaration.JE_OH_Importer = importerOrg.PK;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageSending.Header, NUnit.Framework.Is.TypeOf<CusEntryHeader>(), "messageSending.Header");
				NUnit.Framework.Assert.That(provider.DeclarationOfficeID, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison), "DeclarationOfficeID");
				NUnit.Framework.Assert.That(provider.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("<<FUNCTIONAL REFERENCE ID PLACE HOLDER>>").Using(CustomComparers.TypeComparison), "FunctionalReferenceID");
				NUnit.Framework.Assert.That(provider.Agent, NUnit.Framework.Is.TypeOf<N5167.PartyDetails>(), "Agent");
				NUnit.Framework.Assert.That(provider.Consignment, NUnit.Framework.Is.EqualTo(default(IConsignment)), "Consignment - should be [null]");
				NUnit.Framework.Assert.That(provider.GoodsShipment, NUnit.Framework.Is.TypeOf<GoodsShipment>(), "GoodsShipment");
				NUnit.Framework.Assert.That(provider.Importer, NUnit.Framework.Is.TypeOf<N5167.PartyDetails>(), "Importer");
			});
		}

		[ExpectNoExceptions]
		public void TestGetMessageOwner()
		{
			var classification = Factory.NewWithValidTestData<Customs.Business.BaseCusClassification>();
			classification.CC_Description = "XXXXXXX1Q11";
			classification.CC_LookupCode = "CF12DS";
			classification.CC_TariffNum = "1000.00.00.00Y";
			var buyerOrg = Factory.NewWithValidTestData<OrgHeader>();
			buyerOrg.OH_FullName = "PPPP21222";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "BBBB-CCC";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_CustomsOffice = "AA";
			declaration.JE_OH_Buyer = buyerOrg.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
			entryInstruction.CEI_Style = "G1";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CC = classification.PK;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = classification.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var messageSending = new N5167MessageSendingObject(entryHeader);
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "B333333", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(messageSending.GetMessageOwner(), NUnit.Framework.Is.EqualTo("B333333").Using(CustomComparers.TypeComparison));
			Factory.Save();
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "B111111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(messageSending.GetMessageOwner(), NUnit.Framework.Is.EqualTo("B111111").Using(CustomComparers.TypeComparison));
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "B222222", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(messageSending.GetMessageOwner(), NUnit.Framework.Is.EqualTo("B222222").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dataTestHelper = new N5167DataTestHelper(Factory);
			provider = dataTestHelper.Provider;
			messageSending = dataTestHelper.messageSending;
		}

		IN5167Declaration provider;
		N5167MessageSendingObject messageSending;
	}
}
