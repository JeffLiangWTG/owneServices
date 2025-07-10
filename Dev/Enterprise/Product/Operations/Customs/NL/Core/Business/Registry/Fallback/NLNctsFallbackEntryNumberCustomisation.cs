using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class NLNctsFallbackEntryNumberCustomisation : BillOfLadingNumberCustomisation
{
	public NLNctsFallbackEntryNumberCustomisation()
	{
		foreach (BillOfLadingNumberCustomisationElement unFilteredElement in UnFilteredElements.ToArray())
		{
			if (unFilteredElement.Key != BillOfLadingNumberCustomisationElement.Keys.YearAsDigit
				&& unFilteredElement.Key != BillOfLadingNumberCustomisationElement.Keys.SequenceNumber
				&& unFilteredElement.Key != BillOfLadingNumberCustomisationElement.Keys.Direction
				&& unFilteredElement.Key != BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits
				&& unFilteredElement.Key != BillOfLadingNumberCustomisationElement.Keys.TransportMode)
			{
				UnFilteredElements.Remove(unFilteredElement);
			}
		}
	}

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();
		RemoveFountainPrefix = true;

		SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, e => { e.Order = 1; e.Include = ZBool.True; e.Detail = "2"; e.ReadOnly = ZBool.True;  });
		SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, e => { e.Order = 3; e.Include = ZBool.True; e.Detail = "6"; e.CheckDigit = ZBool.False; });
		SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits, e => { e.Order = 2; e.Include = ZBool.True; e.Detail = "2"; e.CheckDigit = ZBool.False; });
		SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.TransportMode, e => { e.CheckDigit = ZBool.False; });
		SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.Direction, e => { e.CheckDigit = ZBool.False; });
	}

	void SetUnFilteredElement(ZString key, Action<BillOfLadingNumberCustomisationElement> setter)
	{
		var element = UnFilteredElements[key];
		if (element != null)
		{
			using (element.SuspendSettingHasChanges())
			using (element.GetValidationSuspender())
			{
				setter?.Invoke(element);
			}
		}
	}
	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new NLNctsFallbackEntryNumberCustomisation();
	}
}
