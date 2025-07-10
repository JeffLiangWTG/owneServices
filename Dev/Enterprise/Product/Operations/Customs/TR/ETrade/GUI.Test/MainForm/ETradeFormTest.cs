using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeForm))]
	class ETradeFormTest : ZFormBasherTest
	{
		public void TestMessageModeDropEdit()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (var form = new ETradeForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var messageModeDropEdit = asycudaManifestUserControl.FindSingle<ZDropEdit>("MessageModeDropEdit");
				AssertNotNull(messageModeDropEdit);
				AssertEquals("Message Mode", messageModeDropEdit.CaptionResourceString.Caption);
				AssertEquals("MessageMode", ((CargoWise.Windows.UI.ICompositeControlBindingSourceProvider)messageModeDropEdit.Parent).BindingSource.GetBindingMember(messageModeDropEdit));
			}
		}

		public void TestAuditPlugInPresent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ETradeForm(header);
			Assert("Audit PlugIn should be available", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		protected override Form GetFormToBashCore()
		{
			var header = CreateAsycudaManifestHeader();
			Factory.Save();

			return new ETradeForm(header)
			{
				ControllerID = ControllerIDs.Customs.TR.ETrade
			};
		}

		AsycudaManifestHeader CreateAsycudaManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU1987";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			return header;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;
	}
}
