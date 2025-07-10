using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolOrganizationAddressMatcher : OrganisationMatcher
	{
		public ConsolOrganizationAddressMatcher(BusinessObjectFactory factory, IfUnmatched unmatchedBehaviour, ForwardingConsol consolBO, string addressType, ISimpleLogger logger, DataContextType? type = null)
			: base(factory, unmatchedBehaviour, logger, type)
		{
			this.consolBO = consolBO;
			this.addressType = addressType;
		}

		readonly string addressType;
		readonly ForwardingConsol consolBO;

		public override OrgAddress GetMatchingAddress(IOrgHeaderForMatching orgMatchingData, ZString? shortCode, bool onlyMatchByCode)
		{
			var organisation = GetMatchingOrganizationByLocalCode(orgMatchingData);

			return GetAddressFromOrganizationDirectly(organisation, shortCode)
				?? GetMatchAddressFromDefaultAgentAddress(organisation)
				?? base.GetMatchingAddress(orgMatchingData, shortCode, onlyMatchByCode);
		}

		OrgAddress GetMatchAddressFromDefaultAgentAddress(OrgHeader organisation)
		{
			if (organisation != null && consolBO != null)
			{
				RefUNLOCO port = null;
				string direction = string.Empty;

				if (addressType == nameof(DocAddressType.ReceivingForwarderAddress))
				{
					port = consolBO.DischargePort;
					direction = AgentDirectionList.Codes.Import;
				}
				else if (addressType == nameof(DocAddressType.SendingForwarderAddress))
				{
					port = consolBO.LoadPort;
					direction = AgentDirectionList.Codes.Export;
				}

				if (port != null)
				{
					return port.GetBestAgent(organisation, consolBO.JK_TransportMode, direction);
				}
			}

			return null;
		}
	}
}
