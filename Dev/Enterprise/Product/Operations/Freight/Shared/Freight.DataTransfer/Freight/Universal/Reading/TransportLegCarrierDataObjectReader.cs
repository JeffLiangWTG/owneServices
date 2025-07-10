using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegCarrierDataObjectReader : OrganisationDataObjectReader
	{
		public TransportLegCarrierDataObjectReader(OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(addressData, logger, factory)
		{
		}

		protected override OrganisationMatcher GetMatcher(bool canUseUnmatchedOrgNote, ISimpleLogger matchingLogger)
		{
			var behaviourAndType = GetUnmatchedBehaviourAndDataContextType(canUseUnmatchedOrgNote);
			var behaviour = behaviourAndType.IfUnmatchedBehaviour;
			var type = behaviourAndType.DataContextType;

			return new TransportLegCarrierAddressMatcher(factory.BOFactory, behaviour, matchingLogger, type);
		}
	}
}
