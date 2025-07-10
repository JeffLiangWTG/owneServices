using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolOrganisationDataObjectReader : OrganisationDataObjectReader
	{
		public ConsolOrganisationDataObjectReader(OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingConsol consolBO)
			: base(addressData, logger, factory)
		{
			this.addressType = addressData != null
				? addressData.AddressType.GetValueOrDefault().ToString()
				: string.Empty;

			this.consolBO = consolBO;
		}

		readonly string addressType;
		readonly ForwardingConsol consolBO;

		protected override OrganisationMatcher GetMatcher(bool canUseUnmatchedOrgNote, ISimpleLogger matchingLogger)
		{
			var behaviourAndType = GetUnmatchedBehaviourAndDataContextType(canUseUnmatchedOrgNote);
			var behaviour = behaviourAndType.IfUnmatchedBehaviour;
			var type = behaviourAndType.DataContextType;

			return new ConsolOrganizationAddressMatcher(factory.BOFactory, behaviour, consolBO, addressType, matchingLogger, type);
		}
	}
}
