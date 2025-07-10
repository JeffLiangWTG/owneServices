using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NL.GUI;

public class OrganisationsLayoutBuilder : EU.GUI.OrganisationsLayoutBuilder
{
	EU.GUI.OrganisationsControlBag EUBag { get; } = EU.GUI.OrganisationsControlBag.Instance;
	OrganisationsControlBag NLBag { get; } = OrganisationsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(NLBag.ExporterDocAddressControl, jobDeclaration => jobDeclaration.IsExport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(NLBag.DefermentPartyDocAddressControl, jobDeclaration => jobDeclaration.IsImport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(NLBag.IntracomReceiverAddressControl, jobDeclaration => jobDeclaration.IsImport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(CommonBag.ConsigneeAddressControl, jobDeclaration => jobDeclaration.IsImport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(CommonBag.RepresentativeAddressControl, jobDeclaration => !jobDeclaration.IsImport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(EUBag.CarrierEUBorderDocAddressControl, jobDeclaration => jobDeclaration.IsExport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetVisibility(CommonBag.SellerAddressControl, jobDeclaration => !jobDeclaration.IsExport, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
	}

	protected override void SetDefaultCaptions()
	{
		SetCaption(CommonBag.ConsigneeAddressControl, jobDeclaration => BuyerCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(CommonBag.RepresentativeAddressControl, jobDeclaration => RepresentativeCaption);
		SetCaption(EUBag.CarrierEUBorderDocAddressControl, jobDeclaration => CarrierEUBorderCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(NLBag.ExporterDocAddressControl, jobDeclaration => ExporterCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(CommonBag.SellerAddressControl, jobDeclaration => SellerCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(CommonBag.DeclarantOfficeAddressControl, jobDeclaration => DeclarantOfficeCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(NLBag.IntracomReceiverAddressControl, jobDeclaration => RepresentativeCaption);
	}
	
	internal static ResourceStringData BuyerCaption => Res.GetData("09615181-1014-4D99-A2CF-1BD19C25643B", "[UCC 3/26] Buyer");
	internal static ResourceStringData RepresentativeCaption => Res.GetData("5A7835FF-7EAA-4BBC-9DD6-9EB9912D4F9B", "[UCC 3/19] Representative");
	internal static ResourceStringData CarrierEUBorderCaption => Res.GetData("B63A6492-887D-438D-BCE6-5AFB3CDBA114", "[UCC 3/31] Carrier EU border");
	internal static ResourceStringData ExporterCaption => Res.GetData("2FA06DA3-70B3-4EE4-A834-3473FCB44C4A", "[UCC 3/1] Exporter");
	internal static ResourceStringData SellerCaption => Res.GetData("A5DA77DE-0EE5-4C50-8201-72F7405C1515", "[UCC 3/24] Seller");
	internal static ResourceStringData DeclarantOfficeCaption => Res.GetData("9D5FA635-8FC2-425B-9DAE-5EBEB30451E5", "[UCC 3/17] Declarant");
}
