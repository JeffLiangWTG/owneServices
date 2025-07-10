using System;
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
	class CUSCARMessageTextBuilder_RFM : CUSCARMessageTextBuilder
	{
		public CUSCARMessageTextBuilder_RFM(CUSCARMessage edifactMessage, ICusCarHeader dataSource, MessageSubTypes subType, ZString billIssuerCode)
			: base(edifactMessage, dataSource, subType, billIssuerCode)
		{
		}

		protected override ZString GetBillsMasterBillTypeCode()
		{
			return ZString.Empty;
		}

		protected override void AddHeaderRFF_AFB_CARN()
		{
			if (!source.CARN.IsEmpty && IsAmendmentOrCancellation)  // I'm not sure that we would have a CARN and try to send something other than amend/cancel, but just in case.....
			{
				AddHeaderRFF(source.CARN, "AFB");
			}
		}

		protected override void AddHeaderPartyGroupingFZ()
		{
			// Not needed
		}

		protected override void AddHeaderPartyAgentAH()
		{
			var parties = source.GetParties(billIssuerCode).ToArray();
			var carrier = parties.FirstOrDefault(p => p.PartyType == PartyType.ReportingCarrier_RL);
			var agentOrRep = parties.FirstOrDefault(p => p.PartyType == PartyType.TransitPrincipalsAgentOrRep_AH);
			if (agentOrRep != null && carrier != null && !agentOrRep.IdentificationCode.IsEmpty && carrier.Country != Core.Constants.CountryCodes.SouthAfrica)
			{
				AddHeaderParty(PartyType.TransitPrincipalsAgentOrRep_AH);
			}
		}

		// Note, I am unsure whether "Repeat for each additional occupant (Max 4)" means 4 crew and 4 passengers, or 4 crew/passengers.  Assume former. 

		protected override void AddHeaderPartyDriverDR()
		{
			AddHeaderPeople(PartyType.Driver_DR, 1);
		}

		protected override void AddHeaderPartyCrewMemberFM()
		{
			AddHeaderPeople(PartyType.CrewMember_FM, 4);
		}

		protected override void AddHeaderPartyPassengerFL()
		{
			AddHeaderPeople(PartyType.Passenger_FL, 4);
		}

		void AddHeaderPeople(PartyType type, int maxNumber)
		{
			foreach (var person in source.GetPeople(billIssuerCode).Where(p => p.PartyType == type).Take(maxNumber))
			{
				var sg2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var nad = sg2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(new ZString(type.ToString()).Right(2));
				nad.PartyIdentificationDetails.PartyIdentifier = person.PassportNumber.Left(35);
				nad.PartyIdentificationDetails.CodeListIdentificationCode = person.TravelDocumentType;
				nad.PartyName.PartyName1 = person.Surname.Left(35);
				nad.PartyName.PartyName2 = person.FullName.Left(35);
				nad.PartyName.PartyName3 = person.DrivingLicenceNumber.Left(35);
				nad.PartyName.PartyName4 = person.AdditionalInformationOne.Left(35);
				nad.PartyName.PartyName5 = person.AdditionalInformationTwoFor16A.Left(35);
			}
		}

		protected override ZBool RequiresGroup4Loc60_PlaceOfDischarge => false;

		protected override void AddGroup4TdtConveyanceReferenceNumber(TDTSegment tdt) { }

		protected override void AddGroup4TDT20_TransportIdentification(TDTSegment tdt, ZString vesselId)
		{
			tdt.TransportIdentification.TransportMeansIdentificationName = source.ConveyanceNumberOrTransportName.Left(35);
		}

		protected override void CreateGroup4Loc35(SegmentGroup4 sg4)
		{
			var loc35Export = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc35Export.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.ExportationCountry; // 35
			loc35Export.LocationIdentification.LocationIdentifier = source.PortOfLoadingCountry;
		}

		protected override void CreateGroup4Loc36(SegmentGroup4 sg4)
		{
			var loc36Import = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc36Import.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.CountryOfUltimateDestination; //36
			loc36Import.LocationIdentification.LocationIdentifier = source.PortOfDischargeCountry;
		}

		protected override void CreateGroup4Loc17(SegmentGroup4 sg4)
		{
			var loc17CustomsOffice = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc17CustomsOffice.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.BorderCrossingPlace; //17
			loc17CustomsOffice.LocationIdentification.LocationIdentifier = source.CustomsOffice;
		}

		protected override void CreateGroup4Loc42(SegmentGroup4 sg4)
		{
			var customsOffice = source.CustomsOffice;
			var placeOfExit = source.PlaceOfExit;
			if (!placeOfExit.IsEmpty && placeOfExit != customsOffice)
			{
				var loc42PlaceOfExit = sg4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc42PlaceOfExit.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.ConsignmentExitCustomsOfficeLocation; //42
				loc42PlaceOfExit.LocationIdentification.LocationIdentifier = placeOfExit;
			}
		}

		protected override void CreateGroup4DTMArrivalDate(SegmentGroup4 sg4)
		{
			var dtm = sg4.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated; //132
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = source.DateAtCustomsOffice.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd; // 102
		}

		protected override void AddBillLineReferenceLineNumber(int billLineNumber, RFFSegment rff)
		{
			rff.Reference.DocumentLineIdentifier = billLineNumber.ToString(CultureInfo.InvariantCulture);
		}

		protected override ZBool RequiresLOC80_PlaceOfDespatch(ZString placeOfDespatch) => ZBool.False;

		protected override void SetLineGroup10CargoStatusIndicatorForPartShipment(ICusCarPackage pack, FTXSegment ftx)
		{
			ftx.TextReference.FreeTextDescriptionCode = ((int)pack.CargoStatusIndicator).ToString(CultureInfo.InvariantCulture);
		}

		protected override void SetLineGroup10SgpNumberOfPackages(ICusCarPackage pack, SGPSegment sgp)
		{
		}

		protected override void AddDGS_DangerousGoods(ICusCarPackage pack, SegmentGroup14 sg14)
		{
		}

		protected override ZBool RequiresParty(PartyFunctionCodeQualifierList partyFunctionCodeQualifierList)
		{
			return partyFunctionCodeQualifierList == PartyFunctionCodeQualifierList.Consignee ||
					partyFunctionCodeQualifierList == PartyFunctionCodeQualifierList.Consignor;
		}

		protected override ZBool RequiresNAD_Street(PartyFunctionCodeQualifierList partyFunctionCodeQualifier) => false;

		protected override ZString GetManifestNumber()
		{
			var manifestNumber = base.GetManifestNumber();
			var parties = source.GetParties(billIssuerCode);
			var carrier = parties.FirstOrDefault(p => p.PartyType == PartyType.ReportingCarrier_RL);
			if (carrier != null && !carrier.IdentificationCode.IsEmpty && !manifestNumber.StartsWith(carrier.IdentificationCode, StringComparison.CurrentCulture))
			{
				manifestNumber = carrier.IdentificationCode + manifestNumber;
			}
			return manifestNumber.Left(35);
		}

		protected override ZString GetPackageTypeDescriptionCode(ICusCarPackage pack) => !pack.VINNumber.IsEmpty ? (ZString)"VN" : base.GetPackageTypeDescriptionCode(pack);

		protected override void AddMarksAndNumbersToPCI(PCISegment pci, ICusCarPackage pack)
		{
			if (!pack.VINNumber.IsEmpty)
			{
				pci.MarksLabels.ShippingMarksDescription1 = "VIN=" + pack.VINNumber;
			}
			else
			{
				base.AddMarksAndNumbersToPCI(pci, pack);
			}
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

		protected override void CreateGEI_CallPurpose() { }

		protected override ZBool RequiresMEA_GrossMass => ZBool.True;

		protected override ZBool RequiresMEA_VerifiedGrossMass => ZBool.True;

		protected override void AddHeaderRFF_ACL() { }

		protected override bool RequiresReference(ZString shipmentType, ZString type) => true;

		protected override ZBool RequiredHeaderDTMDepartureDate => ZBool.False;
	}
}
