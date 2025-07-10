using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Agency.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerDetentionModule))]
	internal class ContainerDetentionModuleBasherTest : ZModuleBasherTest
	{
		public void TestGetNewStandardMenuItem()
		{
			MenuItem item = MenuAssertion.AssertHasMenu(Module.FormActionMenu, "&New", "Multiple Clients");
			item.PerformClick();
			using (CreateBulkContainerDetentionForm form = Module.LastFormForTesting)
			{
				AssertType(typeof(CreateBulkContainerDetentionForm), form);
				AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestGetNewStandardMenuItem_Unauthorised()
		{
			Env.Security.AgencyContainerDetentionNew.IsAllowed = false;
			MenuItem item = MenuAssertion.AssertHasMenu(Module.FormActionMenu, "&New", "Multiple Clients");
			item.PerformClick();
			const string expectedMessage = "Error You do not have the appropriate security rights to run this function.\r\n" + "\r\n" + "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n" + "\r\n" + "Operate -> Liner & Agency -> Container Detention -> New\r\n" + "";
			using (CreateBulkContainerDetentionForm form = Module.LastFormForTesting)
			{
				AssertType(null, form);
				AssertMultilineASCIIEquals("Expected error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation
		ContainerDetentionModule Module
		{
			get
			{
				return module ?? (module = (ContainerDetentionModule)ZModuleFactory.Instance.Create(GetModuleID()));
			}
		}

		ContainerDetentionModule module;
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyContainerDetention;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}
		#endregion
	}
}
