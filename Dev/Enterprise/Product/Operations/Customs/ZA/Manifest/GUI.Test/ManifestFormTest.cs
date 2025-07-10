using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	[TestedType(typeof(ManifestForm))]
	class ManifestFormTest : ZFormBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			Factory.Save();
			return new ManifestForm(header)
			{
				ControllerID = ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest
			};
		}

		public void TestRoutingPlugIn()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;

			using (var form = new ManifestForm(header))
			{
				AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
			}
		}
	}
}
