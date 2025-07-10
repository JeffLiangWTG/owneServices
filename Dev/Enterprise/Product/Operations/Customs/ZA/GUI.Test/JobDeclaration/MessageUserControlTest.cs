using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class MessageUserControlTest : TestCaseWithFactory
	{
		public void TestSetupEntryHeaderColumns()
		{
			using (var userControl = new MessageUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("User control should have CH_BGMReference column with caption LRN", "LRN", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference).CaptionResourceString.Caption);
					AssertEquals("User control should have PackagesCount column unavailable", true, userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.PackagesCount).IsUnavailable);
					AssertEquals("User control should have EntryNumber column unavailable", true, userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).IsUnavailable);
					var valueAddedTaxColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.ValueAddedTax);
					AssertNotNull("User control should have ValueAddedTax column", valueAddedTaxColumn);
					AssertEquals("ValueAddedTax Column is readonly", true, valueAddedTaxColumn.IsReadOnly);
					var customsDutyColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CustomsDuty);
					AssertNotNull("User control should have CustomsDuty column", customsDutyColumn);
					AssertEquals("CustomsDuty Column is readonly", true, customsDutyColumn.IsReadOnly);
					AssertNotNull("User control should have CH_PaymentMethod column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_PaymentMethod));
					AssertNotNull("User control should have CH_Packages column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_Packages));
					AssertNotNull("User control should have CH_EntryNumber column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryNumber));
					AssertNotNull("User control should have CH_TotalEntries column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TotalEntries));
					var entryStatusColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus);
					AssertNotNull("User control should have CH_EntryStatus column", entryStatusColumn);
					AssertEquals("CH_EntryStatus Column is readonly", true, entryStatusColumn.IsReadOnly);
					var customsProcedureCodeColumn = (ZTextBoxColumnStyleInfo)userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CustomsProcedureCode);
					AssertNotNull("User control should have CustomsProcedureCode column", customsProcedureCodeColumn);
					AssertEquals("CustomsProcedureCode Column is readonly", true, customsProcedureCodeColumn.IsReadOnly);
					AssertEquals("CustomsProcedureCode Column is mandatory", true, customsProcedureCodeColumn.IsMandatory);
					AssertEquals("CustomsProcedureCode Column max length of 8", 8, customsProcedureCodeColumn.MaxLengthOverride);
					var customsProcedureInstructionDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CustomsProcedureInstructionDescription);
					AssertNotNull("User control should have CustomsProcedureInstructionDescription column", customsProcedureInstructionDescriptionColumn);
					AssertEquals("CustomsProcedureInstructionDescription Column is readonly", true, customsProcedureInstructionDescriptionColumn.IsReadOnly);
					var entryReleaseDateColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate);
					AssertNotNull("User control should have CH_EntryReleaseDate column", entryReleaseDateColumn);
					AssertEquals("CH_EntryReleaseDate Column is readonly", true, entryReleaseDateColumn.IsReadOnly);
					var movementReferenceNumberColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumber);
					AssertNotNull("User control should have MovementReferenceNumber column", movementReferenceNumberColumn);
					AssertEquals("MovementReferenceNumber Column is readonly", true, movementReferenceNumberColumn.IsReadOnly);
					AssertEquals("MovementReferenceNumber Column is Uppercase", CharacterCasing.Upper, movementReferenceNumberColumn.CharacterCasing);
					var uniqueConsignmentReferenceColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.UniqueConsignmentReference);
					AssertNotNull("User control should have UniqueConsignmentReference column", uniqueConsignmentReferenceColumn);
					AssertEquals("UniqueConsignmentReference Column is readonly", true, uniqueConsignmentReferenceColumn.IsReadOnly);
					AssertEquals("UniqueConsignmentReference Column is Uppercase", CharacterCasing.Upper, uniqueConsignmentReferenceColumn.CharacterCasing);
					var messageTypeColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType);
					AssertNotNull("User control should have CH_MessageType column", messageTypeColumn);
					AssertEquals("CH_MessageType Column is readonly", true, messageTypeColumn.IsReadOnly);
					var messageTypeDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageTypeDescription);
					AssertNotNull("User control should have CH_MessageTypeDescription column", messageTypeDescriptionColumn);
					AssertEquals("CH_MessageTypeDescription Column is readonly", true, messageTypeDescriptionColumn.IsReadOnly);
					var entrySubmittedDateColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntrySubmittedDate);
					AssertNotNull("User control should have CH_EntrySubmittedDate column", entrySubmittedDateColumn);
					AssertEquals("CH_EntrySubmittedDate Column is readonly", true, entrySubmittedDateColumn.IsReadOnly);
					var entryHeaderStatusDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					AssertNotNull("User control should have EntryHeaderStatusDescription column", entryHeaderStatusDescriptionColumn);
					AssertEquals("EntryHeaderStatusDescription Column is readonly", true, entryHeaderStatusDescriptionColumn.IsReadOnly);
					AssertEquals("EntryHeaderStatusDescription Column is Uppercase", CharacterCasing.Upper, entryHeaderStatusDescriptionColumn.CharacterCasing);
					var statusColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_Status);
					AssertNotNull("User control should have CH_Status column", statusColumn);
					AssertEquals("CH_Status Column is readonly", true, statusColumn.IsReadOnly);
					var messageStatusDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MessageStatusDescription);
					AssertNotNull("User control should have MessageStatusDescription column", messageStatusDescriptionColumn);
					AssertEquals("MessageStatusDescription Column is readonly", true, messageStatusDescriptionColumn.IsReadOnly);
					AssertEquals("MessageStatusDescription Column is Uppercase", CharacterCasing.Upper, messageStatusDescriptionColumn.CharacterCasing);
					AssertNotNull("User control should have CH_BondValidToDate column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BondValidToDate));
					AssertNotNull("User control should have CH_BondAcquittedDate column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BondAcquittedDate));
					AssertNotNull("User control should have CH_RelPrintInd column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_RelPrintInd));
				});
			}
		}

		public void TestChangeControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			MenuItem reqdocRoot;
			MenuItem regenLRN;
			using (var form = new ZForm())
			using (var userControl = new MessageUserControlForTest())
			{
				form.Controls.Add(userControl);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ProcedureCodes._40;
				var invHeader = declaration.Invoices.AddNew();
				var invLine = invHeader.JobComInvoiceLines.AddNew();
				invLine.JI_CEI = instruction.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
				Factory.Save();
				new LineMerger(declaration).DoMerge();
				Factory.Save();
				var header = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
				userControl.SetDataBinding(declaration, "");
				form.Show();
				reqdocRoot = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Customs Resend of Responses", false);
				Assert(reqdocRoot.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
				userControl.SetDataBinding(declaration, "");
				form.Show();
				reqdocRoot = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Customs Resend of Responses", false);
				Assert(!reqdocRoot.Visible);
				userControl.SelectedSingleEntry = header;
				form.Show();
				regenLRN = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Regenerate new LRN", false);
				Assert(regenLRN.Visible);
			}

			AssertExceptionThrown(typeof(NullReferenceException), () =>
			{
				reqdocRoot.Visible = false;
			});
			AssertNoExceptionThrown(() =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			});
		}

		public void TestRegenLRN_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			MenuItem regenLRN;
			using (var form = new ZForm())
			using (var userControl = new MessageUserControlForTest())
			{
				form.Controls.Add(userControl);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_CustomsOffice = "ABC";
				declaration.AgentCode = "123";
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ProcedureCodes._40;
				var invHeader = declaration.Invoices.AddNew();
				var invLine = invHeader.JobComInvoiceLines.AddNew();
				invLine.JI_CEI = instruction.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
				Factory.Save();
				new LineMerger(declaration).DoMerge();
				Factory.Save();
				var header = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
				userControl.SetDataBinding(declaration, "");
				userControl.SelectedSingleEntry = header;
				form.Show();
				regenLRN = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Regenerate new LRN", false);
				regenLRN.PerformClick();
				Assert(header.NeedsNewBGMReference);
				header.MovementReferenceNumberSetter("123");
				form.Show();
				regenLRN = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Regenerate new LRN", false);
				regenLRN.PerformClick();
				Assert(!header.NeedsNewBGMReference);
			}
		}

		sealed class MessageUserControlForTest : Customs.GUI.ImportMessageUserControl
		{
			public MessageUserControlForTest()
			{
				reqdocRoot = new ZMenuItem("Request Customs Resend of Responses");
				regenLRN = new ZMenuItem("Regenerate new LRN");
				var rsqMessagesQuery = new ZQuery(EDIMessageSchema.EM_MessageType, SARSEDIMessage.MessageTypes.CUSRES_REQDOC);
				rsqMessagesQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
				EntriesBoundGrid.ContextMenu.MenuItems.Add(reqdocRoot);
				EntriesBoundGrid.ContextMenu.Popup += (object sender, EventArgs e) =>
				{
					RecDocHelper.RefreshReqdocList(EntriesBoundGrid, reqdocRoot);
					SetMenusEntryOnPopup();
				};
				EntriesBoundGrid.ContextMenu.MenuItems.Add(regenLRN);
				regenLRN.Click += RegenLRN_Click;
				EntriesBoundGrid.ContextMenu.MenuItems.Add(regenLRN);
				InitializeGridLayout();
				ExtendedInfoGroupBox.Visible = false;
			}

			void RegenLRN_Click(object sender, EventArgs e)
			{
				if (SelectedSingleEntry != null)
				{
					if (SelectedSingleEntry.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected)
					{
						SelectedSingleEntry.NeedsNewBGMReference = true;
						Globals.Message.Show("A new LRN will be generated when you save this form.");
						SelectedSingleEntry.HasChanges = true;
					}
					else
					{
						SelectedSingleEntry.NeedsNewBGMReference = false;
						Globals.Message.Show("A new LRN cannot be generated.");
					}
				}
			}

			public void SetMenusEntryOnPopup()
			{
				var clickedEntry = (CusEntryHeader)(EntriesBoundGrid.GetFirstSelectedRow() ?? EntriesBoundGrid.List?.Cast<BusinessObject>().FirstOrDefault());
				if (clickedEntry != null)
				{
					regenLRN.Visible = true;
				}
				else
				{
					regenLRN.Visible = false;
				}

				SelectedSingleEntry = clickedEntry;
			}

			protected override void OnAfterFirstBinding(EventArgs e)
			{
				base.OnAfterFirstBinding(e);
				if (Declaration != null)
				{
					Declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
					JE_MessageTypeInfo_ValueChanged(this, null);
				}
			}

			void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
			{
				if (!IsDisposed)
				{
					reqdocRoot.Visible = Declaration != null && !Declaration.IsImportByExternalBroker;
				}
			}

			public JobDeclaration Declaration => CurrentDataItem as JobDeclaration;

			public CusEntryHeader SelectedSingleEntry { get; set; }

			readonly MenuItem reqdocRoot;
			readonly MenuItem regenLRN;
		}
	}
}
