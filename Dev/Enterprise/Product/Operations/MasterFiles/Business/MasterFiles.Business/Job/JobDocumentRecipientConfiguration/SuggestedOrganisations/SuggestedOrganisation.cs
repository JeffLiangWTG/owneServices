using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SuggestedOrganisation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SuggestedOrganisation(MultilingualString organisationType, IOrgHeader orgHeader)
		{
			Argument.NotNull(organisationType, nameof(organisationType));
			Argument.NotNull(orgHeader, nameof(orgHeader));
			OrganisationType = organisationType;
			OrgHeader = orgHeader;
		}

		[ResourceStringData("SuggestedOrganisation.OrganisationType", Caption = "Organization Type", MediumCaption = "Org. Type", ShortCaption = "Type")]
		public MultilingualString OrganisationType { get; }

		public IOrgHeader OrgHeader { get; }

		[ResourceStringData("SuggestedOrganisation.OrgCode", Caption = "Organization Code", MediumCaption = "Org. Code", ShortCaption = "Code")]
		public ZString OrganisationCode => OrgHeader.OH_Code;
	}
}
