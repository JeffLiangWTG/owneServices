namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USScientificDataAddInfoValidation : AutoUSScientificDataAddInfoValidation
	{
		public USScientificDataAddInfoValidation(AutoUSScientificDataAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_PGACountryCode()
		{
			base.CheckUS_PGACountryCode();

			var scientificData = ScientificData;
			if (scientificData != null)
			{
				if (!scientificData.US_PGACountryCode.IsEmpty && scientificData.US_PGACountryCode != UnknownCountryPrefix)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_PGACountryCodeInfo, Parent.Lookups.USCountryList);
				}
				if (scientificData.US_PGACountryCode.IsEmpty)
				{
					var constElement = scientificData.Parent as ConstituentElement;
					var laceyAct = constElement != null ? constElement.Parent as PGA : null;
					if (laceyAct != null)
					{
						Parent.US_PGACountryCodeInfo.AddMessageError(PGACountryCodeIsRequired);
					}
				}
			}
		}
		internal const string UnknownCountryPrefix = "**";
		internal const string PGACountryCodeIsRequired = "Country is mandatory.";

		protected override void CheckUS_PGAScientificGenusName()
		{
			base.CheckUS_PGAScientificGenusName();
			if (Parent.US_PGAScientificGenusName.IsEmpty)
			{
				Parent.US_PGAScientificGenusNameInfo.AddMessageError(PGAScientificGenusNameIsRequired);
			}
		}
		internal const string PGAScientificGenusNameIsRequired = "Genus Name is mandatory.";

		protected override void CheckUS_PGAScientificSpeciesName()
		{
			base.CheckUS_PGAScientificSpeciesName();
			if (Parent.US_PGAScientificSpeciesName.IsEmpty)
			{
				Parent.US_PGAScientificSpeciesNameInfo.AddMessageError(PGAScientificSpeciesNameIsRequired);
			}
		}
		internal const string PGAScientificSpeciesNameIsRequired = "Species Name is mandatory.";

		protected new USScientificDataAddInfo Parent
		{
			get { return (USScientificDataAddInfo)base.Parent; }
		}

		protected ScientificData ScientificData
		{
			get { return Parent.Parent; }
		}
	}
}
