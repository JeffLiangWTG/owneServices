using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TemporaryOrganisationsCodeBoxTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestF5()
		{
			var mockITemporaryOrganisationFindBox = new Mock<ITemporaryOrganisationFindBox>();

			using (var form = new ZForm())
			{
				var codeBox = new TestTemporaryOrganisationsCodeBox(mockITemporaryOrganisationFindBox.Object);
				form.Controls.Add(codeBox);
				form.Show();

				mockITemporaryOrganisationFindBox.Setup(m => m.ShowTemporaryOrgPopup(It.IsAny<OrganisationEmdeddedModulePopup>()));
				KeySender.PostKeyDown(codeBox, Keys.F5);
				Application.DoEvents();
				mockITemporaryOrganisationFindBox.VerifyAll();

				mockITemporaryOrganisationFindBox.Verify(m => m.ShowTemporaryOrgPopup(It.IsAny<OrganisationEmdeddedModulePopup>()), Times.Once());
				KeySender.PostKeyDown(codeBox, Keys.A);
				Application.DoEvents();
				mockITemporaryOrganisationFindBox.VerifyAll();
			}
		}

		class TestTemporaryOrganisationsCodeBox : TemporaryOrganisationsCodeBox
		{
			public TestTemporaryOrganisationsCodeBox(ITemporaryOrganisationFindBox findBox) : base(findBox)
			{
			}
		}
	}
}
