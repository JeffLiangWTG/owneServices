using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_ALM : CUSCARMessageTextBuilder_TransportModeAIR
	{
		public CUSCARMessageTextBuilder_ALM(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, string billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override void AddHeaderPartyCarrierDEG()
		{
			var party = source.GetParties(billIssuerCode).FirstOrDefault(p => p.PartyType == PartyType.ReportingCarrier_DEG);
			if (party != null)
			{
				var sg2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var nad = sg2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.ConsortiumCarrierMaritime;
				nad.PartyIdentificationDetails.PartyIdentifier = party.IdentificationCode;
				nad.PartyName.PartyName1 = party.PartyName;
			}
		}

		protected override void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			if (!source.ArrivalDate.IsEmpty)
			{
				var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated; //132
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.ArrivalDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd; // 102
			}
		}

		protected override void CreateGroup4DTM369_LoadingDate(SegmentGroup4 sg4)
		{
			if (!source.DepartureDate.IsEmpty)
			{
				var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.DateAndOrTimeOfHandlingEstimated; //369
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.DepartureDate.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm; // 203
			}
		}

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;

		protected override ZString GetPackageTypeDescriptionCode(ICusCarPackage pack)
		{
			var type = pack.PackUQ;
			switch (source.ContainerMode)
			{
				case Core.Constants.ContainerModes.Bulk:
					type = CountableQuantityCodeList.Codes.BulkLiquefiedGasAtAbnormalTemperatureOrPressure;
					break;
				case Core.Constants.ContainerModes.Liquid:
					type = CountableQuantityCodeList.Codes.BulkLiquid;
					break;
			}
			return type;
		}

		protected override void AddSGP_SplitGoodsPlacement(SegmentGroup14 sg14, ICusCarPackage pack)
		{
			var sgp = sg14.SGP.InstantiateAChildAndAddItToChildrenCollection();
			sgp.EquipmentIdentification.EquipmentIdentifier = pack.ContainerNumber.Left(17);
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();
		}

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
			sgp.PackageQuantity = pack.NumberOfPacks.ToString();
		}

		protected override void CreateGEI_CallPurpose() { }

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.ManifestNumber;
		}

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;

		protected override ZBool RequiresSG4_TranshimentDetails => IsTranshipment;
	}
}
