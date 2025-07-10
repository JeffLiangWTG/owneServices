using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Rating.Business.Quote;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(QuoteDocumentSupporter))]
	public class QuoteDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSeaOnlyCFX()
		{
			var testQuote = Factory.New<Quote>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			testQuote.TH_OH = org.PK;

			testQuote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			testQuote.AddRateEntry(RatingConstants.RateCategory.WHS, Core.Constants.RateMode.ALL, "", "");

			testQuote.TH_AirCFX = 0.41;
			testQuote.TH_SeaCFX = 0.42;
			testQuote.TH_ExportAirCFX = 0.43;
			testQuote.TH_ExportSeaCFX = 0.44;

			testQuote.TryAcceptQuote(out var _);

			Factory.Save();

			var savedCfxList = testQuote.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().ToList();

			AssertEquals(2, savedCfxList.Count);
			Assert("Only SEA cfx should be saved", savedCfxList.All(cfx => cfx.JCF_TransportMode == Core.Constants.RateMode.SEA));
			Assert("Job type is SHP for each record", savedCfxList.All(cfx => cfx.JCF_JobType == "SHP"));

			var importCfx = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "IMP");
			AssertEquals(0.42m, importCfx.JCF_CFXPercentage);

			var exportCfx = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "EXP");
			AssertEquals(0.44m, exportCfx.JCF_CFXPercentage);
		}

		public void TestSeaAndAirCFX()
		{
			var testQuote = Factory.New<Quote>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			testQuote.TH_OH = org.PK;
			testQuote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			testQuote.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			testQuote.AddRateEntry(RatingConstants.RateCategory.WHS, Core.Constants.RateMode.ALL, "", "");

			testQuote.TH_AirCFX = 0.41;
			testQuote.TH_SeaCFX = 0.42;
			testQuote.TH_ExportAirCFX = 0.43;
			testQuote.TH_ExportSeaCFX = 0.44;

			testQuote.TryAcceptQuote(out var _);

			Factory.Save();

			var savedCfxList = testQuote.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().ToList();

			AssertEquals(4, savedCfxList.Count);
			AssertEquals("SEA cfx should be saved", 2, savedCfxList.Count(cfx => cfx.JCF_TransportMode == Core.Constants.RateMode.SEA));
			AssertEquals("AIR cfx should be saved", 2, savedCfxList.Count(cfx => cfx.JCF_TransportMode == Core.Constants.RateMode.AIR));
			Assert("Job type is SHP for each record", savedCfxList.All(cfx => cfx.JCF_JobType == "SHP"));

			var cfxImpAir = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "IMP" && cfx.JCF_TransportMode == "AIR");
			AssertEquals(0.41m, cfxImpAir.JCF_CFXPercentage);

			var cfxExpAir = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "EXP" && cfx.JCF_TransportMode == "AIR");
			AssertEquals(0.43m, cfxExpAir.JCF_CFXPercentage);

			var cfxImpSea = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "IMP" && cfx.JCF_TransportMode == "SEA");
			AssertEquals(0.42m, cfxImpSea.JCF_CFXPercentage);

			var cfxExpSea = savedCfxList.FirstOrDefault(cfx => cfx.JCF_ServiceDirection == "EXP" && cfx.JCF_TransportMode == "SEA");
			AssertEquals(0.44m, cfxExpSea.JCF_CFXPercentage);
		}

		public void TestGetOverriddenDeliveryDetails()
		{
			var testQuote = Factory.New<Quote>();
			var docSupporter = (QuoteDocumentSupporter)((IDocumentSupportable)testQuote).DocumentSupporter;
			AssertNull(docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));

			testQuote.TH_OneTimeQuote = true;
			AssertNull(docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));

			testQuote.QuotationClientAddress.E2_AddressOverride = true;
			AssertEquals(testQuote.QuotationClientAddress, docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));

			testQuote.QuotationClientAddress.E2_Contact = "NEW PERSON";
			AssertEquals(testQuote.QuotationClientAddress, docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));

			testQuote.QuotationClientAddress.E2_Contact = "";
			AssertEquals(testQuote.QuotationClientAddress, docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));

			testQuote.QuotationClientAddress.E2_Email = "zayden@example.com";
			AssertEquals(testQuote.QuotationClientAddress, docSupporter.GetOverriddenDeliveryDetails("", ContactType.All, DocumentDirection.ANY));
		}

		public void TestPrintFinalQuoteMakesQuoteReadOnly()
		{
			var testQuote = Factory.New<Quote>();
			var testEntry = testQuote.AddRateEntry("AIR");
			((QuoteDocumentSupporter)((IDocumentSupportable)testQuote).DocumentSupporter).LockAndSaveQuote();

			AssertEquals("Quote is now locked", true, testQuote.TH_IsLocked);
			AssertEquals("Quote is saved", false, testQuote.HasChanges);
			AssertEquals("Quote is readonly", true, testQuote.ReadOnly);
			AssertEquals("Quote is readonly", true, testEntry.ReadOnly);
		}

		public void TestPrintFinalQuoteNotSaveTemporaryStmMenuTemplatePivots()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Printer";
			Factory.Save();

			Env.Security.QuotationApprove.IsAllowed = true;
			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			quote.TH_QuoteNumber = "Q10000";

			var entry = quote.AddRateEntry("LCL", "LCL", "THBKK", "AUBNE");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.CN);
			((MinimumOrPerUnitCalculator)line.Calculator).Minimum = 20;
			((MinimumOrPerUnitCalculator)line.Calculator).PerUnit = 20;

			Factory.Save();

			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(CargoWise.Definitions.BusinessContext.Quotation, "Quotation Pack"));
			command.Parent = quote;
			var menuTemplatePivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, command.PK);
			var numberOfPivotsBeforePrinting = Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot), menuTemplatePivotQuery);

			var supporter = (QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			using (var task = supporter.BuildPrintTask(command))
			{
				task.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;

				var deliveryInstructions = new DeliveryInstructions(task[0]);
				deliveryInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows = true;
				deliveryInstructions.PrinterDelivery.PrintQueuePK = printer.PK;

				task.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);

				supporter.LockAndSaveQuote();
				Factory.Save();
			}

			var numberOfPivotsAfterPrinting = Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot), menuTemplatePivotQuery);
			AssertEquals(numberOfPivotsBeforePrinting, numberOfPivotsAfterPrinting);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuPath.Contains("Optional Pages", StringComparison.InvariantCultureIgnoreCase) || base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var result = Factory.New<Quote>();

			var entry = result.WHSRateEntriesForBinding.AddNew();
			entry.FillWithValidTestData();

			var line = entry.RateLines.AddNew();
			line.FillWithValidTestData();

			line.RateLineItems.AddNew();

			return result;
		}
	}

	#region QuoteProcessTasksProviderTest

	[TestedType(typeof(Quote))]
	public class QuoteWorkflowProviderTest : WorkflowProviderTest<Quote, QuotationProcessTaskCollection>
	{
		public void TestTemplateTasksAreNotCreatedForOneTimeQuotes()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = false;
			quote.HasChanges = true;

			var oneTimeQuote = Factory.New<Quote>();
			oneTimeQuote.TH_OneTimeQuote = true;
			oneTimeQuote.HasChanges = true;

			Factory.Save();

			AssertEquals("Template tasks added", 1, ((IWorkflowProvider)quote).WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT added for one time quote", 0, ((IWorkflowProvider)oneTimeQuote).WorkflowItems.Milestones.Count);
		}

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria_ForClient()
		{
			Quotation.Factory.Save();
			AssertGetTemplateFilterCriteria(Quotation.TH_OHInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		#endregion

		#region Implementation

		Quote Quotation
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.QuotationWorkflowDescriptorCode; }
		}

		protected override Quote GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObject(factory);

			return result;
		}

		#endregion
	}

	#endregion

	#region QuoteSalesRelationActivityTest

	[TestedType(typeof(Quote))]
	sealed class QuoteSalesRelationActivityTest : SalesRelationActivityTestCase<Quote>
	{
		protected override ITableSchema TableSchema
		{
			get { return RatingHeaderSchema.Instance; }
		}

		protected override Quote GetNewActivity()
		{
			return Factory.New<Quote>();
		}
	}

	#endregion

	#region Business Object TestCase

	[TestedType(typeof(Quote))]
	internal sealed class QuoteBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsSuppressedForTestDbHits => false;

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.Quote);

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}

	#endregion
}
