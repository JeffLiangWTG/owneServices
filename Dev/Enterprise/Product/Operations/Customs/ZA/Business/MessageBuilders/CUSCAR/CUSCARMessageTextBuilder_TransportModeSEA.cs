using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_TransportModeSEA : CUSCARMessageTextBuilder
	{
		public CUSCARMessageTextBuilder_TransportModeSEA(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode) : base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void CreateGroup4Loc9(SegmentGroup4 sg4)
		{
			if (IsExport)
			{
				var loc9 = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc9.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfLoading;
				loc9.LocationIdentification.LocationIdentifier = UnlocoToIata(source.PortOfLoading);
			}
		}
	}
}
