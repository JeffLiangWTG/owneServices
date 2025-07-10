using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Integration;

namespace Enterprise.Freight.Business
{
	public sealed class CMRConsignmentNoteDocDataObject : DocDataObject
	{
		public CMRConsignmentNoteDocDataObject(ICMRConsignmentNote cmrConsignmentNote)
		{
			this.cmrConsignmentNote = Argument.NotNull(cmrConsignmentNote, nameof(cmrConsignmentNote));
			textLimitCalculator = cmrConsignmentNote.TextLimitCalculator;
		}

		readonly ICMRConsignmentNote cmrConsignmentNote;
		readonly ITextLimitCalculator textLimitCalculator;

		#region SupplierAddress

		[MaxLength(NotificationTypes.Warning, (CMRConsignmentNoteConstants.Length.MaxLinesForAddress * CMRConsignmentNoteConstants.Length.MaxAddressLineLength))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString SupplierAddress
		{
			get
			{
				return (supplierAddress ?? (supplierAddress = textLimitCalculator.CalculateLimits(cmrConsignmentNote.SupplierAddress.ToUpperInvariant(), CMRConsignmentNoteConstants.Length.MaxLinesForAddress, CMRConsignmentNoteConstants.Length.MaxAddressLineLength))).Value;
			}
			set => SetNonPersistentPropertyValue(SupplierAddressInfo, ref supplierAddress, value);
		}

		ZString? supplierAddress;

		public ZPropertyInfo SupplierAddressInfo => GetZPropertyInfo(nameof(SupplierAddress));

		#endregion

		#region ImporterAddress

		[MaxLength(NotificationTypes.Warning, (CMRConsignmentNoteConstants.Length.MaxLinesForAddress * CMRConsignmentNoteConstants.Length.MaxAddressLineLength))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString ImporterAddress
		{
			get
			{
				return (importerAddress ?? (importerAddress = textLimitCalculator.CalculateLimits(cmrConsignmentNote.ImporterAddress.ToUpperInvariant(), CMRConsignmentNoteConstants.Length.MaxLinesForAddress, CMRConsignmentNoteConstants.Length.MaxAddressLineLength))).Value;
			}
			set => SetNonPersistentPropertyValue(ImporterAddressInfo, ref importerAddress, value);
		}

		ZString? importerAddress;

		public ZPropertyInfo ImporterAddressInfo => GetZPropertyInfo(nameof(ImporterAddress));

		#endregion

		#region InternationalConsignmentNote

		[MaxLength(NotificationTypes.Warning, CMRConsignmentNoteConstants.Length.MaxLengthInternationalConsignementNote)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString InternationalConsignmentNote
		{
			get => (internationalConsignmentNote ?? (internationalConsignmentNote = cmrConsignmentNote.InternationalConsignmentNote).Value).Substring(0, CMRConsignmentNoteConstants.Length.MaxLengthInternationalConsignementNote);
			set => SetNonPersistentPropertyValue(InternationalConsignmentNoteInfo, ref internationalConsignmentNote, value);
		}

		ZString? internationalConsignmentNote;

		public ZPropertyInfo InternationalConsignmentNoteInfo => GetZPropertyInfo(nameof(InternationalConsignmentNote));

		#endregion

		#region PlaceOfDelivery

		[MaxLength(NotificationTypes.Warning, CMRConsignmentNoteConstants.Length.MaxLengthPlaceOfDelivery)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString PlaceOfDelivery
		{
			get => (placeOfDelivery ?? (placeOfDelivery = cmrConsignmentNote.PlaceOfDelivery).Value).Substring(0, CMRConsignmentNoteConstants.Length.MaxLengthPlaceOfDelivery);
			set => SetNonPersistentPropertyValue(PlaceOfDeliveryInfo, ref placeOfDelivery, value);
		}

		ZString? placeOfDelivery;

		public ZPropertyInfo PlaceOfDeliveryInfo => GetZPropertyInfo(nameof(PlaceOfDelivery));

		#endregion

		#region CityCountryDateOfGoodsTakingOver

		public ZString CityCountryDateOfGoodsTakingOver
		{
			get => (cityCountryDateOfGoodsTakingOver ?? (cityCountryDateOfGoodsTakingOver = cmrConsignmentNote.CityCountryDateOfGoodsTakingOver).Value);
			set => SetNonPersistentPropertyValue(CityCountryDateOfGoodsTakingOverInfo, ref cityCountryDateOfGoodsTakingOver, value);
		}

		ZString? cityCountryDateOfGoodsTakingOver;

		public ZPropertyInfo CityCountryDateOfGoodsTakingOverInfo => GetZPropertyInfo(nameof(CityCountryDateOfGoodsTakingOver));

		#endregion

		#region GoodsAttachedDocuments
		public ZString GoodsAttachedDocuments
		{
			get => (goodsAttachedDocuments ?? (goodsAttachedDocuments = cmrConsignmentNote.GoodsAttachedDocuments).Value);
			set => SetNonPersistentPropertyValue(GoodsAttachedDocumentsInfo, ref goodsAttachedDocuments, value);
		}

		ZString? goodsAttachedDocuments;
		public ZPropertyInfo GoodsAttachedDocumentsInfo => GetZPropertyInfo(nameof(GoodsAttachedDocuments));

		#endregion

		#region LineDetailsBox6_7_8_9

		public ZString LineDetailsBox6_7_8_9
		{
			get => (lineDetailsBox6_7_8_9 ?? (lineDetailsBox6_7_8_9 = cmrConsignmentNote.LineDetailsBox6_7_8_9).Value);
			set => SetNonPersistentPropertyValue(LineDetailsBox6_7_8_9Info, ref lineDetailsBox6_7_8_9, value);
		}

		ZString? lineDetailsBox6_7_8_9;

		public ZPropertyInfo LineDetailsBox6_7_8_9Info => GetZPropertyInfo(nameof(LineDetailsBox6_7_8_9));

		#endregion

		#region ZString LineDetailsTariffCodeBox10

		public ZString LineDetailsTariffCodeBox10
		{
			get => (lineDetailsTariffCodeBox10 ?? (lineDetailsTariffCodeBox10 = cmrConsignmentNote.LineDetailsTariffCodeBox10).Value);
			set => SetNonPersistentPropertyValue(LineDetailsTariffCodeBox10Info, ref lineDetailsTariffCodeBox10, value);
		}

		ZString? lineDetailsTariffCodeBox10;

		public ZPropertyInfo LineDetailsTariffCodeBox10Info => GetZPropertyInfo(nameof(LineDetailsTariffCodeBox10));

		#endregion

		#region ZString LineDetailsGrossWeightInKGBox11

		public ZString LineDetailsGrossWeightInKGBox11
		{
			get => (lineDetailsGrossWeightInKGBox11 ?? (lineDetailsGrossWeightInKGBox11 = cmrConsignmentNote.LineDetailsGrossWeightInKGBox11).Value);
			set => SetNonPersistentPropertyValue(LineDetailsGrossWeightInKGBox11Info, ref lineDetailsGrossWeightInKGBox11, value);
		}

		ZString? lineDetailsGrossWeightInKGBox11;

		public ZPropertyInfo LineDetailsGrossWeightInKGBox11Info => GetZPropertyInfo(nameof(LineDetailsGrossWeightInKGBox11));

		#endregion

		#region ZString LineDetailsVolumeInM3Box12

		public ZString LineDetailsVolumeInM3Box12
		{
			get => (lineDetailsVolumeInM3Box12 ?? (lineDetailsVolumeInM3Box12 = cmrConsignmentNote.LineDetailsVolumeInM3Box12).Value);
			set => SetNonPersistentPropertyValue(LineDetailsVolumeInM3Box12Info, ref lineDetailsVolumeInM3Box12, value);
		}

		ZString? lineDetailsVolumeInM3Box12;

		public ZPropertyInfo LineDetailsVolumeInM3Box12Info => GetZPropertyInfo(nameof(LineDetailsVolumeInM3Box12));

		#endregion

		#region CarrierAddress

		[MaxLength(NotificationTypes.Warning, (CMRConsignmentNoteConstants.Length.MaxLinesForAddress * CMRConsignmentNoteConstants.Length.MaxAddressLineLength))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString CarrierAddress
		{
			get
			{
				return (carrierAddress ?? (carrierAddress = textLimitCalculator.CalculateLimits(cmrConsignmentNote.CarrierAddress.ToUpperInvariant(), CMRConsignmentNoteConstants.Length.MaxLinesForAddress, CMRConsignmentNoteConstants.Length.MaxAddressLineLength))).Value;
			}
			set => SetNonPersistentPropertyValue(CarrierAddressInfo, ref carrierAddress, value);
		}

		ZString? carrierAddress;

		public ZPropertyInfo CarrierAddressInfo => GetZPropertyInfo(nameof(CarrierAddress));

		#endregion

		#region IncotermAndTextBox14

		public ZString IncotermAndTextBox14
		{
			get => (incotermAndTextBox14 ?? (incotermAndTextBox14 = cmrConsignmentNote.IncotermAndTextBox14).Value);
			set => SetNonPersistentPropertyValue(IncotermAndTextBox14Info, ref incotermAndTextBox14, value);
		}

		ZString? incotermAndTextBox14;

		public ZPropertyInfo IncotermAndTextBox14Info => GetZPropertyInfo(nameof(IncotermAndTextBox14));

		#endregion

		#region TransportIDBox23

		public ZString TransportIDBox23
		{
			get => (transportIDBox23 ?? (transportIDBox23 = cmrConsignmentNote.TransportIDBox23).Value);
			set => SetNonPersistentPropertyValue(TransportIDBox23Info, ref transportIDBox23, value);
		}

		ZString? transportIDBox23;

		public ZPropertyInfo TransportIDBox23Info => GetZPropertyInfo(nameof(TransportIDBox23));

		#endregion

		#region FreeEnfranchisement

		public ZBool FreeEnfranchisement
		{
			get => freeEnfranchisement;
			set => SetNonPersistentPropertyValue(FreeEnfranchisementInfo, ref freeEnfranchisement, value);
		}

		ZBool freeEnfranchisement;

		public ZPropertyInfo FreeEnfranchisementInfo => GetZPropertyInfo(nameof(FreeEnfranchisement));

		#endregion

		#region AssignedEnfranchisement

		public ZBool AssignedEnfranchisement
		{
			get => assignedEnfranchisement;
			set => SetNonPersistentPropertyValue(AssignedEnfranchisementInfo, ref assignedEnfranchisement, value);
		}

		ZBool assignedEnfranchisement;

		public ZPropertyInfo AssignedEnfranchisementInfo => GetZPropertyInfo(nameof(AssignedEnfranchisement));

		#endregion

		#region JobNumber

		public ZString JobNumber
		{
			get => (jobNumber ?? (jobNumber = cmrConsignmentNote.JobNumber).Value);
			set => SetNonPersistentPropertyValue(JobNumberInfo, ref jobNumber, value);
		}

		ZString? jobNumber;

		public ZPropertyInfo JobNumberInfo => GetZPropertyInfo(nameof(JobNumber));

		#endregion

		#region SubsequentCarriers

		public ZString SubsequentCarriers
		{
			get => subsequentCarriers;
			set => SetNonPersistentPropertyValue(SubsequentCarriersInfo, ref subsequentCarriers, value);
		}

		ZString subsequentCarriers;

		public ZPropertyInfo SubsequentCarriersInfo => GetZPropertyInfo(nameof(SubsequentCarriers));

		#endregion

		#region CarriersReservations

		public ZString CarriersReservations
		{
			get => carriersReservations;
			set => SetNonPersistentPropertyValue(CarriersReservationsInfo, ref carriersReservations, value);
		}

		ZString carriersReservations;

		public ZPropertyInfo CarriersReservationsInfo => GetZPropertyInfo(nameof(CarriersReservations));

		#endregion

		#region SendersInstructions

		[MaxLength(NotificationTypes.Warning, (CMRConsignmentNoteConstants.Length.MaxLinesForSendersInstructions * CMRConsignmentNoteConstants.Length.MaxLengthForSendersInstructions))]
		[BusinessObjectMaxLengthTestExclude]
		public ZString SendersInstructions
		{
			get
			{
				return (sendersInstructions ?? (sendersInstructions = textLimitCalculator.CalculateLimits(cmrConsignmentNote.SendersInstructions.ToUpperInvariant(), CMRConsignmentNoteConstants.Length.MaxLinesForSendersInstructions, CMRConsignmentNoteConstants.Length.MaxLengthForSendersInstructions))).Value;
			}
			set => SetNonPersistentPropertyValue(SendersInstructionsInfo, ref sendersInstructions, value);
		}

		ZString? sendersInstructions;

		public ZPropertyInfo SendersInstructionsInfo => GetZPropertyInfo(nameof(SendersInstructions));

		#endregion

		#region EstablishedInPlace

		public ZString EstablishedInPlace
		{
			get => establishedInPlace ?? (establishedInPlace = cmrConsignmentNote.EstablishedInPlace).Value;
			set => SetNonPersistentPropertyValue(EstablishedInPlaceInfo, ref establishedInPlace, value);
		}

		ZString? establishedInPlace;

		public ZPropertyInfo EstablishedInPlaceInfo => GetZPropertyInfo(nameof(EstablishedInPlace));

		#endregion

		#region EstablishedInDate

		public ZString EstablishedInDate
		{
			get => establishedInDate ?? (establishedInDate = cmrConsignmentNote.EstablishedInDate).Value;
			set => SetNonPersistentPropertyValue(EstablishedInDateInfo, ref establishedInDate, value);
		}

		ZString? establishedInDate;

		public ZPropertyInfo EstablishedInDateInfo => GetZPropertyInfo(nameof(EstablishedInDate));

		#endregion

		#region SpecialAgreements

		[MaxLength(NotificationTypes.Warning, (CMRConsignmentNoteConstants.Length.MaxLinesForSpecialAgreements * CMRConsignmentNoteConstants.Length.MaxLengthForSpecialAgreements))]
		[BusinessObjectMaxLengthTestExclude]

		public ZString SpecialAgreements
		{
			get
			{
				return (specialAgreements ?? (specialAgreements = textLimitCalculator.CalculateLimits(cmrConsignmentNote.SpecialAgreements.ToUpperInvariant(), CMRConsignmentNoteConstants.Length.MaxLinesForSpecialAgreements, CMRConsignmentNoteConstants.Length.MaxLengthForSpecialAgreements))).Value;
			}
			set => SetNonPersistentPropertyValue(SpecialAgreementsInfo, ref specialAgreements, value);
		}

		ZString? specialAgreements;

		public ZPropertyInfo SpecialAgreementsInfo => GetZPropertyInfo(nameof(SpecialAgreements));

		#endregion

		#region GoodsReceivedPlace

		public ZString GoodsReceivedPlace
		{
			get => goodsReceivedPlace;
			set => SetNonPersistentPropertyValue(GoodsReceivedPlaceInfo, ref goodsReceivedPlace, value);
		}

		ZString goodsReceivedPlace;

		public ZPropertyInfo GoodsReceivedPlaceInfo => GetZPropertyInfo(nameof(GoodsReceivedPlace));

		#endregion

		#region GoodsReceivedDate

		public ZString GoodsReceivedDate
		{
			get => goodsReceivedDate;
			set => SetNonPersistentPropertyValue(GoodsReceivedDateInfo, ref goodsReceivedDate, value);
		}

		ZString goodsReceivedDate;

		public ZPropertyInfo GoodsReceivedDateInfo => GetZPropertyInfo(nameof(GoodsReceivedDate));

		#endregion

		#region TrailerID

		public ZString TrailerID
		{
			get => trailerID;
			set => SetNonPersistentPropertyValue(TrailerIDInfo, ref trailerID, value);
		}

		ZString trailerID;

		public ZPropertyInfo TrailerIDInfo => GetZPropertyInfo(nameof(TrailerID));

		#endregion

		#region DangerousGoodsClass

		public ZString DangerousGoodsClass
		{
			get => dangerousGoodsClass ?? (dangerousGoodsClass = cmrConsignmentNote.DangerousGoodsClass).Value;
			set => SetNonPersistentPropertyValue(DangerousGoodsClassInfo, ref dangerousGoodsClass, value);
		}

		ZString? dangerousGoodsClass;

		public ZPropertyInfo DangerousGoodsClassInfo => GetZPropertyInfo(nameof(DangerousGoodsClass));

		#endregion

		#region DangerousGoodsNumber

		public ZString DangerousGoodsNumber
		{
			get => dangerousGoodsNumber ?? (dangerousGoodsNumber = cmrConsignmentNote.DangerousGoodsNumber).Value;
			set => SetNonPersistentPropertyValue(DangerousGoodsNumberInfo, ref dangerousGoodsNumber, value);
		}

		ZString? dangerousGoodsNumber;

		public ZPropertyInfo DangerousGoodsNumberInfo => GetZPropertyInfo(nameof(DangerousGoodsNumber));

		#endregion

		#region DangerousGoodsLetter

		public ZString DangerousGoodsLetter
		{
			get => dangerousGoodsLetter ?? (dangerousGoodsLetter = cmrConsignmentNote.DangerousGoodsLetter).Value;
			set => SetNonPersistentPropertyValue(DangerousGoodsLetterInfo, ref dangerousGoodsLetter, value);
		}

		ZString? dangerousGoodsLetter;

		public ZPropertyInfo DangerousGoodsLetterInfo => GetZPropertyInfo(nameof(DangerousGoodsLetter));

		#endregion

		#region BoxNumbers

		public ZString Box14PaymentCarriage => cmrConsignmentNote.Box14PaymentCarriage;

		public ZString Box19SpecialAgreements => cmrConsignmentNote.Box19SpecialAgreements;

		public ZString Box20ToBePaydBy => cmrConsignmentNote.Box20ToBePaidBy;

		public ZString Box15CashOnDelivery => cmrConsignmentNote.Box15CashOnDelivery;

		public ZString Box23TransportAndTrailerID => cmrConsignmentNote.Box23TransportAndTrailerID;

		#endregion
	}
}
