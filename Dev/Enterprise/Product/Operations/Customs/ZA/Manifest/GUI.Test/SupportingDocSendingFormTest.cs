using System.Windows.Forms;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Customs.ZA.Manifest.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(SupportingDocSendingForm))]
	class SupportingDocSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var headerWrapper = new ManifestSupportingDocSendingObjectParent(header);
			Factory.Save();
			var result = new SupportingDocSendingForm(headerWrapper);
			return result;
		}
	}
}
