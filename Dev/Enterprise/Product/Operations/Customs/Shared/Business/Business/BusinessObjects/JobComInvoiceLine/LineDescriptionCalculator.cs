using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public delegate ZString ZStringReturner();

	public class EntryLineDescriptionCalculator
	{
		public ZString Description { get; private set; }
		public ZString DescriptionSource { get; private set; }

		public void CalculateDescription()
		{
			Description = ZString.Empty;
			var alwaysUseClassificationDescription = Declaration != null && Declaration.AlwaysUseClassificationDescription;
			var prefixPartDescriptionWithPartNumber = Declaration != null && Declaration.PrefixPartDescriptionWithPartNumber;

			var descriptionOverride = DescriptionOverrideDelegate != null ? DescriptionOverrideDelegate.Item1() : ZString.Empty;
			if (Part != null && prefixPartDescriptionWithPartNumber)
			{
				Description = (Part.OP_PartNum + " - ");
			}

			if (!alwaysUseClassificationDescription && !descriptionOverride.IsEmpty)
			{
				Description += descriptionOverride;
				DescriptionSource = DescriptionOverrideDelegate.Item2;
			}
			else if (!alwaysUseClassificationDescription && Part != null && !Part.OP_Desc.IsEmpty)
			{
				Description += Part.OP_Desc.Trim().ToUpper();
				DescriptionSource = ResString.GetMultilingualString("FAFC52C4-2B41-47E3-9D95-1FA335A1354C", "Product Code and Description");
			}
			else if (Class != null && !Class.CC_Description.IsEmpty)
			{
				Description += Class.CC_Description.Trim().ToUpper();
				DescriptionSource = ResString.GetMultilingualString("37DD2EEF-63B9-4291-B1E0-52A1DB1FA073", "Classification Lookup Description");
			}

			var useTariffDescription = Declaration != null && Declaration.UseTariffDescriptionForEntryLine;
			if (Description.IsEmpty && TariffDescriptionDelegate != null && useTariffDescription)
			{
				Description = TariffDescriptionDelegate.Item1();
				DescriptionSource = TariffDescriptionDelegate.Item2;
			}

			if (Description.IsEmpty && !descriptionOverride.IsEmpty)
			{
				Description = descriptionOverride;
				DescriptionSource = DescriptionOverrideDelegate.Item2;
			}
		}

		public BaseJobDeclaration Declaration;
		public Tuple<ZStringReturner, ZString> DescriptionOverrideDelegate;
		public OrgSupplierPart Part;
		public BaseCusClassification Class;
		public Tuple<ZStringReturner, ZString> TariffDescriptionDelegate;
	}
}
