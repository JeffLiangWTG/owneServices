using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrganisationEmdeddedModulePopup))]
	sealed class OrganisationEmdeddedModulePopupTest : EmbeddModulePopupBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DummyFilterGridModule module = new DummyFilterGridModule();
			return new OrganisationEmdeddedModulePopup(module);
		}

		[RequiresSTA]
		public void TestWithZGridOrganisationFindBox()
		{
			var mockForm = new Mock<Form>();
			using (ZOrganisationFindBoxColumnStyle.ZOrganisationGridFindBox findBox = new ZOrganisationFindBoxColumnStyle.ZOrganisationGridFindBox())
			{
				findBox.List = new TestList(Factory);
				DummyFilterGridModule module = new DummyFilterGridModule();
				using (OrganisationEmdeddedModulePopup popup = new OrganisationEmdeddedModulePopup(module))
				{
					try
					{
						popup.ShowModal(findBox, mockForm.Object);
						popup.TemporaryButton.PerformClick();
						AssertEquals(typeof(TemporaryOrganisationsPopup), ZFormModaliser.ActiveForm.GetType());
					}
					finally
					{
						IDisposable disposable = ZFormModaliser.ActiveForm;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}

		class TestList : OrgHeaderCollection
		{
			public TestList(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region IOrgHeaderCollection Members

			public override bool AllowNewTemporaryOrganisations
			{
				get { return true; }
			}

			protected override void SetDefaultsForNewChild(BusinessObject child)
			{
			}

			#endregion
		}
	}
}
