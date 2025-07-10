using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_ECL : CUSCARMessageTextBuilder_TransportModeSEA
	{
		public CUSCARMessageTextBuilder_ECL(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void CreateCNI(SegmentGroup7 sg7) { }

		protected override void AddHeaderPartyGroupingFZ() { }

		protected override IEnumerable<ICusCarContainer> Containers => source.HeaderContainers;

		protected override void CreateGroup5SEL(ICusCarContainer container, SegmentGroup5 sg5) { }

		protected override void CreateGroup7HeaderAndChildBills() { }

		protected override ZBool RequiresParty(PartyFunctionCodeQualifierList partyFunctionCodeQualifierList) => false;

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;

		protected override ZBool RequiresLOC8_PlaceOfDestination => ZBool.False;

		protected override ZBool RequiresLOC9_PlaceOfLoading => ZBool.False;

		protected override ZBool RequiresLOC80_PlaceOfDespatch(ZString placeOfDespatch) => ZBool.False;

		protected override ZBool RequiresRFF_UCR(ZString ucr) => ZBool.False;
	}
}
