using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentService : IDtbConsignmentService
	{
		public Dictionary<string, string> GetDefaultValuesForConsignment(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, string jobType)
		{
			return GetDefaultValuesForConsignment(pickupAddressPK, deliveryAddressPK, jobType, new BusinessObjectFactory());
		}

		public Dictionary<string, string> GetDefaultValuesForConsignment(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, string jobType, BusinessObjectFactory factory)
		{
			var result = new Dictionary<string, string>();

			if (pickupAddressPK.IsEmpty && deliveryAddressPK.IsEmpty)
			{
				result.Add(IncoTermKey, string.Empty);
				result.Add(ServiceLevelKey, string.Empty);
				return result;
			}

			var (pickupOrg, deliveryOrg) = GetOrganizationsForDefaultValues(pickupAddressPK, deliveryAddressPK, factory);
			var relationship = GetBestConsignorConsigneeRelationship(pickupOrg, deliveryOrg, jobType);

			var incoTerm = GetFirstValidIncoterm(pickupOrg, deliveryOrg, relationship);
			var serviceLevel = GetFirstValidServiceLevel(pickupOrg, deliveryOrg, relationship);

			result.Add(IncoTermKey, incoTerm);
			result.Add(ServiceLevelKey, serviceLevel);

			return result;
		}

		static (OrgHeader pickupOrg, OrgHeader deliveryOrg) GetOrganizationsForDefaultValues(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, BusinessObjectFactory factory)
		{
			(OrgHeader pickupOrg, OrgHeader deliveryOrg) result;

			if (!pickupAddressPK.IsEmpty && !deliveryAddressPK.IsEmpty)
			{
				result = GetBothOrganizationsForDefaultValues(pickupAddressPK, deliveryAddressPK, factory);
			}
			else if (!deliveryAddressPK.IsEmpty)
			{
				result = (null, GetOneOrganizationForDefaultValues(deliveryAddressPK, factory));
			}
			else
			{
				result = (GetOneOrganizationForDefaultValues(pickupAddressPK, factory), null);
			}

			if (!result.pickupOrg?.OH_IsConsignor ?? false)
			{
				result.pickupOrg = null;
			}

			if (!result.deliveryOrg?.OH_IsConsignee ?? false)
			{
				result.deliveryOrg = null;
			}

			return result;
		}

		static (OrgHeader pickupOrg, OrgHeader deliveryOrg) GetBothOrganizationsForDefaultValues(ZGuid pickupAddressPK, ZGuid deliveryAddressPK, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(OrgAddressSchema.PK, pickupAddressPK);
			factory.AddFetchHint(OrgAddressSchema.PK, deliveryAddressPK);

			var pickupAddress = factory.Load<OrgAddress>(pickupAddressPK);
			var deliveryAddress = factory.Load<OrgAddress>(deliveryAddressPK);

			pickupAddress?.Factory.AddFetchHint(OrgHeaderSchema.PK, pickupAddress.OA_OH);
			deliveryAddress?.Factory.AddFetchHint(OrgHeaderSchema.PK, deliveryAddress.OA_OH);

			return (pickupAddress?.Header, deliveryAddress?.Header);
		}

		static OrgHeader GetOneOrganizationForDefaultValues(ZGuid addressPK, BusinessObjectFactory factory)
		{
			var address = factory.Load<OrgAddress>(addressPK);

			return address?.Header;
		}

		static OrgSupBuyLinkTrnMode GetBestConsignorConsigneeRelationship(OrgHeader pickupOrg, OrgHeader deliveryOrg, string jobType)
		{
			var bestRelationship =
				GetBestConsignorConsigneeRelationship(deliveryOrg?.SupplierLinks.Select(x => x.OrgSupBuyLinkTrnModes), jobType) ??
				GetBestConsignorConsigneeRelationship(pickupOrg?.BuyerLinks.Select(x => x.OrgSupBuyLinkTrnModes), jobType);

			return bestRelationship;
		}

		static OrgSupBuyLinkTrnMode GetBestConsignorConsigneeRelationship(IEnumerable<OrgSupBuyLinkTrnModeDependentCollection> modes, string jobType)
		{
			return modes?.SelectMany(x => x)
				.Cast<OrgSupBuyLinkTrnMode>()
				.Where(x =>
					x.PF_TransportMode == Constants.TransportModes.All ||
						x.PF_TransportMode == Constants.TransportModes.Road &&
						(x.PF_ContainerMode == jobType || x.PF_ContainerMode.IsEmpty))
				.OrderByDescending(x => x.PF_TransportMode == Constants.TransportModes.Road)
				.ThenByDescending(x => x.PF_ContainerMode == jobType)
				.FirstOrDefault();
		}

		static ZString GetFirstValidIncoterm(OrgHeader pickupOrg, OrgHeader deliveryOrg, OrgSupBuyLinkTrnMode relationship)
		{
			var result = relationship?.PF_IncoTerm;

			if (string.IsNullOrEmpty(result))
			{
				result = deliveryOrg?.MiscServ.OM_IMDefaultINCOTerm;
			}

			if (string.IsNullOrEmpty(result))
			{
				result = pickupOrg?.MiscServ.OM_EXDefaultIncoTerm;
			}

			return result ?? ZString.Empty;
		}

		static ZString GetFirstValidServiceLevel(OrgHeader pickupOrg, OrgHeader deliveryOrg, OrgSupBuyLinkTrnMode relationship)
		{
			var result = relationship?.PF_RS_NKDefaultServiceLevel;

			if (string.IsNullOrEmpty(result))
			{
				result = deliveryOrg?.MiscServ.OM_RS_NKIMDefaultServiceLevel;
			}

			if (string.IsNullOrEmpty(result))
			{
				result = pickupOrg?.MiscServ.OM_RS_NKEXDefaultServiceLevel;
			}

			return result ?? ZString.Empty;
		}

		public const string IncoTermKey = "INC";
		public const string ServiceLevelKey = "SRV";

		string IDtbConsignmentService.IncoTermKey => IncoTermKey;
		string IDtbConsignmentService.ServiceLevelKey => ServiceLevelKey;
	}
}
