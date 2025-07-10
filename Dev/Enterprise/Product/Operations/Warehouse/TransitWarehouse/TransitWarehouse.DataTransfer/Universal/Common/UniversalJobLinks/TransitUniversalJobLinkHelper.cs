using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class TransitUniversalJobLinkHelper
	{
		public static List<(ZGuid owner, string recipientType, string recipientDescription)> GetOwnerRecipientTypeSourceTypeMapByStmUniversalJobLink<T>(T parentBO) where T : IBusiness, IWorkflowProvider
		{
			var ownerRecipientTypeMap = new List<(ZGuid owner, string recipientType, string sourceType)>();

			if (parentBO != null)
			{
				var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, parentBO.PK);

				var links = parentBO.Factory.Load<StmUniversalJobLink>(query);
				ownerRecipientTypeMap = links.Select(s => (s.UCL_OH_Owner, GetRecipientTypeBySourceType(s.UCL_SourceType), MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetDescriptionFromCode(GetRecipientTypeBySourceType(s.UCL_SourceType)))).ToList();
			}

			return ownerRecipientTypeMap;
		}

		public static string GetRecipientTypeBySourceType(string sourceType)
		{
			string code;
			switch (sourceType)
			{
				case nameof(DataContextType.ForwardingShipment):
				case nameof(DataContextType.ForwardingConsol):
					code = MessageRecipientPartyTypeList.Codes.Forwarder;
					break;

				case nameof(DataContextType.AirManifestLine):
				case nameof(DataContextType.UnderBond):
					code = MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent;
					break;

				default:
					code = MessageRecipientPartyTypeList.Codes.Forwarder;
					break;
			}

			return code;
		}
	}
}
