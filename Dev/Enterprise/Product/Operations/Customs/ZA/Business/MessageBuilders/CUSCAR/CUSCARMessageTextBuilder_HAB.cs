using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	class CUSCARMessageTextBuilder_HAB : CUSCARMessageTextBuilder
	{
		public CUSCARMessageTextBuilder_HAB(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override ZString GetBillsMasterBillTypeCode()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? "PBL" : "BOL";
		}

		protected override ZString GetBillsMasterBillDocumentNumber()
		{
			return source.AgentType == Core.Constants.AgentType.CoLoad ? source.MasterBol : source.ManifestNumber;
		}

		protected override CodeListResponsibleAgencyCodeList GetGroup4TdtResponsibleParty()
		{
			return CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation; // 3 = IATA
		}

		protected override void AddGroup4TDT20_TransportIdentification(TDTSegment tdt, ZString vesselId)
		{
		}

		protected override ZBool RequiresGET_ManifestType => ZBool.True;

		protected override void CreateGroup4Loc9(SegmentGroup4 sg4)
		{
			var loc9 = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc9.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfLoading;
			loc9.LocationIdentification.LocationIdentifier = UnlocoToIata(source.PortOfLoading);
		}

		protected override void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeScheduled; //232
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.ArrivalDate.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm; // 203
		}

		protected override bool RequiresContainers => false;

		protected override ZBool RequiresLOC104_DepotOfUnpack(ZString depotOfUnpack) => IsDepotOfUnpackAndTerminalOfDischargeMandatory;

		protected override ZBool RequiresLOC65_TerminalOfDischarge(ZString terminalOfDischarge) => !terminalOfDischarge.IsEmpty;

		protected override void AddPCI_MarksAndNumbers(ICusCarPackage pack, SegmentGroup14 sg14)
		{
			// Specs say to omit this block for HAB.  Brendon says to include it. Let's have a flippetyswitch to try both ways without new builds. Can always ditch the rego switch later. 
			if (ZACustomsRegistry.Instance.CusCarHabSendPciMarks.Value)
			{
				base.AddPCI_MarksAndNumbers(pack, sg14);
			}
		}

		protected override void AddDGS_DangerousGoods(ICusCarPackage pack, SegmentGroup14 sg14)
		{
			if (!pack.UNDGNumber.IsEmpty)
			{
				// Dangerous goods
				var dgs = sg14.DGS.InstantiateAChildAndAddItToChildrenCollection();
				dgs.DangerousGoodsRegulationsCode = DangerousGoodsRegulationsCodeList.IataIcao; // ICA
				dgs.HazardCode.HazardIdentificationCode = pack.UNDGNumber; // NB . 1 We know that the UNDG code is not an IATA code.  2. Specs ask for class in this field but the class is not as useful as the substance number. 
			}
		}

		protected override void AddCST_CustomsStatusOfGoods(ICusCarPackage pack, SegmentGroup14 sg14)
		{
		}
	}
}
