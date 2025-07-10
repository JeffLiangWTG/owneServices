using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TemporaryOrganisationPopupProviderTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestShowTemporaryOrgPopup_NullFindBox()
		{
			new TemporaryOrganisationPopupProvider(null);
		}

		[RequiresSTA]
		public void TestShowTemporaryOrgPopup_NullPopup()
		{
			var mockIModalForm = new Mock<Form>();

			var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
			mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

			var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
			mockFindBox.Setup(m => m.ParentForm).Returns(mockIModalForm.Object);
			mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

			TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
			popupProvider.ShowTemporaryOrgPopup(null);

			try
			{
				mockFindBox.Verify(m => m.List, Times.AtMost(2));
				mockFindBox.VerifyAll();

				TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
				AssertNotNull(popup);
				AssertNull("No parent popup", popup.ParentFindBoxPopUp);
			}
			finally
			{
				DisposeActiveForm();
			}
		}

		[RequiresSTA]
		public void TestShowTemporaryOrgPopup_WithPopup()
		{
			using (OrganisationEmdeddedModulePopup modulePopupForm = new OrganisationEmdeddedModulePopup())
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.ShowTemporaryOrgPopup(modulePopupForm);

				try
				{
					mockFindBox.Verify(m => m.ParentForm, Times.Never);
					mockFindBox.Verify(m => m.List, Times.AtMost(2));
					mockFindBox.VerifyAll();

					TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
					AssertNotNull(popup);
					AssertEquals("Parent popup", modulePopupForm, popup.ParentFindBoxPopUp);
				}
				finally
				{
					DisposeActiveForm();
				}
			}
		}

		public void TestShowTemporaryOrgPopup_AllowNewTemporaryOrganisationsFalse()
		{
			var mockIModalForm = new Mock<Form>();

			var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
			mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(false);

			var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
			mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

			TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
			popupProvider.ShowTemporaryOrgPopup(null);
			AssertNull(ZFormModaliser.ActiveForm);

			mockFindBox.Verify(m => m.ParentForm, Times.Never);
			mockFindBox.VerifyAll();
		}

		[RequiresSTA]
		public void TestCreateEmbeddedPopup_AllowTemp_True()
		{
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);

				EmbeddedModulePopup popup = popupProvider.CreateEmbeddedPopup(module);
				AssertNotNull(popup);
				popup.Dispose();
			}
		}

		public void TestCreateEmbeddedPopup_AllowTemp_False()
		{
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(false);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				AssertNull(popupProvider.CreateEmbeddedPopup(module));
			}
		}

		[ExpectNoExceptions]
		public void TestCreateEmbeddedPopup_NullRef()
		{
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns((IOrgHeaderCollection)null);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.CreateEmbeddedPopup(module); //there was an exception.
			}
		}

		public void TestPopupClosed()
		{
			var mockIModalForm = new Mock<Form>();

			var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
			mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

			var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
			mockFindBox.Setup(m => m.ParentForm).Returns(mockIModalForm.Object);
			mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

			TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
			popupProvider.PopupClosed += new EventHandler(popupProvider_PopupClosed);
			popupProvider.ShowTemporaryOrgPopup(null);

			try
			{
				TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
				AssertNotNull(popup);
				popup.Close();

				Assert("Should have fired close event", closed);
				closed = false;
				mockFindBox.Verify(m => m.List, Times.AtMost(2));
			}
			finally
			{
				DisposeActiveForm();
			}
		}

		#region Temporary Org Popup - Security

		public void TestShowTemporaryOrgPopup_AllowNewTempOrgTrue_Security()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GlbStaff staffWithRights = GetStaffWithOrWithoutRights(true);
			GlbStaff staffWithoutRights = GetStaffWithOrWithoutRights(false);

			Factory.Save();

			Guid pretestBranch = EnvProxy.Instance.CurrentBranch.PK;
			Guid pretestDept = EnvProxy.Instance.CurrentDepartment.PK;

			using (OrganisationEmdeddedModulePopup modulePopupForm = new OrganisationEmdeddedModulePopup())
			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRights.GS_LoginName, pretestBranch, pretestDept))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.ShowTemporaryOrgPopup(modulePopupForm);

				try
				{
					mockFindBox.Verify(m => m.ParentForm, Times.Never);
					mockFindBox.Verify(m => m.List, Times.AtMost(2));
					mockFindBox.VerifyAll();

					TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
					AssertNotNull(popup);
					AssertEquals("Parent popup", modulePopupForm, popup.ParentFindBoxPopUp);
					popup.Dispose();
				}
				finally
				{
					DisposeActiveForm();
				}
			}

			using (OrganisationEmdeddedModulePopup modulePopupForm = new OrganisationEmdeddedModulePopup())
			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithoutRights.GS_LoginName, pretestBranch, pretestDept))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(true);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.ShowTemporaryOrgPopup(modulePopupForm);

				try
				{
					mockFindBox.Verify(m => m.ParentForm, Times.Never);
					mockFindBox.Verify(m => m.List, Times.AtMost(1));
					mockFindBox.VerifyAll();

					TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
					AssertNull(popup);
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
				finally
				{
					DisposeActiveForm();
				}
			}
		}

		public void TestShowTemporaryOrgPopup_AllowNewTempOrgFalse_Security()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff staffWithRights = GetStaffWithOrWithoutRights(true);
			GlbStaff staffWithoutRights = GetStaffWithOrWithoutRights(false);
			Factory.Save();

			Guid pretestBranch = EnvProxy.Instance.CurrentBranch.PK;
			Guid pretestDept = EnvProxy.Instance.CurrentDepartment.PK;

			using (OrganisationEmdeddedModulePopup modulePopupForm = new OrganisationEmdeddedModulePopup())
			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRights.GS_LoginName, pretestBranch, pretestDept))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(false);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.ShowTemporaryOrgPopup(modulePopupForm);

				try
				{
					mockFindBox.Verify(m => m.ParentForm, Times.Never);
					mockFindBox.Verify(m => m.List, Times.AtMost(1));
					mockFindBox.VerifyAll();

					TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
					AssertNull(popup);
					Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
				finally
				{
					DisposeActiveForm();
				}
			}

			using (OrganisationEmdeddedModulePopup modulePopupForm = new OrganisationEmdeddedModulePopup())
			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithoutRights.GS_LoginName, pretestBranch, pretestDept))
			{
				var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
				mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(false);

				var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
				mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);
				mockFindBox.Verify(m => m.ParentForm, Times.Never);
				mockFindBox.Verify(m => m.List, Times.AtMost(1));

				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider(mockFindBox.Object);
				popupProvider.ShowTemporaryOrgPopup(modulePopupForm);

				try
				{
					mockFindBox.VerifyAll();

					TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
					AssertNull(popup);
					Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
				finally
				{
					DisposeActiveForm();
				}
			}
		}

		#endregion

		#region Create Embedded Popup - Security

		[RequiresSTA]
		public void TestCreateEmbeddedPopup_AllowNewTempOrgTrue_Security()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GlbStaff staffWithRights = GetStaffWithOrWithoutRights(true);
			GlbStaff staffWithoutRights = GetStaffWithOrWithoutRights(false);

			Factory.Save();

			Guid pretestBranch = EnvProxy.Instance.CurrentBranch.PK;
			Guid pretestDept = EnvProxy.Instance.CurrentDepartment.PK;

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRights.GS_LoginName, pretestBranch, pretestDept))
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				Mock mockFindBox = GetFindBoxForEmbeddedModule(true);
				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider((ITemporaryOrganisationFindBox)mockFindBox.Object);

				EmbeddedModulePopup popup = popupProvider.CreateEmbeddedPopup(module);
				AssertNotNull(popup);
				popup.Dispose();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithoutRights.GS_LoginName, pretestBranch, pretestDept))
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				Mock mockFindBox = GetFindBoxForEmbeddedModule(true);
				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider((ITemporaryOrganisationFindBox)mockFindBox.Object);

				EmbeddedModulePopup popup = popupProvider.CreateEmbeddedPopup(module);
				AssertNull(popup);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateEmbeddedPopup_AllowNewTempOrgFalse_Security()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GlbStaff staffWithRights = GetStaffWithOrWithoutRights(true);
			GlbStaff staffWithoutRights = GetStaffWithOrWithoutRights(false);

			Factory.Save();

			Guid pretestBranch = EnvProxy.Instance.CurrentBranch.PK;
			Guid pretestDept = EnvProxy.Instance.CurrentDepartment.PK;

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRights.GS_LoginName, pretestBranch, pretestDept))
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				Mock mockFindBox = GetFindBoxForEmbeddedModule(false);
				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider((ITemporaryOrganisationFindBox)mockFindBox.Object);
				EmbeddedModulePopup popup = popupProvider.CreateEmbeddedPopup(module);
				AssertNull(popup);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithoutRights.GS_LoginName, pretestBranch, pretestDept))
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				Mock mockFindBox = GetFindBoxForEmbeddedModule(false);
				TemporaryOrganisationPopupProvider popupProvider = new TemporaryOrganisationPopupProvider((ITemporaryOrganisationFindBox)mockFindBox.Object);
				EmbeddedModulePopup popup = popupProvider.CreateEmbeddedPopup(module);
				AssertNull(popup);
				Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
		}

		#endregion

		#region Implementation

		bool closed;
		void popupProvider_PopupClosed(object sender, EventArgs e)
		{
			closed = true;
		}

		void DisposeActiveForm()
		{
			IDisposable formToDispose = ZFormModaliser.ActiveForm;
			if (formToDispose != null)
			{
				formToDispose.Dispose();
			}
		}

		Mock GetFindBoxForEmbeddedModule(bool tempOrgsAllowed)
		{
			var mockIOrgHeaderCollection = new Mock<IOrgHeaderCollection>();
			mockIOrgHeaderCollection.Setup(m => m.AllowNewTemporaryOrganisations).Returns(tempOrgsAllowed);

			var mockFindBox = new Mock<ITemporaryOrganisationFindBox>();
			mockFindBox.Setup(m => m.List).Returns(mockIOrgHeaderCollection.Object);

			return mockFindBox;
		}

		GlbStaff GetStaffWithOrWithoutRights(bool allowed)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_GS = staff.PK;
			security.GU_SecurityRight = EnvProxy.Instance.Security.OrgDetailsNewIsTemporaryOrg.Code;
			security.GU_SecurityItemIsAllowed = allowed;
			return staff;
		}

		#endregion
	}
}
