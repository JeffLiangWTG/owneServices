using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForACE_FDA : AddInfoDataObjectReader<ACEFDA>
	{
		public AddInfoDataObjectReaderForACE_FDA(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USACEFDAAddInfoSchema.Instance)
		{
		}

		ZBool HasFSVPImporter
		{
			get { return hasFSVPImporter != null && hasFSVPImporter.Value; }
		}
		ZBool? hasFSVPImporter;

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddressContainer, Action<ZString, ZString, string> setValue)
		{
			var organizationAddressCollection = organizationAddressContainer.OrganizationAddressCollection;
			if (organizationAddressCollection != null)
			{
				if (hasFSVPImporter == null)
				{
					hasFSVPImporter = organizationAddressCollection.Any(address => address.AddressType.Equals(Constants.AddressType.FSVPImporter));
				}

				var fda = (ACEFDA)addInfoManager;
				SetAddressPK(setValue, fda.US_DeliverToPartyAddressInfo, organizationAddressContainer, Constants.AddressType.DeliverToPartyAddress, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_OwnerAddressInfo, organizationAddressContainer, Constants.AddressType.Owner, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_FDAImporterAddressInfo, organizationAddressContainer, Constants.AddressType.FDAImporter, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_ProducerAddressInfo, organizationAddressContainer, Constants.AddressType.Producer, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_LocationOfGoodsAddressInfo, organizationAddressContainer, nameof(DocAddressType.Location), OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_ManufacturerAddressInfo, organizationAddressContainer, nameof(DocAddressType.Manufacturer), OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_OA_ShipperAddressInfo, organizationAddressContainer, Constants.AddressType.FDAShipper, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_FSVPImporterAddressInfo, organizationAddressContainer, Constants.AddressType.FSVPImporter, OrganisationTypes.None);
			}
		}

		protected override void AfterUpdateRelatedPropertyCompletedCore(Customs.Business.IAddInfoManager addInfoManager)
		{
			var header = addInfoManager as ACEFDA;
			if (header != null && !header.US_FSVPImporterAddress.IsEmpty && !HasFSVPImporter)
			{
				header.AffirmationCodes.Reload(false);

				if (!header.IsFSVPImpRequired)
				{
					header.US_FSVPImporterAddress = ZGuid.Empty;
					header.UpdateAddInfoProperties();
				}
			}
		}
	}
}
