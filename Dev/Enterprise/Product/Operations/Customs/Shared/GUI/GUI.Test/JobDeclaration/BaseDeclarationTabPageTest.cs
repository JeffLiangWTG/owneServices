using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseDeclarationTabPageTest : TestCaseWithFactory
	{
		public void TestGetTopLevelBusinessEntityForPlugIns()
		{
			using (BaseCustomsBrokerageUserControl userControl = new BaseCustomsBrokerageUserControl())
			using (LazyLoadedTabControl tabControl = new LazyLoadedTabControl())
			{
				userControl.Controls.Add(tabControl);
				BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
				userControl.JobDeclaration = dec;
				AssertEquals("Declaration returned", dec, tabControl.GetTopLevelBusinessEntityForPlugInsInternal());
			}
		}

		public void TestLoadTabPageOnSelectedIndexChanged()
		{
			CombineAssertions(() =>
			{
				AssertLoadTabPageOnSelectedIndexChanged("ContainerTabPage", u => u.ContainerTabPage, u => u.ContainerUserControl, CreateContainersRequiredJobDeclaration);
				AssertLoadTabPageOnSelectedIndexChanged("PackingTabPage", u => u.PackingTabPage, u => (ZUserControl)u.Packing);
				AssertLoadTabPageOnSelectedIndexChanged("InvoiceGroupingTabPage", u => u.InvoiceGroupingTabPage, u => u.InvoiceGroupUserControl);
				AssertLoadTabPageOnSelectedIndexChanged("InvoicesTabPage", u => u.InvoicesTabPage, u => u.SupplierHeaderUserControl);
				AssertLoadTabPageOnSelectedIndexChanged("MessagesTabPage", u => u.MessagesTabPage, u => u.MessageUserControl);
				AssertLoadTabPageOnSelectedIndexChanged("InvoiceLinesTabPage", u => u.InvoiceLinesTabPage, u => u.InvoiceLinesUserControl);
				AssertLoadTabPageOnSelectedIndexChanged("MiscOptionsTabPage", u => u.MiscOptionsTabPage, u => u.DynamicMiscOptions);
				AssertLoadTabPageOnSelectedIndexChanged("EntryInstructionDetailsTabPage", u => u.EntryInstructionDetailsTabPage, u => u.CustomsEntryInstructionUserControl);
				AssertLoadTabPageOnSelectedIndexChanged("PickupTabPage", u => u.PickupTabPage, u => u.CustomsPickupUserControl, CreateExportJobDeclaration);
				AssertLoadTabPageOnSelectedIndexChanged("DeliveryTabPage", u => u.DeliveryTabPage, u => u.CustomsDeliveryUserControl, CreateImportJobDeclaration);
			});
		}

		void AssertLoadTabPageOnSelectedIndexChanged(string testCase, Func<BaseCustomsBrokerageUserControl, ZTabPage> getSelectedTab, Func<BaseCustomsBrokerageUserControl, ZUserControl> getCustomsUserControl, Func<BaseJobDeclaration> getDeclaration = null)
		{
			var mockBrokerageUserControl = new Mock<BaseCustomsBrokerageUserControl>();
			mockBrokerageUserControl.CallBase = true;
			mockBrokerageUserControl.Setup(m => m.EntryInstructionsTabVisibleForCountry).Returns(true);
			using (var userControl = mockBrokerageUserControl.Object)
			{
				var tabControl = userControl.MainTabControl;
				userControl.JobDeclaration = getDeclaration?.Invoke() ?? CreateJobDeclaration();
				var index = tabControl.TabPages.IndexOf(getSelectedTab.Invoke(userControl));
				AssertEquals(testCase + "-Tab Exists", true, index != -1);
				tabControl.SelectedIndex = index;
				tabControl.OnSelectedIndexChangedInternal(EventArgs.Empty);
				AssertNotNull(testCase + "-CustomsControl is Loaded", getCustomsUserControl.Invoke(userControl));
			}
		}

		BaseJobDeclaration CreateJobDeclaration() => BaseJobDeclaration.New(Factory);

		BaseJobDeclaration CreateContainersRequiredJobDeclaration()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ContainersRequired).Returns(true);
			return mockDeclaration.Object;
		}

		BaseJobDeclaration CreateExportJobDeclaration()
		{
			var jobDeclaration = BaseJobDeclaration.New(Factory);
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return jobDeclaration;
		}

		BaseJobDeclaration CreateImportJobDeclaration()
		{
			var jobDeclaration = BaseJobDeclaration.New(Factory);
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return jobDeclaration;
		}
	}
}
