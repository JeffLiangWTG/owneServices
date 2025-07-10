using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class BaseShipmentDataObjectReader<TCommonShipment> : ShipmentDataObjectReader<TCommonShipment>
		where TCommonShipment : CommonShipment
	{
		protected BaseShipmentDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalFreightHelper helper)
			: base(shipmentDataObject, logger, factory)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}
		protected readonly IUniversalFreightHelper helper;

		protected override IModuleMatcher<IShipmentDataObjectReader, TCommonShipment> GetReferenceAndPartyIDMatcher(UniversalObjectFactory factory)
		{
			var forwardingShipmentFilter = new ZQuery();
			helper.AddShipmentParameters(forwardingShipmentFilter);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, TCommonShipment>(factory, forwardingShipmentFilter);

			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.BookingConfirmationReference, JobShipmentSchema.JS_BookingReference, MatchableOrganizationType.ConsignorDocumentaryAddress, DocAddressType.ConsignorDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 }, OrganisationTypes.Consignor);
			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.BookingConfirmationReference, JobShipmentSchema.JS_BookingReference, MatchableOrganizationType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 }, OrganisationTypes.Consignee);

			// Insert OAG Additional Reference matching here.

			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.InterimReceiptNumber, JobShipmentSchema.JS_InterimReceipt, MatchableOrganizationType.ConsignorDocumentaryAddress, DocAddressType.ConsignorDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 }, OrganisationTypes.Consignor);

			matcher.AddPossibleMatchReferenceAndOrgHeaderPKs(ReferenceElementName.WayBillNumber, JobShipmentSchema.JS_HouseBill, MatchableOrganizationType.SendingForwarderAddress, new ForwarderFinder(factory).GetSendingForwarderPKs, new Score() { FullMatch = 180, ReferenceOnlyMatch = 30, Conflict = -180 });

			return matcher;
		}

		#region ForwarderFinder

		class ForwarderFinder
		{
			public ForwarderFinder(UniversalObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly UniversalObjectFactory factory;

			internal ZGuid[] GetSendingForwarderPKs(CommonShipment matchingBO)
			{
				var departureConsol = matchingBO.DepartureConsol;
				if (departureConsol != null)
				{
					var sendingForwarder = departureConsol.SendingForwarder;
					if (sendingForwarder != null)
					{
						return new[] { sendingForwarder.PK };
					}
				}

				var companyQuery = new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True);
				companyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, ZGuid.Empty);

				var companies = factory.Load<GlbCompany>(companyQuery);
				return companies.Select(o => o.GC_OH_OrgProxy).ToArray();
			}
		}

		#endregion
	}
}
