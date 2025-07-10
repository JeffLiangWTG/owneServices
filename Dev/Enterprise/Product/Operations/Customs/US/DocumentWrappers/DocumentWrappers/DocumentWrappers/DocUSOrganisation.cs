using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocUSOrganisation : DocOrganisation
	{
		DocUSOrganisation(USOrganisationSource source, BusinessObjectFactory factoryForWrapper)
			: base(source, factoryForWrapper)
		{
		}

		public static DocUSOrganisation New(USOrganisation uSOrganisation, BusinessObjectFactory factoryForWrapper)
		{
			DocUSOrganisation result = null;
			if (uSOrganisation != null && !uSOrganisation.IsDeleted)
			{
				var usOrganisationDocAddress = uSOrganisation.USOrganisationDocAddress;
				if ((usOrganisationDocAddress != null && !usOrganisationDocAddress.IsDeleted && usOrganisationDocAddress.E2_AddressOverride) || uSOrganisation.Organisation != null)
				{
					result = new DocUSOrganisation(USOrganisationSource.New(uSOrganisation, factoryForWrapper), factoryForWrapper);
				}
			}
			return result;
		}

		public DocDocAddress LocationAddress
		{
			get { return Source.LocationAddress; }
		}

		protected new USOrganisationSource Source
		{
			get { return (USOrganisationSource)base.Source; }
		}

		protected USOrganisation USOrganisation
		{
			get { return ((DocBaseWrapper)WrappedObject).WrappedObject as USOrganisation; }
		}

		public override OrgHeader OrgHeader => USOrganisation.Organisation;

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return USOrganisation.Organisation ?? base.GetParentBOForNoteStorageEDocsAndDocData();
		}
	}
}
