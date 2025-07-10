using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GateManagementOrganisationDataObjectReader : OrganisationDataObjectReader, IGateManagementOrganisationDataObjectReader
	{
		public GateManagementOrganisationDataObjectReader(OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(addressData, logger, factory)
		{
			this.addressData = addressData;
		}

		readonly OrganizationAddress addressData;
		GateManagementOrganisationAddressMatcher Matcher { get; set; }

		public IOrgAddress GetMatched() {
			var matchedAddress = base.GetMatched();
			var uxmlCode = addressData.RegistrationNumberCollection?.Find(x => (string)x.Type.Code == OrgCusCode.CodeTypes.ContainerChainCommunityCode)?.Value;
			if (!Matcher.FoundCommunityCodeMatch && matchedAddress != null && !string.IsNullOrEmpty(uxmlCode))
			{
				PopulateContainerChainCommunityCode(matchedAddress, (ZString)uxmlCode);
			}

			return matchedAddress;
		}

		void PopulateContainerChainCommunityCode(OrgAddress address, ZString code)
		{
			var existingCC1Code = address.CustomsCodes.Find(x => x.OK_CodeType == OrgCusCode.CodeTypes.ContainerChainCommunityCode).FirstOrDefault();
			if (existingCC1Code != null)
			{
				if (existingCC1Code.OK_CustomsRegNo != (string)code)
				{
					logger.Log(LogType.Warning, Res.GetString("b6fab58c-9111-4ea6-9f82-f82a58ab2a20", "Received Container Chain Community Code {0} does not match the existing code {1} for the matched address", code, existingCC1Code.OK_CustomsRegNo));
				}
				return;
			}

			var existingCodeForDatabaseQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			existingCodeForDatabaseQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
			var duplicateCode = factory.LoadTop1<OrgCusCode>(existingCodeForDatabaseQuery);
			if (duplicateCode != null)
			{
				logger.Log(LogType.Warning, Res.GetString("e0fedfb1-9ff9-4d66-bd7f-0c89e19d3bd4", "Container Chain Community Code {0} is already configured for another address", code));
				return;
			}

			var newOrgCode = factory.New<OrgCusCode>();
			newOrgCode.OK_CodeType = OrgCusCode.CodeTypes.ContainerChainCommunityCode;
			newOrgCode.OK_OA_PremisesAddress = address.PK;
			newOrgCode.OK_CustomsRegNo = code;
			newOrgCode.OK_OH = address.OA_OH;
			newOrgCode.OK_RN_NKCodeCountry = "";
			logger.Log(LogType.Information, Res.GetString("1e44f962-30a6-48cf-b3ea-24901b89b676", "Populated Container Chain Community Code {0} on matched address", code));
		}

		public IJobDocAddress GetMatchedOrNew(IDocAddresses jobDocAddressParent) => base.GetMatchedOrNew(jobDocAddressParent);

		public void PopulateJobDocAddress(IOrgAddress orgAddress, IJobDocAddress jobDocAddress) => base.PopulateJobDocAddress(orgAddress, jobDocAddress as JobDocAddress);

		protected override OrganisationMatcher GetMatcher(bool canUseUnmatchedOrgNote, ISimpleLogger matchingLogger)
		{
			var behaviourAndType = GetUnmatchedBehaviourAndDataContextType(canUseUnmatchedOrgNote);
			var behaviour = behaviourAndType.IfUnmatchedBehaviour;
			var type = behaviourAndType.DataContextType;
			Matcher = new GateManagementOrganisationAddressMatcher(factory.BOFactory, behaviour, matchingLogger, type);
			return Matcher;
		}
	}
}
