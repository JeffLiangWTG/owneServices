using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class MessageUserControlTest : TestCaseWithFactory
{
	public void TestAddEntryHeaderColumns_MovementReferenceNumberExpiryDate_Caption()
	{
		var columnStyle = GetGridColumnInfo(CusEntryHeader.Schema.MovementReferenceNumberExpiryDate);
		AssertEquals("Expiry Date", columnStyle.CaptionResourceString.Caption);
	}

	public void TestAddEntryHeaderColumns_CH_EntrySubmittedDate_Caption()
	{
		var columnStyle = GetGridColumnInfo(CusEntryHeader.Schema.CH_EntrySubmittedDate);
		AssertEquals("Issue Date", columnStyle.CaptionResourceString.Caption);
	}

	public void TestAddEntryHeaderColumns_CH_PhaseStatus()
	{
		var columnStyle = GetGridColumnInfo(CusEntryHeader.Schema.CH_PhaseStatus);
		AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
	}

	public void TestAddEntryHeaderColumns_CH_PhaseStatusDescription()
	{
		var columnStyle = GetGridColumnInfo(CusEntryHeader.Schema.CH_PhaseStatusDescription);
		AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
	}

	public void TestAddEntryHeaderColumns_Import_Order()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = MessageTypeList.Codes.Import;

		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var expectedOrder = new string[]
			{
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.PackagesCount,
				CusEntryHeader.Schema.Duty,
				CusEntryHeader.Schema.VAT,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.CusEntryNumberIssueDate,
				CusEntryHeader.Schema.MovementReferenceNumberExpiryDate,
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.DeclarationUCR,
				CusEntryHeader.Schema.EntryTypeFriendlyName,
				CusEntryHeader.Schema.CH_TotalPaid,
			};

			var gridOrder = userControl.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).Select(x => x.ColumnName).ToArray();
			AssertSequencesEqual("Columns should be in the expected order", expectedOrder, gridOrder);
		}
	}

	public void TestAddEntryHeaderColumns_Export_Order()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var exitHeader = Factory.New<CusExitHeader>();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		var exitReport = exitHeader.CusExitReports.AddNew();
		exitConsignment.CXC_MovementReference = "MRN123";
		exitReport.CER_CXC_Consignment = exitConsignment.PK;
		exitHeader.Parent = declaration;

		using var form = new ZForm(declaration);
		using var userControl = new MessageUserControl();
		userControl.Dock = DockStyle.Fill;
		form.Controls.Add(userControl);
		form.SetDataBinding(declaration, ".");
		form.Show();

		var expectedOrder = new string[]
		{
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.PackagesCount,
			CusEntryHeader.Schema.MovementReferenceNumberExpiryDate,
			CusEntryHeader.Schema.EntryTypeFriendlyName,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.CH_PhaseStatus,
			CusEntryHeader.Schema.CH_PhaseStatusDescription,
			CusEntryHeader.Schema.CH_EntrySubmittedDate,
			CusEntryHeader.Schema.CusEntryNumberIssueDate,
			CusEntryHeader.Schema.CH_EntryReleaseDate,
			CusEntryHeader.Schema.CH_ExitDate,
			CusEntryHeader.Schema.CH_ExitedStatus,
			CusEntryHeader.Schema.DeclarationUCR,
		};

		var gridOrder = userControl.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).Select(x => x.ColumnName).ToArray();
		AssertSequencesEqual("Columns should be in the expected order", expectedOrder, gridOrder);
	}

	public void TestAddEntryHeaderColumns_Export_ExitStatus_NoExitControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123");

		CombineAssertions(() =>
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				AssertNull("Column Exit Status (CH_ExitedStatus)", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus));
			}
		});
	}

	public void TestAddEntryHeaderColumns_Export_ExitStatus_ExitControlNoMatchingMRN()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var exitHeader = Factory.New<CusExitHeader>();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		var exitReport = exitHeader.CusExitReports.AddNew();
		exitConsignment.CXC_MovementReference = "MRN789";
		exitReport.CER_CXC_Consignment = exitConsignment.PK;
		exitHeader.Parent = declaration;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				AssertNull("Column Exit Status (CH_ExitedStatus)", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus));
			}
		});
	}

	public void TestAddEntryHeaderColumns_Export_ExitStatus_ExitControlWithMatchingMRN()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var exitHeader = Factory.New<CusExitHeader>();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		var exitReport = exitHeader.CusExitReports.AddNew();
		exitConsignment.CXC_MovementReference = "MRN123";
		exitReport.CER_CXC_Consignment = exitConsignment.PK;
		exitHeader.Parent = declaration;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var columnStyle_ExitStatus = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus);
				AssertEquals("CH_ExitedStatus should be visible", columnStyle_ExitStatus.IsVisible, true);
				AssertEquals("Caption of CH_ExitedStatus column must be Exit Status", "Exit Status", columnStyle_ExitStatus.CaptionResourceString.Caption);
			}
		});
	}

	public static void TestBaseMessagesTabUserControlType()
	{
		using (var control = new MessageUserControlForTest())
		{
			AssertEquals(typeof(MessagesTabUserControl), control.GetBaseMessagesTabUserControlTypeExposed());
		}
	}

	public void TestAmendmentReasonVisibility()
	{
		var testDec = Factory.New<JobDeclaration>();
		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("AmendmentReasonTextBox", false, userControl.FindSingle<Control>("AmendmentReasonTextBox").Visible);
				AssertEquals("AmendmentReasonLabel", false, userControl.FindSingle<Control>("AmendmentReasonLabel").Visible);
			});
		}
	}

	public void TestCustomsMessageRemarksControls()
	{
		using (var control = new MessageUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("CustomsRemarksGrid", control.FindSingleOrDefault<ZGrid>("CustomsRemarksGrid"));
				AssertNotNull("CustomsRemarksGroupBox", control.FindSingleOrDefault<ZGroupBox>("CustomsRemarksGroupBox"));
			});
		}
	}

	public void TestCustomsMessageRemarksGrid()
	{
		using (var control = new MessageUserControl())
		{
			var customsRemarksGrid = control.FindSingleOrDefault<ZGrid>("CustomsRemarksGrid");
			var columnNames = customsRemarksGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();

			CombineAssertions(() =>
			{
				AssertSequencesEqual("Columns", new[] { nameof(NLEDIMessage.EM_MessageSubType), nameof(NLEDIMessage.StatementType), nameof(NLEDIMessage.StatementDescription) }, columnNames);
				AssertEquals("MessageType ColumnStyle", "ZTextBoxColumnStyle", customsRemarksGrid.GetColumnStyle(nameof(NLEDIMessage.EM_MessageSubType)).ColumnStyleType.Name);
				AssertEquals("StatementType ColumnStyle", "ZTextBoxColumnStyle", customsRemarksGrid.GetColumnStyle(nameof(NLEDIMessage.StatementType)).ColumnStyleType.Name);
				AssertEquals("StatementDescription ColumnStyle", "ZTextBoxColumnStyle", customsRemarksGrid.GetColumnStyle(nameof(NLEDIMessage.StatementDescription)).ColumnStyleType.Name);
			});
		}
	}

	public static void TestEntryLineAdditionalDataUserControl()
	{
		using (var messageUserControl = new MessageUserControlForTest())
		{
			messageUserControl.InitializeGridLayout();
			using (var entryLineAdditionalDataUserControl = messageUserControl.GetEntryLineAdditionalDataExposed())
			{
				AssertType(typeof(EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl);
			}
		}
	}

	ZGridColumnInfo GetGridColumnInfo(string columnName)
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = MessageTypeList.Codes.Export;
		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			return userControl.EntriesBoundGrid.GetColumnStyle(columnName);
		}
	}

	sealed class MessageUserControlForTest : MessageUserControl
	{
		public Type GetBaseMessagesTabUserControlTypeExposed() => base.GetBaseMessagesTabUserControlType();

		internal EntryLineAdditionalDataUserControl GetEntryLineAdditionalDataExposed() => base.GetEntryLineAdditionalData() as EntryLineAdditionalDataUserControl;
	}
}
