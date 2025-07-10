using System;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NL.Business;

public class NLNctsFallbackEntryNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
{
	public NLNctsFallbackEntryNumberCustomisationRegistryDataType() : base(new NLNctsFallbackEntryNumberCustomisation())
	{
		GeneratedNumberName = ResString.GetMultilingualString("01B6A226-52E2-4E90-A98A-76D1CB05665C", "DVA Departure emergency procedure sequence numbers");
		MaxLength = 10;
	}

	protected override Type DataTypeCore => typeof(NLNctsFallbackEntryNumberCustomisation);
}
