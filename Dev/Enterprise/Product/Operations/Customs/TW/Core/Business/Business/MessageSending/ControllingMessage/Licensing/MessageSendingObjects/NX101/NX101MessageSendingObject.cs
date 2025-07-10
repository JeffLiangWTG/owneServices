using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;

namespace Enterprise.Customs.TW.Business
{
	public class NX101MessageSendingObject : LicensingMessageSendingObject, INX101
	{
		public NX101MessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		public new NX101MessageSendingObjectLookups Lookups => (NX101MessageSendingObjectLookups)base.Lookups;

		protected override LicensingMessageSendingObjectLookups GetNewLookups() => new NX101MessageSendingObjectLookups(this);

		protected override ZString GetEM_MessageTypeCore() => MessageTypeList.Codes._101;

		public new INX101Consignment Consignment => new NX101Consignment(Header);

		protected override IGoodsShipment GetGoodsShipmentCore() => new NX101GoodsShipment(Header);

		public IEnumerable<IGovernmentProcedure> GovernmentProcedure => NX101GovernmentProcedure.GovernmentProcedures(Header);

		protected override IPackaging GetPackagingCore() => new NX101Packaging(Header);

		public IPreviousDocument PreviousDocument => new NX101PreviousDocument(Header);

		public new INX101Application Application => new NX101Application(Header, this);

		public IPartyDetails COImporter => Header.ImporterDocumentaryAddress is TWJobDocAddress importerDocumentaryAddress ? new NX101PartyDetailsWrapper(importerDocumentaryAddress, Header.IsCertificate15) : null;

		protected override ITWMessageBuilder GetMessageBuilder() => new NX101MessageBuilder();

		protected override ControllingMessageSendingObjectValidation GetNewValidation()
		{
			return new NX101MessageSendingObjectValidation(this);
		}

		public new NX101MessageSendingObjectValidation Validation => (NX101MessageSendingObjectValidation)base.Validation;
	}
}
