using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyHouseBill : DocDataObject, IHouseBill, IDataSourceProvider, ICustomFieldProviderProxy
	{
		public AgencyHouseBill(ZString sourceType, ZString sourceID, CustomBusinessObject customBusinessObject = null)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
			this.customBusinessObject = customBusinessObject;
		}

		readonly ZString sourceID;
		readonly ZString sourceType;

		#region IHouseBill

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

		#region IDataSourceProvider

		public ZString SourceID => sourceID;

		public ZString SourceType => sourceType;

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

		#region ICustomFieldProviderProxy

		public CustomBusinessObject CustomBusinessObject => customBusinessObject;

		readonly CustomBusinessObject customBusinessObject;

		#endregion

		#region IsOriginal

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

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ReleaseType

		public ICodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(releaseType, value);
		}

		ICodeDescription releaseType;

		#endregion

		#region OceanBillNumber

		public ZString OceanBillNumber
		{
			get => oceanBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(OceanBillNumberInfo, ref oceanBillNumber, value))
				{
				}
			}
		}

		ZString oceanBillNumber;

		public ZPropertyInfo OceanBillNumberInfo => GetZPropertyInfo(nameof(OceanBillNumber));

		#endregion

		#region Logo

		public IAgencyHouseBillLogo Logo
		{
			get => logo;
			set => logo = SetChild(logo, value);
		}

		IAgencyHouseBillLogo logo;

		#endregion

		#region TermsAndConditions

		public IAgencyHouseBillTermsAndConditions TermsAndConditions
		{
			get => termsAndConditions;
			set => termsAndConditions = SetChild(termsAndConditions, value);
		}

		IAgencyHouseBillTermsAndConditions termsAndConditions;

		#endregion

		#region Transports

		public ITransports Transports
		{
			get => transports;
			set => transports = (ITransports)SetChildCollection(transports, value);
		}

		ITransports transports;

		#endregion

		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		IAddress shipper;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		IAddress consignee;

		#endregion

		#region NotifyParty

		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		IAddress notifyParty;

		#endregion

		#region Vessel

		public ZString Vessel
		{
			get => vessel;
			set
			{
				if (SetNonPersistentPropertyValue(VesselInfo, ref vessel, value))
				{
				}
			}
		}

		ZString vessel;

		public ZPropertyInfo VesselInfo => GetZPropertyInfo(nameof(Vessel));

		#endregion

		#region Voyage

		public ZString Voyage
		{
			get => voyage;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageInfo, ref voyage, value))
				{
				}
			}
		}

		ZString voyage;

		public ZPropertyInfo VoyageInfo => GetZPropertyInfo(nameof(Voyage));

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region PortOfLoading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		IUnloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		IUnloco portOfDischarge;

		#endregion

		#region FreightPayableAt

		public IUnloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		IUnloco freightPayableAt;

		#endregion

		#region PlaceOfIssue

		public IUnloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}

		IUnloco placeOfIssue;

		#endregion

		#region PlaceOfReceipt

		public IUnloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}

		IUnloco placeOfReceipt;

		#endregion

		#region PlaceOfDelivery

		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		IUnloco placeOfDelivery;

		#endregion

		#region NumberOfOriginals

		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
					Validate(NumberOfOriginalsInfo);
				}
			}
		}

		ZInt numberOfOriginals;

		public ZPropertyInfo NumberOfOriginalsInfo => GetZPropertyInfo(nameof(NumberOfOriginals));

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

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

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

		#region TotalNumberOfPackages

		public ZInt TotalNumberOfPackages
		{
			get => totalNumberOfPackages;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNumberOfPackagesInfo, ref totalNumberOfPackages, value))
				{
				}
			}
		}

		ZInt totalNumberOfPackages;

		public ZPropertyInfo TotalNumberOfPackagesInfo => GetZPropertyInfo(nameof(TotalNumberOfPackages));

		#endregion

		#region Clause

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

		#region ShippedOnBoardDate

		public ZDateTime ShippedOnBoardDate
		{
			get => shippedOnBoardDate;
			set
			{
				if (SetNonPersistentPropertyValue(ShippedOnBoardDateInfo, ref shippedOnBoardDate, value))
				{
				}
			}
		}

		ZDateTime shippedOnBoardDate;

		public ZPropertyInfo ShippedOnBoardDateInfo => GetZPropertyInfo(nameof(ShippedOnBoardDate));

		#endregion

		#region ShippedOnBoard

		public ICodeDescription ShippedOnBoard
		{
			get => shippedOnBoard;
			set => shippedOnBoard = SetChild(shippedOnBoard, value);
		}

		ICodeDescription shippedOnBoard;

		#endregion

		#region PaymentTerm

		public ICodeDescription PaymentTerm
		{
			get => paymentTerm;
			set => paymentTerm = SetChild(paymentTerm, value);
		}

		ICodeDescription paymentTerm;

		#endregion

		#region Charges

		public IChargesCollection Charges
		{
			get => charges;
			set => charges = (IChargesCollection)SetChildCollection(charges, value);
		}

		IChargesCollection charges;

		#endregion

		#region OuterPacks

		public ZInt OuterPacks
		{
			get => outerPacks;
			set
			{
				if (SetNonPersistentPropertyValue(OuterPacksInfo, ref outerPacks, value))
				{
				}
			}
		}

		ZInt outerPacks;

		public ZPropertyInfo OuterPacksInfo => GetZPropertyInfo(nameof(OuterPacks));

		#endregion

		#region OuterPacksPackType

		public ZString OuterPacksPackType
		{
			get => outerPacksPackType;
			set
			{
				if (SetNonPersistentPropertyValue(OuterPacksPackTypeInfo, ref outerPacksPackType, value))
				{
					Validate(OuterPacksPackTypeInfo);
				}
			}
		}

		ZString outerPacksPackType;

		public ZPropertyInfo OuterPacksPackTypeInfo => GetZPropertyInfo(nameof(OuterPacksPackType));

		#endregion

		#region ArrivalDate

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

		#region ExcessValueDeclaration

		public ZString ExcessValueDeclaration
		{
			get => excessValueDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(ExcessValueDeclarationInfo, ref excessValueDeclaration, value))
				{
				}
			}
		}

		ZString excessValueDeclaration;

		public ZPropertyInfo ExcessValueDeclarationInfo => GetZPropertyInfo(nameof(ExcessValueDeclaration));

		#endregion

		#region LoosePackingLines

		public IReadOnlyCollection<PackingLine> LoosePackingLines
		{
			get => loosePackingLines;
			set => loosePackingLines = SetChildCollection(loosePackingLines, value);
		}

		IReadOnlyCollection<PackingLine> loosePackingLines;

		#endregion

		#region TopLevelPackingLines

		public IReadOnlyCollection<PackingLine> TopLevelPackingLines
		{
			get => topLevelPackingLines;
			set => topLevelPackingLines = SetChildCollection(topLevelPackingLines, value);
		}

		IReadOnlyCollection<PackingLine> topLevelPackingLines;

		#endregion
	}
}
