using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public abstract class CommonCartageValueObjectDataAdapter : FreightValueObjectDataAdapter<CommonCartage, Xsd.CartageJob>
	{
		public override string RootCollectionElementName
		{
			get { return "CartageJobs"; }
		}

		public override string RootElementName
		{
			get { return "CartageJob"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleCartageJobSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.CartageJobsSchema; }
		}

		protected override string GetXmlNamespace()
		{
			return "";
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return false; }
		}

		public Xsd.XmlInterchange GetXMLIntechangeWithTargetType(CommonCartage cartage, NotificationBuffer buffer)
		{
			var interchange = ToXmlInterchange(new CommonCartage[] { cartage }, new ValueObjectExportContext(buffer));
			interchange.InterchangeInfo.Target.Type = TargetType;
			interchange.InterchangeInfo.Target.TypeSpecified = true;
			return interchange;
		}

		public abstract Xsd.InterchangeInfoTargetType TargetType { get; }

		protected override void ExportToValueObjectCore(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			if (cartage != null)
			{
				xsdCartage.Type = CartageJobHeaderType;
				xsdCartage.JobType = cartage.JJ_E3_NKJobType;
				xsdCartage.JobNumber = cartage.JJ_ConsignmentID != "" ? cartage.JJ_ConsignmentID.ToString() : ((IJobNumber)cartage).JobNumber;
				xsdCartage.DropMode = cartage.JJ_DropMode;
				xsdCartage.Action = BookingAction;
				xsdCartage.ActionType = cartage.BookingInformation.CalculatedBookingStatus;
				xsdCartage.MessageDescription = cartage.BookingInformation.BookingComment;
				xsdCartage.MessageResponseAddress = Env.Registry.MailboxEmailAddress;
				xsdCartage.MessageSystemType = BrandingFactory.Instance.ProductName;

				xsdCartage.GoodsDescription = cartage.JJ_GoodsDescription;
				xsdCartage.ClientJobReference = GetJobReference(cartage);

				OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy ?? GlbBranch.CurrentBranch.OrgProxy;
				xsdCartage.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(orgProxy, context);

				if (cartage.HasParent && cartage.CartageInternalType != null && cartage.CartageParent != null)
				{
					OrgHeader cartageOrg = cartage.CartageInternalType.LocalTransportProviderAddress != null ? cartage.CartageInternalType.LocalTransportProviderAddress.Header : null;
					if (cartageOrg != null)
					{
						xsdCartage.CartageOrg = new OrganisationValueObjectDataAdapter().ExportToValueObject(cartageOrg, context);
					}

					OrgHeader carrier = GetCarrier(cartage.CartageParent);
					if (carrier != null)
					{
						xsdCartage.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(carrier, context);
					}

					ExportCustomAttributes(cartage.CartageParent, xsdCartage);
					ExportConsigneeConsignor(cartage.CartageParent, xsdCartage, context);
				}

				if (cartage.IsContainerised)
				{
					ExportContainers(cartage, xsdCartage, context);

					xsdCartage.OuterPacks.OuterPacksWeight.IsSpecified = true;
					xsdCartage.OuterPacks.OuterPacksVolume.IsSpecified = true;
				}
				else
				{
					ExportPackages(cartage, xsdCartage, context);
				}

				ExportSailingInfo(cartage, xsdCartage, context);
				ExportNotes(cartage, xsdCartage, context);
			}
		}

		void ExportConsigneeConsignor(ICartageParent cartageParent, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			ICartageParentExtra cartageParentExtra = cartageParent as ICartageParentExtra;
			if (cartageParentExtra != null)
			{
				xsdCartage.Consignee = new DocAddressValueObjectHelper((NoResString)"Consignee DocAddress").ExportToValueObject(cartageParentExtra.ConsigneeDocumentaryAddress, context);
				xsdCartage.Consignor = new DocAddressValueObjectHelper((NoResString)"Consignor DocAddress").ExportToValueObject(cartageParentExtra.ConsignorDocumentaryAddress, context);
			}
		}

		void ExportCustomAttributes(ICartageParent cartageParent, Xsd.CartageJob xsdCartage)
		{
			ICartageParentExtra cartageParentExtra = cartageParent as ICartageParentExtra;
			if (cartageParentExtra != null)
			{
				xsdCartage.CustomAttributes.CustomAttr1 = cartageParentExtra.CustomAttrib1;
				xsdCartage.CustomAttributes.CustomAttr2 = cartageParentExtra.CustomAttrib2;
				xsdCartage.CustomAttributes.CustomDate1 = cartageParentExtra.CustomDate1;
				xsdCartage.CustomAttributes.CustomDate2 = cartageParentExtra.CustomDate2;
				if (!cartageParentExtra.CustomDecimal1.IsEmpty)
				{
					xsdCartage.CustomAttributes.CustomDecimal1 = cartageParentExtra.CustomDecimal1;
				}
				if (!cartageParentExtra.CustomDecimal2.IsEmpty)
				{
					xsdCartage.CustomAttributes.CustomDecimal2 = cartageParentExtra.CustomDecimal2;
				}
				xsdCartage.CustomAttributes.CustomFlag1 = cartageParentExtra.CustomFlag1;
				xsdCartage.CustomAttributes.CustomFlag2 = cartageParentExtra.CustomFlag2;
			}
		}

		OrgHeader GetCarrier(ICartageParent cartageParent)
		{
			ILocalShippingLineProvider lineProvider = cartageParent as ILocalShippingLineProvider;
			return lineProvider != null ? lineProvider.ShippingLine : null;
		}

		void ExportContainers(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			foreach (CommonContainer container in cartage.Containers)
			{
				foreach (CommonBookedCtgMove move in cartage.GetBookedMoves(container))
				{
					foreach (CommonCartageLeg leg in move.CartageLegs)
					{
						Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs.AddNew();
						ExportCartageLegInformation(leg, xsdLeg, context);
						ExportContainerInformation(cartage, container, xsdLeg, context);
					}
				}
			}
		}

		void ExportContainerInformation(CommonCartage cartage, CommonContainer container, Xsd.CartageLeg xsdLeg, IValueObjectExportContext context)
		{
			Xsd.CartageLegContainer xsdContainer = new Xsd.CartageLegContainer();
			xsdContainer.ContainerNumber = container.JC_ContainerNum;
			xsdContainer.ContainerType = new ContainerTypeValueObjectDataAdapter().ExportToValueObject(container.RefContainer, context);
			xsdContainer.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(container.JC_ContainerMode, "CommonContainer " + container.JC_ContainerNum, context);
			xsdContainer.Seal = container.JC_SealNum;
			xsdContainer.AirVentFlow = container.JC_AirVentFlow;
			xsdContainer.AirVentFlowSpecified = container.JC_AirVentFlow > 0;
			xsdContainer.AirVentFlowRateUnit = container.JC_AirVentFlowRateUnit;
			xsdContainer.HumidityPercent = container.JC_HumidityPercent;
			xsdContainer.HumidityPercentSpecified = container.JC_HumidityPercent > 0;
			xsdContainer.SetPointTemperature = container.JC_SetPointTemp;
			xsdContainer.SetPointTemperatureSpecified = container.JC_SetPointTemp != 0;
			xsdContainer.SetPointTemperatureUnit = container.JC_SetPointTempUnit;

			if (container.JC_ArrivalEstimatedDelivery.IsValid)
			{
				xsdContainer.EstimatedDelivery = container.JC_ArrivalEstimatedDelivery;
			}

			ZDateTime alternativeFCLAvailable = ZDateTime.Empty;
			ZDateTime alternativeFCLStorage = ZDateTime.Empty;
			ZDateTime alternativeLCLAvailable = ZDateTime.Empty;
			ZDateTime alternativeLCLStorage = ZDateTime.Empty;
			if (cartage.HasParent && cartage.CartageParent != null && cartage.CartageInternalType != null)
			{
				alternativeFCLAvailable = cartage.CartageInternalType.FCLAvailabilityDate;
				alternativeFCLStorage = cartage.CartageInternalType.FCLStorageDate;
				alternativeLCLAvailable = cartage.CartageInternalType.LCLAvailabilityDate;
				alternativeLCLStorage = cartage.CartageInternalType.LCLStorageDate;
			}

			if (container.JC_LCLAvailable.IsValid)
			{
				xsdContainer.LCLAvailable = container.JC_LCLAvailable;
			}
			else if (alternativeLCLAvailable.IsValid)
			{
				xsdContainer.LCLAvailable = alternativeLCLAvailable;
			}

			if (container.JC_FCLAvailable.IsValid)
			{
				xsdContainer.FCLAvailable = container.JC_FCLAvailable;
			}
			else if (alternativeFCLAvailable.IsValid)
			{
				xsdContainer.FCLAvailable = alternativeFCLAvailable;
			}

			xsdContainer.ContainerAdditionalInfo.AdditionalSealNo = container.JC_AdditionalSealNum;
			xsdContainer.ContainerAdditionalInfo.GrossWeight.Value = container.JC_GrossWeight;
			xsdContainer.ContainerAdditionalInfo.GrossWeight.DimensionType = container.JC_GrossWeightUQ;
			xsdContainer.ContainerAdditionalInfo.TareWeight.Value = container.JC_TareWeight;
			xsdContainer.ContainerAdditionalInfo.TareWeight.DimensionType = container.JC_GrossWeightUQ;
			xsdContainer.ContainerAdditionalInfo.TempRecorderNo = container.JC_TempRecorderSerialNo;
			xsdContainer.ContainerAdditionalInfo.ReleaseNo = container.JC_ReleaseNum;

			var containerMovement = cartage.GetBookedMoves(container).First();
			if (containerMovement != null)
			{
				xsdContainer.ContainerAdditionalInfo.DropMode = containerMovement.EW_DropMode;
			}

			if (container.JC_EmptyRequired.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.EmptyRequiredDate = container.JC_EmptyRequired;
			}

			if (container.JC_PackDate.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.PackDate = container.JC_PackDate;
			}

			if (container.JC_DepartureSlotDateTime.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.DepartureSlotDate = container.JC_DepartureSlotDateTime;
			}
			xsdContainer.ContainerAdditionalInfo.DepartureSlotRef = container.JC_DepartureSlotReference;

			if (container.JC_ArrivalSlotDateTime.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.ArrivalSlotDate = container.JC_ArrivalSlotDateTime;
			}
			xsdContainer.ContainerAdditionalInfo.ArrivalSlotRef = container.JC_ArrivalSlotReference;

			if (container.JC_LCLStorageCommences.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.LCLStorageDate = container.JC_LCLStorageCommences;
			}
			else if (alternativeLCLStorage.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.LCLStorageDate = alternativeLCLStorage;
			}

			if (container.JC_ArrivalCTOStorageStartDate.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.FCLStorageDate = container.JC_ArrivalCTOStorageStartDate;
			}
			else if (alternativeFCLStorage.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.FCLStorageDate = alternativeFCLStorage;
			}

			if (container.JC_LCLUnpack.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.PackDate = container.JC_LCLUnpack;
			}

			if (container.JC_EmptyReturnedBy.IsValid)
			{
				xsdContainer.ContainerAdditionalInfo.EmptyReturnedByDate = container.JC_EmptyReturnedBy;
			}

			xsdLeg.Item = xsdContainer;
		}

		void ExportPackages(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			foreach (CommonBookedCtgMove move in cartage.LooseBookedMoves)
			{
				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs.AddNew();
					ExportCartageLegInformation(leg, xsdLeg, context);
					ExportPackageInformation(leg, xsdLeg, context);
				}
			}
			ExportOuterPacks(cartage, xsdCartage, context);
		}

		void ExportPackageInformation(CommonCartageLeg leg, Xsd.CartageLeg xsdLeg, IValueObjectExportContext context)
		{
			CommonBookedCtgMove looseMove = leg.BookedCtgMove;

			BusinessObjectFactory dummyFactory = new BusinessObjectFactory();
			PackLine dummyPackline = dummyFactory.New<PackLine>();

			dummyPackline.JL_Height = looseMove.EW_BookedHeight;
			dummyPackline.JL_Length = looseMove.EW_BookedLength;
			dummyPackline.JL_Width = looseMove.EW_BookedWidth;
			dummyPackline.JL_UnitOfDimension = looseMove.EW_DimUnit;

			dummyPackline.JL_ActualVolume = looseMove.EW_BookedVolume;
			dummyPackline.JL_ActualVolumeUQ = looseMove.EW_VolumeUQ;

			dummyPackline.JL_ActualWeight = looseMove.EW_BookedWeight;
			dummyPackline.JL_ActualWeightUQ = looseMove.EW_WeightUQ;

			dummyPackline.JL_PackageCount = looseMove.EW_BookedPackCount;
			dummyPackline.JL_F3_NKPackType = looseMove.EW_F3_NKPackType;

			foreach (UNDGDataItem dgDataItem in looseMove.UNDGs)
			{
				UNDGDataItem packLineDGDataItem = dummyPackline.UNDGs.AddNew();
				packLineDGDataItem.DI_DG = dgDataItem.DI_DG;
				packLineDGDataItem.DI_DGFlashPoint = dgDataItem.DI_DGFlashPoint;
				packLineDGDataItem.DI_OC_DGContact = dgDataItem.DI_OC_DGContact;
			}

			Xsd.CartageLegPackageRecords xsdPacks = new Xsd.CartageLegPackageRecords();
			Xsd.Package xsdNewPackage = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>().ExportToValueObject(dummyPackline, context);
			xsdPacks.Packs.Add(xsdNewPackage);

			xsdLeg.Item = xsdPacks;
		}

		void ExportOuterPacks(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			if (cartage.JJ_OuterPacks != 0)
			{
				xsdCartage.OuterPacks.OuterPacksType = PkgUnitXmlCodeMappings.Instance.GetExternalCode(cartage.JJ_F3_NKPackType, Res.GetString("b7ec59db-88eb-489a-ae11-8d32848be079", "Outer Packs Type"), context);
				xsdCartage.OuterPacks.OuterPacksNo = cartage.JJ_OuterPacks;
				xsdCartage.OuterPacks.OuterPacksNoSpecified = true;
			}

			if (cartage.JJ_Weight != 0)
			{
				xsdCartage.OuterPacks.OuterPacksWeight = new Xsd.DimensionValue();
				xsdCartage.OuterPacks.OuterPacksWeight.DimensionType = cartage.JJ_WeightUQ;
				xsdCartage.OuterPacks.OuterPacksWeight.Value = cartage.JJ_Weight;
			}
			else
			{
				xsdCartage.OuterPacks.OuterPacksWeight.IsSpecified = true;
			}

			if (cartage.JJ_Volume != 0)
			{
				xsdCartage.OuterPacks.OuterPacksVolume = new Xsd.DimensionValue();
				xsdCartage.OuterPacks.OuterPacksVolume.DimensionType = cartage.JJ_VolumeUQ;
				xsdCartage.OuterPacks.OuterPacksVolume.Value = cartage.JJ_Volume;
			}
			else
			{
				xsdCartage.OuterPacks.OuterPacksVolume.IsSpecified = true;
			}
		}

		void ExportCartageLegInformation(CommonCartageLeg leg, Xsd.CartageLeg xsdLeg, IValueObjectExportContext context)
		{
			xsdLeg.Type = CartageJobLegType;
			xsdLeg.LegStatus = leg.JU_Status;

			if (leg.PickupFromDocAddress != null)
			{
				xsdLeg.Pickup.DocAddress = new DocAddressValueObjectHelper((NoResString)"Pickup").ExportToValueObject(leg.PickupFromDocAddress, context);
				ExportAdditionalInstructions(xsdLeg.Pickup, leg.PickupFromDocAddress);
			}

			if (leg.DeliverToDocAddress != null)
			{
				xsdLeg.Delivery.DocAddress = new DocAddressValueObjectHelper((NoResString)"Delivery").ExportToValueObject(leg.DeliverToDocAddress, context);
				ExportAdditionalInstructions(xsdLeg.Delivery, leg.DeliverToDocAddress);
			}

			ZDateTime estimatedPickupDate = leg.JU_PlannedPickupTime;
			ZDateTime estimatedDeliveryDate = leg.JU_EstimatedDeliveryTime;

			if (estimatedPickupDate.IsValid)
			{
				xsdLeg.CartageLegDates.EstimatedPickupDate = estimatedPickupDate;
			}

			if (estimatedDeliveryDate.IsValid)
			{
				xsdLeg.CartageLegDates.EstimatedDeliveryDate = estimatedDeliveryDate;
			}

			xsdLeg.CartageLegDates.PickupTimeInDate = leg.JU_PickupTimeIn;
			xsdLeg.CartageLegDates.PickupTimeOutDate = leg.JU_PickupTimeOut;
			xsdLeg.CartageLegDates.DeliverTimeInDate = leg.JU_DeliverTimeIn;
			xsdLeg.CartageLegDates.DeliverTimeOutDate = leg.JU_DeliverTimeOut;

			if (leg.JU_CartagePickupDemurrage.IsValid)
			{
				xsdLeg.CartageLegDates.DeliveryDemurrage = leg.JU_CartagePickupDemurrage;
			}

			if (leg.JU_CartageDeliveryDemurrage.IsValid)
			{
				xsdLeg.CartageLegDates.PickupDemurrage = leg.JU_CartageDeliveryDemurrage;
			}

			if (leg.JU_PlannedPickupTime.IsValid)
			{
				xsdLeg.CartageLegDates.PlannedPickupFrom = leg.JU_PlannedPickupTime;
			}

			if (leg.JU_PlannedPickupTimeEnd.IsValid)
			{
				xsdLeg.CartageLegDates.PlannedPickupTo = leg.JU_PlannedPickupTimeEnd;
			}

			if (!leg.JU_IsEmptyContainer)
			{
				xsdLeg.DangerousGoods.ExportFromUNDGDataItems(leg.BookedCtgMove.UNDGs.ToArray(), "", context);

				// Legacy
				if (leg.BookedCtgMove.UNDGs.Count > 0)
				{
					var firstUNDG = leg.BookedCtgMove.UNDGs[0];
					xsdLeg.MostDangerousGoodsCode = (firstUNDG.Substance != null) ? firstUNDG.Substance.DG_Code : ZString.Empty;
				}
			}
		}

		void ExportAdditionalInstructions(Xsd.CartageLegDocAddress xsdAddress, JobDocAddress orgDocAddress)
		{
			if (orgDocAddress != null && orgDocAddress.Address != null)
			{
				OrgAddress orgAddress = orgDocAddress.Address;

				Xsd.AdditionalInstructionsLoadingUnloadingConstraints constraints = xsdAddress.AdditionalInstructions.LoadingUnloadingConstraints;
				constraints.AccessPoint = orgAddress.OA_AccessPoint;
				constraints.Communication = orgAddress.OA_CommunicationRequired;
				constraints.DockHeight = orgAddress.OA_Dock_Height;
				constraints.ContainerHandling = orgAddress.OA_ContainerHandling;
				constraints.LabourRequired = orgAddress.OA_LabourRequired;
				constraints.FurtherConstraints = orgAddress.OA_LoadingUnloadingConstraints;
			}
		}

		void ExportSailingInfo(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			ZString transportMode = cartage.JJ_ShippingTransportMode;
			JobSailing sailing = GetSailingFromCartage(cartage, transportMode);

			if (sailing != null)
			{
				Xsd.CartageJobSailingInfo xsdCartageSailing = new Xsd.CartageJobSailingInfo();
				if (transportMode == Core.Constants.TransportModes.Air)
				{
					Xsd.FlightWithFlightNumber xsdFlight = new Xsd.FlightWithFlightNumber();
					xsdFlight.FlightNoJourneyNoTruckRegNo = sailing.Voyage.JV_VoyageFlight;
					xsdCartageSailing.Item = xsdFlight;
				}
				else
				{
					Xsd.SailingWithVesselVoyage xsdSailingWithVesselVoyage = new Xsd.SailingWithVesselVoyage();

					xsdSailingWithVesselVoyage.VesselName = sailing.Voyage.JV_RV_NKVessel;
					xsdSailingWithVesselVoyage.VoyageNo = sailing.Voyage.JV_VoyageFlight;
					if (sailing.Vessel != null)
					{
						xsdSailingWithVesselVoyage.LloydsNo = sailing.Vessel.RV_LloydsNumber;
					}
					xsdCartageSailing.Item = xsdSailingWithVesselVoyage;
				}

				SailingValueObjectDataAdapter.RunExport(sailing, xsdCartageSailing.Item, context);

				xsdCartageSailing.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(cartage.Factory, cartage.PortOfLoading, cartage.E_DEP, cartage.E_DEP).Port.Value;
				xsdCartageSailing.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(cartage.Factory, cartage.PortOfDischarge, cartage.E_ARV, cartage.E_ARV).Port.Value;
				xsdCartage.SailingInfo = xsdCartageSailing;
			}
		}

		JobSailing GetSailingFromCartage(CommonCartage cartage, ZString transportMode)
		{
			JobSailing result = cartage.SailingStandalone;

			if (result == null && !cartage.PortOfLoading.IsEmpty && !cartage.PortOfDischarge.IsEmpty)
			{
				var locator = new SailingLocator(cartage.Factory);
				result = locator.FindOrCreateSailingFromSailingManager(transportMode,
					cartage.PortOfLoading,
					cartage.PortOfDischarge,
					cartage.Vessel,
					cartage.VoyageFlight,
					ZGuid.Empty,
					cartage.E_DEP,
					cartage.E_ARV)
					.Sailing;
			}
			return result;
		}

		void ExportNotes(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			foreach (BusinessObject bizo in cartage.BusinessObjectsWithRelatedNotes)
			{
				ExportNotesOnObject((IStmNoteParent)bizo, xsdCartage, context);
			}
			ExportNotesOnObject(cartage, xsdCartage, context);
		}

		void ExportNotesOnObject(IStmNoteParent bizoWithNotes, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			StmNote[] notes = bizoWithNotes.Notes.GetAllNotes().ToArray<StmNote>();
			Array.Sort(notes, delegate(StmNote x, StmNote y)
			{ return x.ST_Description.CompareTo(y.ST_Description); });

			foreach (StmNote note in notes)
			{
				if (note.ST_NoteType == StmNoteDescription.Pub
					&& (note.ST_Description == PredefinedNoteTypes.Instance.SpecialInstructions.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.PickupInstructionsNote.Description
					|| note.ST_Description == PredefinedNoteTypes.Instance.HandlingInstructions.Description))
				{
					bool found = false;
					foreach (Xsd.NotesNote xsdNote in xsdCartage.Notes)
					{
						if (xsdNote.NoteData == note.ST_NoteDataAsText)
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						Xsd.NotesNote xsdNote1 = xsdCartage.Notes.AddNew();
						NoteValueObjectDataAdapter noteAdapter = new NoteValueObjectDataAdapter();
						noteAdapter.ExportToValueObject(note, xsdNote1, context);
					}
				}
			}
		}

		protected void ImportBookingStatus(Logs logCollection, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var notificationMessage = EventLogReferenceBuilder.New()
				.AddShortenable(GetReceivedNotificationMessage(xsdCartage, ZDateTime.Empty, context))
				.Build();

			logCollection.AddNew(Events.StatusUpdated, notificationMessage);
			logCollection.AddNew(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile);
		}

		protected void ImportComments(Notes noteCollection, Xsd.CartageJob xsdCartage, IValueObjectImportContext context, string additionalMessage = "")
		{
			StmNote[] notes = noteCollection.FindByDescription(FreightConstants.LocalCartageNote);
			StmNote note = null;
			if (notes.Length == 0)
			{
				note = noteCollection.AddNew();
				note.ST_NoteType = "PUB";
				note.ST_IsCustomDescription = ZBool.True;
				context.SetPropertyInfoValue(note.ST_DescriptionInfo, FreightConstants.LocalCartageNote);
			}
			else
			{
				note = notes[0];
			}

			var notificationMessage = GetReceivedNotificationMessage(xsdCartage, ZDateTime.Now, context);
			var commentsMessage = GetCommentsMessage(xsdCartage);

			var msg = new ZStringBuilder();
			if (!note.ST_NoteDataAsText.IsEmpty)
			{
				msg.Append(note.ST_NoteDataAsText);
				msg.Append("---");
			}
			msg.AppendIfNotEmpty(notificationMessage);
			msg.AppendIfNotEmpty(commentsMessage);
			msg.AppendIfNotEmpty(additionalMessage);

			note.ST_NoteDataAsText = msg.ToStringWithNewLineBetweenAppends().Trim();
		}

		protected ZString GetReceivedNotificationMessage(Xsd.CartageJob xsdCartage, ZDateTime time, IValueObjectImportContext context)
		{
			var timeDescription = GetTimeDesciption(time);
			var timeStamp = !timeDescription.IsEmpty ? timeDescription + " - " : "";
			var ediStatusDescription = CommonCartageValueObjectDataAdapter.GetStatusDescription(xsdCartage.ActionType, context);
			var orgDescription = GetOrganisationDescription(context.FindOrganisation(xsdCartage.BillTo, null, OrganisationTypes.Debtor));
			var clientJobReference = xsdCartage.ClientJobReference;

			return ZString.Format((NoResString)"{0}{1} received from {2}, Reference {3}.", timeStamp, ediStatusDescription, orgDescription, clientJobReference);
		}

		protected ZString GetCommentsMessage(Xsd.CartageJob xsdCartage)
		{
			return GetCommentDescription(xsdCartage.MessageDescription);
		}

		public static ZString GetTimeDesciption(ZDateTime time)
		{
			return time.IsValid ? "@" + time.ToLongTimeString() : "";
		}

		public static ZString GetCommentDescription(ZString comment)
		{
			return !comment.IsEmpty ? Res.GetString("b155f6e5-1c65-4341-862a-0409d77c56a8", "Comments: {0}", comment) : "";
		}

		public static ZString GetOrganisationDescription(OrgHeader organisation)
		{
			return organisation != null ? organisation.OH_Code + " - " + organisation.OH_FullNameTruncated : " - ";
		}

		public static ZString GetStatusDescription(ZString actionCode, IValueObjectImportContext context)
		{
			var interchange = (Xsd.XmlInterchange)context.Interchange;
			var interchangeInfo = interchange.InterchangeInfo;
			var source = interchangeInfo.Source;
			var isEDI = !source.CompanyCode.IsEmpty && !source.EnterpriseCode.IsEmpty && !source.OriginServer.IsEmpty;

			return GetStatusDescription(actionCode, isEDI);
		}

		public static ZString GetStatusDescription(ZString actionCode, bool useEDILocalTransportDescription)
		{
			var actionDescription = (ZString)CommonFreightCodePairLists.CartageJobBookingLookupList().GetDescriptionFromCode(actionCode);
			if (actionDescription.IsEmpty)
			{
				actionDescription = actionCode;
			}

			var moduleDescription = useEDILocalTransportDescription ? ediLocalTransportDescription : LocalTransportDescription;
			return actionCode + "-" + actionDescription + " " + moduleDescription;
		}

		static ZString ediLocalTransportDescription
		{
			get { return "(PortTransport)"; }
		}

		static ZString LocalTransportDescription
		{
			get { return Res.GetString("b0ba51ff-a644-4845-898d-e2f64a2d0c3b", "(Port Transport)"); }
		}

		protected override CommonCartage NewBusinessObject(Xsd.CartageJob value, IValueObjectImportContext context)
		{
			var cartage = base.NewBusinessObject(value, context);

			// clear Set Default Values
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = "";
			cartage.JJ_Direction = "";
			cartage.JJ_ContainerMode = "";

			return cartage;
		}

		protected abstract ZString BookingAction { get; }
		protected abstract ZString GetJobReference(CommonCartage cartage);

		public const string CartageJobHeaderType = "CJHR";
		public const string CartageJobLegType = "CJLG";
	}
}
