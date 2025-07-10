using System.Collections;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using BookingCodes = Enterprise.Freight.Common.Business.CommonFreightConstants.LocalCartageBookingStatus.Codes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class CommonCartageBookingValueObjectDataAdapter : CommonCartageValueObjectDataAdapter
	{
		public override Xsd.InterchangeInfoTargetType TargetType
		{
			get { return Xsd.InterchangeInfoTargetType.LocalCartageBooking; }
		}

		protected override ZString BookingAction
		{
			get { return FreightConstants.CartageJobExport; }
		}

		protected override ZString GetJobReference(CommonCartage cartage)
		{
			return cartage.CartageParent != null ? cartage.CartageParent.UniqueConsignmentID : cartage.JJ_ConsignmentID;
		}

		protected override void ImportFromValueObjectCore(CommonCartage bizObj, Xsd.CartageJob value, IValueObjectImportContext context)
		{
			ImportCartageJob(bizObj, value, context);
		}

		protected override CommonCartage FindBusinessObject(Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			CommonCartage cartage = null;

			IOrgHeaderForMatching client = context.FindOrganisation(xsdCartage.BillTo, cartage, OrganisationTypes.Debtor);
			if (client != null)
			{
				ZDBOnlySubQuery jobCartageJobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobCartageJobHeader.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				addressQuery.AddToFilter(OrgAddressSchema.OA_OH, client.PK);
				jobCartageJobHeader.AddSubQuery(addressQuery, JoinCondition.And);

				ZDBOnlyQuery jobCartageDBOnlyQuery = new ZDBOnlyQuery(typeof(CommonCartage));
				jobCartageDBOnlyQuery.AddToFilter(JobCartageSchema.JJ_OrderReferenceNumber, xsdCartage.ClientJobReference);
				jobCartageDBOnlyQuery.AddSubQuery(jobCartageJobHeader, JoinCondition.And);

				cartage = context.Factory.LoadTop1<CommonCartage>(jobCartageDBOnlyQuery);
			}
			return cartage;
		}

		protected override bool ShouldUpdateExistingObject(CommonCartage cartage, INotifications notifications)
		{
			bool result = true;
			if (cartage != null)
			{
				ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
				ZQuery refFilter = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, BookingCodes.BookingAccepted + "-");
				refFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, BookingCodes.BookingRejected + "-");
				filter.AddToFilter(refFilter);

				if (cartage.Logs.GetAllLogs().Find(filter).Length > 0)
				{
					result = false;
					var errorMsg = Res.GetString("b2510f18-5b58-4274-90ce-e61118e11845", "REJECTED. The Port Transport Job {0} has already been confirmed and cannot be updated.", cartage.JJ_ConsignmentID);
					notifications.Notify(new ErrorNotification(ErrorType.ImportingDataError, errorMsg));
				}
			}

			return result;
		}

		protected override void OnUserDeclinedImport(CommonCartage bizObj, Xsd.CartageJob value, IValueObjectImportContext context)
		{
			base.OnUserDeclinedImport(bizObj, value, context);

			ImportComments(bizObj.Notes, value, context, context.LastNotificationMessage);
		}

		void ImportCartageJob(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			if (xsdCartage != null)
			{
				context.Notify(new InfoNotification(System.Environment.NewLine + Res.GetString("70f89b29-da71-4550-b0a8-d91b17c35e60", "Importing Port Transport from Job {0}.", xsdCartage.ClientJobReference)));

				var client = cartage.Factory.Load<OrgHeader>(context.FindOrCreateTempOrganisationPK(xsdCartage.BillTo, cartage, OrganisationTypes.Debtor));
				if (client == null)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("1bfb42bf-3111-4315-aebe-cbbcda499013", "REJECTED. The Client cannot be determined from the Import File.")));
				}
				else
				{
					if (!((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Target.BranchCode.IsEmpty)
					{
						ZQuery filter = new ZQuery(GlbBranchSchema.GB_Code, ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Target.BranchCode);
						filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
						GlbBranch branch = context.Factory.LoadTop1<GlbBranch>(filter);
						if (branch != null)
						{
							cartage.JJ_GB = branch.PK;
						}
					}

					ImportJobType(cartage, xsdCartage, context);

					if (cartage.JJ_E3_NKJobType.IsEmpty)
					{
						ImportModes(cartage, xsdCartage);
					}

					context.SetPropertyInfoValue(cartage.JJ_GoodsDescriptionInfo, xsdCartage.GoodsDescription, xsdCartage.GoodsDescriptionSpecified, Res.GetString("c6f79065-e6a1-4752-9918-78fee2835078", "Goods Description"));
					context.SetPropertyInfoValue(cartage.JJ_OrderReferenceNumberInfo, xsdCartage.ClientJobReference, xsdCartage.ClientJobReferenceSpecified, Res.GetString("8f95a24b-54e0-469f-bdb3-6226308a6851", "Client Job Reference"));
					context.SetPropertyInfoValue(cartage.JJ_DropModeInfo, xsdCartage.DropMode, xsdCartage.DropModeSpecified, Res.GetString("e02fd78f-a528-493f-8ba7-9d6458cf314f", "Drop Mode"));

					if (cartage.IsContainerised)
					{
						ImportContainers(cartage, xsdCartage, context);
					}
					else
					{
						ImportPackages(cartage, xsdCartage, context);
						ImportOuterPacks(cartage, xsdCartage, context);
					}

					ImportSailingInformation(cartage, xsdCartage, context);

					new JobHeader.Loader(cartage).TryLoadOrCreate(); //Should not do this. Should depend on 'Job create on save' registry item. But we have no where else to store the Client.
					cartage.LocalClientAddressPK = client.MainAddress.PK;

					ImportBookingStatus(cartage.Logs, xsdCartage, context);
					ImportComments(cartage.Notes, xsdCartage, context);
					ImportNotes(cartage, xsdCartage, context);

					var notificationMessage = GetReceivedNotificationMessage(xsdCartage, ZDateTime.Now, context);
					var commentsMessage = GetCommentsMessage(xsdCartage);
					context.Notify(new InfoNotification(notificationMessage + System.Environment.NewLine + commentsMessage));
				}
			}
		}

		void ImportJobType(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			string newJobType = xsdCartage.JobType;
			if (newJobType.Length == 3)
			{
				newJobType = GetNewJobType(newJobType);
			}

			context.SetPropertyInfoValue(cartage.JJ_E3_NKJobTypeInfo, newJobType, xsdCartage.JobTypeSpecified, Res.GetString("ab1a3caf-f7f3-4953-a373-e2c86dfb9edb", "Port Transport Job Type"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string GetNewJobType(string jobType)
		{
			switch (jobType)
			{
				case Constants.CartageJobType.AirExport:
					return Constants.CartageJobType.NEW_AirExport;
				case Constants.CartageJobType.AirImport:
					return Constants.CartageJobType.NEW_AirImport;

				case Constants.CartageJobType.DomesticContainerizedDelivery:
					return Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
				case Constants.CartageJobType.DomesticContainerizedPickup:
					return Constants.CartageJobType.NEW_DomesticContainerizedPickup;
				case Constants.CartageJobType.DomesticLooseDelivery:
					return Constants.CartageJobType.NEW_DomesticLooseDelivery;
				case Constants.CartageJobType.DomesticLoosePickup:
					return Constants.CartageJobType.NEW_DomesticLoosePickup;

				case Constants.CartageJobType.EmptyCFSToYard:
					return Constants.CartageJobType.NEW_EmptyCFStoCYD;
				case Constants.CartageJobType.EmptyImporterToYard:
					return Constants.CartageJobType.NEW_EmptyCNEtoCYD;
				case Constants.CartageJobType.EmptyYardToCFS:
					return Constants.CartageJobType.NEW_EmptyCYDtoCFS;
				case Constants.CartageJobType.EmptyYardToExporter:
					return Constants.CartageJobType.NEW_EmptyCYDtoSHP;

				case Constants.CartageJobType.FullCFSToCTO:
					return Constants.CartageJobType.NEW_FCLCFStoCTO;
				case Constants.CartageJobType.FullCTOToCFS:
					return Constants.CartageJobType.NEW_FCLCTOtoCFS;
				case Constants.CartageJobType.FullCTOToImporter:
					return Constants.CartageJobType.NEW_FCLCTOtoCNE;
				case Constants.CartageJobType.FullExporterToCTO:
					return Constants.CartageJobType.NEW_FCLSHPtoCTO;

				case Constants.CartageJobType.CTOToImporterToYard:
					return Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD;
				case Constants.CartageJobType.YardToExporterToCTO:
					return Constants.CartageJobType.NEW_FCLCYDtoSHPWAITtoCTO;

				case Constants.CartageJobType.FCLPack:
					return Constants.CartageJobType.NEW_FCLExportPack;
				case Constants.CartageJobType.FCLExport:
					return Constants.CartageJobType.NEW_FCLExportToSHP;

				case Constants.CartageJobType.FCLUnpack:
					return Constants.CartageJobType.NEW_FCLImportUnpack;
				case Constants.CartageJobType.FCLImport:
					return Constants.CartageJobType.NEW_FCLImportToCNE;

				case Constants.CartageJobType.LCLExport:
					return Constants.CartageJobType.NEW_LCLExport;
				case Constants.CartageJobType.LCLImport:
					return Constants.CartageJobType.NEW_LCLImport;

				default:
					return Constants.CartageJobType.NEW_LineHaulFTLCFStoCFS;
			}
		}

		void ImportModes(CommonCartage cartage, Xsd.CartageJob xsdCartage)
		{
			var legs = xsdCartage.CartageLegs.Cast<Xsd.CartageLeg>();

			var hasContainers = false;
			var hasLoose = false;
			var hasCNR = false;
			var hasDeliveryCTO = false;
			var hasCNE = false;
			var hasPickupCTO = false;

			foreach (var leg in legs)
			{
				if (leg.Item is Xsd.CartageLegContainer)
				{
					hasContainers = true;
				}

				if (leg.Item is Xsd.CartageLegPackageRecords)
				{
					hasLoose = true;
				}

				var pickupAddress = leg.Pickup.DocAddress;
				if (pickupAddress.IsSpecified)
				{
					if (pickupAddress.AddressType == Xsd.DocAddressAddressType.LCE)
					{
						hasCNR = true;
					}

					if (pickupAddress.AddressType == Xsd.DocAddressAddressType.LCT)
					{
						hasPickupCTO = true;
					}
				}

				var deliveryAddress = leg.Delivery.DocAddress;
				if (deliveryAddress.IsSpecified)
				{
					if (deliveryAddress.AddressType == Xsd.DocAddressAddressType.LCI)
					{
						hasCNE = true;
					}

					if (deliveryAddress.AddressType == Xsd.DocAddressAddressType.LCT)
					{
						hasDeliveryCTO = true;
					}
				}
			}

			var xsdSailing = xsdCartage.SailingInfo.Item;
			var isAir = xsdSailing != null && xsdSailing.IsSpecified && xsdSailing is Xsd.FlightWithFlightNumber; // sea is SailingWithVesselVoyage
			var isExport = hasDeliveryCTO || hasCNR;
			var isImport = hasPickupCTO || hasCNE;
			var isLocal = isExport && isImport;

			cartage.JJ_ContainerMode = hasContainers && hasLoose ? Constants.CartageContainerMode.Mixed : hasContainers ? Constants.CartageContainerMode.Containerized : Constants.CartageContainerMode.Loose;
			cartage.JJ_ShippingTransportMode = isAir ? Constants.TransportModes.Air : Constants.TransportModes.Sea;
			cartage.JJ_Direction = isLocal ? Constants.CartageDirection.Local : isExport ? Constants.CartageDirection.Export : Constants.CartageDirection.Import;
		}

		void ImportNotes(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			NoteValueObjectDataAdapter noteAdapter = new NoteValueObjectDataAdapter();
			noteAdapter.ImportNotesAndAttachToBusinessObjectNotes(cartage.Notes, xsdCartage.Notes, context);
		}

		void ImportContainers(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			Hashtable existingContainers = new Hashtable();

			foreach (Xsd.CartageLeg xsdLeg in xsdCartage.CartageLegs)
			{
				if (xsdLeg.Item != null && xsdLeg.Item is Xsd.CartageLegContainer)
				{
					Xsd.CartageLegContainer xsdContainer = (Xsd.CartageLegContainer)xsdLeg.Item;
					string key = xsdLeg.Type + xsdContainer.ContainerNumber;

					bool hasBeenAlreadyCreated = false;
					CommonContainer commonContainer = null;

					if (existingContainers[key] == null)
					{
						var move = cartage.ContainerBookedMoves.AddNew();
						commonContainer = move.Container;
						existingContainers[key] = commonContainer;
					}
					else
					{
						hasBeenAlreadyCreated = true;
						commonContainer = (CommonContainer)existingContainers[key];
					}

					if (commonContainer != null)
					{
						if (!hasBeenAlreadyCreated)
						{
							ImportContainerInformation(cartage, commonContainer, xsdContainer, context);
						}

						CommonBookedCtgMove move = null;

						if (cartage.GetBookedMoves(commonContainer).Any())
						{
							move = cartage.GetBookedMoves(commonContainer)[0];
						}
						else
						{
							move = cartage.Factory.New<CommonBookedCtgMove>();
							move.EW_JC_Container = commonContainer.PK;
							cartage.ContainerBookedMoves.Add(move);
							move.CartageLegs.DeleteAll();
						}

						var leg = move.CartageLegs.AddNew();

						SetCartageAddressesAndDates(cartage, leg, xsdLeg, context, xsdCartage.CartageLegs.Count == 1);
						ImportContainerLegInformation(leg, xsdLeg, context);

						move.DefaultAddresses();
					}
				}
			}
		}

		void SetCartageAddressesAndDates(CommonCartage cartage, CommonCartageLeg leg, Xsd.CartageLeg xsdLeg, IValueObjectImportContext context, bool setCartargeDeliveryDate)
		{
			if (xsdLeg.Pickup.DocAddress.IsSpecified)
			{
				JobDocAddress picAddress = CreateOrUpdateFromValueObject(Res.GetString("38715676-87c5-4639-bf82-f7d52d892a55", "Leg Pickup Address"), cartage, xsdLeg.Pickup.DocAddress, context);
				leg.JU_E2PickupAddressID = picAddress.PK;
			}

			if (xsdLeg.Delivery.DocAddress.IsSpecified)
			{
				JobDocAddress dlvAddress = CreateOrUpdateFromValueObject(Res.GetString("ebb86add-a184-4d67-9db8-39fe09736f36", "Leg Delivery Address"), cartage, xsdLeg.Delivery.DocAddress, context);
				leg.JU_E2DeliveryAddressID = dlvAddress.PK;
			}

			if (cartage.JJ_EstimatedPickup.IsEmpty && xsdLeg.CartageLegDates.EstimatedPickupDate.IsValid)
			{
				context.SetPropertyInfoValue(cartage.JJ_EstimatedPickupInfo, xsdLeg.CartageLegDates.EstimatedPickupDate.ToDateTime());
			}

			if (setCartargeDeliveryDate || xsdLeg.Delivery.DocAddress.AddressType != Xsd.DocAddressAddressType.LCY
				&& xsdLeg.Pickup.DocAddress.AddressType != Xsd.DocAddressAddressType.LCY)
			{
				if (cartage.JJ_EstimatedDelivery.IsEmpty && xsdLeg.CartageLegDates.EstimatedDeliveryDate.IsValid)
				{
					context.SetPropertyInfoValue(cartage.JJ_EstimatedDeliveryInfo, xsdLeg.CartageLegDates.EstimatedDeliveryDate.ToDateTime());
				}
			}
		}

		JobDocAddress CreateOrUpdateFromValueObject(string error, CommonCartage cartage, Xsd.DocAddress docAddressValue, IValueObjectImportContext context)
		{
			JobDocAddress jobDocAddress = null;
			DocAddressType addressType = DocAddressTypes.GetDocAddressTypeFromCode(context.Factory, docAddressValue.AddressType.ToString());
			switch (addressType)
			{
				case DocAddressType.LocalCartageAddress1:
					jobDocAddress = cartage.FirstDocAddress;
					break;
				case DocAddressType.LocalCartageAddress2:
					jobDocAddress = cartage.SecondDocAddress;
					break;
				case DocAddressType.LocalCartageAddress3:
					jobDocAddress = cartage.ThirdDocAddress;
					break;
				case DocAddressType.LocalCartageAddress4:
					jobDocAddress = cartage.FourthDocAddress;
					break;
			}

			if (jobDocAddress != null)
			{
				new DocAddressValueObjectHelper(error).ImportFromValueObject(docAddressValue, jobDocAddress, context);
			}
			else
			{
				var organisationType = addressType.GetOrganisationType();
				var organisationSubType = addressType.GetOrganisationSubType();
				var req = ((IDocAddresses)cartage).GetDocAddressRequirement(addressType);
				var foundDocAddress = cartage.DocAddresses.FindByDocAddressType(addressType);
				if (req != null && req.DefaultMax != 1 && foundDocAddress != null && !foundDocAddress.IsEmpty) //Can have multiple DocAddresses with that type already existing... so match
				{
					jobDocAddress = cartage.DocAddresses.CreateWithAddressType(addressType);
					new DocAddressValueObjectHelper(error).ImportFromValueObject(docAddressValue, jobDocAddress, context, organisationType, organisationSubType);

					if (foundDocAddress.IsTheSameAddressAs(jobDocAddress))
					{
						jobDocAddress.Delete();
						jobDocAddress = foundDocAddress;
					}
				}
				else
				{
					jobDocAddress = cartage.DocAddresses.FindOrCreateWithDocAddressType(addressType);
					new DocAddressValueObjectHelper(error).ImportFromValueObject(docAddressValue, jobDocAddress, context, organisationType, organisationSubType);
				}
			}

			return jobDocAddress;
		}

		void ImportContainerInformation(CommonCartage cartage, CommonContainer container, Xsd.CartageLegContainer xsdContainer, IValueObjectImportContext context)
		{
			if (cartage.JJ_DropMode.IsEmpty)
			{
				context.SetPropertyInfoValue(cartage.JJ_DropModeInfo, xsdContainer.ContainerAdditionalInfo.DropMode, xsdContainer.ContainerAdditionalInfo.DropModeSpecified, "DropMode");
			}

			context.SetPropertyInfoValue(container.JC_ContainerNumInfo, xsdContainer.ContainerNumber, xsdContainer.ContainerNumberSpecified, (NoResString)"Container Number");
			context.SetPropertyInfoValue(container.JC_SealNumInfo, xsdContainer.Seal, xsdContainer.SealSpecified);
			context.SetPropertyInfoValue(container.JC_AdditionalSealNumInfo, xsdContainer.ContainerAdditionalInfo.AdditionalSealNo, xsdContainer.ContainerAdditionalInfo.AdditionalSealNoSpecified, "ContainerAdditionalSealNo");

			new ContainerValueObjectHelper(context).ImportContainerType(container.JC_RCInfo, xsdContainer.ContainerType);

			if (container.JC_TareWeight == ZDecimal.Zero)
			{
				container.JC_TareWeight = xsdContainer.ContainerAdditionalInfo.TareWeight.Value;
			}

			container.JC_GrossWeight = xsdContainer.ContainerAdditionalInfo.GrossWeight.Value;
			context.SetPropertyInfoValue(container.JC_GrossWeightUQInfo, xsdContainer.ContainerAdditionalInfo.GrossWeight.DimensionType, xsdContainer.ContainerAdditionalInfo.GrossWeight.DimensionTypeSpecified);

			context.SetPropertyInfoValueIfValueNotEmpty(container.JC_ContainerModeInfo, ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(xsdContainer.PackingMode.ToString(), Res.GetString("a9b38d9c-50e0-44f9-8cfb-de26dc5ae2d6", "Container {0}", container.JC_ContainerNum), context));
			context.SetPropertyInfoValue(container.JC_TempRecorderSerialNoInfo, xsdContainer.ContainerAdditionalInfo.TempRecorderNo, xsdContainer.ContainerAdditionalInfo.TempRecorderNoSpecified, "TempRecorderNo");
			context.SetPropertyInfoValue(container.JC_ReleaseNumInfo, xsdContainer.ContainerAdditionalInfo.ReleaseNo, xsdContainer.ContainerAdditionalInfo.ReleaseNoSpecified);

			if (xsdContainer.AirVentFlowSpecified)
			{
				container.JC_AirVentFlow = xsdContainer.AirVentFlow;
			}

			context.SetPropertyInfoValue(container.JC_AirVentFlowRateUnitInfo, xsdContainer.AirVentFlowRateUnit, xsdContainer.AirVentFlowRateUnitSpecified);
			if (xsdContainer.HumidityPercentSpecified)
			{
				container.JC_HumidityPercent = xsdContainer.HumidityPercent;
			}

			if (xsdContainer.SetPointTemperatureSpecified)
			{
				container.JC_SetPointTemp = xsdContainer.SetPointTemperature;
			}

			context.SetPropertyInfoValue(container.JC_SetPointTempUnitInfo, xsdContainer.SetPointTemperatureUnit, xsdContainer.SetPointTemperatureUnitSpecified);

			if (xsdContainer.ContainerAdditionalInfo.EmptyRequiredDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_EmptyRequiredInfo, xsdContainer.ContainerAdditionalInfo.EmptyRequiredDate.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.PackDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_PackDateInfo, xsdContainer.ContainerAdditionalInfo.PackDate.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.DepartureSlotDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_DepartureSlotDateTimeInfo, xsdContainer.ContainerAdditionalInfo.DepartureSlotDate.ToDateTime());
			}
			context.SetPropertyInfoValue(container.JC_DepartureSlotReferenceInfo, xsdContainer.ContainerAdditionalInfo.DepartureSlotRef, xsdContainer.ContainerAdditionalInfo.DepartureSlotRefSpecified, "DepartureSlotRef");
			if (xsdContainer.ContainerAdditionalInfo.ArrivalSlotDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_ArrivalSlotDateTimeInfo, xsdContainer.ContainerAdditionalInfo.ArrivalSlotDate.ToDateTime());
			}
			context.SetPropertyInfoValue(container.JC_ArrivalSlotReferenceInfo, xsdContainer.ContainerAdditionalInfo.ArrivalSlotRef, xsdContainer.ContainerAdditionalInfo.ArrivalSlotRefSpecified);
			if (xsdContainer.LCLAvailable.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_LCLAvailableInfo, xsdContainer.LCLAvailable.ToDateTime());
			}
			if (xsdContainer.FCLAvailable.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_FCLAvailableInfo, xsdContainer.FCLAvailable.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.LCLStorageDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_LCLStorageCommencesInfo, xsdContainer.ContainerAdditionalInfo.LCLStorageDate.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.FCLStorageDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_ArrivalCTOStorageStartDateInfo, xsdContainer.ContainerAdditionalInfo.FCLStorageDate.ToDateTime());
			}
			if (xsdContainer.EstimatedDelivery.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_ArrivalEstimatedDeliveryInfo, xsdContainer.EstimatedDelivery.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.UnpackDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_LCLUnpackInfo, xsdContainer.ContainerAdditionalInfo.UnpackDate.ToDateTime());
			}
			if (xsdContainer.ContainerAdditionalInfo.EmptyReturnedByDate.IsValid)
			{
				context.SetPropertyInfoValue(container.JC_EmptyReturnedByInfo, xsdContainer.ContainerAdditionalInfo.EmptyReturnedByDate.ToDateTime());
			}
		}

		void ImportContainerLegInformation(CommonCartageLeg leg, Xsd.CartageLeg xsdLeg, IValueObjectImportContext context)
		{
			xsdLeg.DangerousGoods.ImportToUNDGDataItems(() => leg.BookedCtgMove.UNDGs, "", context);

			// Legacy
			if (xsdLeg.MostDangerousGoodsCodeSpecified)
			{
				leg.BookedCtgMove.UNDGs.TryGetOrCreate(xsdLeg.MostDangerousGoodsCode, xsdLeg.MostDangerousGoodsStandard);
			}

			if (xsdLeg.CartageLegDates.EstimatedPickupDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_PlannedPickupTimeInfo, xsdLeg.CartageLegDates.EstimatedPickupDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.EstimatedDeliveryDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_EstimatedDeliveryTimeInfo, xsdLeg.CartageLegDates.EstimatedDeliveryDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.PickupTimeInDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_PickupTimeInInfo, xsdLeg.CartageLegDates.PickupTimeInDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.PickupTimeOutDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_PickupTimeOutInfo, xsdLeg.CartageLegDates.PickupTimeOutDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.DeliverTimeInDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_DeliverTimeInInfo, xsdLeg.CartageLegDates.DeliverTimeInDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.DeliverTimeOutDate.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_DeliverTimeOutInfo, xsdLeg.CartageLegDates.DeliverTimeOutDate.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.PlannedPickupFrom.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_PlannedPickupTimeInfo, xsdLeg.CartageLegDates.PlannedPickupFrom.ToDateTime());
			}
			if (xsdLeg.CartageLegDates.PlannedPickupTo.IsValid)
			{
				context.SetPropertyInfoValue(leg.JU_PlannedPickupTimeEndInfo, xsdLeg.CartageLegDates.PlannedPickupTo.ToDateTime());
			}
		}

		void ImportPackages(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			cartage.LooseBookedMoves.DeleteAll(); //remove any defaulted

			foreach (Xsd.CartageLeg xsdLeg in xsdCartage.CartageLegs)
			{
				if (xsdLeg.Item != null && xsdLeg.Item is Xsd.CartageLegPackageRecords)
				{
					Xsd.CartageLegPackageRecords xsdPacks = (Xsd.CartageLegPackageRecords)xsdLeg.Item;

					foreach (Xsd.Package pack in xsdPacks.Packs)
					{
						BusinessObjectFactory dummyFactory = new BusinessObjectFactory();
						ValueObjectImportContext dummyContext = new ValueObjectImportContext(dummyFactory, context);
						PackLine dummyPackline = dummyFactory.New<PackLine>();
						new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>().ImportFromValueObject(dummyPackline, pack, dummyContext);

						CommonBookedCtgMove looseMove = cartage.LooseBookedMoves.AddNew();
						looseMove.EW_BookedHeight = dummyPackline.JL_Height;
						looseMove.EW_BookedLength = dummyPackline.JL_Length;
						looseMove.EW_BookedWidth = dummyPackline.JL_Width;
						looseMove.EW_DimUnit = dummyPackline.JL_UnitOfDimension;

						looseMove.EW_BookedVolume = dummyPackline.JL_ActualVolume;
						looseMove.EW_VolumeUQ = dummyPackline.JL_ActualVolumeUQ;

						looseMove.EW_BookedWeight = dummyPackline.JL_ActualWeight;
						looseMove.EW_WeightUQ = dummyPackline.JL_ActualWeightUQ;

						looseMove.EW_BookedPackCount = dummyPackline.JL_PackageCount;
						looseMove.EW_F3_NKPackType = dummyPackline.JL_F3_NKPackType;

						foreach (UNDGDataItem dgDataItem in dummyPackline.UNDGs)
						{
							UNDGDataItem looseMoveDGDataItem = looseMove.UNDGs.AddNew();
							looseMoveDGDataItem.DI_DG = dgDataItem.DI_DG;
							looseMoveDGDataItem.DI_DGFlashPoint = dgDataItem.DI_DGFlashPoint;
							looseMoveDGDataItem.DI_OC_DGContact = dgDataItem.DI_OC_DGContact;
						}

						looseMove.CartageLegs.DeleteAll();
						CommonCartageLeg leg = looseMove.CartageLegs.AddNew();
						SetCartageAddressesAndDates(cartage, leg, xsdLeg, context, xsdCartage.CartageLegs.Count > 0);
						ImportContainerLegInformation(leg, xsdLeg, context);

						looseMove.DefaultAddresses();
					}
				}
			}
		}

		void ImportOuterPacks(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			if (xsdCartage.OuterPacks.OuterPacksNoSpecified)
			{
				context.SetPropertyInfoValue(cartage.JJ_F3_NKPackTypeInfo, PkgUnitXmlCodeMappings.Instance.GetEnterpriseCode(xsdCartage.OuterPacks.OuterPacksType, (NoResString)"OuterPack No", context), xsdCartage.OuterPacks.OuterPacksTypeSpecified);
				cartage.JJ_OuterPacks = xsdCartage.OuterPacks.OuterPacksNo;
			}

			if (xsdCartage.OuterPacks.OuterPacksWeight != null)
			{
				context.SetPropertyInfoValue(cartage.JJ_WeightUQInfo, WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(xsdCartage.OuterPacks.OuterPacksWeight.DimensionType, (NoResString)"OuterPacks Weight", context), xsdCartage.OuterPacks.OuterPacksWeight.DimensionTypeSpecified);
				cartage.JJ_Weight = xsdCartage.OuterPacks.OuterPacksWeight.Value;
			}

			if (xsdCartage.OuterPacks.OuterPacksVolume != null)
			{
				context.SetPropertyInfoValue(cartage.JJ_VolumeUQInfo, VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(xsdCartage.OuterPacks.OuterPacksVolume.DimensionType, (NoResString)"OuterPacks Volume", context), xsdCartage.OuterPacks.OuterPacksVolume.DimensionTypeSpecified);
				cartage.JJ_Volume = xsdCartage.OuterPacks.OuterPacksVolume.Value;
			}
		}

		void ImportSailingInformation(CommonCartage cartage, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			Xsd.SailingBase xsdSailing = xsdCartage.SailingInfo.Item;
			if (xsdSailing != null && xsdSailing.IsSpecified)
			{
				ZString vesselName = ZString.Empty;
				ZString voyageFlightNo = ZString.Empty;

				var flight = xsdSailing as Xsd.FlightWithFlightNumber;
				var sailing = xsdSailing as Xsd.SailingWithVesselVoyage;

				var transportMode = Core.Constants.TransportModes.Sea;
				if (flight != null)
				{
					voyageFlightNo = flight.FlightNoJourneyNoTruckRegNo;
					transportMode = Core.Constants.TransportModes.Air;
				}
				else if (sailing != null)
				{
					vesselName = VesselNameImportHelper.MatchVesselAndGetVesselName(sailing, context.Factory);
					voyageFlightNo = sailing.VoyageNo;
				}

				ZString portOfLoading = xsdCartage.SailingInfo.PortOfLoading;
				ZString portOfDischarge = xsdCartage.SailingInfo.PortOfDischarge;

				SailingValueObjectDataAdapter sailingAdapter = new SailingValueObjectDataAdapter(
					transportMode, portOfLoading, portOfDischarge, vesselName, voyageFlightNo, ZGuid.Empty);

				JobSailing newOrUpdatedSailing = sailingAdapter.CreateOrUpdateFromValueObjectWithSchedule(xsdSailing, context);

				if (newOrUpdatedSailing != null)
				{
					cartage.JJ_JX_Sailing = newOrUpdatedSailing.PK;
				}
			}
		}
	}
}
