using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class OrgCarrierNamedAccountFilterStripCommonControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilterStripControlsAreVisible()
		{
			using (var stripControl = new OrgCarrierNamedAccountFilterStripControlForTest(Factory.New<OrgHeader>(), filterBO))
			{
				Assert(stripControl.ManageButtonExposed.Visible);
				Assert(stripControl.SaveButtonExposed.Visible);
				Assert(stripControl.GroupButtonExposed.Visible);
				Assert(stripControl.FindButtonExposed.Visible);
				Assert(!stripControl.ExposedColorPicker.Visible);
				AssertEquals(92, stripControl.MaxFilterStripPanelHeightExposed);
			}
		}

		[RequiresSTA]
		public void TestPerformSearch()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;

			var cna1 = carrier.CarrierNamedAccounts.AddNew();
			cna1.ONA_ForeignName = "Nike";
			cna1.ONA_OH_Organization = org.PK;

			var cna2 = carrier.CarrierNamedAccounts.AddNew();
			cna2.ONA_ForeignName = "Nike Inc.";
			cna2.ONA_OH_Organization = org.PK;

			Factory.Save();

			using (var stripControl = new OrgCarrierNamedAccountFilterStripCommonControl(carrier.CarrierNamedAccounts, filterBO))
			{
				stripControl.FirePerformSearch();
				AssertEquals(2, carrier.CarrierNamedAccounts.Count);

				stripControl.FilterBusinessObject.AddTextFilterStrip("ForeignName", "Nike Inc.");
				stripControl.FirePerformSearch();
				AssertEquals(1, carrier.CarrierNamedAccounts.Count);
			}
		}

		[RequiresSTA]
		public void TestPerformSearch_MaxNumber()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;

			var cna1 = carrier.CarrierNamedAccounts.AddNew();
			cna1.ONA_ForeignName = "Nike";
			cna1.ONA_OH_Organization = org.PK;

			var cna2 = carrier.CarrierNamedAccounts.AddNew();
			cna2.ONA_ForeignName = "Nike Inc.";
			cna2.ONA_OH_Organization = org.PK;

			Factory.Save();

			using (var stripControl = new OrgCarrierNamedAccountFilterStripCommonControl(carrier.CarrierNamedAccounts, filterBO))
			{
				SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				UnitTestUserNotification.Instance.ClearMessages();
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

				stripControl.FirePerformSearch();
				AssertEquals("Too many records to display. Only the first 1 records have been loaded. Configurable in registry Physical Server -> Display Grid -> Max No. of Records to Show", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestPerformSearch_HasChanges()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;

			var cna1 = carrier.CarrierNamedAccounts.AddNew();
			cna1.ONA_ForeignName = "Nike";
			cna1.ONA_OH_Organization = org.PK;

			var cna2 = carrier.CarrierNamedAccounts.AddNew();
			cna2.ONA_ForeignName = "Nike Inc.";
			cna2.ONA_OH_Organization = org.PK;

			// No Factory.Save()

			using (var stripControl = new OrgCarrierNamedAccountFilterStripCommonControl(carrier.CarrierNamedAccounts, filterBO))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

				stripControl.FirePerformSearch();
				AssertEquals("You have unsaved changes. Please save your changes and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		readonly FilterStripBusinessObject filterBO = new UrsNamedAccountFilterBusinessObject();

		class OrgCarrierNamedAccountFilterStripControlForTest : OrgCarrierNamedAccountFilterStripCommonControl
		{
			public OrgCarrierNamedAccountFilterStripControlForTest(OrgHeader header, FilterStripBusinessObject filterBO)
				: base(header.CarrierNamedAccounts, filterBO)
			{ }

			public ZToolStripSplitButton ManageButtonExposed
			{
				get { return ToolStripManageDropButton; }
			}

			public ZToolStripButton SaveButtonExposed
			{
				get { return ToolStripSaveLayoutButton; }
			}

			public ZToolStripSplitButton FindButtonExposed
			{
				get { return ToolStripFindDropButton; }
			}

			public ZToolStripButton GroupButtonExposed
			{
				get { return ToolStripAddGroupButton; }
			}

			public ZFilterGrid ExposedGrid
			{
				get { return Grid; }
			}

			public ZToolStrip ExposedColorPicker
			{
				get { return ToolStripColourPicker; }
			}

			public int MaxFilterStripPanelHeightExposed
			{
				get { return MaxFilterStripPanelHeight; }
			}
		}
	}
}
