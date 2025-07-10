using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ZADefaultBillPartiesLayouts : ASYCUDA.GUI.DefaultBillPartiesLayouts
	{
		public ZADefaultBillPartiesLayouts()
			: base()
		{
			var layOut = ((IPanelLayoutProvider)this).Layout;
			if (layOut.ControlBags.Count > 0)
			{
				var controlBag = layOut.ControlBags[0] as ASYCUDA.GUI.CommonBillPartiesControlBag;
				layOut.SetCaption<ASYCUDA.Business.AsycudaBill>(controlBag.ConigneeCountryCodeFindBox, b => Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("5301FEA4-531D-47EA-8DE5-2FB3C6BF1931", "Country/Region"), b => b.ABL_RN_NKConsigneeCountryInfo);
				layOut.SetCaption<ASYCUDA.Business.AsycudaBill>(controlBag.ShipperCountryCodeFindBox, b => Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("928208C9-2554-4E94-85EE-EE096277C002", "Country/Region"), b => b.ABL_RN_NKShipperCountryInfo);
				layOut.SetCaption<ASYCUDA.Business.AsycudaBill>(controlBag.NotifyPartyCountryCodeFindBox, b => Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("06A8C89F-B0F1-43CF-A992-B57FDDF4B50F", "Country/Region"), b => b.ABL_RN_NKNotifyPartyCountryInfo);
			}
		}
	}
}
