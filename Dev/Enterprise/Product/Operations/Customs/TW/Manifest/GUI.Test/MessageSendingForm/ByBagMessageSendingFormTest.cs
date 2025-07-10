using System.Windows.Forms;
using Enterprise.Customs.TW.Manifest.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(ByBagMessageSendingForm))]
	sealed class ByBagMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ByBagMessageSendingForm(sendingObjParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeaderForTestSendingObject>();
			header.Bills.AddNew();
			sendingObjParent = new MessageSendingObjectParentForTestSendingObject(header);
		}

		MessageSendingObjectParentForTestSendingObject sendingObjParent;
	}
}
