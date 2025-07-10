using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class StatusQueryBillSelectionDialogTest : AIMBillsSelectionDialogTest
	{
		public void TestRequestCodeDropEdit()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var bills = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();
			var chooser1 = new AIMMessageChooser(header, bills, AIMMessageSubTypes.FSQ);
			using (var dlg = new StatusQueryBillSelectionDialog(chooser1, "Bills"))
			{
				dlg.Show();
				AssertEquals("ReasonDropEdit Not Visible", false, dlg.FindSingleOrDefault<ZDropEdit>("ReasonDropEdit").Visible);
				AssertEquals("RequestCodeDropEdit Visible", true, dlg.FindSingleOrDefault<ZDropEdit>("RequestCodeDropEdit").Visible);
			}
		}
	}
}
