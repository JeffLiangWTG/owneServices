using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
using Enterprise.MasterFiles.DataTransfer.Testing;
#endif

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbParentInfoLoader : IDtbParentInfoLoader
	{
		IDtbParentInfo IDtbParentInfoLoader.Load(DtbBookingConsolidation consolidation)
		{
			return consolidation.IsParentHidden ? null : new DtbParentInfo(consolidation);
		}

		/// <summary>
		/// 2011 Parent --> DO
		///		Top Level is Consol
		///		TopLevel.Source is the Shimpemnt
		///	
		/// 2012 Parent --> DO
		///		Top Level is Shipment
		///		Top Level.ParentShipmentCollection Consol
		/// </summary>
		/// 
		public static Shipment GetConsolDO(Shipment topLevelDO, DtbBookingDirection direction)
		{
			Shipment result = null;
			if (SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				result = GetParentDataObject_2012(topLevelDO, direction);
			}
			else
			{
				result = topLevelDO;
			}

			return result;
		}

		static Shipment GetParentDataObject_2012(Shipment dataObject, DtbBookingDirection direction)
		{
			Shipment parent = null;

			var parentCollection = dataObject.ParentShipmentCollection;
			if (parentCollection != null)
			{
				parent = direction == DtbBookingDirection.PIC ? parentCollection.FirstOrDefault() : parentCollection.LastOrDefault();
			}

			return parent != null ? GetParentDataObject_2012(parent, direction) : dataObject;
		}

#if DEBUG

		IDisposable IDtbParentInfoLoader.SetDummyWriterDecider(bool isFCL, bool populateActualDates)
		{
			return DummyWithWorkflowDataContextManager.SetWriterDecider(new DummyBookingWritingDecider(isFCL, populateActualDates));
		}

		class DummyBookingWritingDecider : DummyWithWorkflowDataContextManager.DummyWriterDecider
		{
			public DummyBookingWritingDecider(bool isFCL, bool populateActualDates)
					: base()
			{
				this.IsFCL = isFCL;
				this.PopulateActualDates = populateActualDates;
			}
			readonly bool IsFCL;
			readonly bool PopulateActualDates;

			public override ITopLevelDataObjectWriter GetWriter()
			{
				return new DummyWithDtbBookingShipmentDataObjectWriter(IsFCL, PopulateActualDates);
			}
		}

#endif
	}

	public abstract class ConsolidationParentDataObjectStategy
	{
		public abstract Shipment SourceDO { get; }
		public abstract Shipment ConsolDO { get; }

		internal Lazy<IDtbBookingParent> GetParent(DtbBookingConsolidation consolidation)
		{
			Argument.NotNull(consolidation, "consolidation");
			return new Lazy<IDtbBookingParent>(() =>
			{
				IDtbBookingParent parent = null;
				var parentID = consolidation.KB_ParentID;
				var parentTableCode = consolidation.KB_ParentTableCode;
				if (!parentID.IsEmpty && !parentTableCode.IsEmpty)
				{
					parent = consolidation.Factory.Load(parentTableCode, parentID) as IDtbBookingParent;
				}
				return parent;
			});
		}
	}

	public class HasConsolidationParentDataObjectStrategy : ConsolidationParentDataObjectStategy
	{
		public HasConsolidationParentDataObjectStrategy(Shipment sourceDO, Shipment consolDO = null)
		{
			this.sourceDO = Argument.NotNull(sourceDO, "sourceDO");
			this.consolDO = consolDO;
		}

		readonly Shipment sourceDO;
		readonly Shipment consolDO;

		public override Shipment SourceDO
		{
			get { return sourceDO; }
		}

		public override Shipment ConsolDO
		{
			get { return consolDO; }
		}
	}

	class GetConsolidationParentDataObjectStrategy : ConsolidationParentDataObjectStategy
	{
		public GetConsolidationParentDataObjectStrategy(DtbBookingConsolidation consolidation)
		{
			this.Consolidation = Argument.NotNull(consolidation, "consolidation");
			parentBO = GetParent(consolidation);
		}

		readonly DtbBookingConsolidation Consolidation;

		IDtbBookingParent ParentBO => parentBO.Value;
		readonly Lazy<IDtbBookingParent> parentBO;

		// where TopLevel DO is just the top level DO, not the ultimate parent
		Shipment TopLevelDO
		{
			get
			{
				if (ParentBO != null && topLevelDO == null)
				{
					topLevelDO = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.CTG, (BusinessObject)ParentBO, DataContextType.TransportBookingConsolidation);
				}

				return topLevelDO;
			}
		}

		Shipment topLevelDO;

		public override Shipment SourceDO
		{
			get
			{
				if (TopLevelDO != null && parentDO == null)
				{
					parentDO = Shipment.GetSourceDataObject(TopLevelDO);
				}

				return parentDO;
			}
		}

		Shipment parentDO;

		public override Shipment ConsolDO
		{
			get { return consolDO ?? (consolDO = DtbParentInfoLoader.GetConsolDO(TopLevelDO, Consolidation.Direction)); }
		}
		Shipment consolDO;
	}

	class DtbParentInfo : IDtbParentInfo
	{
		public DtbParentInfo(DtbBookingConsolidation consolidation)
				: this(consolidation, new GetConsolidationParentDataObjectStrategy(consolidation))
		{
		}

		public DtbParentInfo(DtbBookingConsolidation consolidation, ConsolidationParentDataObjectStategy getDOStrategy)
		{
			this.Consolidation = Argument.NotNull(consolidation, "consolidation");
			this.GetDOStrategy = Argument.NotNull(getDOStrategy, "getDOStrategy");
			parentBO = GetDOStrategy.GetParent(Consolidation);
		}

		readonly DtbBookingConsolidation Consolidation;
		readonly ConsolidationParentDataObjectStategy GetDOStrategy;

		IDtbBookingParent ParentBO => parentBO.Value;
		readonly Lazy<IDtbBookingParent> parentBO;

		public Shipment SourceDO
		{
			get { return GetDOStrategy.SourceDO; }
		}

		public Shipment ConsolDO
		{
			get { return GetDOStrategy.ConsolDO; }
		}

		ZBool IsOrigin
		{
			get { return Consolidation.Direction == DtbBookingDirection.PIC; }
		}

		ZGuid IDtbParentInfo.PK
		{
			get { return ParentBO != null ? ParentBO.PK : ZGuid.Empty; }
		}

		string IDtbParentInfo.TablePrefix
		{
			get { return ParentBO != null ? ParentBO.TablePrefix : ""; }
		}

		bool IDtbParentInfo.HasChanges
		{
			get { return ParentBO != null && ParentBO.HasChanges; }
		}

		ZString IDtbParentInfo.HumanReadableName
		{
			get { return ParentBO != null ? ParentBO.HumanReadableName : ZString.Empty; }
		}

		bool IDtbParentInfo.IsInDatabase
		{
			get { return ParentBO != null && ParentBO.IsInDatabase; }
		}

		BusinessObjectFactory IDtbParentInfo.Factory
		{
			get { return Consolidation != null ? Consolidation.Factory : null; }
		}

		ControllerID IDtbParentInfo.ControllerID
		{
			get { return ParentBO != null ? ParentBO.ControllerID : null; }
		}

		IJobInvoicingPlugIn IDtbParentInfo.InvoicingJob
		{
			get { return ParentBO != null ? ParentBO.InvoicingJob : null; }
		}

		ZString IDtbParentInfo.JobNumber
		{
			get { return ParentBO != null ? ParentBO.JobNumber : ZString.Empty; }
		}

		ZString IDtbParentInfo.JobDescription
		{
			get { return ParentBO != null ? ParentBO.JobTypeDescription : ZString.Empty; }
		}

		ZString IDtbParentInfo.JobType
		{
			get { return ParentBO != null ? ParentBO.JobType : ZString.Empty; }
		}

		ZString IDtbParentInfo.ClientServiceLevel
		{
			get { return SourceDO != null ? SourceDO.ServiceLevel.GetCodeAsUpperCase() : null; }
		}

		ZString? IDtbParentInfo.TransportMode
		{
			get { return SourceDO != null ? SourceDO.TransportMode.GetNullableCodeAsUpperCase() : null; }
		}

		ZString? IDtbParentInfo.ContainerMode
		{
			get { return SourceDO != null ? SourceDO.ContainerMode.GetNullableCodeAsUpperCase() : null; }
		}

		ZString? IDtbParentInfo.CarrierServiceLevel
		{
			get
			{
				var result = (ZString?)null;

				if (SourceDO != null)
				{
					result = SourceDO.CarrierServiceLevel.GetNullableCodeAsUpperCase();

					if (result.GetValueOrDefault().IsEmpty)
					{
						result = DtbDataObjectExtractor.GetCarrierServiceLevelFromRouting(SourceDO, IsOrigin).GetNullableCodeAsUpperCase();
					}
				}

				return result;
			}
		}

		ZString? IDtbParentInfo.TransportReference
		{
			get
			{
				var result = (ZString?)null;

				if (SourceDO != null)
				{
					if (SourceDO.LocalProcessing != null)
					{
						if (IsOrigin)
						{
							result = "";
						}
						else
						{
							result = SourceDO.LocalProcessing.ArrivalCartageRef;
						}
					}

					if (result.GetValueOrDefault().IsEmpty && SourceDO.Order != null) // warehouse
					{
						result = SourceDO.Order.TransportReference;
					}

					if (result.GetValueOrDefault().IsEmpty) // agency
					{
						result = DtbDataObjectExtractor.GetCarrierReferenceFromRouting(SourceDO, IsOrigin);
					}
				}

				return result;
			}
		}

		ZString? IDtbParentInfo.DropMode
		{
			get
			{
				ZString? result = null;

				if (SourceDO != null)
				{
					ZString code;
					if (!SourceDO.LocalTransportEquipmentNeeded.TryGetCodeAsUpperCase(out code))
					{
						if (SourceDO.LocalProcessing != null)
						{
							if (IsOrigin)
							{
								result = SourceDO.LocalProcessing.FCLPickupEquipmentNeeded.GetNullableCodeAsUpperCase();
							}
							else
							{
								result = SourceDO.LocalProcessing.FCLDeliveryEquipmentNeeded.GetNullableCodeAsUpperCase();
							}
						}
					}
					else
					{
						result = code;
					}

					if (!result.HasValue && SourceDO.Order != null)
					{
						SourceDO.Order.DropMode.TryGetCodeAsUpperCase(out code);
						result = code;
					}
				}

				return result;
			}
		}

		void IDtbParentInfo.UpdateAddress(JobDocAddress docAddressToUpdate, bool allowOverride, IEnumerable<ZInt> containerLinksFilter, IEnumerable<ZString> containerNumbersFilter)
		{
			UpdateAddressCore(docAddressToUpdate, allowOverride, containerLinksFilter, containerNumbersFilter);
		}

		void UpdateAddressCore(JobDocAddress docAddressToUpdate, bool allowOverride, IEnumerable<ZInt> containerLinksFilter, IEnumerable<ZString> containerNumbersFilter)
		{
			var addressDO = DtbDataObjectExtractor.GetAddressDO(SourceDO, docAddressToUpdate.DocAddressType, Consolidation.Direction, ConsolDO, containerLinksFilter: containerLinksFilter, containerNumbersFilter: containerNumbersFilter);

			if (addressDO != null)
			{
				var reader = new OrganisationDataObjectReader(addressDO, DummyLogger, DummyFactory);
				var orgAddress = reader.GetMatched(true);
				if (orgAddress != null || allowOverride)
				{
					reader.PopulateJobDocAddress(orgAddress, docAddressToUpdate);
				}
			}

			// should we blank out address if parent doesn't have it?
		}

		DatesAndReference IDtbParentInfo.GetDatesAndReferences(ZString confirmationTypeCode, ZString addressType, IPkgPackage packageID, IReadOnlyDictionary<PkgPackage, ZString> releaseNumbersByPackage)
		{
			var releaseNumber = releaseNumbersByPackage != null && releaseNumbersByPackage.TryGetValue((PkgPackage)packageID, out var releaseNumFromDict)
			   ? releaseNumFromDict
			   : ZString.Empty;

			if (IsOrigin)
			{
				return GetOriginDatesAndReferences(confirmationTypeCode, addressType, packageID, releaseNumber);
			}
			else
			{
				return GetDestinationDatesAndReferences(confirmationTypeCode, addressType, packageID, releaseNumber);
			}
		}

		DatesAndReference GetOriginDatesAndReferences(ZString confirmationTypeCode, ZString addressType, IPkgPackage iPackage, ZString releaseNumber)
		{
			// KP_F3_NKPackType
			// KP_PackageID
			var package = (PkgPackage)iPackage;

			var isContainer = package != null && package.KP_F3_NKPackType == "CNT";
			var isPickup = confirmationTypeCode == ConfirmationTypes.Codes.PickUp;
			var isDelivery = confirmationTypeCode == ConfirmationTypes.Codes.Delivery;
			var isCYD = addressType == "CYD";
			var isCFS = addressType == "CFS";
			var isCNR = addressType == "CNR";
			var isCTO = addressType == "CTO";
			var isWHS = addressType == "WHS";

			var localDO = SourceDO?.LocalProcessing ?? new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			var transportDO = GetParentRoutingLeg();
			var orderDateCollection = SourceDO?.Order?.DateCollection ?? new List<Date>();

			DatesAndReference result;
			if (isContainer)
			{
				result = GetOriginDatesAndReferencesForContainer(releaseNumber, package, isPickup, isDelivery, isCYD, isCFS, isCNR, isCTO, isWHS, localDO, transportDO, orderDateCollection);
			}
			else
			{
				result = GetOriginDatesAndReferencesForNonContainer(isPickup, isDelivery, isCFS, isCNR, isWHS, localDO, transportDO, orderDateCollection);
			}

			return result;
		}

		DatesAndReference GetOriginDatesAndReferencesForNonContainer(bool isPickup, bool isDelivery, bool isCFS, bool isCNR, bool isWHS, LocalProcessing localDO, TransportLeg transportDO, List<Date> orderDateCollection)
		{
			var result = DatesAndReference.Empty;

			if (isPickup)
			{
				if (isCNR)
				{
					var estimated = GetValidDate(localDO.EstimatedPickup, GetDateFromList(orderDateCollection, DateType.Departure, isEst: true));
					var actual = GetValidDate(localDO.PickupCartageCompleted);
					var reqFrom = GetValidDate(localDO.PickupRequiredFrom);
					var reqTo = GetValidDate(localDO.PickupRequiredBy);

					result = new DatesAndReference(estimated, actual, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
				}
			}
			else if (isDelivery)
			{
				if (isCFS)
				{
					var reqFrom = GetValidDate(transportDO.LCLReceivalCommences);
					var reqTo = GetValidDate(transportDO.LCLCutOff);

					result = new DatesAndReference(ZDateTime.Empty, ZDateTime.Empty, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
				}
				else if (isWHS)
				{
					var estimated = GetValidDate(GetDateFromList(orderDateCollection, DateType.Arrival, isEst: true));
					var actual = GetValidDate(GetDateFromList(orderDateCollection, DateType.Arrival));

					result = new DatesAndReference(estimated, actual, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
				}
			}

			return result;
		}

		DatesAndReference GetOriginDatesAndReferencesForContainer(ZString releaseNumber, PkgPackage package, bool isPickup, bool isDelivery, bool isCYD, bool isCFS, bool isCNR, bool isCTO, bool isWHS, LocalProcessing localDO, TransportLeg transportDO, List<Date> orderDateCollection)
		{
			var result = DatesAndReference.Empty;

			var container = GetContainer(package, releaseNumber);
			if (container != null)
			{
				if (isPickup)
				{
					if (isCYD)
					{
						var actual = GetValidDate(container.ContainerParkEmptyPickupGateOut);
						var reqTo = GetValidDate(container.EmptyRequired);
						var reference = container.ReleaseNum ?? ZString.Empty;

						result = new DatesAndReference(ZDateTime.Empty, actual, ZDateTime.Empty, reqTo, reference, ZDateTime.Empty, ZString.Empty);
					}
					else if (isCNR)
					{
						var estimated = GetValidDate(container.DepartureEstimatedPickup, localDO.EstimatedPickup, GetDateFromList(orderDateCollection, DateType.Departure, isEst: true));
						var actual = GetValidDate(container.DepartureCartageComplete, localDO.PickupCartageCompleted);
						var reqTo = GetValidDate(localDO.PickupRequiredBy);
						var reqFrom = GetValidDate(localDO.PickupRequiredFrom);

						result = new DatesAndReference(estimated, actual, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
					}
				}
				else if (isDelivery)
				{
					if (isCNR)
					{
						var reqTo = GetValidDate(container.EmptyRequired);
						result = new DatesAndReference(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, reqTo, "", ZDateTime.Empty, ZString.Empty);
					}
					else if (isCTO)
					{
						var actual = GetValidDate(container.FCLWharfGateIn);
						var reqFrom = GetValidDate(transportDO.FCLReceivalCommences);
						var reqTo = GetValidDate(transportDO.FCLCutOff);
						var slotDate = GetValidDate(container.DepartureSlotDateTime);
						var slotReference = container.DepartureSlotReference ?? ZString.Empty;

						var reference = container.ReleaseNum ?? ZString.Empty;

						result = new DatesAndReference(ZDateTime.Empty, actual, reqFrom, reqTo, reference, slotDate, slotReference);
					}
					else if (isWHS)
					{
						var estimated = GetValidDate(GetDateFromList(orderDateCollection, DateType.Arrival, isEst: true));
						var actual = GetValidDate(GetDateFromList(orderDateCollection, DateType.Arrival));

						result = new DatesAndReference(estimated, actual, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
					}
				}
			}

			return result;
		}

		DatesAndReference GetDestinationDatesAndReferences(ZString confirmationTypeCode, ZString addressType, IPkgPackage iPackage, ZString releaseNumber)
		{
			// KP_F3_NKPackType
			// KP_PackageID
			var package = (PkgPackage)iPackage;

			var isContainer = package != null && package.KP_F3_NKPackType == "CNT";
			var isPickup = confirmationTypeCode == ConfirmationTypes.Codes.PickUp;
			var isDelivery = confirmationTypeCode == ConfirmationTypes.Codes.Delivery;
			var isCTO = addressType == "CTO";
			var isCFS = addressType == "CFS";
			var isCNE = addressType == "CNE";
			var isCYD = addressType == "CYD";

			var localDO = SourceDO?.LocalProcessing ?? new LocalProcessing(DefaultDataObjectWriterStrategy.Instance);
			var transportDO = GetParentRoutingLeg();

			var result = DatesAndReference.Empty;

			if (isContainer)
			{
				var container = GetContainer(package, releaseNumber);
				if (container != null)
				{
					if (isPickup)
					{
						if (isCTO)
						{
							var actual = GetValidDate(container.FCLWharfGateOut);
							var reqFrom = GetValidDate(container.FCLAvailable, localDO.FCLAvailable, transportDO.FCLAvailability);
							var reqTo = GetValidDate(container.FCLStorageCommences, localDO.FCLStorageCommences, transportDO.FCLStorage);
							var reference = container.ContainerImportDORelease ?? ZString.Empty;
							var slotReference = container.ArrivalSlotReference ?? ZString.Empty;
							var slotDate = GetValidDate(container.ArrivalSlotDateTime);
							result = new DatesAndReference(ZDateTime.Empty, actual, reqFrom, reqTo, reference, slotDate, slotReference);
						}
						else if (isCNE)
						{
							var reqFrom = GetValidDate(container.EmptyReadyForReturn);

							result = new DatesAndReference(ZDate.Empty, ZDateTime.Empty, reqFrom, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
						}
					}
					else if (isDelivery)
					{
						if (isCNE)
						{
							var estimated = GetValidDate(container.ArrivalEstimatedDelivery, localDO.EstimatedDelivery);
							var actual = GetValidDate(container.ArrivalCartageComplete, localDO.DeliveryCartageCompleted);
							var reqFrom = GetValidDate(localDO.DeliveryRequiredFrom);
							var reqTo = GetValidDate(localDO.DeliveryRequiredBy);

							result = new DatesAndReference(estimated, actual, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
						}
						else if (isCYD)
						{
							var actual = GetValidDate(container.ContainerParkEmptyReturnGateIn);
							var reqTo = GetValidDate(container.EmptyReturnedBy);
							var reference = container.EmptyReturnRef ?? ZString.Empty;

							result = new DatesAndReference(ZDate.Empty, actual, ZDate.Empty, reqTo, reference, ZDateTime.Empty, ZString.Empty);
						}
					}
				}
			}
			else
			{
				if (isPickup)
				{
					if (isCFS)
					{
						var reqFrom = GetValidDate(localDO.LCLAvailable, transportDO.LCLAvailability);
						var reqTo = GetValidDate(localDO.LCLStorageCommences, transportDO.LCLStorageDate);

						result = new DatesAndReference(ZDateTime.Empty, ZDateTime.Empty, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
					}
				}
				else if (isDelivery)
				{
					if (isCNE)
					{
						var estimated = GetValidDate(localDO.EstimatedDelivery);
						var actual = GetValidDate(localDO.DeliveryCartageCompleted);
						var reqFrom = GetValidDate(localDO.DeliveryRequiredFrom);
						var reqTo = GetValidDate(localDO.DeliveryRequiredBy);

						result = new DatesAndReference(estimated, actual, reqFrom, reqTo, "", ZDateTime.Empty, ZString.Empty);
					}
				}
			}

			return result;
		}

		ZDateTime GetValidDate(params ZDateTime?[] dates)
		{
			var first = dates.FirstOrDefault(d => d.HasValue && d.Value.IsValid);
			return first ?? ZDateTime.Empty;
		}

		ZDateTime GetDateFromList(List<Date> listOfDates, DateType dateType, bool isEst = false)
		{
			var date = listOfDates.FirstOrDefault(d => (d.Type.Equals(dateType) && ((d.IsEstimate ?? false) == isEst)));
			return (date != null && date.Value.HasValue) ? (ZDateTime)date.Value : ZDateTime.Empty;
		}

		TransportLeg GetParentRoutingLeg()
		{
			TransportLeg result = null;

			var transports = GetTransportLegs();
			if (transports != null)
			{
				if (IsOrigin)
				{
					result = transports.FirstOrDefault(l => !l.LegType.Equals(LegType.LocalTransport));
				}
				else
				{
					result = transports.LastOrDefault(l => !l.LegType.Equals(LegType.LocalTransport));
				}
			}

			return result ?? new TransportLeg(DefaultDataObjectWriterStrategy.Instance);
		}

		DataObjectList<TransportLeg> GetTransportLegs()
		{
			var transports = SourceDO?.TransportLegCollection;
			if (transports == null)
			{
				var consolDO = ConsolDO;
				if (consolDO != null)
				{
					transports = consolDO.TransportLegCollection;
				}
			}
			return transports;
		}

		Container GetContainer(PkgPackage package, ZString releaseNumber)
		{
			var container = GetContainer(package, SourceDO?.ContainerCollection, releaseNumber);
			if (container == null)
			{
				var consolDO = ConsolDO;
				if (consolDO != null)
				{
					container = GetContainer(package, consolDO.ContainerCollection, releaseNumber);
				}
			}
			return container;
		}

		Container GetContainer(PkgPackage package, DataObjectList<Container> containerCollection, ZString releaseNumber)
		{
			return containerCollection != null ? containerCollection.FirstOrDefault(c => ContainerMatching(c, package, releaseNumber)) : null;
		}

		bool ContainerMatching(Container container, PkgPackage package, ZString releaseNumber)
		{
			return (!package.KP_PackageID.IsEmpty && container.ContainerNumber.Equals(package.KP_PackageID.ToUpper()))
				|| (!releaseNumber.IsEmpty && container.ReleaseNum.Equals(releaseNumber));
		}

		//#region Events

		//void IDtbParentInfo.OnBookingConsolidationPickupComplete(ZDateTime completionDate)
		//{
		//    //throw new NotImplementedException();
		//}

		//void IDtbParentInfo.OnBookingConsolidationDeliveryComplete(ZDateTime completionDate, ZString signedBy)
		//{
		//    //throw new NotImplementedException();
		//}

		//#endregion

		BusinessObject IDtbParentInfo.ParentWithWorkflow
		{
			get { return (BusinessObject)ParentBO; }
		}

		DummyLogger DummyLogger
		{
			get { return dummyLogger ?? (dummyLogger = new DummyLogger()); }
		}

		DummyLogger dummyLogger;

		UniversalObjectFactory DummyFactory
		{
			get { return dummyFactory ?? (dummyFactory = new UniversalObjectFactory()); }
		}

		UniversalObjectFactory dummyFactory;

		public bool SupportsDirectSailing
		{
			get { return ParentBO != null && ParentBO.IsSupportsDirectSchedule; }
		}

		public DataObjectList<TransportLeg> GetParentRoutingTransportLegs()
		{
			return SourceDO?.TransportLegCollection;
		}
	}
}
