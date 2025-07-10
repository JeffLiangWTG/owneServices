using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(AIMBillsSelectionDialog))]
	class AIMBillsSelectionDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			Factory.Save();
			var result = new AIMBillsSelectionDialog(new AIMMessageChooser(header, new[] { bill1, bill2 }, AIMMessageSubTypes.FRI), "Bills");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();
			return result;
		}
	}
}
