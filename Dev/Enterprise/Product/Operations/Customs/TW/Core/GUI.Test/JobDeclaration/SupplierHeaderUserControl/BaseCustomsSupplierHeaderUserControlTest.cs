using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CusPackingList = Enterprise.Customs.TW.Business.CusPackingList;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class BaseCustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestNetWeightDecimals()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = brokerageControl.SupplierHeaderUserControl;
						AssertEquals(3, supplierHeaderUserControl.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit").Decimals);
					}
				}
			}
		}

		public void TestExchangeRateInChargesGrid()
		{
			CombineAssertions(() =>
			{
				AssertExchangeRateInChargesGrid(TWJobMessageTypeList.Codes.Import);
				AssertExchangeRateInChargesGrid(TWJobMessageTypeList.Codes.Export);
			}

			);
		}

		public void AssertExchangeRateInChargesGrid(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
					var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
					var baseGroupChargesGrid = brokerageControl.SupplierHeaderUserControl.BaseGroupChargesGrid;
					AssertEquals("J7_ExchangeRate column is hidden in invoiceCharges When " + messageType, false, invoiceChargesGrid.GetColumnStyle("J7_ExchangeRate").IsVisible);
					AssertEquals("J7_ExchangeRate column is Visible in baseGroupCharges When " + messageType, true, baseGroupChargesGrid.GetColumnStyle("J7_ExchangeRate").IsVisible);
					AssertEquals("IsJ7_ExchangeRateUserEnterable column is hidden in invoiceCharges When " + messageType, false, invoiceChargesGrid.GetColumnStyle("IsJ7_ExchangeRateUserEnterable").IsVisible);
					AssertEquals("IsJ7_ExchangeRateUserEnterable column is Visible in baseGroupCharges When " + messageType, true, baseGroupChargesGrid.GetColumnStyle("IsJ7_ExchangeRateUserEnterable").IsVisible);
				}
			}
		}

		public void TestResetColumnsInInvoiceChargesGrid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
					var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
					AssertEquals(true, invoiceChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription).IsVisible);
					AssertNull(invoiceChargesGrid.Columns[Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription]);

					var isIncludedInInvoiceAmountColumnInfo = invoiceChargesGrid.GetColumnStyle(Enterprise.Customs.Business.BaseJobComInvHeaderCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);
					var invoiceLineTotalColumnInfo = brokerageControl.SupplierHeaderUserControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.InvoiceLineTotal);
					CombineAssertions(() =>
					{
						var isIncludedInInvoiceAmountColumnCaptionResourceString = isIncludedInInvoiceAmountColumnInfo.CaptionResourceString;
						AssertEquals("J7_Calc_IsIncludedInInvoiceAmount Caption", "Included in Inv. Amt", isIncludedInInvoiceAmountColumnCaptionResourceString.Caption);
						AssertEquals("J7_Calc_IsIncludedInInvoiceAmount ShortCaption", "Included in Inv. Amt", isIncludedInInvoiceAmountColumnCaptionResourceString.ShortCaption);
						AssertEquals("J7_Calc_IsIncludedInInvoiceAmount FullDescription", "It indicates whether the charge is included in the invoice amount. The Incoterm and charge code determine whether the charge is included by default.", isIncludedInInvoiceAmountColumnCaptionResourceString.FullDescription);

						var invoiceLineTotalColumnInfoCaptionResourceString = invoiceLineTotalColumnInfo.CaptionResourceString;
						AssertEquals("InvoiceLineTotal Caption", "Invoice Line Total", invoiceLineTotalColumnInfoCaptionResourceString.Caption);
						AssertEquals("InvoiceLineTotal ShortCaption", "Line Total", invoiceLineTotalColumnInfoCaptionResourceString.ShortCaption);
					});
				}
			}
		}

		public void TestControlsVisible()
		{
			CombineAssertions(() =>
			{
				AssertControlsVisible(Common.Shared.SharedJobMessageTypeList.Codes.Import);
				AssertControlsVisible(Common.Shared.SharedJobMessageTypeList.Codes.Export);
			}

			);
		}

		public void AssertControlsVisible(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
						var cifControl = supplierHeaderUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "JZ_CIFAmountBoundCurrencyControl");
						AssertNotNull(cifControl);
						Assert(!cifControl.Visible);
						var tniControl = supplierHeaderUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "JZ_Calc_TNIBoundInvoiceCurrencyControl");
						AssertNotNull(tniControl);
						Assert(!tniControl.Visible);

						var organizationsTab = supplierHeaderUserControl.FindSingle<ZTabPage>(c => c.Name == "OrganizationsTabPage");
						supplierHeaderUserControl.InvoiceTabControl.SelectedTab = organizationsTab;
						var supplierDocAddressControl = supplierHeaderUserControl.FindSingleOrDefault<TWJobDocAddressControl>(c => c.Name == "SupplierDocumentaryAddressControl");
						Assert(supplierDocAddressControl.Visible);
						var buyerDocAddressControl = supplierHeaderUserControl.FindSingleOrDefault<TWJobDocAddressControl>(c => c.Name == "BuyerDocumentaryAddressControl");
						Assert(buyerDocAddressControl.Visible);
					}
				}
			}
		}

		public void TestCharacterCasing()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
					var controls = brokerageControl.SupplierHeaderUserControl;
					foreach (var name in new string[] { "TW_MarksAndNumbersLongTextBox" })
					{
						var longTextTextBox = controls.FindSingleOrDefault<LongTextControl>(c => c.Name == name);
						AssertNotNull(name, longTextTextBox);
						AssertEquals(name, CharacterCasing.Normal, longTextTextBox.CharacterCasing);
					}
				}
			}
		}

		public void TestFOBAmountBoundCurrencyControlCaptionResourceString()
		{
			CombineAssertions(() =>
			{
				AssertFOBAmountBoundCurrencyControlCaptionResourceString(Common.Shared.SharedJobMessageTypeList.Codes.Import);
				AssertFOBAmountBoundCurrencyControlCaptionResourceString(Common.Shared.SharedJobMessageTypeList.Codes.Export);
			}

			);
		}

		public void AssertFOBAmountBoundCurrencyControlCaptionResourceString(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
						var fobCaptionResourceString = supplierHeaderUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "JZ_FOBAmountBoundCurrencyControl")?.CaptionResourceString;
						AssertNotNull(fobCaptionResourceString);
						switch (messageType)
						{
							case Common.Shared.SharedJobMessageTypeList.Codes.Import:
								AssertEquals("CIF", fobCaptionResourceString.Caption);
								break;
							case Common.Shared.SharedJobMessageTypeList.Codes.Export:
								AssertEquals("FOB", fobCaptionResourceString.Caption);
								break;
							default:
								AssertEquals("VFD", fobCaptionResourceString.Caption);
								AssertEquals("VFD", fobCaptionResourceString.ShortCaption);
								AssertEquals("Value for Duty", fobCaptionResourceString.FullDescription);
								break;
						}
					}
				}
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			foreach (var messageType in new ZString[] { "EXP", "IMP" })
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
					using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
					{
						if (jobDeclarationUserControl != null)
						{
							brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
							var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
							var baseGroupChargesGrid = brokerageControl.SupplierHeaderUserControl.BaseGroupChargesGrid;
							var apportionedChargesGrid = brokerageControl.SupplierHeaderUserControl.ApportionedChargesGrid;
							var jobComInvoiceHeadersBoundGrid = brokerageControl.SupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid;
							AssertEquals("The J7_IsGSTApplicable column on invoiceChargesGrid is Unavailable when import", messageType == "IMP", invoiceChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);
							Assert("The J7_IsGSTApplicable column should be visible on BaseGroupChargesGrid", baseGroupChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsVisible);
							Assert("The J7_IsGSTApplicable column should be visible on ApportionedChargesGrid", apportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsVisible);
							AssertNull("Invoice Header Supplier is not relevant for TW", jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_OH_Supplier]);
							AssertNull("Invoice Header Supplier Name is not relevant for TW", jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.SupplierName]);
							AssertNull("Invoice Header Importer is not relevant for TW", jobComInvoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_OH_Buyer]);
							AssertEquals("The TW_MarksAndNumbers column should be visibility on ComInvoiceHeadersBoundGrid", true, jobComInvoiceHeadersBoundGrid.GetColumnStyle("TW_MarksAndNumbers").IsVisible);
							CombineAssertions("JobComInvoiceHeadersBoundGrid default display columns", () =>
							{
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceNumber).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTerm).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTermPlace).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceDate).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceAmount).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.InvoiceLineTotal).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.TW_MarksAndNumbers).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Weight).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_WeightUQ).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NetWeight).IsVisible);
								Assert(jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NetWeightUQ).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_ChargesExcludedFromITOT).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NoOfPacks).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.NoOfPacksPackType).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentNo).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentAmount).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentDate).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentExRate).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceDisplaySequence).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Volume).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_VolumeUQ).IsVisible);
								Assert(!jobComInvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OrderNumber).IsVisible);
							}

							);
						}
					}
				}
			}
		}

		public void TestIsGSTApplicableCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var invoiceChargesGrid = brokerageControl.SupplierHeaderUserControl.InvoiceChargesGrid;
						var baseGroupChargesGrid = brokerageControl.SupplierHeaderUserControl.BaseGroupChargesGrid;
						var apportionedChargesGrid = brokerageControl.SupplierHeaderUserControl.ApportionedChargesGrid;
						AssertEquals("Caption of J7_IsGSTApplicable column is 'Add to MoF?'", "Add to MoF?", invoiceChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
						AssertEquals("Caption of J7_IsGSTApplicable column is 'Add to MoF?'", "Add to MoF?", baseGroupChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
						AssertEquals("Caption of J7_IsGSTApplicable column is 'Add to MoF?'", "Add to MoF?", apportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
					}
				}
			}
		}

		public void TestJobComInvoiceHeadersBoundGridColumnsTypes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = brokerageControl.SupplierHeaderUserControl;
						AssertType(typeof(ZMultiLineTextBoxColumnStyle), supplierHeaderUserControl.InvoiceHeadersBoundGrid.Columns.FirstOrDefault(t => t.ColumnName == JobComInvoiceHeader.Schema.TW_MarksAndNumbers).ColumnStyle);
						var calcEditColumnStyleInfo = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NoOfPacks) as ZCalcEditColumnStyleInfo;
						AssertEquals(0, calcEditColumnStyleInfo.Decimals);
					}
				}
			}
		}

		public void TestBindingMembers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = brokerageControl.SupplierHeaderUserControl;
						var bingdingSource = supplierHeaderUserControl.BindingSource;
						var marksAndNumbersLongTextBox = supplierHeaderUserControl.FindSingleOrDefault<LongTextControl>(c => c.Name == "TW_MarksAndNumbersLongTextBox");
						AssertEquals("BindingMember", "Invoices.TW_MarksAndNumbers", bingdingSource.GetBindingMember(marksAndNumbersLongTextBox));
					}
				}
			}
		}

		public void TestControlAnchor()
		{
			CombineAssertions(() =>
			{
				AssertChargesGroupsControlAnchor(Common.Shared.SharedJobMessageTypeList.Codes.Import);
				AssertChargesGroupsControlAnchor(Common.Shared.SharedJobMessageTypeList.Codes.Export);
			}

			);
		}

		public void AssertChargesGroupsControlAnchor(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
						var chargesGroupBoxControl = supplierHeaderUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "ChargesGroupBox");
						AssertEquals(AnchorStyles.Top | AnchorStyles.Left, chargesGroupBoxControl.Anchor);
						var baseGroupChargesGroupBoxControl = supplierHeaderUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "BaseGroupChargesGroupBox");
						AssertEquals(AnchorStyles.Top | AnchorStyles.Left, baseGroupChargesGroupBoxControl.Anchor);
						var leftBottomPanel = supplierHeaderUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "LeftBottomPanel");
						AssertEquals(AnchorStyles.Top | AnchorStyles.Left, leftBottomPanel.Anchor);
						var rightBottomPanel = supplierHeaderUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "RightBottomPanel");
						AssertEquals(AnchorStyles.Top | AnchorStyles.Left, rightBottomPanel.Anchor);
					}
				}
			}
		}

		public void TestPanel2MinSize()
		{
			CombineAssertions(() =>
			{
				AssertPanel2MinSize(Common.Shared.SharedJobMessageTypeList.Codes.Import);
				AssertPanel2MinSize(Common.Shared.SharedJobMessageTypeList.Codes.Export);
			}

			);
		}

		public void AssertPanel2MinSize(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
						var supplierHeaderUserControl = (BaseCustomsSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
						var splitContainer = supplierHeaderUserControl.FindSingleOrDefault<KSplitContainer>(c => c.Name == "Splitter");
						AssertEquals(374, splitContainer.Panel2MinSize);
					}
				}
			}
		}

		public void TestIncoTermExplainButtonVisible()
		{
			using (var control = new BaseCustomsSupplierHeaderUserControl())
			{
				var incoTermExplainButton = control.FindSingleOrDefault<ZButton>(c => c.Name == "IncoTermExplainButton");
				AssertEquals("Do not display IncoTermExplainButton in TW Customs.", false, incoTermExplainButton.Visible);
			}
		}

		public void TestPackingListMenuItemCaption()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;

				var invoiceHeaderUserControl = form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var boundGrid = invoiceHeaderUserControl.InvoiceHeadersBoundGrid;

				var packingListSeparatorMenuItem = boundGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "packingListMenuItemSeparatorMenuItem");
				var packingListMenuItem = boundGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "packingListMenuItem");
				AssertNotNull(packingListSeparatorMenuItem);
				AssertNotNull(packingListMenuItem);

				PopupMenu(boundGrid.ContextMenu, EventArgs.Empty);
				AssertEquals("Create Packing List", packingListMenuItem.Text);

				var packingList = Factory.New<CusPackingList>();
				packingList.CUL_JZ = header.PK;
				Factory.Save();

				PopupMenu(boundGrid.ContextMenu, EventArgs.Empty);
				AssertEquals("Edit Packing List", packingListMenuItem.Text);
			}
		}

		void PopupMenu(ContextMenu menuItem, EventArgs e)
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { e });
		}

		public void TestCreatePackingListOnlyAfterInvoiceHeaderSaved()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			var header = declaration.Invoices.AddNew();
			var query = new ZQuery(CusPackingListSchema.CUL_JZ, header.PK);
			var packingList = Factory.LoadTop1<CusPackingList>(query);
			AssertNull(packingList);
			using (var testControl = new BaseCustomsSupplierHeaderUserControl())
			using (var form = new ZForm())
			{
				testControl.BindingSource.DataSource = declaration;
				form.Controls.Add(testControl);
				form.Show();

				var headersGrid = testControl.InvoiceHeadersBoundGrid;
				headersGrid.Select(0);
				PopupMenu(headersGrid.ContextMenu, EventArgs.Empty);
				var createPackingListItem = headersGrid.ContextMenu.MenuItems.FindByText("Create Packing List");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackingListItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(packingList);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				PopupMenu(headersGrid.ContextMenu, EventArgs.Empty);
				createPackingListItem.PerformClick();
				var openedForm = (testControl.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				entity.PackageJob.Packages.First().KP_MarksAndNumbers = "marks and numbers";
				openedForm.FireSaveButton();
				packingList = Factory.LoadTop1<CusPackingList>(query);
				AssertNotNull(packingList);
				(testControl.LastController.LastShownForm as ZForm).Close();
			}
		}

		public void TestCreatePackingListAddsDefaultPackage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			declaration.Invoices.AddNew();
			Factory.Save();

			using (var testControl = new BaseCustomsSupplierHeaderUserControl())
			using (var form = new ZForm())
			{
				testControl.BindingSource.DataSource = declaration;
				form.Controls.Add(testControl);
				form.Show();

				var headersGrid = testControl.InvoiceHeadersBoundGrid;
				headersGrid.Select(0);
				PopupMenu(headersGrid.ContextMenu, EventArgs.Empty);
				var createPackingListItem = headersGrid.ContextMenu.MenuItems.FindByText("Create Packing List");
				createPackingListItem.PerformClick();
				var openedForm = (testControl.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				AssertEquals(1, entity.PackageJob.Packages.Count);
				(testControl.LastController.LastShownForm as ZForm).Close();
			}
		}
	}
}
