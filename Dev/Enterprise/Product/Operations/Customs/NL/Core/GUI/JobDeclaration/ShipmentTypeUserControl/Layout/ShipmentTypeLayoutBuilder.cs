using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public class ShipmentTypeLayoutBuilder : ShipmentTypeLayoutBuilder<JobDeclaration>
{
	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(ShipmentTypeControlBag.Instance.SecurityDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
		SetVisibility(ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, h => h.AreTransportChargedAndSpecificCircumstanceVisibleForDeclarationType(), h => h.CustomsEntryInstructions[0]?.CEI_StyleInfo, h => h.JE_MessageTypeInfo);
		SetVisibility(ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, h => !h.IsImport, h => h.JE_MessageTypeInfo);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetCaption(ShipmentTypeControlBag.Instance.EntryStyleDropEdit, h => EntryStyleCaption, h => h.JE_MessageTypeInfo);
	}

	internal static ResourceStringData EntryStyleCaption => Res.GetData("1A23CABB-156B-43D9-8CEF-F29D44B602C8", "Entry Style", "[UCC 1/1] Entry Style");
}
