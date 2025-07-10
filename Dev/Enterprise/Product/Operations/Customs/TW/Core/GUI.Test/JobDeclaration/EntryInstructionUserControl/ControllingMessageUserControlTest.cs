using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(ControllingMessageUserControl))]
	public sealed class ControllingMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEmpty_TAC_MIF_CusSupportingInfoShouldNotBeCreatedWhileSavingControllingMessageHeader()
		{
			var orgHeaderDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderDeclarantCusCode = orgHeaderDeclarant.CustomsCodes.AddNew();
			orgHeaderDeclarantCusCode.FillWithValidTestData();
			orgHeaderDeclarantCusCode.OK_CodeType = "VAT";
			orgHeaderDeclarantCusCode.OK_RN_NKCodeCountry = "TW";
			orgHeaderDeclarantCusCode.OK_CustomsRegNo = "96944490";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();
			var extPassword1 = Factory.New<Business.GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_OA_DeclarantAddress = orgHeaderDeclarant.MainAddress.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			Factory.Save();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			if (jobDeclarationUserControl != null)
			{
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				var entryInstruction = declaration.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes._20;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
				var newFactory = NewFactory();
				var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber);
				var typeApprovalCertificateNumberInDB = newFactory.Load(typeof(CusSupportingInfo), query);
				Assert("Should not have the TAC CusSupportingInfo.", !typeApprovalCertificateNumberInDB.Any());
				query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
				query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier);
				var medicalInstrumentPartyIdentifierInDB = newFactory.Load(typeof(CusSupportingInfo), query);
				Assert("Should not have the MIF CusSupportingInfo.", !medicalInstrumentPartyIdentifierInDB.Any());
			}
		}

		public void TestControlsVisibleAndDefaultColumnsInSortOrder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			if (jobDeclarationUserControl != null)
			{
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				var controllingMessageDetailsGrid = controllingMessageUserControl.FindSingleOrDefault<ZGrid>(x => x.Name == "ControllingMessageDetailsGrid");
				AssertNotNull(controllingMessageDetailsGrid);
				var columnNameList = new List<string>(new string[] { "TW1_Sequence", "TW1_FunctionalReferenceId", "TW1_ControllingMessageType", "ControllingMessageTypeDescription", "TW1_ControllingAgency", "TW1_CertificateType", "TW1_BusinessType", "TW1_ProcessingUnit", "TW1_PaymentMethod", "PermitNumber", "TW1_AppointmentDate", "TW1_AppointmentPeriod", "TW1_RequestDescription" });
				foreach (var columnName in columnNameList)
				{
					var columnInfo = controllingMessageDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull(columnInfo);
					AssertEquals($"The column name '{columnInfo.ColumnName}' in grid should be visibility", columnInfo.IsVisible, true);
				}

				for (var i = 0; i < columnNameList.Count; i++)
				{
					var column = controllingMessageDetailsGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = columnNameList[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestAssignCMHeaderToInvoicesActionMenuItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var controllingMessageDetailsGrid = controllingMessageUserControl.FindSingleOrDefault<ZGrid>(x => x.Name == "ControllingMessageDetailsGrid");
			controllingMessageDetailsGrid.ListManager.Position = 0;
			var contextMenu = controllingMessageDetailsGrid.ContextMenu;
			var assignCMHeaderToInvoicesActionMenuItem = contextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Assign CM Header to Invoice Lines");
			PopupMenu(contextMenu, EventArgs.Empty);
			AssertEquals("Precondition: AssignCMHeaderToInvoices Menu Item should be visible", true, assignCMHeaderToInvoicesActionMenuItem.Visible);
			controllingMessageDetailsGrid.SetCurrentHitTestForTest(0, 0);
			PopupMenu(contextMenu, EventArgs.Empty);
			AssertEquals("AssignCMHeaderToInvoices Menu Item should be enabled", true, assignCMHeaderToInvoicesActionMenuItem.Enabled);
			assignCMHeaderToInvoicesActionMenuItem.PerformClick();
			Assert(invoiceLine1.HasLinkedCMHeader);
			Assert(invoiceLine2.HasLinkedCMHeader);
			Assert(invoiceLine1.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader.PK).IsLinkedCMHeader);
			Assert(invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader.PK).IsLinkedCMHeader);
			controllingMessageDetailsGrid.SetCurrentHitTestForTest(1, 0);
			PopupMenu(contextMenu, EventArgs.Empty);
			AssertEquals("AssignCMHeaderToInvoices Menu Item should be disabled", false, assignCMHeaderToInvoicesActionMenuItem.Enabled);

			void PopupMenu(ContextMenu menuItem, EventArgs e)
			{
				typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { e });
			}
		}

		public void TestLocalProcessorOrganisationControl()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			AssertEquals(1, controllingMessageUserControl.FindAll<TWJobDocAddressControl>(x => x.Name == "LocalProcessorAddressControl").Count());
		}

		public void TestInvoiceLinesGridColumnsWhenDeclarationIsImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageUserControl.ControllingMessageTabControl.SelectedTab = controllingMessageUserControl.FindSingle<ZTabPage>("LinesTabPage");
			var invoiceLinesGrid = controllingMessageUserControl.FindSingle<ZGrid>("InvoiceLinesGrid");

			CombineAssertions(() =>
			{
				AssertEquals(70, invoiceLinesGrid.Columns.Count);
				AssertEquals(71, invoiceLinesGrid.ColumnStyles.Count);

				var bondedGoodsCodeColumnStyleInfo = invoiceLinesGrid.GetColumnStyle(ControllingMessageHeaderLinkInvoiceLine.Schema.BondedGoodsCode);
				AssertEquals(true, bondedGoodsCodeColumnStyleInfo.IsUnavailable);

				var procedureColumnStyleInfo = invoiceLinesGrid.GetColumnStyle(ControllingMessageHeaderLinkInvoiceLine.Schema.Procedure);
				AssertEquals("Duty Treatment", procedureColumnStyleInfo.CaptionResourceString.Caption);
			});

			IEnumerable<(string ColumnName, bool IsMandatory, bool IsVisible)> expectedColumnSettings = new List<(string ColumnName, bool IsMandatory, bool IsVisible)>
				{
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Link, true, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceSequence, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceNumber, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceLineSequence, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceQuantity, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceQuantityUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.UnitPrice, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.LinePrice, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Tariff, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CountryOfOrigin, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Model, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.BrandName, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PrimaryPreference, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Procedure, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.RH_NKCommodity_Code, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Transmission, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartNo, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.OrderNumber, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.MatchingKey, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Calc_OrderLineNumberAndSubLine, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.TariffAdditionalCode, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.IsClassUsageCommentRead, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NKClassUsageCommentReviewer, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.ClassUsageComment, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Displacement, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Group, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Description, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.VatPymntMthd, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.DtyPymntMthd, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NDescription, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Volume, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.VolumeUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Weight, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.WeightUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSecondQuantity, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSecondQuantityUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedAdValoremDutyRate, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedSpecificDutyRate, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib4, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib5, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib6, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NetWeight, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NetWeightUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSupplierPartNo, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CitesPermit, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomTextBlob1, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CusValueConvRatio, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.LHD, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.AlcoholPercentage, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.HighTechLicense, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.HasCatalystConverter, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CarCondition, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CarType, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Cylinders, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.EngineType, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Gears, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.ModelYear, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NumberOfDoor, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Seats, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.TariffDescription, false, true)
				};

			foreach (var columnSetting in expectedColumnSettings)
			{
				var columnStyleInfo = invoiceLinesGrid.GetColumnStyle(columnSetting.ColumnName);
				AssertEquals(columnSetting.ColumnName + " is mandatory.", columnSetting.IsMandatory, columnStyleInfo.IsMandatory);
				AssertEquals(columnSetting.ColumnName + " is visible by default.", columnSetting.IsVisible, columnStyleInfo.IsVisible);
			}
		}

		public void TestInvoiceLinesGridColumnsWhenDeclarationIsExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageUserControl.ControllingMessageTabControl.SelectedTab = controllingMessageUserControl.FindSingle<ZTabPage>("LinesTabPage");
			var invoiceLinesGrid = controllingMessageUserControl.FindSingle<ZGrid>("InvoiceLinesGrid");

			CombineAssertions(() =>
			{
				AssertEquals(49, invoiceLinesGrid.Columns.Count);
				AssertEquals(71, invoiceLinesGrid.ColumnStyles.Count);
				var procedureColumnStyleInfo = invoiceLinesGrid.GetColumnStyle(ControllingMessageHeaderLinkInvoiceLine.Schema.Procedure);
				AssertEquals("Mode of Statistics", procedureColumnStyleInfo.CaptionResourceString.Caption);
			});

			IEnumerable<(string ColumnName, bool IsMandatory, bool IsVisible)> expectedColumnSettings = new List<(string ColumnName, bool IsMandatory, bool IsVisible)>
				{
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Link, true, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceSequence, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceNumber, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceLineSequence, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceQuantity, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.InvoiceQuantityUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.UnitPrice, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.LinePrice, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Tariff, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CountryOfOrigin, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Model, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.BrandName, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Procedure, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.RH_NKCommodity_Code, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartNo, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.MatchingKey, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Calc_OrderLineNumberAndSubLine, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.IsClassUsageCommentRead, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NKClassUsageCommentReviewer, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.ClassUsageComment, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Group, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Description, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NDescription, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Volume, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.VolumeUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.Weight, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.WeightUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSecondQuantity, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSecondQuantityUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib4, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib5, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomAttrib6, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NewPartAttribute3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib1, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib2, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.PartAttrib3, false, false),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NetWeight, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.NetWeightUQ, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomsSupplierPartNo, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CustomTextBlob1, false, true),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.TariffDescription, false, true)
				};

			foreach (var columnSetting in expectedColumnSettings)
			{
				var columnStyleInfo = invoiceLinesGrid.GetColumnStyle(columnSetting.ColumnName);
				AssertEquals(columnSetting.ColumnName + " should be mandatory.", columnSetting.IsMandatory, columnStyleInfo.IsMandatory);
				AssertEquals(columnSetting.ColumnName + " should be visible by default.", columnSetting.IsVisible, columnStyleInfo.IsVisible);
			}

			IEnumerable<string> expectedUnavailableColumns = new List<string>
				{
					ControllingMessageHeaderLinkInvoiceLine.Schema.CitesPermit,
					ControllingMessageHeaderLinkInvoiceLine.Schema.HighTechLicense,
					ControllingMessageHeaderLinkInvoiceLine.Schema.TariffAdditionalCode,
					ControllingMessageHeaderLinkInvoiceLine.Schema.AlcoholPercentage,
					ControllingMessageHeaderLinkInvoiceLine.Schema.CusValueConvRatio,
					ControllingMessageHeaderLinkInvoiceLine.Schema.CarType,
					ControllingMessageHeaderLinkInvoiceLine.Schema.Transmission,
					ControllingMessageHeaderLinkInvoiceLine.Schema.EngineType,
					ControllingMessageHeaderLinkInvoiceLine.Schema.LHD,
					ControllingMessageHeaderLinkInvoiceLine.Schema.HasCatalystConverter,
					ControllingMessageHeaderLinkInvoiceLine.Schema.CarCondition,
					ControllingMessageHeaderLinkInvoiceLine.Schema.PrimaryPreference,
					ControllingMessageHeaderLinkInvoiceLine.Schema.ModelYear,
					ControllingMessageHeaderLinkInvoiceLine.Schema.Displacement,
					ControllingMessageHeaderLinkInvoiceLine.Schema.NumberOfDoor,
					ControllingMessageHeaderLinkInvoiceLine.Schema.Cylinders,
					ControllingMessageHeaderLinkInvoiceLine.Schema.Seats,
					ControllingMessageHeaderLinkInvoiceLine.Schema.Gears,
					ControllingMessageHeaderLinkInvoiceLine.Schema.DtyPymntMthd,
					ControllingMessageHeaderLinkInvoiceLine.Schema.VatPymntMthd,
					ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedAdValoremDutyRate,
					ControllingMessageHeaderLinkInvoiceLine.Schema.FormattedSpecificDutyRate
				};

			foreach (var unavailableColumns in expectedUnavailableColumns)
			{
				var columnStyleInfo = invoiceLinesGrid.GetColumnStyle(unavailableColumns);
				AssertEquals(columnStyleInfo.ColumnName + " should be unavailable.", true, columnStyleInfo.IsUnavailable);
			}
		}

		public void TestInvoiceLinesGridColumnDecimals()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageUserControl.ControllingMessageTabControl.SelectedTab = controllingMessageUserControl.FindSingle<ZTabPage>("LinesTabPage");
			var invoiceLinesGrid = controllingMessageUserControl.FindSingle<ZGrid>("InvoiceLinesGrid");

			IEnumerable<(string ColumnName, int Decimals)> expectedColumnDecimals = new List<(string ColumnName, int Decimals)>
				{
					(ControllingMessageHeaderLinkInvoiceLine.Schema.LinePrice, 2),
					(ControllingMessageHeaderLinkInvoiceLine.Schema.CusValueConvRatio, 4),
				};

			foreach (var columnDecimals in expectedColumnDecimals)
			{
				var columnStyleInfo = invoiceLinesGrid.Columns[columnDecimals.ColumnName];
				AssertEquals("The decimals of " + columnDecimals.ColumnName + " should be " + columnDecimals.Decimals + ".", columnDecimals.Decimals, ((ZCalcEditColumnStyle)columnStyleInfo.ColumnStyle).Decimals);
			}
		}

		public void TestCertificateOfOriginTabPageVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var certificateOfOriginTabPage = controllingMessageUserControl.FindSingle<ZTabPage>("CertificateOfOriginTabPage");
			AssertEquals(true, certificateOfOriginTabPage.TabVisible);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertEquals(false, certificateOfOriginTabPage.TabVisible);
		}

		public void TestECFAPrintedRemarksTextBoxEnable()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var certificateOfOriginTabPage = controllingMessageUserControl.FindSingle<ZTabPage>("CertificateOfOriginTabPage");
			var ecfaPrintedRemarksTextBox = certificateOfOriginTabPage.FindSingle<ZTextBox>("ECFAPrintedRemarksTextBox");
			AssertEquals(true, ecfaPrintedRemarksTextBox.Enabled);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			AssertEquals(false, ecfaPrintedRemarksTextBox.Enabled);
		}

		public void TestApplicantDocumentaryAddressControl()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
				var control = controllingMessageUserControl.FindSingle<TWJobDocAddressControl>(x => x.Name == "ApplicantDocumentaryAddressControl");

				AssertEquals("Code boxes should not be visible", false, control.CodeBoxesVisible);
			});
		}

		public void TestSupplierDocumentaryAddressControl()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
				var control = controllingMessageUserControl.FindSingle<TWJobDocAddressControl>(x => x.Name == "SupplierDocumentaryAddressControl");

				AssertEquals("Code boxes should not be visible", false, control.CodeBoxesVisible);
			});
		}

		public void TestImporterDocumentaryAddressControl()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var controllingMessageHeader = controllingMessageHeaders.AddNew();
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
				var control = controllingMessageUserControl.FindSingle<TWJobDocAddressControl>(x => x.Name == "ImporterDocumentaryAddressControl");

				AssertEquals("Code boxes should not be visible", false, control.CodeBoxesVisible);
			});
		}

		public void TestNotesTextBox()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var certificateOfOriginTabPage = controllingMessageUserControl.FindSingle<ZTabPage>("CertificateOfOriginTabPage");
			var notesGroupBox = certificateOfOriginTabPage.FindSingle<ZGroupBox>("NotesGroupBox");
			AssertEquals("Notes", notesGroupBox.CaptionResourceString.Caption);
			var notesTextBox = certificateOfOriginTabPage.FindSingle<ZTextBox>("NotesTextBox");
			Assert(notesTextBox.Enabled);
		}

		public void TestControllingMessageDetailsGridColumns()
		{
			var expectedNotDisplayedColumnsImport = new string[]
			{
					CusTWControllingMessageHeader.Schema.TW1_CertificateType,
					CusTWControllingMessageHeader.Schema.TW1_IsEstimatedLoadingDate,
					CusTWControllingMessageHeader.Schema.TW1_IsSpecialApplication,
					CusTWControllingMessageHeader.Schema.TW1_SpecialApplicationId,
					CusTWControllingMessageHeader.Schema.TW1_OriginalQuantity,
					CusTWControllingMessageHeader.Schema.TW1_CopyQuantity,
					CusTWControllingMessageHeader.Schema.TW1_EUSteelProductNo,
					CusTWControllingMessageHeader.Schema.TW1_EUSteelProductPhase,
					CusTWControllingMessageHeader.Schema.TW1_ManufacturerPrintingCode,
					CusTWControllingMessageHeader.Schema.TW1_ReturnPreviousCOO,
					CusTWControllingMessageHeader.Schema.TW1_BeforeClearanceApplicationReason,
					CusTWControllingMessageHeader.Schema.TW1_PrintingCode,
					CusTWControllingMessageHeader.Schema.TW1_IsTriangularTrade
			};

			var expectedNotDisplayedColumnsExport = new string[]
			{
					CusTWControllingMessageHeader.Schema.BulkApplicationID,
					CusTWControllingMessageHeader.Schema.TW1_PortOfBulkCommodity,
					CusTWControllingMessageHeader.Schema.BulkPaymentID,
					CusTWControllingMessageHeader.Schema.PermitNoExpirationDate,
					CusTWControllingMessageHeader.Schema.TW1_ElectronicReceipt,
					CusTWControllingMessageHeader.Schema.TW1_ApplyForSampleReturn,
					CusTWControllingMessageHeader.Schema.TW1_InspectionRegistrationNumber,
					CusTWControllingMessageHeader.Schema.TW1_PreWineInspectionStatus,
					CusTWControllingMessageHeader.Schema.TW1_Purpose,
					CusTWControllingMessageHeader.Schema.TW1_SampleReturnAddress,
					CusTWControllingMessageHeader.Schema.TW1_SamplingReductionReason,
			};

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			var controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			var controllingMessageDetailsGrid = controllingMessageUserControl.FindSingle<ZGrid>("ControllingMessageDetailsGrid");

			CombineAssertions("Import", () =>
			{
				foreach (var columnName in expectedNotDisplayedColumnsImport)
				{
					Assert($"{columnName} should be unavailable.", controllingMessageDetailsGrid.GetColumnStyle(columnName).IsUnavailable);
				}

				foreach (var columnName in expectedNotDisplayedColumnsExport)
				{
					Assert($"{columnName} should be available.", !controllingMessageDetailsGrid.GetColumnStyle(columnName).IsUnavailable);
				}
			});

			brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
			controllingMessageUserControl = brokerageControl.EntryInstructionDetailsTabPage.Controls.OfType<ControllingMessageUserControl>().First();
			controllingMessageDetailsGrid = controllingMessageUserControl.FindSingle<ZGrid>("ControllingMessageDetailsGrid");
			CombineAssertions("Export", () =>
			{
				foreach (var columnName in expectedNotDisplayedColumnsExport)
				{
					Assert($"{columnName} should be unavailable.", controllingMessageDetailsGrid.GetColumnStyle(columnName).IsUnavailable);
				}

				foreach (var columnName in expectedNotDisplayedColumnsImport)
				{
					Assert($"{columnName} should be available.", !controllingMessageDetailsGrid.GetColumnStyle(columnName).IsUnavailable);
				}
			});
		}
	}
}
