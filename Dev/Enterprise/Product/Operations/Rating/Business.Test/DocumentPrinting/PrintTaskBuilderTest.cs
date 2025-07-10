using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class PrintTaskBuilderTest : RatingTestCase
	{
		#region Other Documents

		#region System Document Menu

		public void TestOtherDocument_SystemDocumentMenu_WithOtherDocument()
			=> TestOtherDocument_SystemDocumentMenu
			(
				systemDocumentMenuName: "Quotation Pack",
				otherDocumentName: "Index Page",
				expectedPrintTaskCount: 1,
				expectedDocuments: new[]
				{
					"Cover Page",
					"Acceptance Page",
					"Contacts Page",
					"Recommended Agents Page",
					"One Off Pricing Page"
				},
				message: "System DocumentMenu does not support OtherDocument yet"
			);

		public void TestOtherDocument_SystemDocumentMenu_WithNoOtherDocument()
			=> TestOtherDocument_SystemDocumentMenu
			(
				systemDocumentMenuName: "Quotation Pack",
				otherDocumentName: string.Empty,
				expectedPrintTaskCount: 1,
				expectedDocuments: new[]
				{
					"Cover Page",
					"Acceptance Page",
					"Contacts Page",
					"Recommended Agents Page",
					"One Off Pricing Page"
				},
				message: "System DocumentMenu does not support OtherDocument yet"
			);

		void TestOtherDocument_SystemDocumentMenu(string systemDocumentMenuName, string otherDocumentName, int expectedPrintTaskCount, IEnumerable<string> expectedDocuments, string message = default)
		{
			var documentMenuQuery = new DocumentZQuery("Quotation", systemDocumentMenuName);
			var documentMenu = Factory.LoadTop1<DocumentCommand>(documentMenuQuery);
			TestOtherDocument(documentMenu, otherDocumentName, expectedPrintTaskCount, expectedDocuments, message);
		}

		#endregion

		#region Non System Document Menu

		public void TestOtherDocument_NonSystemDocumentMenu_WithTemplateAndOtherDocument()
			=> TestOtherDocument_NonSystemDocumentMenu
			(
				templateName: "Quotation Cover Page",
				otherDocumentName: "One Off Pricing Page",
				expectedPrintTaskCount: 2,
				expectedDocuments: new[]
				{
					"Quotation Cover Page",
					"One Off Pricing Page"
				}
			);

		public void TestOtherDocument_NonSystemDocumentMenu_WithTemplateAndNoOtherDocument()
			=> TestOtherDocument_NonSystemDocumentMenu
			(
				templateName: "Quotation Cover Page",
				otherDocumentName: string.Empty,
				expectedPrintTaskCount: 1,
				expectedDocuments: new[]
				{
					"Quotation Cover Page",
				}
			);

		public void TestOtherDocument_NonSystemDocumentMenu_WithNoTemplateAndOtherDocument()
			=> TestOtherDocument_NonSystemDocumentMenu
			(
				templateName: string.Empty,
				otherDocumentName: "One Off Pricing Page",
				expectedPrintTaskCount: 1,
				expectedDocuments: new[]
				{
					"One Off Pricing Page"
				}
			);

		public void TestOtherDocument_NonSystemDocumentMenu_WithNoTemplateAndNoOtherDocument()
			=> TestOtherDocument_NonSystemDocumentMenu
			(
				templateName: string.Empty,
				otherDocumentName: string.Empty,
				expectedPrintTaskCount: 0,
				expectedDocuments: Array.Empty<string>()
			);

		void TestOtherDocument_NonSystemDocumentMenu(string templateName, string otherDocumentName, int expectedPrintTaskCount, IEnumerable<string> expectedDocuments, string message = default)
		{
			var documentMenu = Factory.New<DocumentCommand>();

			if (!string.IsNullOrEmpty(templateName))
			{
				var template = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.Equal, templateName));
				var templatePivot = Factory.New<StmMenuTemplatePivot>();
				templatePivot.SI_DocumentTitle = templateName;
				templatePivot.SI_SO = template.PK;
				templatePivot.SI_SU = documentMenu.PK;
				templatePivot.SI_IsSystemDefined = ZBool.True;
			}

			TestOtherDocument(documentMenu, otherDocumentName, expectedPrintTaskCount, expectedDocuments, message);
		}

		void TestOtherDocument(DocumentCommand documentMenu, string otherDocumentName, int expectedPrintTaskCount, IEnumerable<string> expectedDocuments, string message = default)
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "USLAX", "FRT", 100m);

			documentMenu.Parent = quote;

			var job = new Job.Loader(quote).TryCreate();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = NewClient.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 5;

			if (!string.IsNullOrEmpty(otherDocumentName))
			{
				var commandFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, otherDocumentName);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Optional Pages");
				var otherDocument = Factory.LoadTop1<DocumentCommand>(commandFilter);

				var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
				stmMenuMenuPivot.SF_SU_Inward = documentMenu.PK;
				stmMenuMenuPivot.SF_SU_Outward = otherDocument.PK;
			}

			var supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			var printTask = supporter.BuildPrintTask(documentMenu);

			CombineAssertions(message, () =>
			{
				AssertEquals("PrintTask Count", expectedPrintTaskCount, printTask?.Count ?? 0);
				AssertContainsExactElementsInAnyOrder("Documents", expectedDocuments, GetDocumentNames(printTask));
			});
		}

		#endregion

		static IEnumerable<string> GetDocumentNames(PrintTask task)
		{
			var totalTask = task?.Count ?? 0;

			for (var i = 0; i < totalTask; i++)
			{
				foreach (IDeliverable deliverable in task[i])
				{
					if (deliverable.IncludedInPrint)
					{
						yield return deliverable.Name;
					}
				}
			}
		}

		#endregion

		public void TestInitialDataIsDiscarded()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var task = new PrintTask(null);
			task.Add(new DocumentPack());
			AssertEquals(1, task.Count);
			var pack1 = task[0];

			new PrintTaskBuilder(quote).BuildPrintTask(task);
			AssertEquals(1, task.Count);
			var pack2 = task[0];
			AssertNotEquals(pack1, pack2);
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var task = new PrintTask(null);
			task.Add(new DocumentPack());
			AssertEquals(1, task.Count);

			new PrintTaskBuilder(quote).BuildPrintTask(task);
			AssertEquals(1, task.Count);

			var filter = new DocumentZQuery(((IDocumentSupportable)quote).DocumentSupporter.BusinessContext, Constants.MenuNameConstantsForPrinting.QuotationPack);
			var menuItem = new BusinessObjectFactory().LoadTop1<DocumentCommand>(filter);
			AssertEquals("Task delivery instructions PK", menuItem.PK, task.DeliveryInstructionsDefaultPK);
		}

		public void TestTrailingPages()
		{
			Env.Registry.Rating.SetQuoteTermsAndConditionsPages(new Image[] { new Bitmap(100, 100), null, null, null, null, new Bitmap(101, 101), null, null, null, null });

			var set = Factory.LoadTop1<RateAttachmentSet>(new ZQuery(RateAttachmentSetSchema.TS_AttachmentName, "Published Agents"));
			set.TS_Sequence = 20;

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			SelectPages(quote, "Forwarding Standard Pricing Page", "Trailing Page 1", "Trailing Page 6", "Published Agents");

			AssertEquals(4, quote.SelectedPages.Count);
			AssertEquals("Forwarding Standard Pricing Page", quote.SelectedPages[0].TA_RateAttachmentName);
			AssertEquals("Trailing Page 1", quote.SelectedPages[1].TA_RateAttachmentName);
			AssertEquals("Trailing Page 6", quote.SelectedPages[2].TA_RateAttachmentName);
			AssertEquals("Published Agents", quote.SelectedPages[3].TA_RateAttachmentName);

			var pack = new PrintTaskBuilder(quote).BuildDocumentPack(null);
			AssertEquals(3, pack.Count);
			AssertEquals("Forwarding Standard Pricing Page", pack[0].Name);
			AssertEquals("Terms And Conditions", pack[1].Name);
			AssertEquals("Recommended Agents Page", pack[2].Name);
		}

		public void TestStandardPricingPage()
		{
			const string expected1 = @"
Forwarding Standard Pricing Page
";

			const string expected2 = @"
Forwarding Standard Pricing Page
Shipping Standard Pricing Page
";

			PrintTask task;

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.AddRateEntry(RatingConstants.RateCategory.FCL);
			quote.AddRateEntry(RatingConstants.RateCategory.SCO);

			SelectPages(quote, "Forwarding Standard Pricing Page");

			task = new PrintTask(null);
			new PrintTaskBuilder(quote).BuildPrintTask(task);

			AssertMultilineASCIIEquals("", expected1, RenderDocumentNames(task[0]));

			SelectPages(quote, "Shipping Standard Pricing Page", "Forwarding Standard Pricing Page");

			task = new PrintTask(null);
			new PrintTaskBuilder(quote).BuildPrintTask(task);

			AssertMultilineASCIIEquals("", expected2, RenderDocumentNames(task[0]));
		}

		public void TestGenericWrappers_Header()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blaticus Template";
			template.SO_DataContext = nameof(DataContext.GenericFreightJob);
			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{B}-[<JobNumber>]
{A}-[#EndOfReport]
");

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "Blaticus Menu";
			command.SU_BusinessContext = nameof(BusinessContext.Quotation);

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "Blaticus";

			var set = Factory.New<RateAttachmentSet>();
			set.TS_AttachmentName = "Blaticus Set";
			set.TS_SU = command.PK;

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_QuoteNumber = "Quote";
			SelectPages(testQuote, "Blaticus Set");

			using (var task = new PrintTask(null))
			{
				new PrintTaskBuilder(testQuote).BuildPrintTask(task);
				AssertNotNull(task);
				AssertEquals(1, task.Count);
				AssertEquals(1, task[0].Count);
				AssertEquals("Blaticus", task[0][0].Name);

				using (var stream = new MemoryStream())
				{
					var report = (Report)task[0][0];
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", "{B}-[Quote]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestPrintQuotationPackDoNotAddTempPivot()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.AddRateEntry(RatingConstants.RateCategory.FCL);
			quote.AddRateEntry(RatingConstants.RateCategory.SCO);
			SelectPages(quote, "Shipping Standard Pricing Page", "Forwarding Standard Pricing Page", "Published Agents");
			Factory.Save();

			using (var task = new PrintTask(null))
			{
				new PrintTaskBuilder(quote).BuildPrintTask(task);
				using (var stream = new MemoryStream())
				{
					foreach (Report report in task[0])
					{
						report.Save(stream);
					}
				}
			}

			quote.AddRateEntry(RatingConstants.RateCategory.AIR);
			Factory.Save();

			var emptyNamePivots = Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, ""));
			AssertEquals(0, emptyNamePivots.Length);
		}

		public void TestDocumentTitleUsesCorrectRegistryValue()
		{
			Env.Instance.Registry.Rating.QuoteTitleText = "regularQuote";
			Env.Instance.Registry.Rating.OneOffQuoteTitleText = "oneOffQuote";
			var documentNameTemplate = "Eagle Datamation International - BN - AUBNE - {0} - Q001234567 - Test Client #1";
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_QuoteNumber = "Q001234567";

			testQuote.TH_OneTimeQuote = false;
			var docPack = new PrintTaskBuilder(testQuote).BuildDocumentPack(null);
			AssertEquals("The document name should be the same as expected", string.Format(documentNameTemplate, "regularQuote"), docPack.EmailSubjectForConsolidateReports);

			testQuote.TH_OneTimeQuote = true;
			docPack = new PrintTaskBuilder(testQuote).BuildDocumentPack(null);
			AssertEquals("The document name should be the same as expected", string.Format(documentNameTemplate, "oneOffQuote"), docPack.EmailSubjectForConsolidateReports);
		}

		public void TestEmailSubjectUsesDocumentsRegistry()
		{
			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));

			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_QuoteNumber = "Q000123456";
			var docPack = new PrintTaskBuilder(quote).BuildDocumentPack(null);
			var expectedEmailSubject = "Quotation - Q000123456 - Test Client #1";

			AssertEquals(expectedEmailSubject, docPack.EmailSubjectForConsolidateReports);

			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("2", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));

			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			docPack = new PrintTaskBuilder(quote).BuildDocumentPack(null);
			expectedEmailSubject = string.Format("Quotation - Q000123456 - {0} - Test Client #1", GlbCompany.CurrentCompany.GC_Name);

			AssertEquals(expectedEmailSubject, docPack.EmailSubjectForConsolidateReports);
		}

		#region Implementation

		string RenderDocumentNames(DocumentPack pack)
		{
			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (IDeliverable deliverable in pack)
			{
				builder.AppendLine(deliverable.Name);
			}

			return builder.ToString();
		}

		static void SelectPages(Quote quote, params string[] pageNames)
		{
			var sets = quote.Factory.Load<RateAttachmentSet>(new ZQuery(RateAttachmentSetSchema.TS_AttachmentName, pageNames));

			quote.SelectedPages.RemoveAndDeleteAll();
			quote.SelectedPages.AddPagesToSelectedPagesCollection(sets);
		}

		#endregion
	}
}
