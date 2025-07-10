using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class PartyWrapper : AddressWrapper, IParty
	{
		public PartyWrapper(Party party, bool dummy = false)
			: base(party, dummy)
		{
			this.party = party;
		}

		#region Implementation of IParty

		public ZString PartyId
		{
			get { return party.E2_GovRegNum; }
		}

		public ZString PartyIdType
		{
			get { return party.E2_GovRegNumType; }
		}

		public ZString ABIRoutingCode
		{
			get { return party.ABIRoutingCode; }
		}

		public ZString PartyType
		{
			get { return party.E2_AddressType; }
		}

		public ZString PartyName
		{
			get { return FallbackToDummy(party.E2_CompanyNameTruncated); }
		}

		public ZString Phone
		{
			get { return party.E2_Phone.KeepNumericCharacters(); }
		}

		public ZString Email
		{
			get { return party.E2_Email; }
		}

		#endregion

		readonly Party party;
	}
}
