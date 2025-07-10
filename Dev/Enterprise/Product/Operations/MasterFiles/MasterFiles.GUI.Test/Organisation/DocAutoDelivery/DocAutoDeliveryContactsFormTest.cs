using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DocAutoDeliveryContactsForm))]
	sealed class DocAutoDeliveryContactsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			DocAutoDeliveryContactViewer contactViewer = new DocAutoDeliveryContactViewer(org);
			return new DocAutoDeliveryContactsForm(contactViewer);
		}
	}
}
