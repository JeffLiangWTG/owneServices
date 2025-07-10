using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class EntryHeaderControllerTest : ZControllerBasherTest
	{
		public virtual void TestFormReturnedIsOfRightType()
		{
			AssertControllerNotNull();
			if (ExpectedFormType != null)
			{
				try
				{
					IZForm formFromGetFormOnNew = Controller.ShowNewForm();
					AssertEquals("Check to make sure your Controller is properly registered in ZModules, and that GetForm() is working properly.\r\n\r\nformFromGetFormOnNew.GetType()", ExpectedFormType, formFromGetFormOnNew.GetType());
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		public void TestControllerReturnedFromSpecifiedControllerIDIsOfTheRightType()
		{
			AssertControllerNotNull();
			if (ExpectedControllerType != null)
			{
				try
				{
					AssertEquals("Check to make sure your Controller is properly registered in ZModules.\r\n\r\nController.GetType()", ExpectedControllerType, Controller.GetType());
				}
				catch (ModuleFeatureNotSupportedException)
				{
				}
			}
		}

		public void TestOpenedFormAndSelectedCorrectEntryHeader()
		{
			AssertControllerNotNull();
			var controller = new EntryHeaderController();
			var entry1 = GetNewEntryHeader();
			var entry2 = entry1.Declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			using (var form = controller.ShowEditForm(entry1) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				var customsBrokerageUserControl = form.FindSingle<BaseCustomsBrokerageUserControl>();
				var entriesGrid = customsBrokerageUserControl.Controls.Find("EntriesBoundGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull("Cannot find EntriesBoundGrid, if you're not using ImportMessageUserControl and EntriesAndEntryLinesUserControl, please override method FindEntriesBoundGrid()", entriesGrid);
				var selectedEntryHeader = entriesGrid.SelectedElements[0] as CusEntryHeader;
				AssertEquals("Selected entry should be entry 1", entry1.PK, selectedEntryHeader.PK);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new EntryHeaderController();
			AssertEquals(Env.Security.CustomsDeclarationEnquiry, controller.CheckPointForViewExposedForTest);
			AssertEquals(Env.Security.CustomsDeclarationEnquiryEdit, controller.CheckPointForEditExposedForTest);
			AssertEquals(null, controller.CheckPointForNewExposedForTest);
			AssertEquals(null, controller.CheckPointForDeleteExposedForTest);
		}

		protected sealed override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.EntryHeader;
		}

		protected sealed override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var entry = GetNewEntryHeader();
			Factory.Save();
			return entry;
		}

		protected abstract CusEntryHeader GetNewEntryHeader();

		protected abstract Type ExpectedFormType { get; }

		protected Type ExpectedControllerType => TestedTypeHelper.GetTestedType(GetType());
	}
}
