using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBill : DocDataObject, IHouseBill, IDataSourceProvider, IHouseBillOverrideProvider, ICustomFieldProviderProxy
	{
		public HouseBill(ZString sourceType, ZString sourceID, CustomBusinessObject customBusinessObject = null)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
			this.customBusinessObject = customBusinessObject;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region ICustomFieldProviderProxy members

		CustomBusinessObject ICustomFieldProviderProxy.CustomBusinessObject => customBusinessObject;
		readonly CustomBusinessObject customBusinessObject;

		#endregion

		#region NumberOfCopies

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Copy Bills'")]
		public ZInt NumberOfCopies
		{
			get => numberOfCopies;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value))
				{
				}
			}
		}

		ZInt numberOfCopies;

		public ZPropertyInfo NumberOfCopiesInfo => GetZPropertyInfo(nameof(NumberOfCopies));

		#endregion

		#region IsEditingElectronicBOL

		public ZBool IsElectronicBOL
		{
			get => isElectronicBOL;
			set
			{
				if (SetNonPersistentPropertyValue(IsElectronicBOLInfo, ref isElectronicBOL, value))
				{
				}
			}
		}
		ZBool isElectronicBOL;

		public ZPropertyInfo IsElectronicBOLInfo => GetZPropertyInfo(nameof(IsElectronicBOL));

		#endregion

		#region ElectronicBillOfLadingVersion

		public ZShort ElectronicBillOfLadingVersion
		{
			get => electronicBillOfLadingVersion;
			set
			{
				if (SetNonPersistentPropertyValue(ElectronicBillOfLadingVersionInfo, ref electronicBillOfLadingVersion, value))
				{
					Validate(ElectronicBillOfLadingVersionInfo);
				}
			}
		}

		ZShort electronicBillOfLadingVersion;

		public ZPropertyInfo ElectronicBillOfLadingVersionInfo => GetZPropertyInfo(nameof(ElectronicBillOfLadingVersion));

		#endregion

		#region NumberOfOriginals

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Originals'")]
		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
				}
			}
		}

		ZInt numberOfOriginals;

		public ZPropertyInfo NumberOfOriginalsInfo => GetZPropertyInfo(nameof(NumberOfOriginals));

		#endregion

		#region HouseBillNumber

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> House Bill'")]
		public ZString HouseBillNumber
		{
			get => houseBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value))
				{
				}
			}
		}

		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		#endregion

		#region ShipmentNumber

		[DocumentField("Automatically generated when the shipment is created and shown on shipment form caption.")]
		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
				}
			}
		}

		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region ShippersReference

		[DocumentField("Corresponding to 'Shipment -> Pickup -> Shipper's Ref'")]
		public ZString ShippersReference
		{
			get => shippersReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersReferenceInfo, ref shippersReference, value))
				{
				}
			}
		}

		ZString shippersReference;

		public ZPropertyInfo ShippersReferenceInfo => GetZPropertyInfo(nameof(ShippersReference));

		#endregion

		#region CarrierBookingReference

		[DocumentField("Corresponding to 'Departure Consol -> Carrier Bkg. Ref'")]
		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region CoLoadBookingReference

		[DocumentField("Corresponding to 'Departure Consol -> Co-Load Bkg. Ref'")]
		public ZString CoLoadBookingReference
		{
			get => coLoadBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CoLoadBookingReferenceInfo, ref coLoadBookingReference, value))
				{
				}
			}
		}

		ZString coLoadBookingReference;

		public ZPropertyInfo CoLoadBookingReferenceInfo => GetZPropertyInfo(nameof(CoLoadBookingReference));

		#endregion

		#region CoLoadMasterBillNumber

		[DocumentField("Corresponding to 'Departure Consol -> Co-Load MBL'")]
		public ZString CoLoadMasterBillNumber
		{
			get => coLoadMasterBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CoLoadMasterBillNumberInfo, ref coLoadMasterBillNumber, value))
				{
					Validate(CoLoadMasterBillNumberInfo);
				}
			}
		}
		ZString coLoadMasterBillNumber;

		public ZPropertyInfo CoLoadMasterBillNumberInfo => GetZPropertyInfo(nameof(CoLoadMasterBillNumber));

		#endregion

		#region IsToSpecificAfricanCountry

		public ZBool IsToSpecificAfricanCountry
		{
			get => isToSpecificAfricanCountry;
			set
			{
				if (SetNonPersistentPropertyValue(IsToSpecificAfricanCountryInfo, ref isToSpecificAfricanCountry, value))
				{
				}
			}
		}
		ZBool isToSpecificAfricanCountry;

		public ZPropertyInfo IsToSpecificAfricanCountryInfo => GetZPropertyInfo(nameof(IsToSpecificAfricanCountry));

		#endregion

		#region CTKNumber

		[DocumentField("Corresponding to 'CTK in the Shipment > Additional Details > Reference Numbers.'")]
		public ZString CTKNumber
		{
			get => ctkNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase));

				if (SetNonPersistentPropertyValue(CTKNumberInfo, ref ctkNumber, formattedValue))
				{
					Validate(CTKNumberInfo);
				}
			}
		}
		ZString ctkNumber;

		public ZPropertyInfo CTKNumberInfo => GetZPropertyInfo(nameof(CTKNumber));

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region IsOriginal

		[DocumentField("Returns true if current template title for the form menu is 'Original'")]
		public ZBool IsOriginal
		{
			get => isOriginal;
			set
			{
				if (SetNonPersistentPropertyValue(IsOriginalInfo, ref isOriginal, value))
				{
				}
			}
		}

		ZBool isOriginal;

		public ZPropertyInfo IsOriginalInfo => GetZPropertyInfo(nameof(IsOriginal));

		#endregion

		#region DateOfIssue

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Issue Date' fall back to 'Shipment -> Routing -> First Main Sea Leg -> ATD'")]
		public ZDateTime DateOfIssue
		{
			get => dateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfIssueInfo, ref dateOfIssue, value))
				{
				}
			}
		}

		ZDateTime dateOfIssue;

		public ZPropertyInfo DateOfIssueInfo => GetZPropertyInfo(nameof(DateOfIssue));

		#endregion

		#region ArrivalDate

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> ETA'")]
		public ZDateTime ArrivalDate
		{
			get => arrivalDate;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalDateInfo, ref arrivalDate, value))
				{
				}
			}
		}

		ZDateTime arrivalDate;

		public ZPropertyInfo ArrivalDateInfo => GetZPropertyInfo(nameof(ArrivalDate));

		#endregion

		#region DepartureDate

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> ETD'")]
		public ZDateTime DepartureDate
		{
			get => departureDate;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureDateInfo, ref departureDate, value))
				{
				}
			}
		}

		ZDateTime departureDate;

		public ZPropertyInfo DepartureDateInfo => GetZPropertyInfo(nameof(DepartureDate));

		#endregion

		#region Clause

		[DocumentField("Corresponding to 'Registry -> Freight -> House Bills -> Clauses -> Standard'")]
		public ZString Clause
		{
			get => clause;
			set
			{
				if (SetNonPersistentPropertyValue(ClauseInfo, ref clause, value))
				{
				}
			}
		}

		ZString clause;

		public ZPropertyInfo ClauseInfo => GetZPropertyInfo(nameof(Clause));

		#endregion

		#region IsPrepaid

		[DocumentField("Look up 'Shipment -> Basic Registration -> Incoterm' setting in 'Registry -> AutoRating -> Charge Code Groups -> Incoterm Charge Code Group Configuration'. If 'Freight' is 'CNE' then it is false (Collect), otherwise returns true (Prepaid).")]
		public ZBool IsPrepaid
		{
			get => isPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(IsPrepaidInfo, ref isPrepaid, value))
				{
				}
			}
		}

		ZBool isPrepaid;

		public ZPropertyInfo IsPrepaidInfo => GetZPropertyInfo(nameof(IsPrepaid));

		#endregion

		#region MoveTypeFrom/MoveTypeTo

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> HBL Dlv. Mode' (first part of the code before '/')")]
		[List(nameof(MoveTypeList))]
		public ZString MoveTypeFrom
		{
			get => moveTypeFrom;
			set
			{
				if (SetNonPersistentPropertyValue(MoveTypeFromInfo, ref moveTypeFrom, value))
				{
				}
			}
		}

		ZString moveTypeFrom;

		public ZPropertyInfo MoveTypeFromInfo => GetZPropertyInfo(nameof(MoveTypeFrom));

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> HBL Dlv. Mode' (second part of the code after '/')")]
		[List(nameof(MoveTypeList))]
		public ZString MoveTypeTo
		{
			get => moveTypeTo;
			set
			{
				if (SetNonPersistentPropertyValue(MoveTypeToInfo, ref moveTypeTo, value))
				{
				}
			}
		}

		ZString moveTypeTo;

		public ZPropertyInfo MoveTypeToInfo => GetZPropertyInfo(nameof(MoveTypeTo));

		public CodeDescriptionPairList MoveTypeList
		{
			get => moveTypeList ?? new CodeDescriptionPairList();
			set => moveTypeList = value;
		}

		CodeDescriptionPairList moveTypeList;

		#endregion

		#region GoodsDetailsTextOverride

		[DocumentField("Corresponding to 'Shipment -> Notes -> House Bill Goods Details Override")]
		public ZString GoodsDetailsTextOverride
		{
			get => goodsDetailsTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDetailsTextOverrideInfo, ref goodsDetailsTextOverride, value))
				{
				}
			}
		}

		ZString goodsDetailsTextOverride;

		public ZPropertyInfo GoodsDetailsTextOverrideInfo => GetZPropertyInfo(nameof(GoodsDetailsTextOverride));

		#endregion

		#region HasGoodsDetailsTextOverride

		public ZBool HasGoodsDetailsTextOverride
		{
			get => hasGoodsDetailsTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasGoodsDetailsTextOverrideInfo, ref hasGoodsDetailsTextOverride, value))
				{
				}
			}
		}

		ZBool hasGoodsDetailsTextOverride;

		public ZPropertyInfo HasGoodsDetailsTextOverrideInfo => GetZPropertyInfo(nameof(HasGoodsDetailsTextOverride));

		#endregion

		#region ChargesTextOverride

		[DocumentField("Corresponding to 'Shipment -> Notes -> House Bill Charges Override")]
		public ZString ChargesTextOverride
		{
			get => chargesTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(ChargesTextOverrideInfo, ref chargesTextOverride, value))
				{
				}
			}
		}

		ZString chargesTextOverride;

		public ZPropertyInfo ChargesTextOverrideInfo => GetZPropertyInfo(nameof(ChargesTextOverride));

		#endregion

		#region HasChargesTextOverride

		public ZBool HasChargesTextOverride
		{
			get => hasChargesTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasChargesTextOverrideInfo, ref hasChargesTextOverride, value))
				{
				}
			}
		}

		ZBool hasChargesTextOverride;

		public ZPropertyInfo HasChargesTextOverrideInfo => GetZPropertyInfo(nameof(HasChargesTextOverride));

		#endregion

		#region FollowOnTextOverride

		[DocumentField("Corresponding to 'Shipment -> Notes -> House Bill Follow On Override")]
		public ZString FollowOnTextOverride
		{
			get => followOnTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(FollowOnTextOverrideInfo, ref followOnTextOverride, value))
				{
				}
			}
		}

		ZString followOnTextOverride;

		public ZPropertyInfo FollowOnTextOverrideInfo => GetZPropertyInfo(nameof(FollowOnTextOverride));

		#endregion

		#region HasFollowOnTextOverride

		public ZBool HasFollowOnTextOverride
		{
			get => hasFollowOnTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasFollowOnTextOverrideInfo, ref hasFollowOnTextOverride, value))
				{
				}
			}
		}

		ZBool hasFollowOnTextOverride;

		public ZPropertyInfo HasFollowOnTextOverrideInfo => GetZPropertyInfo(nameof(HasFollowOnTextOverride));

		#endregion

		#region TotalWeight

		public IMeasurement TotalWeight
		{
			get => totalWeight;
			set => totalWeight = SetChild(totalWeight, value);
		}
		IMeasurement totalWeight;

		#endregion

		#region TotalVolume

		public IMeasurement TotalVolume
		{
			get => totalVolume;
			set => totalVolume = SetChild(totalVolume, value);
		}
		IMeasurement totalVolume;

		#endregion

		#region TotalPackCount

		public ZInt TotalPackCount
		{
			get => totalPackCount;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPackCountInfo, ref totalPackCount, value))
				{
				}
			}
		}

		ZInt totalPackCount;

		public ZPropertyInfo TotalPackCountInfo => GetZPropertyInfo(nameof(TotalPackCount));

		#endregion

		#region TotalPackType

		public ICodeDescription TotalPackType
		{
			get => totalPackType;
			set => totalPackType = SetChild(totalPackType, value);
		}
		ICodeDescription totalPackType;

		#endregion

		#region HouseBillOfLadingType

		public ICodeDescription HouseBillOfLadingType
		{
			get => houseBillOfLadingType;
			set => houseBillOfLadingType = SetChild(houseBillOfLadingType, value);
		}
		ICodeDescription houseBillOfLadingType;

		#endregion

		#region TotalLoosePackCount

		public ZInt TotalLoosePackCount
		{
			get => totalLoosePackCount;
			set
			{
				if (SetNonPersistentPropertyValue(TotalLoosePackCountInfo, ref totalLoosePackCount, value))
				{
				}
			}
		}

		ZInt totalLoosePackCount;

		public ZPropertyInfo TotalLoosePackCountInfo => GetZPropertyInfo(nameof(TotalLoosePackCount));

		#endregion

		#region TotalLoosePackType

		public ICodeDescription TotalLoosePackType
		{
			get => totalLoosePackType;
			set => totalLoosePackType = SetChild(totalLoosePackType, value);
		}
		ICodeDescription totalLoosePackType;

		#endregion

		#region ExcessValueDeclaration

		public IMoney ExcessValueDeclaration
		{
			get => excessValueDeclaration;
			set => excessValueDeclaration = SetChild(excessValueDeclaration, value);
		}
		IMoney excessValueDeclaration;

		#endregion

		#region AsAgentDetail

		public ZString AsAgentDetail
		{
			get => asAgentDetail;
			set
			{
				if (SetNonPersistentPropertyValue(AsAgentDetailInfo, ref asAgentDetail, value))
				{
				}
			}
		}
		ZString asAgentDetail;

		public ZPropertyInfo AsAgentDetailInfo => GetZPropertyInfo(nameof(AsAgentDetail));

		#endregion

		#region FreightNominee

		public ZString FreightNominee
		{
			get => freightNominee;
			set
			{
				if (SetNonPersistentPropertyValue(FreightNomineeInfo, ref freightNominee, value))
				{
				}
			}
		}
		ZString freightNominee;

		public ZPropertyInfo FreightNomineeInfo => GetZPropertyInfo(nameof(FreightNominee));

		#endregion

		#region CarrierAgent

		public ZString CarrierAgent
		{
			get => carrierAgent;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierAgentInfo, ref carrierAgent, value))
				{
				}
			}
		}
		ZString carrierAgent;

		public ZPropertyInfo CarrierAgentInfo => GetZPropertyInfo(nameof(CarrierAgent));

		#endregion

		#region ShipperLoadAndCount

		public ICodeDescription ShipperLoadAndCount
		{
			get => shipperLoadAndCount;
			set => shipperLoadAndCount = SetChild(shipperLoadAndCount, value);
		}

		ICodeDescription shipperLoadAndCount;

		#endregion

		#region INCO

		public ICodeDescription INCO
		{
			get => inco;
			set => inco = SetChild(inco, value);
		}

		ICodeDescription inco;

		#endregion

		#region ACIDNO

		public ZString ACIDNO
		{
			get => acidNO;
			set
			{
				if (SetNonPersistentPropertyValue(ACIDNOInfo, ref acidNO, value))
				{
					Validate(ACIDNOInfo);
				}
			}
		}

		ZString acidNO;

		public ZPropertyInfo ACIDNOInfo => GetZPropertyInfo(nameof(ACIDNO));

		#endregion

		#region ExportStatement

		public ZString ExportStatement
		{
			get => exportStatement;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementInfo, ref exportStatement, value))
				{
					Validate(ExportStatementInfo);
				}
			}
		}
		ZString exportStatement;

		public ZPropertyInfo ExportStatementInfo => GetZPropertyInfo(nameof(ExportStatement));

		#endregion

		#region IsDraft

		public ZBool IsDraft
		{
			get => isDraft;
			set
			{
				if (SetNonPersistentPropertyValue(IsDraftInfo, ref isDraft, value))
				{
				}
			}
		}

		ZBool isDraft;

		public ZPropertyInfo IsDraftInfo => GetZPropertyInfo(nameof(IsDraft));

		#endregion

		#region HIR Reference

		public RegistrationNumber HIRReference
		{
			get => hirReference;
			set => hirReference = SetChild(hirReference, value);
		}

		RegistrationNumber hirReference;

		#endregion

		#region TariffLineItemReference

		public ZString TariffLineItemReference
		{
			get => tariffLineItemReference;
			set
			{
				if (SetNonPersistentPropertyValue(TariffLineItemReferenceInfo, ref tariffLineItemReference, value))
				{
					Validate(TariffLineItemReferenceInfo);
				}
			}
		}
		ZString tariffLineItemReference;

		public ZPropertyInfo TariffLineItemReferenceInfo => GetZPropertyInfo(nameof(TariffLineItemReference));

		#endregion

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Container'")]
		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}
		ICodeDescription containerMode;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Release Type'")]
		public ICodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}
		ICodeDescription releaseType;

		[DocumentField(@"Look up 'Shipment -> Basic Registration -> Incoterm' setting in 'Registry -> AutoRating -> Charge Code Groups -> Incoterm Charge Code Group Configuration'. If 'Freight' is 'CNE' then it is 'CCX' (Collect), otherwise returns 'PPD' (Prepaid).")]
		public ICodeDescription PaymentTerms
		{
			get => paymentTerms;
			set => paymentTerms = SetChild(paymentTerms, value);
		}
		ICodeDescription paymentTerms;

		public ICodeDescription AsAgentOption
		{
			get => asAgentOption;
			set => asAgentOption = SetChild(asAgentOption, value);
		}
		ICodeDescription asAgentOption;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> On Board'")]
		public IShippedOnBoard ShippedOnBoard
		{
			get => shippedOnBoard;
			set => shippedOnBoard = SetChild(shippedOnBoard, value);
		}
		IShippedOnBoard shippedOnBoard;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Customs Entry Type and Number'")]
		public CustomsEntryNumber CustomsEntryNumber
		{
			get => customsEntryNumber;
			set => customsEntryNumber = SetChild(customsEntryNumber, value);
		}
		CustomsEntryNumber customsEntryNumber;

		[DocumentField("Only modifiable where it appears on the House Bill itself")]
		public IMoney DeclaredValueOfGoods
		{
			get => declaredValueOfGoods;
			set => declaredValueOfGoods = SetChild(declaredValueOfGoods, value);
		}
		IMoney declaredValueOfGoods;

		[DocumentField("Only modifiable where it appears on the House Bill itself")]
		public IMoney FreightAmount
		{
			get => freightAmount;
			set => freightAmount = SetChild(freightAmount, value);
		}
		IMoney freightAmount;

		[DocumentField("If 'Registry -> Freight -> House Bills -> House Bill of Lading Types -> House Bill of Lading Settings' is configured to 'Print Logo' for current house bill, the logo will be getting from 'Registry -> Freight -> House Bills -> House Bill of Lading Types -> Logos'")]
		public IHouseBillLogo Logo
		{
			get => logo;
			set => logo = SetChild(logo, value);
		}
		IHouseBillLogo logo;

		[DocumentField("If 'Registry -> Freight -> House Bills -> House Bill of Lading Types -> House Bill of Lading Settings' is configured to have 'Terms & Conditions' for current house bill, the Terms & Condition will be getting from 'Registry -> Freight -> House Bills -> House Bill of Lading Types -> Terms & Conditions'")]
		public IHouseBillTermsAndConditions TermsAndConditions
		{
			get => termsAndConditions;
			set => termsAndConditions = SetChild(termsAndConditions, value);
		}
		IHouseBillTermsAndConditions termsAndConditions;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Consignor'")]
		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		IAddress shipper;

		[DocumentField("Corresponding to 'Shipment -> Pickup -> Pickup From'")]
		public IAddress ConsignorPickup
		{
			get => consignorPickup;
			set => consignorPickup = SetChild(consignorPickup, value);
		}
		IAddress consignorPickup;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Consignee'")]
		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		IAddress consignee;

		[DocumentField("Corresponding to 'Shipment -> Delivery -> Deliver To'")]
		public IAddress ConsigneeDelivery
		{
			get => consigneeDelivery;
			set => consigneeDelivery = SetChild(consigneeDelivery, value);
		}
		IAddress consigneeDelivery;

		[DocumentField("Corresponding to 'Branch Form (Current) -> Branch Details -> Org. Proxy -> Address -> Main Address'")]
		public IAddress ForwardingAgent
		{
			get => forwardingAgent;
			set => forwardingAgent = SetChild(forwardingAgent, value);
		}
		IAddress forwardingAgent;

		[DocumentField("Corresponding to 'Shipment -> Delivery -> Delivery Agent -> Address -> Main Address'. Fall back to 'Consol(Departure) -> Details -> Organizations -> Receiving Agent -> Main Address'")]
		public IAddress GoodsDelivery
		{
			get => goodsDelivery;
			set => goodsDelivery = SetChild(goodsDelivery, value);
		}
		IAddress goodsDelivery;

		[DocumentField("Corresponding to 'Shipment' -> Addresses -> Notify Party'")]
		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		IAddress notifyParty;

		[DocumentField("Corresponding to 'Shipment' -> Addresses -> Notify Party 2'")]
		public IAddress NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}
		IAddress notifyParty2;

		[DocumentField("Corresponding to 'Shipment' -> Addresses -> Notify Party 3'")]
		public IAddress NotifyParty3
		{
			get => notifyParty3;
			set => notifyParty3 = SetChild(notifyParty3, value);
		}
		IAddress notifyParty3;

		[DocumentField("Corresponding to 'Consol(Departure) -> Details -> Organizations -> Sending Agent -> Main Address'")]
		public IAddress SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}
		IAddress sendingForwarder;

		[DocumentField("Corresponding to 'Consol(Departure) -> Details -> Organizations -> Receiving Agent -> Main Address'")]
		public IAddress ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}
		IAddress receivingForwarder;

		[DocumentField("Corresponding to 'Consol(Departure) -> Details -> Organizations -> Co-Load With'")]
		public IAddress ColoadWith
		{
			get => coloadWith;
			set => coloadWith = SetChild(coloadWith, value);
		}
		IAddress coloadWith;

		public IAddress ElectronicBillOfLadingShipper
		{
			get => electronicBillOfLadingShipper;
			set => electronicBillOfLadingShipper = SetChild(electronicBillOfLadingShipper, value);
		}
		IAddress electronicBillOfLadingShipper;

		public IAddress ElectronicBillOfLadingConsignee
		{
			get => electronicBillOfLadingConsignee;
			set => electronicBillOfLadingConsignee = SetChild(electronicBillOfLadingConsignee, value);
		}
		IAddress electronicBillOfLadingConsignee;

		public IAddress ElectronicBillOfLadingToOrder
		{
			get => electronicBillOfLadingToOrder;
			set => electronicBillOfLadingToOrder = SetChild(electronicBillOfLadingToOrder, value);
		}
		IAddress electronicBillOfLadingToOrder;

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		IAddress currentUser;

		public IAddress Holder
		{
			get => holder;
			set => holder = SetChild(holder, value);
		}
		IAddress holder;

		public IAddress SurrenderParty
		{
			get => surrenderParty;
			set => surrenderParty = SetChild(surrenderParty, value);
		}
		IAddress surrenderParty;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Origin'")]
		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}
		IUnloco portOfOrigin;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Destination'")]
		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}
		IUnloco portOfDestination;

		[DocumentField("If is Manufacturer House Bill of Lading, then uses Shipment -> Addresses -> Goods Manufacturer Address -> Organisation ->UNLOCO. Otherwise uses 'Shipment -> Routing -> MainLeg(First Sea Order By Ports) -> Load Port'")]
		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

		[DocumentField("Corresponding to 'Shipment -> Routing -> MainLeg(First Sea Order By Ports) -> Discharge Port'")]
		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		IUnloco portOfDischarge;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Origin'")]
		public IUnloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		IUnloco placeOfReceipt;

		[DocumentField("Corresponding to 'Shipment -> Basic Registration -> Destination'")]
		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		IUnloco placeOfDelivery;

		[DocumentField("Corresponding to 'Branch Form (Current) -> Branch Details -> Home Port'")]
		public IUnloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}
		IUnloco placeOfIssue;

		[DocumentField("If Payment Term is 'Collect', then uses Destination, otherwise uses Origin")]
		public IUnloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}
		IUnloco freightPayableAt;

		[DocumentField("Corresponding to 'Shipment -> Job Management Links'")]
		public IReadOnlyCollection<OrderDataObject> Orders
		{
			get => orders;
			set => orders = SetChildCollection(orders, value);
		}
		IReadOnlyCollection<OrderDataObject> orders;

		[DocumentField("Corresponding to 'Shipment -> Job Management Links -> Order Refs'")]
		public IReadOnlyCollection<string> OrderReferences
		{
			get => orderReferences;
			set => orderReferences = SetChildCollection(orderReferences, value);
		}
		IReadOnlyCollection<string> orderReferences;

		[DocumentField("Corresponding to 'Shipment -> Routing'")]
		public ITransports Transports
		{
			get => transports;
			set => transports = (ITransports)SetChildCollection(transports, value);
		}
		ITransports transports;

		[DocumentField("Corresponding to 'Shipment -> Additional Details -> Consolidation Details'")]
		public IForwardingConsolDataObjectsCollection Consols
		{
			get => consols;
			set => consols = (IForwardingConsolDataObjectsCollection)SetChildCollection(consols, value);
		}
		IForwardingConsolDataObjectsCollection consols;

		[DocumentField("Corresponding to 'Shipment -> Packing -> Pack Lines -> Containers related to the Packlines'")]
		public IReadOnlyCollection<IContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<IContainer> containers;

		[DocumentField("Corresponding to 'Shipment -> Packing -> Pack Lines (with no Container)'")]
		public IReadOnlyCollection<IPackingLine> LoosePackingLines
		{
			get => loosePackingLines;
			set => loosePackingLines = SetChildCollection(loosePackingLines, value);
		}
		IReadOnlyCollection<IPackingLine> loosePackingLines;

		public IReadOnlyCollection<HouseBill> SubHouseBills
		{
			get => subHouseBills;
			set => subHouseBills = SetChildCollection(subHouseBills, value);
		}
		IReadOnlyCollection<HouseBill> subHouseBills;

		[DocumentField("Corresponding to 'Shipment -> Billing -> Charges'")]
		public IChargesCollection Charges
		{
			get => charges;
			set => charges = (IChargesCollection)SetChildCollection(charges, value);
		}
		IChargesCollection charges;

		[DocumentField("Corresponding to 'Shipment -> Additional Details -> Reference Numbers'")]
		public IReadOnlyCollection<IReferenceNumber> ReferenceNumbers
		{
			get => referenceNumbers;
			set => referenceNumbers = SetChildCollection(referenceNumbers, value);
		}
		IReadOnlyCollection<IReferenceNumber> referenceNumbers;

		[DocumentField("Corresponding to 'Shipment -> Notes'")]
		public IReadOnlyCollection<INote> Notes
		{
			get => notes;
			set => notes = SetChildCollection(notes, value);
		}
		IReadOnlyCollection<INote> notes;

		#region SendingAgentTaxInfo

		public TaxInfo ShipperTaxInfo
		{
			get => shipperTaxInfo;
			set => shipperTaxInfo = SetChild(shipperTaxInfo, value);
		}
		TaxInfo shipperTaxInfo;

		#endregion

		#region ConsigneeTaxInfo

		public TaxInfo ConsigneeTaxInfo
		{
			get => consigneeTaxInfo;
			set => consigneeTaxInfo = SetChild(consigneeTaxInfo, value);
		}
		TaxInfo consigneeTaxInfo;

		#endregion

		#region ConsigneeTaxInfoOriginal

		public TaxInfo ConsigneeTaxInfoOriginal
		{
			get => consigneeTaxInfoOriginal;
			set => consigneeTaxInfoOriginal = SetChild(consigneeTaxInfoOriginal, value);
		}
		TaxInfo consigneeTaxInfoOriginal;

		#endregion

		#region NotifyPartyTaxInfo

		public TaxInfo NotifyPartyTaxInfo
		{
			get => notifyPartyTaxInfo;
			set => notifyPartyTaxInfo = SetChild(notifyPartyTaxInfo, value);
		}
		TaxInfo notifyPartyTaxInfo;

		#endregion

		#region NotifyPartyTaxInfoOriginal

		public TaxInfo NotifyPartyTaxInfoOriginal
		{
			get => notifyPartyTaxInfoOriginal;
			set => notifyPartyTaxInfoOriginal = SetChild(notifyPartyTaxInfoOriginal, value);
		}
		TaxInfo notifyPartyTaxInfoOriginal;

		#endregion

		#region IHouseBillOverrideProvider members

		string IHouseBillOverrideProvider.GoodsDetailsTextOverride => HasGoodsDetailsTextOverride
			? (string)GoodsDetailsTextOverride
			: null;

		string IHouseBillOverrideProvider.ChargesTextOverride => HasChargesTextOverride
			? (string)ChargesTextOverride
			: null;

		string IHouseBillOverrideProvider.FollowOnTextOverride => HasFollowOnTextOverride
			? (string)FollowOnTextOverride
			: null;

		#endregion

		public ZString ConsignorShipperTerminology
		{
			get => FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
		}

		public ZString AmendmentRequestID
		{
			get => amendmentRequestID;
			set => amendmentRequestID = value;
		}
		ZString amendmentRequestID;

		#region BillTerms

		public ICodeDescription BillTerms
		{
			get => billTerms;
			set => billTerms = SetChild(billTerms, value);
		}
		ICodeDescription billTerms;

		#endregion

		#region BillType

		public ICodeDescription BillType
		{
			get => billType;
			set => billType = SetChild(billType, value);
		}
		ICodeDescription billType;

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion
	}
}
