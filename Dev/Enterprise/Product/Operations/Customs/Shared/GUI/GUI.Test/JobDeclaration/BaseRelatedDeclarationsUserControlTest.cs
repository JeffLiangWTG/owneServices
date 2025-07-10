using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseRelatedDeclarationsUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestBaseRelatedDeclarationsUserControl()
		{
			using (var sgDeclarationUserControl = new BaseRelatedDeclarationsUserControl())
			{
			}
		}

		public void TestCopy()
		{
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(Factory))
			{
				AssertEquals("pre-condition", 0, OpenedFormCache.GetInstance().Count);
				Factory.Save();
				relatedDeclarationUserControl.ButtonNew.PerformClick();
				AssertEquals(1, OpenedFormCache.GetInstance().Count);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestEdit()
		{
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(Factory))
			{
				relatedDeclarationUserControl.ButtonEdit.PerformClick();
				AssertEquals("Please Select One Declaration to Edit", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("pre-condition", 0, OpenedFormCache.GetInstance().Count);
				relatedDeclarationUserControl.selectedElements = new BusinessObject[1];
				relatedDeclarationUserControl.selectedElements[0] = Factory.New<BaseJobDeclaration>();
				Factory.Save();
				relatedDeclarationUserControl.RelatedDeclarationsGrid.SelectAllElements();
				relatedDeclarationUserControl.ButtonEdit.PerformClick();
				AssertEquals(1, OpenedFormCache.GetInstance().Count);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestViewChild_ReadOnlyMode()
		{
			AssertViewChild(ODisplayMode.ReadOnly);
		}

		public void TestViewChild_DeleteMode()
		{
			AssertViewChild(ODisplayMode.Delete);
		}

		public void TestViewChild_NotRealyOnlyModeNorDeleteMode()
		{
			AssertViewChild(ODisplayMode.Edit, ODisplayMode.Browse);
		}

		void AssertViewChild(ODisplayMode displayMode, ODisplayMode expectChildFormDisplayMode = ODisplayMode.ReadOnly)
		{
			var parentDeclaration = Factory.New<BaseJobDeclaration>();
			var childDeclaration = parentDeclaration.RelatedDeclarations.AddNew();
			Factory.Save();
			using (var form = new ZForm(parentDeclaration))
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(Factory))
			{
				relatedDeclarationUserControl.selectedElements = new BusinessObject[1];
				relatedDeclarationUserControl.selectedElements[0] = childDeclaration;
				form.Controls.Add(relatedDeclarationUserControl);
				form.DisplayMode = displayMode;
				form.Show();
				relatedDeclarationUserControl.RelatedDeclarationsGrid.SelectAllElements();
				relatedDeclarationUserControl.RelatedDeclarationsGrid.PerformDoubleClickForTest();
				AssertEquals(1, OpenedFormCache.GetInstance().Count);
				var childForm = (ZForm)OpenedFormCache.GetInstance().GetForm(childDeclaration.PK.ToGuid(), "JobDeclaration");
				AssertEquals("Child Form should open in ReadOnly mode", expectChildFormDisplayMode, childForm.DisplayMode);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestParent_WithRelatedDeclaration()
		{
			var parentDeclaration = Factory.New<BaseJobDeclaration>();
			var childDeclaration = parentDeclaration.RelatedDeclarations.AddNew();
			Factory.Save();
			using (var form = new ZForm(childDeclaration))
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(childDeclaration))
			{
				form.Controls.Add(relatedDeclarationUserControl);
				form.Show();
				AssertEquals("Parent button invisible", true, relatedDeclarationUserControl.ParentButton.Visible);
				relatedDeclarationUserControl.ParentButton.PerformClick();
				AssertEquals(1, OpenedFormCache.GetInstance().Count);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestParent_WithRelatedDeclaration_ReadOnlyMode()
		{
			AssertViewParent(ODisplayMode.ReadOnly);
		}

		public void TestParentView_WithRelatedDeclaration_DeleteMode()
		{
			AssertViewParent(ODisplayMode.Delete);
		}

		public void TestParentView_WithRelatedDeclaration_NotRealyOnlyModeNorDeleteMode()
		{
			AssertViewParent(ODisplayMode.Edit, ODisplayMode.Browse);
		}

		public void TestParent_WithoutRelatedDeclaration()
		{
			var parentDeclaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			using (var form = new ZForm(parentDeclaration))
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(parentDeclaration))
			{
				form.Controls.Add(relatedDeclarationUserControl);
				form.Show();
				AssertEquals("Parent button visible", false, relatedDeclarationUserControl.ParentButton.Visible);
				AssertEquals(0, OpenedFormCache.GetInstance().Count);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		void AssertViewParent(ODisplayMode displayMode, ODisplayMode expectParentFormDisplayMode = ODisplayMode.ReadOnly)
		{
			var parentDeclaration = Factory.New<BaseJobDeclaration>();
			var childDeclaration = parentDeclaration.RelatedDeclarations.AddNew();
			Factory.Save();
			using (var form = new ZForm(childDeclaration))
			using (var relatedDeclarationUserControl = new RelatedDeclarationsUserControlforTest(childDeclaration))
			{
				form.Controls.Add(relatedDeclarationUserControl);
				form.DisplayMode = displayMode;
				form.Show();
				AssertEquals("Parent button should be visible", true, relatedDeclarationUserControl.ParentButton.Visible);
				relatedDeclarationUserControl.ParentButton.PerformClick();
				var parentForm = (ZForm)OpenedFormCache.GetInstance().GetForm(parentDeclaration.PK.ToGuid(), "JobDeclaration");
				AssertEquals("Parent Form should open in ReadOnly mode", expectParentFormDisplayMode, parentForm.DisplayMode);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		sealed class RelatedDeclarationsUserControlforTest : BaseRelatedDeclarationsUserControl
		{
			public RelatedDeclarationsUserControlforTest(BusinessObjectFactory factory) : base()
			{
				JobDeclaration = factory.New<BaseJobDeclaration>();
				JobDeclaration.JE_HouseBill = "Bill1";
				JobDeclaration.JE_MasterBill = "MBill1";
				selectedElements = Array.Empty<BusinessObject>();
			}

			public RelatedDeclarationsUserControlforTest(BaseJobDeclaration childDeclaration)
			{
				JobDeclaration = childDeclaration;
			}

			public BusinessObject[] selectedElements;
			protected override BusinessObject[] GetSelectedElements()
			{
				return selectedElements;
			}
		}
	}
}
