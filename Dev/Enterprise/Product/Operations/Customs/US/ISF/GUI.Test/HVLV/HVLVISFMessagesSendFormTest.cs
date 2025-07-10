using System.Windows.Forms;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	[TestedType(typeof(HVLVISFMessagesSendForm))]
	public class HVLVISFMessagesSendFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new HVLVISFMessagesSendForm(GetISFMetaHeaderForTest());

		HVLVISFMetaHeader GetISFMetaHeaderForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var isfHeader = Factory.NewWithValidTestData<CusISFHeader>();
			isfHeader.BF_JS_Shipment = shipment.PK;

			return new HVLVISFMetaHeader(shipment.Factory, shipment.PK);
		}
	}
}
