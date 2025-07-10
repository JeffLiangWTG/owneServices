using System;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatingHeaderDocumentSupporter))]
	internal sealed class RatingHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		#region GetDataStateBeforeRun

		public void TestGetDataStateBeforeRun_Quote() => TestGetDataStateBeforeRun(Helper.NewQuote(Helper.NewOrgHeader()));

		public void TestGetDataStateBeforeRun_ClientRate() => TestGetDataStateBeforeRun(Helper.NewClientRate(Helper.NewOrgHeader()));

		public void TestGetDataStateBeforeRun_Costing() => TestGetDataStateBeforeRun(Helper.NewCosting(Helper.NewOrgHeader()));

		public void TestGetDataStateBeforeRun_CompanyTariff() => TestGetDataStateBeforeRun(Helper.NewCompanyTariff());

		void TestGetDataStateBeforeRun(IDocumentSupportable documentSupportable)
		{
			var supporter = documentSupportable.DocumentSupporter;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Test Quote Document";

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = ".ClientRate";

			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = menu.PK;
			templatePivot.SI_SO = template.PK;

			Factory.Save();

			AssertEquals
			(
				"The data context .ClientRate is invalid. Please check your template, amend the data context, reload the template and try again.",
				supporter.GetDataStateBeforeRun(menu).ErrorMessage
			);
		}

		#endregion

		public void TestDeliverQuotationPack()
		{
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = ZBool.True;
			quote.TH_GC = GlbCompany.CurrentCompany.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = quote.PK;

			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = job.PK;

			var query = new DocumentZQuery("Quotation", "Quotation Pack");
			var documentCommand = Factory.LoadTop1<DocumentCommand>(query);
			var documentDeliveryJob = new MockAutoDocumentDeliveryJob(quote, documentCommand.PK);

			var notifications = new DummyNotifications();

			documentDeliveryJob.Deliver(notifications);

			const string expected = @"
documentDeliveryJob.LastDocumentPrintSetForDelivery[0]
   Cover Page
   One Off Pricing Page
   Acceptance Page
   Contacts Page
   Recommended Agents Page
";

			AssertMultilineASCIIEquals("", expected, documentDeliveryJob.LastDocumentsDelivered);
		}

		[GuiTest]
		public void TestDeliverPricingPage()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line.TL_RateDesc = "Packing";

			var calc = (UnitCalculator)line.Calculator;
			calc.PerUnit = 500;

			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Printer";

			Factory.Save();

			var supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)tariff).DocumentSupporter;

			var task = new PrintTaskForTesting();

			supporter.RunTask(task);

			AssertEquals("should pass instructions", true, task.instructionsWerePassed);
			AssertEquals("should exclude empty documents", true, task.passedInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows);
		}

		public void TestHideFilterValue()
		{
			var tariff = Factory.New<CompanyTariff>() as IDocumentSupportable;
			AssertEquals("N", tariff.DocumentSupporter.GetFilterValue(DocumentFilters.HIDE));
		}

		#region Implementation

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ClientRate>();
		}

		class DummyNotifications : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		[Serializable]
		class MockAutoDocumentDeliveryJob : AutoDocumentDeliveryJob
		{
			internal string LastDocumentsDelivered;

			internal MockAutoDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK)
				: base(businessObject, false, documentCommandPK)
			{
			}

			protected override void OnDeliveredBeforeDocumentPrintSetDisposed(DocumentPrintSet documentPrintSet)
			{
				base.OnDeliveredBeforeDocumentPrintSetDisposed(documentPrintSet);

				var builder = new StringBuilder();
				builder.AppendLine();
				for (var i = 0; i < documentPrintSet.Count; i++)
				{
					var docpack = documentPrintSet[i];

					builder.Append("documentDeliveryJob.LastDocumentPrintSetForDelivery[");
					builder.Append(i);
					builder.AppendLine("]");

					foreach (IDeliverable deliverable in docpack)
					{
						builder.Append(' ', 3);
						builder.AppendLine(deliverable.Name);
					}
				}
				LastDocumentsDelivered = builder.ToString();
			}
		}

		class PrintTaskForTesting : PrintTask
		{
			public DeliveryInstructions passedInstructions;
			public bool instructionsWerePassed;

			public override DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				instructionsWerePassed = true;
				passedInstructions = deliveryInstructions;
				return base.RunWithPartialInstructions(deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint);
			}
		}

		#endregion
	}
}
