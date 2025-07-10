using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MaximumCreditLimitControl))]
	public class MaximumCreditLimitControlTest : RegistryZUserControlTestCase
	{
		public void TestMaximumCreditLimitControl()
		{
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();

				var actionLabel = form.Controls.Find("ActionLabel", true)[0] as ZLabel;
				AssertEquals(@"Apply mandatory Credit Report purchase to all Organizations that exceed Credit Limit set in Maximum Credit Limit registry.
- This can be a time-consuming process.
- This process cannot be reversed.

If this process is run during business hours on large system it can potentially slow operational processing.
The safest approach is to run out of business hours.", actionLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestControlsSetToReadOnly()
		{
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControlForTest())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				AssertEquals("Precondition: ", false, creditReportsRegistryControl.MaximumCreditLimitRegistryGridForTest.ReadOnly);

				creditReportsRegistryControl.SetControlOrBusinessEntityReadOnly(true);
				AssertEquals(true, creditReportsRegistryControl.MaximumCreditLimitRegistryGridForTest.ReadOnly);

				creditReportsRegistryControl.SetControlOrBusinessEntityReadOnly(false);
				AssertEquals(false, creditReportsRegistryControl.MaximumCreditLimitRegistryGridForTest.ReadOnly);
			}
		}

		public void TestEventsCreatedInBatchesCorrectlyForCreditLimitExceeded()
		{
			var headers = new List<OrgHeader>();

			for (int i = 0; i < 21; ++i)
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				headers.Add(header);

				var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData.OB_ARCreditLimit = new ZDecimal(2000001);

				var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany.GC_RX_NKLocalCurrency = "AUD";

				companyData.OB_OH = header.PK;
				companyData.OB_GC = glbCompany.PK;
			}

			Factory.Save();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var creditLimitCollection = new MaximumCreditLimitCollection();

			var audMaximumCreditLimitItem = creditLimitCollection.AddNew();
			audMaximumCreditLimitItem.CurrencyPK = audCurrency.PK;
			audMaximumCreditLimitItem.CreditLimit = "2000000";

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(creditLimitCollection, "");

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				AssertEquals(false, applyToAllOrganizationsButton.Enabled);

				audMaximumCreditLimitItem.CreditLimit = "3000000";
				applyToAllOrganizationsButton.PerformClick();
				AssertEquals("Please save the data before performing credit limit check.", UnitTestUserNotification.Instance.LastMessage.Text);

				audMaximumCreditLimitItem.CreditLimit = "2000000";
				audMaximumCreditLimitItem.ClearHasChanges();
				AssertEquals(true, applyToAllOrganizationsButton.Enabled);

				applyToAllOrganizationsButton.PerformClick();

				AssertEquals(21, headers.Count(h => h.GetLogs().Find(u => u.SL_Reference.StartsWith("Maximum Credit Limit Exceeded")).ToArray().Length == 1));
				AssertEquals("A total of 21 event(s) for organization exceeding credit limit are created.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEventsCreatedCorrectlyForCreditLimitExceeded()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1999999);
			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_ARCreditLimit = new ZDecimal(2000001);

			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = "AUD";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_RX_NKLocalCurrency = "AUD";

			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;
			companyData2.OB_OH = header.PK;
			companyData2.OB_GC = glbCompany2.PK;

			Factory.Save();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var cnyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");

			var creditLimitCollection = new MaximumCreditLimitCollection();

			var audMaximumCreditLimitItem = creditLimitCollection.AddNew();
			audMaximumCreditLimitItem.CurrencyPK = audCurrency.PK;
			audMaximumCreditLimitItem.CreditLimit = "2000000";

			var cnyMaximumCreditLimitItem = creditLimitCollection.AddNew();
			cnyMaximumCreditLimitItem.CurrencyPK = cnyCurrency.PK;
			cnyMaximumCreditLimitItem.CreditLimit = "1999998";

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(creditLimitCollection, "");

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				AssertEquals(false, applyToAllOrganizationsButton.Enabled);

				audMaximumCreditLimitItem.CreditLimit = "3000000";
				applyToAllOrganizationsButton.PerformClick();
				AssertEquals("Please save the data before performing credit limit check.", UnitTestUserNotification.Instance.LastMessage.Text);

				audMaximumCreditLimitItem.CreditLimit = "2000000";
				audMaximumCreditLimitItem.ClearHasChanges();
				cnyMaximumCreditLimitItem.ClearHasChanges();
				AssertEquals(true, applyToAllOrganizationsButton.Enabled);

				applyToAllOrganizationsButton.PerformClick();

				var logs1 = header.GetLogs().Find(u => u.SL_Reference.StartsWith("Maximum Credit Limit Exceeded")).ToArray();
				AssertEquals(1, logs1.Length);
				var reqLog1 = logs1.First();
				AssertEquals(AutoEvents.CreditCheckEvent.Code, reqLog1.SL_SE_NKEvent);
				AssertEquals("CRL", reqLog1.Parameters[Params.Codes.Type]);
				AssertEquals("MCL", reqLog1.Parameters[Params.Codes.Reason]);
				AssertEquals(glbCompany2.GC_Code.ToString(), reqLog1.Parameters[Params.Codes.Company]);
				AssertEquals("Event Type: Credit Limit | Reason: Maximum Credit Limit Exceeded | Company Code: " + glbCompany2.GC_Code.ToString(), reqLog1.DisplayEventReference.ToString());
				AssertEquals("A total of 1 event(s) for organization exceeding credit limit are created.", UnitTestUserNotification.Instance.LastMessage.Text);

				var companyData3 = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData3.OB_ARCreditLimit = new ZDecimal(2000001);
				var glbCompany3 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany3.GC_RX_NKLocalCurrency = "AUD";
				companyData3.OB_OH = header.PK;
				companyData3.OB_GC = glbCompany3.PK;

				Factory.Save();

				audMaximumCreditLimitItem.CreditLimit = "3000000";
				applyToAllOrganizationsButton.PerformClick();
				AssertEquals("Please save the data before performing credit limit check.", UnitTestUserNotification.Instance.LastMessage.Text);

				audMaximumCreditLimitItem.CreditLimit = "2000000";
				audMaximumCreditLimitItem.ClearHasChanges();
				cnyMaximumCreditLimitItem.ClearHasChanges();
				AssertEquals(true, applyToAllOrganizationsButton.Enabled);

				applyToAllOrganizationsButton.PerformClick();

				var logs2 = header.GetLogs().Find(u => u.SL_Reference.StartsWith("Maximum Credit Limit Exceeded")).ToArray();
				AssertEquals(2, logs2.Length);
				var reqLog2 = logs2[1];
				AssertEquals(AutoEvents.CreditCheckEvent.Code, reqLog2.SL_SE_NKEvent);
				AssertEquals("CRL", reqLog2.Parameters[Params.Codes.Type]);
				AssertEquals("MCL", reqLog2.Parameters[Params.Codes.Reason]);
				var expectedCompanyCodesPossible1 = glbCompany2.GC_Code.ToString() + ", " + glbCompany3.GC_Code.ToString();
				var expectedCompanyCodesPossible2 = glbCompany3.GC_Code.ToString() + ", " + glbCompany2.GC_Code.ToString();
				var expectedCompanyCodesPossibles = new[] { expectedCompanyCodesPossible1, expectedCompanyCodesPossible2 };
				AssertCollectionContains(reqLog2.Parameters[Params.Codes.Company], expectedCompanyCodesPossibles);
				AssertEquals("Event Type: Credit Limit | Reason: Maximum Credit Limit Exceeded | Company Code: " + reqLog2.Parameters[Params.Codes.Company], reqLog2.DisplayEventReference.ToString());
				AssertEquals("A total of 1 event(s) for organization exceeding credit limit are created.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoEventCreatedForCreditLimitExceeded()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_ARCreditLimit = new ZDecimal(1999999);

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RX_NKLocalCurrency = "AUD";

			orgCompanyData.OB_OH = header.PK;
			orgCompanyData.OB_GC = glbCompany.PK;

			Factory.Save();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var cnyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");

			var creditLimitCollection = new MaximumCreditLimitCollection();

			var audMaximumCreditLimitItem = creditLimitCollection.AddNew();
			audMaximumCreditLimitItem.CurrencyPK = audCurrency.PK;
			audMaximumCreditLimitItem.CreditLimit = "2000000";

			var cnyMaximumCreditLimitItem = creditLimitCollection.AddNew();
			cnyMaximumCreditLimitItem.CurrencyPK = cnyCurrency.PK;
			cnyMaximumCreditLimitItem.CreditLimit = "1999998";

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(creditLimitCollection, "");

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				AssertEquals(false, applyToAllOrganizationsButton.Enabled);

				audMaximumCreditLimitItem.CreditLimit = "3000000";
				applyToAllOrganizationsButton.PerformClick();
				AssertEquals("Please save the data before performing credit limit check.", UnitTestUserNotification.Instance.LastMessage.Text);

				audMaximumCreditLimitItem.CreditLimit = "2000000";
				audMaximumCreditLimitItem.ClearHasChanges();
				cnyMaximumCreditLimitItem.ClearHasChanges();
				AssertEquals(true, applyToAllOrganizationsButton.Enabled);

				applyToAllOrganizationsButton.PerformClick();

				var logs = header.GetLogs().Find(u => u.SL_Reference.StartsWith("Maximum Credit Limit Exceeded")).ToArray();
				AssertEquals(0, logs.Length);
				AssertEquals("No organization has exceeded credit limit.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEventsCreatedWhenCompanyLevel()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = "AUD";
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_RX_NKLocalCurrency = "AUD";
			companyData2.OB_OH = header.PK;
			companyData2.OB_GC = glbCompany2.PK;

			Factory.Save();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var creditLimitCollection = new MaximumCreditLimitCollection();
			creditLimitCollection.CurrentFallbackLevel = new FallbackLevel(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var audMaximumCreditLimitItem = creditLimitCollection.AddNew();
			audMaximumCreditLimitItem.CurrencyPK = audCurrency.PK;
			audMaximumCreditLimitItem.CreditLimit = "2000000";

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(creditLimitCollection, "");

				audMaximumCreditLimitItem.CreditLimit = "1800000";
				audMaximumCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals($"Event Type: Credit Limit | Reason: Maximum Credit Limit Exceeded | Company Code: {glbCompany1.GC_Code}", header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Single().DisplayEventReference);
			}
		}

		public void TestMaximumCreditLimitRegistryGridForCompanyFallback_ShouldSetDefaultCurrencyAndMakeCurrencyReadOnly()
		{
			// Arrange
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = "AUD";
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_RX_NKLocalCurrency = "SGD";
			companyData2.OB_OH = header.PK;
			companyData2.OB_GC = glbCompany2.PK;

			Factory.Save();
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControlForTest())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				creditReportsRegistryControl.SetControlOrBusinessEntityReadOnly(false);

				var creditLimitCollection = new MaximumCreditLimitCollection();
				creditLimitCollection.CurrentFallbackLevel = new FallbackLevel(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);
				form.SetDataBinding(creditLimitCollection, "");

				var creditReportsGrid = creditReportsRegistryControl.MaximumCreditLimitRegistryGridForTest;
				var maximumCreditReportsItem1 = creditReportsGrid.List.AddNew() as MaximumCreditLimitItem;

				// Assert Currency is Set as per selected company
				AssertEquals("Currency must be set to AUD", glbCompany1.LocalCurrency.PK, maximumCreditReportsItem1.CurrencyPK);
				// Assert Currency is not editable field
				AssertEquals("Currency column should be a read only field", true, creditReportsGrid.Columns[MaximumCreditLimitItem.Schema.CurrencyPK].ColumnStyle.ReadOnly);

				creditLimitCollection.CurrentFallbackLevel = new FallbackLevel(glbCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var maximumCreditReportsItem2 = creditReportsGrid.List.AddNew() as MaximumCreditLimitItem;

				// Assert Currency is Set as per selected company
				AssertEquals("Currency must be set to SGD", glbCompany2.LocalCurrency.PK, maximumCreditReportsItem2.CurrencyPK);
				// Assert Currency is not editable field
				AssertEquals("Currency column should be a read only field", true, creditReportsGrid.Columns[MaximumCreditLimitItem.Schema.CurrencyPK].ColumnStyle.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestMaximumCreditLimitRegistryGridForCompanyFallback_ShouldNotAllowMoreThanOneEntry()
		{
			// Assert Grid doesn't allow to add more than one element
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = "AUD";
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			Factory.Save();

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControlForTest())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				var creditLimitCollection = new MaximumCreditLimitCollectionForTest();
				creditLimitCollection.CurrentFallbackLevel = new FallbackLevel(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);
				form.SetDataBinding(creditLimitCollection, "");

				var creditReportsGrid = creditReportsRegistryControl.MaximumCreditLimitRegistryGridForTest;
				AssertEquals("Should have maximum rows set to one if fallback level is set to company", 1, creditReportsGrid.MaximumRows);

				var audMaximumCreditLimitItem = creditLimitCollection.AddNew();
				AssertEquals("Should not allow more than one row if the fallback level is set to company", false, creditLimitCollection.CanAddNewRows);
			}
		}

		#region Test Maximum CreditLimit Fallback Level

		public void TestCreditLimitSystemLevel_ApplyCompanyLevel_WhenThereIsCompanyLevel()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = audCurrency.Code;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var companyMaxCreditLimitCollection = new MaximumCreditLimitCollection();
			var companyMaxCreditLimitItem = companyMaxCreditLimitCollection.AddNew();
			companyMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			companyMaxCreditLimitItem.CreditLimit = "2000";

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "1500";

			Factory.Save();

			using (OrganisationRegistry.Instance.MaximumCreditLimit.SetTemporaryValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMaxCreditLimitCollection))
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "500";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals("Should not have events created", 0, header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Count());
			}
		}

		public void TestCreditLimitSystemLevel_ApplySystemLevel_WhenThereIsNoCompanyLevel_WithDefaultValue()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = audCurrency.Code;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "2000";

			Factory.Save();

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "500";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals($"Event Type: Credit Limit | Reason: Maximum Credit Limit Exceeded | Company Code: {glbCompany1.GC_Code}", header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Single().DisplayEventReference);
			}
		}

		[RequiresSTA]
		public void TestCreditLimitSystemLevel_DoesNotApply_WhenThereIsNoCompanyLevel_WithOverrideValue()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = audCurrency.Code;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var companyMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "2000";

			Factory.Save();

			using (OrganisationRegistry.Instance.MaximumCreditLimit.SetTemporaryValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMaxCreditLimitCollection))
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "500";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals("Should not have events created", 0, header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Count());
			}
		}

		public void TestCreditLimitSystemLevel_WhenThereIsNoCompanyLevel_WithDifferentCurrency()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = audCurrency.Code;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = usdCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "2000";

			Factory.Save();

			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "500";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals("Should not have events created", 0, header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Count());
			}
		}

		[RequiresSTA]
		public void TestCreditLimitSystemLevel_DoesNotApplyToDemoCompany()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var demoCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			if (demoCompany == null)
			{
				demoCompany = Factory.NewWithValidTestData<GlbCompany>();
				demoCompany.GC_Code = "DEM";
			}
			demoCompany.GC_RX_NKLocalCurrency = audCurrency.Code;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = demoCompany.PK;

			var companyMaxCreditLimitCollection = new MaximumCreditLimitCollection();
			var companyMaxCreditLimitItem = companyMaxCreditLimitCollection.AddNew();
			companyMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			companyMaxCreditLimitItem.CreditLimit = "500";

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "200";

			Factory.Save();

			using (OrganisationRegistry.Instance.MaximumCreditLimit.SetTemporaryValue(demoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMaxCreditLimitCollection))
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "300";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals("Should not have events created", 0, header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Count());
			}
		}

		public void TestCreditLimitSystemLevel_DoesNotApplyToInactiveCompany()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(1000);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = audCurrency.Code;
			glbCompany1.GC_IsActive = false;
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			var companyMaxCreditLimitCollection = new MaximumCreditLimitCollection();
			var companyMaxCreditLimitItem = companyMaxCreditLimitCollection.AddNew();
			companyMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			companyMaxCreditLimitItem.CreditLimit = "300";

			var systemMaxCreditLimitCollection = new MaximumCreditLimitCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var systemMaxCreditLimitItem = systemMaxCreditLimitCollection.AddNew();
			systemMaxCreditLimitItem.CurrencyPK = audCurrency.PK;
			systemMaxCreditLimitItem.CreditLimit = "400";

			Factory.Save();

			using (OrganisationRegistry.Instance.MaximumCreditLimit.SetTemporaryValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMaxCreditLimitCollection))
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new MaximumCreditLimitControl())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				form.SetDataBinding(systemMaxCreditLimitCollection, "");

				systemMaxCreditLimitItem.CreditLimit = "500";
				systemMaxCreditLimitItem.ClearHasChanges();

				var applyToAllOrganizationsButton = creditReportsRegistryControl.GetField("ApplyToAllOrganizationsButton") as ZButton;
				applyToAllOrganizationsButton.PerformClick();

				AssertEquals("Should not have events created", 0, header.GetLogs().Find(u => u.SL_SE_NKEvent == "CCE").Count());
			}
		}

		#endregion

		protected override IBusiness GetNewBusinessEntity()
		{
			return new MaximumCreditLimitCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((MaximumCreditLimitControl)control).MaximumCreditLimitRegistryGrid.ReadOnly;
		}
	}

	public class MaximumCreditLimitControlForTest : MaximumCreditLimitControl
	{
		public ZGrid MaximumCreditLimitRegistryGridForTest => MaximumCreditLimitRegistryGrid;
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);
		protected override int BatchSize => 5;
	}

	public class MaximumCreditLimitCollectionForTest : MaximumCreditLimitCollection
	{
		public bool CanAddNewRows => AllowNew;
	}
}
