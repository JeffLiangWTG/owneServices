using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class CommonConsolDataObjectReader<TConsol> : ShipmentDataObjectReader<TConsol>
		where TConsol : CommonConsol
	{
		public CommonConsolDataObjectReader(UniversalShipment consolDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consolDataObject, logger, factory)
		{
			logger.IsUpdatingConsol = true;
		}

		protected override IModuleMatcher<IShipmentDataObjectReader, TConsol> GetReferenceAndPartyIDMatcher(UniversalObjectFactory factory)
		{
			var matcher = new ModuleMatcher<IShipmentDataObjectReader, TConsol>(factory);

			matcher.AddPossibleMatchReferenceAndOrgAddress(ReferenceElementName.AgentsReference, JobConsolSchema.JK_AgentsReference, MatchableOrganizationType.SendingForwarderAddress, JobConsolSchema.JK_OA_SendingForwarderAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 }, OrganisationTypes.Forwarder, OrganisationsSubTypeList.Codes.SendingForwarder);
			matcher.AddPossibleMatchReferenceAndOrgAddress(ReferenceElementName.AgentsReference, JobConsolSchema.JK_AgentsReference, MatchableOrganizationType.ReceivingForwarderAddress, JobConsolSchema.JK_OA_ReceivingForwarderAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 }, OrganisationTypes.Forwarder, OrganisationsSubTypeList.Codes.ReceivingForwarder);

			matcher.AddPossibleMatchReferenceAndOrgAddress(ReferenceElementName.BookingConfirmationReference, JobConsolSchema.JK_BookingReference, MatchableOrganizationType.ShippingLineAddress, JobConsolSchema.JK_OA_ShippingLineAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = -90 }, OrganisationTypes.Carrier);

			matcher.AddPossibleMatchReferenceAndOrgAddress(ReferenceElementName.WayBillNumber, JobConsolSchema.JK_MasterBillNum, MatchableOrganizationType.ShippingLineAddress, JobConsolSchema.JK_OA_ShippingLineAddress, new Score() { FullMatch = 180, ReferenceOnlyMatch = 30, Conflict = -180 }, OrganisationTypes.Carrier);

			return matcher;
		}
	}
}
