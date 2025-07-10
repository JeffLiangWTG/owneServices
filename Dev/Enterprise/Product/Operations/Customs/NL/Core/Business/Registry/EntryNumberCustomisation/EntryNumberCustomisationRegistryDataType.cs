using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business;

public class EntryNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
{
	public EntryNumberCustomisationRegistryDataType() : base(new EntryNumberCustomisation())
	{
		GeneratedNumberName = ResString.GetMultilingualString("AB5C9530-6359-4E2B-BED2-72B18AFBF22C", "Entry Number Customization");
		MaxLength = CusEntryHeaderSchema.CH_BGMReference.MaxLength;
	}

	protected override Type DataTypeCore => typeof(EntryNumberCustomisation);
}
