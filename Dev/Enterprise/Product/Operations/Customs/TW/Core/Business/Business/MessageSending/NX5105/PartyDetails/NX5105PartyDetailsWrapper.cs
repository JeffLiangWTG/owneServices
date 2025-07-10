using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105PartyDetailsWrapper : PartyDetailsWrapper
	{
		protected readonly OrgAddress orgAddressForId;
		protected readonly string[] codesToLookFor;
		protected (ZString Type, ZString ID) validRegNoAndType;

		public NX5105PartyDetailsWrapper(OrgAddress orgAddress, OrgAddress orgAddressForId, params string[] codesToLookFor)
			: base(orgAddress)
		{
			this.orgAddressForId = orgAddressForId;
			this.codesToLookFor = codesToLookFor;
			validRegNoAndType = GetTypeAndId();
		}

		(ZString Type, ZString ID) GetTypeAndId()
		{
			var result = (ZString.Empty, ZString.Empty);
			foreach (var codeType in codesToLookFor)
			{
				var id = orgAddressForId.GetCustomsRegNo(codeType);
				if (!id.IsEmpty)
				{
					result = (codeType, id);
					break;
				}
			}
			return result;
		}

		protected override ZString IDCore => validRegNoAndType.ID;

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				if (!Communications1IdCore.IsEmpty)
				{
					yield return new CommunicationWrapper(Communications1IdCore, Communications1TypeID);
				}

				if (!Communications2IdCore.IsEmpty)
				{
					yield return new CommunicationWrapper(Communications2IdCore, Communications2TypeID);
				}
			}
		}
		protected virtual ZString Communications1IdCore { get; }
		protected ZString Communications1TypeID => MessageConstants.CommunicationTypeIDs.TE;
		protected virtual ZString Communications2IdCore { get; }
		protected ZString Communications2TypeID => MessageConstants.CommunicationTypeIDs.MA;

		protected ZString GetTypeCodeBasedOnOrgCusCode() => GetTypeCodeBasedOnOrgCusCode(validRegNoAndType.Type);
	}
}
