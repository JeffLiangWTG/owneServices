using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business;
using CusEntryInstruction = Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.NL.GUI;

public class EntryInstructionDetailsBasicUserControlLayoutBuilder : EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>
{
	public EntryInstructionBasicDetailsControlBag NLBag => EntryInstructionBasicDetailsControlBag.Instance;

	EU.GUI.EntryInstructionBasicDetailsControlBag eBag => EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		SetVisibility(NLBag.IsHighValueOvrdCheckBox, x => x.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5 }), x => x.CEI_StyleInfo);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();
		SetCaption(eBag.LocationOfGoodsUserControl, x => Caption);
	}

	internal static ResourceStringData Caption => Res.GetData("43436AB8-E4FE-4E23-B888-6142FD7A83C7", "Location of Goods", "[UCC 5/23] Location of Goods");
}
