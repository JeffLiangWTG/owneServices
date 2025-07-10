using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_TransportModeAIR : CUSCARMessageTextBuilder
	{
		public CUSCARMessageTextBuilder_TransportModeAIR(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void CreateDTMs()
		{
			AddHeaderDTM(source.ManifestDate, DateOrTimeOrPeriodFunctionCodeQualifierList.DocumentIssueDateTime);
			AddHeaderDTMDepartureDate();
		}

		protected override ZString GetBillsMasterBillTypeCode() => AsycudaBill.ChildBolCode;

		protected override void SetLineGroup10CargoStatusIndicatorForPartShipment(ICusCarPackage pack, FTXSegment ftx)
		{
			ftx.TextReference.FreeTextDescriptionCode = ((int)pack.CargoStatusIndicator).ToString(CultureInfo.InvariantCulture);
		}
		protected override void AddSGP_SplitGoodsPlacement(SegmentGroup14 sg14, ICusCarPackage pack) { }
		protected override void PopulateCNTInSg7(SegmentGroup7 sg7) { }
	}
}
