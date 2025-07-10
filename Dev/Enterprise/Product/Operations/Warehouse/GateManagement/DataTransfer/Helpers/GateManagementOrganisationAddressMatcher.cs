using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GateManagementOrganisationAddressMatcher : OrganisationMatcher
	{
		readonly BusinessObjectFactory factory;
		readonly ISimpleLogger logger;

		public bool FoundCommunityCodeMatch { get; private set; }

		public GateManagementOrganisationAddressMatcher(BusinessObjectFactory factory, IfUnmatched unmatchedBehaviour, ISimpleLogger logger, DataContextType? type = null) : base(factory, unmatchedBehaviour, logger, type)
		{
			this.factory = factory;
			this.logger = logger;
		}

		protected override OrgCusCode GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(IOrgHeaderForMatching orgMatchingData, ZString? shortCode)
		{
			FoundCommunityCodeMatch = false;
			OrgCusCode result = null;
			var communityCode = orgMatchingData.CustomsCodes?.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.ContainerChainCommunityCode);

			if (communityCode != null && !communityCode.OK_CustomsRegNo.IsEmpty)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, communityCode.OK_CustomsRegNo);

				var cusCodes = factory.Load<OrgCusCode>(query);
				result = cusCodes.FirstOrDefault(x => x.Header.OH_IsActive);

				if (result == null)
				{
					logger.Log(LogType.Warning, Res.GetString("46d94695-fbc6-472e-b686-c1c0366a35e4", "Community Code '{0}' is not configured.", communityCode.OK_CustomsRegNo));
				}
			}

			FoundCommunityCodeMatch = result != null;
			return result ?? base.GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(orgMatchingData, shortCode);
		}

		protected override string GetMessageForMatchedAddressByRegistrationDetail(string code, string countryCode, string codeType, string customsRegNo, string orgCode)
		{
			return FoundCommunityCodeMatch
				? Res.GetString("46b1d28a-8e67-4de8-a902-21ba615e3ef1", "Matched to address '{0}' on '{1}' by Container Chain community code '{2}'", code, orgCode, customsRegNo)
				: base.GetMessageForMatchedAddressByRegistrationDetail(code, countryCode, codeType, customsRegNo, orgCode);
		}
	}
}
