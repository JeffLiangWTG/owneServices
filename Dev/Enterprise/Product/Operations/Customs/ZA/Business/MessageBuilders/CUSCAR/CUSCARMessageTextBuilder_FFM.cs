using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_FFM : CUSCARMessageTextBuilder_TransportModeAIR
	{
		public CUSCARMessageTextBuilder_FFM(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override ZBool RequiresGET_ManifestType => ZBool.True;

		protected override void CreateGEI_CargoType()
		{
			var cmode = source.CustomsCodeForContainerMode;
			var gei = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(cmode);
			gei.ProcessingIndicator.CodeListIdentificationCode = "122";
			gei.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		protected override void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeScheduled; //232
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.ArrivalDate.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm; // 203
		}

		protected override void AddLineGroup7Party(SegmentGroup8 sg8, ICusCarParty party) { }

		protected override void AddLineGroup14Package(SegmentGroup14 sg14, ICusCarPackage pack, int packLineNumber)
		{
			AddGID_GoodsItemDetails(sg14.GID.InstantiateAChildAndAddItToChildrenCollection(), pack, packLineNumber);
			AddFTX_GoodsDescription(sg14, pack);
			AddMEA_GrossMass(sg14, pack);
		}

		protected override void AddHeaderPartyGroupingFZ() { }

		protected override void CreateGroup5ContainerEQD(ICusCarContainer container, SegmentGroup5 sg5) { }

		protected override void CreateGroup5SEL(ICusCarContainer container, SegmentGroup5 sg5) { }

		protected override void AddHeaderRFF_ACL() { }

		protected override ZBool RequiresParty(PartyFunctionCodeQualifierList partyFunctionCodeQualifierList) => false;

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;

		protected override ZBool RequiresRFF_UCR(ZString ucr) => ZBool.False;
	}
}
