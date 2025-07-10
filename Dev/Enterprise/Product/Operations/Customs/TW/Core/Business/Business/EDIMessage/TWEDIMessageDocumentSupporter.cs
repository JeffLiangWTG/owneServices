using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TWEDIMessageDocumentSupporter : Customs.Business.EDIMessageDocumentSupporter
	{
		public TWEDIMessageDocumentSupporter(TWMessage message) : base(message) { }

		CusEntryHeader EntryHeader => GetOrLoad(ref entryHeader, EdiMessage.EM_LinkUniqueID);
		CusEntryHeader entryHeader;

		JobDeclaration Declaration => EntryHeader?.Declaration;

		T GetOrLoad<T>(ref T bizObj, ZGuid foreignKey)
			where T : BusinessObject
		{
			if (bizObj == null || bizObj.IsDeleted || (bizObj.PK != foreignKey && !foreignKey.IsEmpty))
			{
				bizObj = (T)Factory.Load(typeof(T), foreignKey);
			}
			return bizObj != null && !bizObj.IsDeleted ? bizObj : null;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			var declaration = Declaration;
			if (declaration != null)
			{
				if (contactType == ContactType.Consignee)
				{
					result = new OrgHeaderContact(declaration.Consignee, declaration.Consignor, null);
				}
				else if (contactType == ContactType.Consignor)
				{
					result = new OrgHeaderContact(declaration.Consignor, declaration.Consignee, null);
				}
			}

			return result;
		}

		public override string TransportMode => Declaration?.JE_TransportMode ?? ZString.Empty;
	}
}
