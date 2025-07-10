using System;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class QuoteDocumentSupporterBaseTest : RatingTestCase
	{
		#region SetPrintModeOnDocument

		public void TestSetPrintModeOnDocument_DocumentDraftOption()
			=> TestSetPrintModeOnDocument
			(
				documentDraftOption: DraftOptionsList.Codes.Draft,
				finalMode: null,
				expectedQuoteSetFinalModeIsCalled: false,
				expectedDeliveryInstructions: true,
				expectedDocumentPrintMode: QuotationDocumentMode.Draft,
				expectedIsDraft: true,
				expectedIsDraftReadOnly: true
			);

		public void TestSetPrintModeOnDocument_DocumentFinalOption()
			=> TestSetPrintModeOnDocument
			(
				documentDraftOption: DraftOptionsList.Codes.Final,
				finalMode: null,
				expectedQuoteSetFinalModeIsCalled: false,
				expectedDeliveryInstructions: true,
				expectedDocumentPrintMode: QuotationDocumentMode.Draft,
				expectedIsDraft: false,
				expectedIsDraftReadOnly: true
			);

		public void TestSetPrintModeOnDocument_DocumentBothOption_NotFinalized()
			=> TestSetPrintModeOnDocument
			(
				documentDraftOption: DraftOptionsList.Codes.Both,
				finalMode: false,
				expectedQuoteSetFinalModeIsCalled: true,
				expectedDeliveryInstructions: true,
				expectedDocumentPrintMode: QuotationDocumentMode.Draft,
				expectedIsDraft: true,
				expectedIsDraftReadOnly: true
			);

		public void TestSetPrintModeOnDocument_DocumentBothOption_Finalized()
			=> TestSetPrintModeOnDocument
			(
				documentDraftOption: DraftOptionsList.Codes.Both,
				finalMode: true,
				expectedQuoteSetFinalModeIsCalled: true,
				expectedDeliveryInstructions: false
			);

		void TestSetPrintModeOnDocument(string documentDraftOption, bool? finalMode, bool expectedQuoteSetFinalModeIsCalled, bool expectedDeliveryInstructions, QuotationDocumentMode expectedDocumentPrintMode = QuotationDocumentMode.Unknown, bool expectedIsDraft = false, bool expectedIsDraftReadOnly = false)
		{
			var quote = GetQuote();

			if (finalMode != null)
			{
				setFinalModeResult = finalMode.Value;
			}

			quote.SetFinalMode += Quote_SetFinalMode;

			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(CargoWise.Definitions.BusinessContext.Quotation, "Cover Page"));
			command.Parent = quote;
			command.SU_DraftOption = documentDraftOption;
			var supporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			using (var task = supporter.BuildPrintTask(command))
			{
				task.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;
				supporter.RunTask(task);

				AssertEquals("quoteSetfinalModeIsCalled", expectedQuoteSetFinalModeIsCalled, quoteSetfinalModeIsCalled);

				if (expectedDeliveryInstructions)
				{
					AssertNotNull("DeliveryInstructions", supporter.DeliveryInstructionsForTest);
					AssertEquals("DocumentPrintMode", expectedDocumentPrintMode, quote.DocumentPrintMode);
					AssertEquals("IsDraft", expectedIsDraft, supporter.DeliveryInstructionsForTest.IsDraft);
					AssertEquals("IsDraft_ReadOnly", expectedIsDraftReadOnly, supporter.DeliveryInstructionsForTest.IsDraft_ReadOnly);
				}
				else
				{
					AssertNull("DeliveryInstructions", supporter.DeliveryInstructionsForTest);
				}
			}
		}

		#endregion

		#region Delivery Instruction - Draft ReadOnly

		public void TestDeliveryInstructionIsDraftReadOnly_QuotationPack_Draft()
			=> TestDeliveryInstructionIsDraftReadOnly(menuName: "Quotation Pack", finalMode: false, expectedDeliveryInstructions: true, expectedIsDraft: true, expectedIsDraftReadOnly: true);

		public void TestDeliveryInstructionIsDraftReadOnly_QuotationPack_Final()
			=> TestDeliveryInstructionIsDraftReadOnly(menuName: "Quotation Pack", finalMode: true, expectedDeliveryInstructions: false);

		public void TestDeliveryInstructionIsDraftReadOnly_CoverPage_Draft()
			=> TestDeliveryInstructionIsDraftReadOnly(menuName: "Cover Page", finalMode: false, expectedDeliveryInstructions: true, expectedIsDraft: true, expectedIsDraftReadOnly: true);

		public void TestDeliveryInstructionIsDraftReadOnly_CoverPage_Final()
			=> TestDeliveryInstructionIsDraftReadOnly(menuName: "Cover Page", finalMode: true, expectedDeliveryInstructions: false);

		void TestDeliveryInstructionIsDraftReadOnly(string menuName, bool finalMode, bool expectedDeliveryInstructions, bool expectedIsDraft = false, bool expectedIsDraftReadOnly = false)
		{
			var quote = GetQuote();

			setFinalModeResult = finalMode;
			quote.SetFinalMode += Quote_SetFinalMode;

			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(CargoWise.Definitions.BusinessContext.Quotation, menuName));
			command.Parent = quote;
			var supporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			using (var task = supporter.BuildPrintTask(command))
			{
				task.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;
				supporter.RunTask(task);

				if (expectedDeliveryInstructions)
				{
					AssertNotNull("DeliveryInstructions", supporter.DeliveryInstructionsForTest);
					AssertEquals("DocumentPrintMode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
					AssertEquals("IsDraft", expectedIsDraft, supporter.DeliveryInstructionsForTest.IsDraft);
					AssertEquals("IsDraft_ReadOnly", expectedIsDraftReadOnly, supporter.DeliveryInstructionsForTest.IsDraft_ReadOnly);
				}
				else
				{
					AssertNull("DeliveryInstructions", supporter.DeliveryInstructionsForTest);
				}
			}
		}

		#endregion

		#region Implementation

		protected virtual Quote GetQuote() => throw new NotImplementedException();

		void Quote_SetFinalMode(object sender, EventArgs eventArgs)
		{
			((Quote.SetFinalModeArgs)eventArgs).Result = setFinalModeResult;
			quoteSetfinalModeIsCalled = true;
		}

		bool setFinalModeResult;
		bool quoteSetfinalModeIsCalled;

		#endregion
	}
}
