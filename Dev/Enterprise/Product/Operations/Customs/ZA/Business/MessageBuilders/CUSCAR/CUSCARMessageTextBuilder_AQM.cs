using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_AQM : CUSCARMessageTextBuilder_TransportModeAIR
	{
		public CUSCARMessageTextBuilder_AQM(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated; //132
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.ArrivalDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd; // 102
		}

		protected override void AddLineGroup8GEIs(SegmentGroup8 sg8, ICusCarLine bill)
		{
			var gei = sg8.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation; // 5
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(bill.CargoReleaseStatus);
			gei.ProcessingIndicator.CodeListIdentificationCode = "42"; // Business Function
			gei.ProcessingIndicator.ProcessingIndicatorDescription = bill.CargoReleaseStatusDescription;
		}

		protected override void AddLineGroup7Party(SegmentGroup8 sg8, ICusCarParty party) { }

		protected override void AddLineGroup14Package(SegmentGroup14 sg14, ICusCarPackage pack, int packLineNumber)
		{
			AddGID_GoodsItemDetails(sg14.GID.InstantiateAChildAndAddItToChildrenCollection(), pack, packLineNumber);
		}

		protected override void CreateGEI_CallPurpose() { }

		protected override void CreateGroup5ContainerEQD(ICusCarContainer container, SegmentGroup5 sg5) { }

		protected override void CreateGroup5SEL(ICusCarContainer container, SegmentGroup5 sg5) { }

		protected override void AddHeaderRFF_ACL() { }

		protected override ZBool RequiresParty(PartyFunctionCodeQualifierList partyFunctionCodeQualifierList) => false;

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;

		protected override ZBool RequiresLOC8_PlaceOfDestination => ZBool.False;

		protected override ZBool RequiresLOC9_PlaceOfLoading => ZBool.False;

		protected override ZBool RequiresLOC80_PlaceOfDespatch(ZString placeOfDespatch) => ZBool.False;

		protected override bool RequiresContainers => false;

		protected override ZBool RequiresRFF_UCR(ZString ucr) => ZBool.False;

		protected override bool RequiresReference(ZString shipmentType, ZString type) => (string)type switch
		{
			"AFM" => shipmentType == ShipmentTypeList.Codes.Import23 || shipmentType == ShipmentTypeList.Codes.Transhipment28 || shipmentType == ShipmentTypeList.Codes.Transit24,
			"ABT" => shipmentType == ShipmentTypeList.Codes.Export22,
			_ => false,
		};
	}
}
