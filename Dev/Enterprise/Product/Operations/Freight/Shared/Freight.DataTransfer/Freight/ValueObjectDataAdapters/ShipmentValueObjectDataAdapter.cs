
using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class ShipmentValueObjectDataAdapter<TBusinessObject> : FreightValueObjectDataAdapter<TBusinessObject, Xsd.Shipment>
		where TBusinessObject : CommonShipment
	{
		#region Construction

		public ShipmentValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		public ShipmentValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}

		protected readonly EventsWithSourceType TriggeredByEvents;

		public ShipmentValueObjectDataAdapter(CommonConsol existingConsol)
			: this(existingConsol, EventsWithSourceType.Empty)
		{
		}

		public ShipmentValueObjectDataAdapter(CommonConsol existingConsol, EventsWithSourceType triggeredByEvents)
			: this(triggeredByEvents)
		{
			this.ExistingConsol = existingConsol;
		}

		public readonly CommonConsol ExistingConsol;

		#endregion

		#region Overrides

		public override string RootCollectionElementName { get { return (NoResString)"Shipments"; } }
		public override string RootElementName { get { return (NoResString)"Shipment"; } }
		public override XmlSchema Schema { get { return FreightXmlSchemaDefinitions.Instance.SingleShipmentSchema; } }
		public override XmlSchema CollectionSchema { get { return FreightXmlSchemaDefinitions.Instance.ShipmentsSchema; } }

		protected override void AfterImportFromValueObject(TBusinessObject bizObj, Xsd.Shipment value, IValueObjectImportContext context)
		{
			base.AfterImportFromValueObject(bizObj, value, context);
			bizObj.DeactivateActiveBusinessObjectCollections();
		}

		#endregion

		#region Business Objects

		public override TBusinessObject CreateOrUpdateFromValueObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			TBusinessObject result = null;
			if (ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.ShipmentNumber && shipmentValue.ShipmentDetails.AgentReference.IsEmpty)
			{
				var errorMessage = Res.GetString("f48e8fe2-110c-4d59-9480-356d4102b911",
												"Registry '{0}' is set to match on Agent's Reference however no Agent's Reference (Shipment Number) is provided in the XML file.",
												((IRegistryItemInternals)SystemRegistry.ImportShipmentNoFromXml).Location);

				context.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, errorMessage));
			}
			else
			{
				result = base.CreateOrUpdateFromValueObject(shipmentValue, context);
			}
			return result;
		}

		protected override TBusinessObject FindBusinessObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			var shipmentLocator = new ShipmentLocator<TBusinessObject>(context.Factory, GetFindQuery(), ExistingConsol, IncludeInactiveForMatchingOnAgentReference);
			return shipmentLocator.Find(shipmentValue);
		}

		protected virtual ZQuery GetFindQuery()
		{
			return new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, true);
		}

		internal TBusinessObject FindShipment(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			return FindBusinessObject(shipmentValue, context);
		}

		protected virtual bool IncludeInactiveForMatchingOnAgentReference
		{
			get { return true; }
		}

		protected override bool ShouldUpdateExistingObject(TBusinessObject bizObj, INotifications notifications)
		{
			bool result = true;
			if (ImportShipmentNumberFromXml)
			{
				if (bizObj.JS_IsCancelled)
				{
					string cannotUpdateInactiveShipmentWarningMesg = Res.GetString("bd60ec18-63b6-4e11-aa47-cb1dfd586540", "Cannot update Shipment {0} as it is flagged as inactive.", bizObj.JS_UniqueConsignRef);
					notifications.Notify(new WarningNotification(cannotUpdateInactiveShipmentWarningMesg));
					result = false;
				}
				else if (ExistingConsol != null)
				{
					var msg = string.Empty;

					if (bizObj.Consols.Count == 0)
					{
						msg = Res.GetString("cafeb224-82eb-4f31-9b8b-654f1aaafe23", "Cannot update Shipment {0} as it is not attached to Consol {1}.", bizObj.JS_UniqueConsignRef, ExistingConsol.JK_UniqueConsignRef);
					}
					else if (bizObj.Consols.Count > 1 || bizObj.Consols[0].PK != ExistingConsol.PK)
					{
						var notExpectedConsols = bizObj.Consols.Cast<CommonConsol>().Where(c => c.PK != ExistingConsol.PK).Select(c => c.JK_UniqueConsignRef);

						msg = Res.GetString(
							"76d64079-2f3f-45d6-8b7a-39e884645a3d",
							"Cannot update Shipment {0} as it is attached to one or more other Consols ({1}).",
							bizObj.JS_UniqueConsignRef,
							string.Join(", ", notExpectedConsols));
					}

					if (!string.IsNullOrEmpty(msg))
					{
						notifications.Notify(new WarningNotification(msg));
						result = false;
					}
				}
			}
			else
			{
				if (IsBatchJob)
				{
					switch (bizObj.JS_TransportMode)
					{
						case Core.Constants.TransportModes.Air:
							result = RegistryDefaultForImportingAir;
							break;
						case Core.Constants.TransportModes.Sea:
							result = RegistryDefaultForImportingSea;
							break;
						default:
							result = RegistryDefaultForImporting;
							break;
					}
				}
				else
				{
					result = base.ShouldUpdateExistingObject(bizObj, notifications);
				}
			}

			return result;
		}

		protected bool ImportShipmentNumberFromXml
		{
			get { return ImportMatchingCriteria != Constants.ShipmentNumberImportTypes.Code.HouseBill; }
		}

		ZString ImportMatchingCriteria
		{
			get { return SystemRegistry.ImportShipmentNoFromXml.Value; }
		}

		protected override TBusinessObject NewBusinessObject(Xsd.Shipment value, IValueObjectImportContext context)
		{
			TBusinessObject result = null;
			if (ExistingConsol == null)
			{
				result = context.Factory.New<TBusinessObject>();
			}
			else
			{
				result = (TBusinessObject)ExistingConsol.Shipments.AddNew();
			}
			return result;
		}

		#endregion

		#region Import

		protected override bool MinimumRequirementsMetForNewImport(Xsd.Shipment shipmentValue, IValueObjectImportContext importContext)
		{
			bool minimumsMet = true;
			if (shipmentValue.ShipmentDetailsSpecified)
			{
				if (!shipmentValue.ShipmentDetails.TransportModeSpecified)
				{
					importContext.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, Res.GetString("e9bdc841-d1dc-4f96-90d8-bd5993bd7094", "Transport Mode is required for Shipment Imports")));
					minimumsMet = false;
				}

				bool hasConsignorOverride = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRD) != null;
				bool hasConsigneeOverride = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CED) != null;

				if (!shipmentValue.ShipmentDetails.ConsignorSpecified && !shipmentValue.ShipmentDetails.ConsigneeSpecified && !hasConsignorOverride && !hasConsigneeOverride)
				{
					importContext.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, Res.GetString("151a2a6c-3a17-41b7-b174-6e1487e20742", "Consignor and/or Consignee are required for Shipment Imports")));
					minimumsMet = false;
				}
			}
			return minimumsMet;
		}

		protected override void ImportFromValueObjectCore(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			var interchange = (Xsd.XmlInterchange)context.Interchange;

			ImportUniqueConsignRef(shipment, value, context);
			ImportHouseBillIdentifier(shipment, value, context);
			ImportCoLoadMaster(shipment, value, context);
			ImportDocAddresses(shipment, value, context);

			var errorContext = Res.GetString("c72d32d3-87bf-4fad-8439-0232dd424bb7", "House Bill '{0}'", shipment.JS_HouseBill);
			StmALogValueObjectDataAdapter.New(shipment, errorContext, TriggeredByEvents).FromXmlCollectionValueObject(value.Events, context);

			if ((value.ShipmentDetailsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value) && value.ShipmentDetails != null)
			{
				ImportModeAndMovementInfo(shipment, value, context, errorContext);

				if (value.ShipmentDetails.TotalInnerPacksQty.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.JS_TotalPackageCount = (ZInt)value.ShipmentDetails.TotalInnerPacksQty.Value;
				}

				ImportPackageType(shipment.JS_F3_NKTotalCountPackTypeInfo, value.ShipmentDetails.TotalInnerPacksQty.DimensionType, errorContext, context, shipment, value.ShipmentDetails.TotalInnerPacksQty.IsSpecified);

				ImportCargoInfo(shipment, value, context, errorContext);

				if (value.ShipmentDetails.LoadingMetersSpecified)
				{
					context.SetPropertyInfoValue(shipment.JS_LoadingMetersInfo, value.ShipmentDetails.LoadingMeters, JobShipmentSchema.JS_LoadingMeters);
				}

				if (value.ShipmentDetails.GoodsValue.IsSpecified)
				{
					context.SetPropertyInfoValue(shipment.JS_RX_NKGoodsValueCurrInfo, value.ShipmentDetails.GoodsValue.CurrencyCode, ForeignKeyType.CurrencyNK);
				}

				if (value.ShipmentDetails.GoodsValue.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					context.SetPropertyInfoValue(shipment.JS_GoodsValueInfo, value.ShipmentDetails.GoodsValue.Value, JobShipmentSchema.JS_GoodsValue);
				}

				if (value.ShipmentDetails.InsuranceValue.IsSpecified)
				{
					context.SetPropertyInfoValue(shipment.JS_RX_NKInsuranceCurrencyInfo, value.ShipmentDetails.InsuranceValue.CurrencyCode, ForeignKeyType.CurrencyNK);
				}

				if (value.ShipmentDetails.InsuranceValue.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					context.SetPropertyInfoValue(shipment.JS_InsuranceValueInfo, value.ShipmentDetails.InsuranceValue.Value, JobShipmentSchema.JS_InsuranceValue);
				}

				if (value.ShipmentDetails.ChargeableWeight.IsSpecified)
				{
					context.SetPropertyInfoValue(shipment.JS_ActualChargeableInfo, value.ShipmentDetails.ChargeableWeight.Value, JobShipmentSchema.JS_ActualChargeable);
				}
				else if (!SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.UpdateChargeableWeights();
				}

				if (value.ShipmentDetails.FreightRate.IsSpecified)
				{
					context.SetPropertyInfoValue(shipment.JS_RX_NKFrtRateCurrencyInfo, value.ShipmentDetails.FreightRate.CurrencyCode, ForeignKeyType.CurrencyNK);
				}

				if (value.ShipmentDetails.FreightRate.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					context.SetPropertyInfoValue(shipment.JS_UnitFreightRateInfo, value.ShipmentDetails.FreightRate.Value, JobShipmentSchema.JS_UnitFreightRate);
				}

				context.SetPropertyInfoValue(shipment.JS_ReleaseTypeInfo, ReleaseTypeXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.ReleaseType, errorContext, context), value.ShipmentDetails.ReleaseTypeSpecified);

				context.SetPropertyInfoValue(shipment.JS_MarksAndNumbersInfo, value.ShipmentDetails.MarksAndNumbers, value.ShipmentDetails.MarksAndNumbersSpecified);
				if (value.ShipmentDetails.ServiceLevelSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					context.SetPropertyInfoValue(shipment.JS_RS_NKServiceLevelInfo, value.ShipmentDetails.ServiceLevel, ForeignKeyType.RefServiceLevelNK);
				}

				if (value.ShipmentDetails.IncotermSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					context.SetPropertyInfoValue(shipment.JS_INCOInfo, value.ShipmentDetails.Incoterm, ForeignKeyType.IncoTermNK);
				}
				context.SetPropertyInfoValue(shipment.JS_AdditionalTermsInfo, value.ShipmentDetails.AdditionalTerms, value.ShipmentDetails.AdditionalTermsSpecified);

				context.SetPropertyInfoValue(shipment.JS_BookingReferenceInfo, value.ShipmentDetails.BookingReference, value.ShipmentDetails.BookingReferenceSpecified);

				context.SetPropertyInfoValue(shipment.JS_InterimReceiptInfo, value.ShipmentDetails.InterimReceipt, value.ShipmentDetails.InterimReceiptSpecified);
				if (value.ShipmentDetails.HBLIssueDate.IsValid)
				{
					shipment.JS_HouseBillIssueDate = value.ShipmentDetails.HBLIssueDate.ToSmallDateTime();
				}
				if (value.ShipmentDetails.ShippedOnBoardDate.IsValid)
				{
					shipment.JS_ShippedOnBoardDate = value.ShipmentDetails.ShippedOnBoardDate.ToSmallDateTime();
				}
				context.SetPropertyInfoValue(shipment.JS_HBLContainerPackModeOverrideInfo, value.ShipmentDetails.HBLContainerMode, value.ShipmentDetails.HBLContainerModeSpecified);
				context.SetPropertyInfoValue(shipment.JS_ShippedOnBoardInfo, ShippedOnBoardTypeCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.ShippedOnBoardType, errorContext, context), value.ShipmentDetails.ShippedOnBoardTypeSpecified);
				context.SetPropertyInfoValue(shipment.JS_NoOriginalBillsInfo, value.ShipmentDetails.NoOriginalBills, value.ShipmentDetails.NoOriginalBillsSpecified);
				context.SetPropertyInfoValue(shipment.JS_NoCopyBillsInfo, value.ShipmentDetails.NoCopyBills, value.ShipmentDetails.NoCopyBillsSpecified);

				ImportPackages(shipment, value, context);

				XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(value.ShipmentDetails.CustomsEntryNumbers, shipment.PK, JobShipmentSchema.Constants.TableName, () => shipment.CusEntryNumbers, context);
				ImportPickupAndDeliveryInformation(shipment, value, context);
				ImportCustomAttributeInformation(shipment, value, context);
				XsdPlannedLegObjectHelper.ImportPlannedLegs(shipment.Transports, value.ShipmentDetails.TransportPlan, context, "");

				ImportJobInfo(shipment, value, context);

				if (!value.ShipmentDetails.BookedDate.IsEmpty)
				{
					shipment.JS_A_BKD = value.ShipmentDetails.BookedDate.ToSmallDateTime();
				}

				if (!value.ShipmentDetails.ExporterStatement.IsEmpty)
				{
					shipment.DocsAndCartage.JP_ExportStatement = value.ShipmentDetails.ExporterStatement.Left(shipment.DocsAndCartage.JP_ExportStatementInfo.MaxLength);
				}
			}

			ImportReferenceNumbers(shipment, value, context);

			ImportOrganisationDetails(shipment, value, context);

			ImportNotesFromShipment(shipment, value, context);
			ImporteDocs(shipment, value.Documents, context);
			ImportBilling(shipment, value, context);
			ImportConsignorCOD(shipment, value, context);

			OnAfterImportFromValueObjectCore(shipment, value, context);
			DocDataValueObjectDataAdapter.ImportData(shipment, value.DocData, context);

			AddImportEvent(shipment, value);
		}

		protected void ImportJobInfo(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if ((value.ShipmentDetails.LocalClient.IsSpecified || !value.ShipmentDetails.SalesRep.IsEmpty) && SystemDataRegistry.Instance.AllowBillingImportIntoShipment.Value)
			{
				var loader = new JobHeader.Loader(shipment);
				var jobHeader = shipment.IsInDatabase ? loader.TryLoadOrCreateWithMutex() : loader.TryLoadOrCreate();
				if (jobHeader != null)
				{
					jobHeader.JH_ParentID = shipment.PK;

					if (jobHeader.JH_GE.IsEmpty)
					{
						jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
					}

					if (value.ShipmentDetails.LocalClient.IsSpecified)
					{
						var org = shipment.Factory.Load<OrgHeader>(context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.LocalClient, shipment, OrganisationTypes.Debtor));
						jobHeader.JH_OA_LocalChargesAddr = org != null ? org.MainAddress.PK : ZGuid.Empty;
					}

					if (!value.ShipmentDetails.SalesRep.IsEmpty)
					{
						var salesRep = shipment.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, value.ShipmentDetails.SalesRep));
						if (salesRep != null)
						{
							jobHeader.JH_GS_NKRepSales = salesRep.GS_Code;
						}
					}

					if (jobHeader.JH_GB.IsEmpty)
					{
						jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
					}
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.Error, loader.GetJobCreationError()));
				}
			}
		}

		protected void ImportCargoInfo(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context, string errorContext)
		{
			if (value.ShipmentDetails.TotalOuterPacksQty.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				shipment.JS_OuterPacks = (ZInt)value.ShipmentDetails.TotalOuterPacksQty.Value;
			}

			ImportPackageType(shipment.JS_F3_NKPackTypeInfo
				, value.ShipmentDetails.TotalOuterPacksQty.DimensionType
				, errorContext
				, context
				, shipment
				, value.ShipmentDetails.TotalOuterPacksQty.IsSpecified);

			if (value.ShipmentDetails.Weight.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				context.SetPropertyInfoValue(shipment.JS_ActualWeightInfo
					, value.ShipmentDetails.Weight.Value
					, JobShipmentSchema.JS_ActualWeight);
			}

			context.SetPropertyInfoValue(shipment.JS_UnitOfWeightInfo
				, WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.Weight.DimensionType, errorContext, context)
				, value.ShipmentDetails.Weight.DimensionTypeSpecified);

			if (value.ShipmentDetails.Volume.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				context.SetPropertyInfoValue(shipment.JS_ActualVolumeInfo
					, value.ShipmentDetails.Volume.Value
					, JobShipmentSchema.JS_ActualVolume);
			}

			context.SetPropertyInfoValue(shipment.JS_UnitOfVolumeInfo
				, VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.Volume.DimensionType, errorContext, context)
				, value.ShipmentDetails.Volume.DimensionTypeSpecified);
		}

		protected void ImportModeAndMovementInfo(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context, string errorContext)
		{
			if (value.ShipmentDetails.TransportModeSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(shipment.JS_TransportModeInfo,
					TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.TransportMode, errorContext, context));
			}

			if (value.ShipmentDetails.PackingModeSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(shipment.JS_PackingModeInfo,
					ContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.PackingMode.ToString(), errorContext, context));
			}

			context.SetPropertyInfoValue(shipment.JS_BookingReferenceInfo
				, value.ShipmentDetails.BookingReference
				, value.ShipmentDetails.BookingReferenceSpecified);

			if (value.ShipmentDetails.ForwardingShipmentTypeSpecified)
			{
				context.SetPropertyInfoValue(shipment.JS_ShipmentTypeInfo
					, ForwardingShipmentTypeToXmlCodeMappings.Instance.GetEnterpriseCode(value.ShipmentDetails.ForwardingShipmentType, errorContext, context));
			}

			XsdMovement.ToPortEstimatedActualDates(value.ShipmentDetails.PortOfOrigin, shipment.JS_RL_NKOriginInfo, shipment.JS_E_DEPInfo, null, Res.GetString("981a6868-0646-4cd0-b428-df5f2f71b221", "Port of origin"), context);
			XsdMovement.ToPortEstimatedActualDates(value.ShipmentDetails.PortofDestination, shipment.JS_RL_NKDestinationInfo, shipment.JS_E_ARVInfo, null, Res.GetString("93200a89-b437-4cfb-8249-aabcc8b64dee", "Port of destination"), context);
		}

		protected void ImportUniqueConsignRef(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (!value.ShipmentDetails.AgentReference.IsEmpty)
			{
				if (ImportShipmentNumberFromXml)
				{
					if (value.ShipmentDetails.AgentReference.Length <= shipment.JS_UniqueConsignRefInfo.MaxLength)
					{
						shipment.JS_UniqueConsignRef = value.ShipmentDetails.AgentReference.ToUpper();
					}
					else
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("204f4723-1335-4b46-8403-8cd44d33d28e", "The agent reference is too long.")));
					}
				}

				shipment.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference, value.ShipmentDetails.AgentReference);
			}
		}

		protected virtual void ImportReferenceNumbers(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			ReferenceNumberDataAdapter.ImportReferenceNumbers(shipment.Numbers, value.ShipmentDetails.ReferenceNumbers, context);
		}

		protected virtual void AddImportEvent(TBusinessObject shipment, Xsd.Shipment shipmentValue)
		{
			AddImportEvent(shipment);
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportOther.Value; }
		}

		protected virtual bool RegistryDefaultForImportingAir
		{
			get { return SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportAir.Value; }
		}

		protected virtual bool RegistryDefaultForImportingSea
		{
			get { return SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportSea.Value; }
		}

		void ImportPackageType(ZPropertyInfo propertyInfo, ZString value, string errorContext, IValueObjectImportContext context, TBusinessObject shipment, bool isSpecified)
		{
			if (isSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				string packageContextMsg = Res.GetString("9d8172bc-dcd9-4c31-be2e-b35f2f948b16", "The Package type ({0}) of {1}", value, shipment.HumanReadableName);
				CommonDefinedResourceStrings.SetPackageTypeProperty(propertyInfo, value, packageContextMsg, context, isSpecified);
			}
		}

		protected virtual void OnAfterImportFromValueObjectCore(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
		}

		protected void ImportNotesFromShipment(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(shipment.Notes, value.Notes, context);

			if (!value.ShipmentDetails.GoodsDescription.IsEmpty)
			{
				shipment.JS_GoodsDescription = value.ShipmentDetails.GoodsDescription.SubstringSafe(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);
				if (shipment.DetailedGoodsDescriptionNoteText.IsEmpty && value.ShipmentDetails.GoodsDescription.Length > JobShipmentSchema.JS_GoodsDescription.MaxLength)
				{
					shipment.DetailedGoodsDescriptionNoteText = value.ShipmentDetails.GoodsDescription;
				}
			}
			else
			{
				if (shipment.JS_GoodsDescription.IsEmpty)
				{
					shipment.JS_GoodsDescription = shipment.DetailedGoodsDescriptionNoteText.SubstringSafe(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);
				}
			}
		}

		protected void ImportHouseBillIdentifier(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentIdentifier != null)
			{
				foreach (Xsd.ShipmentIdentifier identifier in value.ShipmentIdentifier)
				{
					if (identifier.ShipmentIdentifierType == Xsd.ShipmentIdentifierType.Housebill)
					{
						context.Notify(new InfoNotification(Res.GetString("297f6869-7d1d-492c-a9a0-843c2dc5560c", "Importing shipment with House Bill '{0}'", identifier.Value)));
						context.SetPropertyInfoValueIfValueNotEmpty(shipment.JS_HouseBillInfo, identifier.Value);
					}
				}
			}
		}

		protected internal void ImportCoLoadMaster(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			var coloadMasterIdentifier = value.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.CoLoadMaster);
			if (coloadMasterIdentifier != null && !coloadMasterIdentifier.Value.IsEmpty)
			{
				var coloadMasterFilter = new ZQuery(JobShipmentSchema.JS_HouseBill, coloadMasterIdentifier.Value);
				coloadMasterFilter.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual,
					new[] { Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.HighVolumeLowValue, Constants.ShipmentTypes.HighVolumeLowValueLegacy });
				coloadMasterFilter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, shipment.PK);

				var coloadMasterShipments = shipment.Factory.Load<CommonShipment>(coloadMasterFilter);
				if (coloadMasterShipments.Length == 1)
				{
					shipment.JS_JS_ColoadMasterShipment = coloadMasterShipments[0].PK;
				}
				else if (coloadMasterShipments.Length > 1)
				{
					context.Notify(new InfoNotification(Res.GetString("266bec33-b9af-498d-92a8-1bf882fbefae", "Unable to identify Coload Master")));
				}
			}
		}

		#region Import Packages

		protected void ImportPackages(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			ImportOuterPackages(shipment, value, context);
			ImportInnerPackages(shipment, value, context);

			foreach (var container in shipment.Containers)
			{
				var (result, message) = NumericalCheckHelper.GetValidDecimalValue(JobContainerSchema.JC_GrossWeight, container.JC_GrossWeight);
				if (!message.IsEmpty)
				{
					container.JC_GrossWeight = result;
					context.Notify(new WarningNotification(message));
				}
			}
		}

		bool CanImportPackages(TBusinessObject shipment)
		{
			return shipment.CanHaveOwnPackLines;
		}

		void ImportOuterPackages(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.ShipmentDetails != null && CanImportPackages(shipment))
			{
				if (shipmentValue.ShipmentDetails.Packages.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					if (shipment.OuterPackLines.Count == 1 && shipmentValue.ShipmentDetails.Packages.Count == 1)
					{
						UpdateSingleOuterPackage(shipment, context, shipmentValue.ShipmentDetails.Packages[0], shipment.OuterPackLines[0]);
					}
					else
					{
						shipment.OuterPackLines.RemoveAndDeleteAll();

						foreach (Xsd.Package package in shipmentValue.ShipmentDetails.Packages)
						{
							PackLine packLine = shipment.OuterPackLines.AddNew();
							UpdateSingleOuterPackage(shipment, context, package, packLine);
						}
					}
				}
			}
		}

		void UpdateSingleOuterPackage(TBusinessObject shipment, IValueObjectImportContext context, Xsd.Package package, PackLine packLine)
		{
			packLine.CurrentConsol = this.ExistingConsol;
			ImportOuterPackagesCore(packLine, package, context);
			if (ExistingConsol != null && !package.ContainerNumber.IsEmpty)
			{
				var existingContainer = ExistingConsol.Containers.Cast<CommonContainer>().Where(x => x.JC_ContainerNum == package.ContainerNumber).ToArray();
				if (existingContainer.Length == 1)
				{
					packLine.SetContainer(ExistingConsol, existingContainer[0]);
				}
				else
				{
					context.Notify(new ErrorNotification(FreightErrorType.ContainerNumberNotFound, package.ContainerNumber));
				}
			}
		}

		protected virtual void ImportOuterPackagesCore(PackLine packline, Xsd.Package package, IValueObjectImportContext context)
		{
			new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>().ImportFromValueObject(packline, package, context);
		}

		void ImportInnerPackages(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.ShipmentDetails != null && CanImportPackages(shipment))
			{
				if (shipmentValue.ShipmentDetails.InnerPackages.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					if (shipment.InnerPackLines.Count == 1 && shipmentValue.ShipmentDetails.InnerPackages.Count == 1)
					{
						new InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>().ImportFromValueObject(shipment.InnerPackLines[0], shipmentValue.ShipmentDetails.InnerPackages[0], context);
					}
					else
					{
						shipment.InnerPackLines.RemoveAndDeleteAll();
						foreach (Xsd.PackageBase innerPackage in shipmentValue.ShipmentDetails.InnerPackages)
						{
							PackLine packLine = shipment.InnerPackLines.AddNew();
							var adapter = new InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>();
							adapter.ImportFromValueObject(packLine, innerPackage, context);
						}
					}
				}
			}
		}

		#endregion

		void ImportOrganisationDetails(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria();
			if (shipmentValue.ShipmentDetails != null)
			{
				if (shipmentValue.ShipmentDetails.ImportBroker.IsSpecified)
				{
					unmatchOrgRecordCriteria.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ImportBroker;
					shipment.JS_OH_ImportBroker = context.FindOrCreateTempOrganisationPK(shipmentValue.ShipmentDetails.ImportBroker, shipment, OrganisationTypes.Broker, unmatchOrgRecordCriteria);
				}
				else if (shipment.JS_OH_ImportBroker.IsEmpty)
				{
					DefaultImportBroker(shipment);
				}

				if (shipmentValue.ShipmentDetails.Deliver.CartageCompany.IsSpecified)
				{
					shipment.DocsAndCartage.DeliveryCartageCoPK = context.FindOrCreateTempOrganisationPK(shipmentValue.ShipmentDetails.Deliver.CartageCompany, shipment, OrganisationTypes.Carrier);
				}
				else
				{
					DefaultImportCartage(shipment, shipmentValue);
				}

				if (SystemDataRegistry.Instance.AllowExportBrokerImport.Value)
				{
					if (shipmentValue.ShipmentDetails.ExportBroker.IsSpecified)
					{
						unmatchOrgRecordCriteria.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ExportBroker;
						shipment.JS_OH_ExportBroker = context.FindOrCreateTempOrganisationPK(shipmentValue.ShipmentDetails.ExportBroker, shipment, OrganisationTypes.Broker, unmatchOrgRecordCriteria);
					}
					else if (shipment.JS_OH_ExportBroker.IsEmpty)
					{
						DefaultExportBroker(shipment);
					}
				}

				if (shipmentValue.ShipmentDetails.Pickup.CartageCompany.IsSpecified)
				{
					shipment.DocsAndCartage.PickupCartageCoPK = context.FindOrCreateTempOrganisationPK(shipmentValue.ShipmentDetails.Pickup.CartageCompany, shipment, OrganisationTypes.Carrier);
				}
				else
				{
					DefaultExportCartage(shipment, shipmentValue);
				}
			}
		}

		#region DefaultExportBroker

#if DEBUG
		public
#endif
 void DefaultExportBroker(TBusinessObject shipment)
		{
			shipment.SetDefaultExportBroker();
		}

		#endregion

		#region DefaultImportBroker

#if DEBUG
		public
#endif
		void DefaultImportBroker(TBusinessObject shipment)
		{
			shipment.SetDefaultImportBroker();
		}

		void DefaultCartage(TBusinessObject shipment, Xsd.ShipmentShipmentDetails shipmentDetails, OrgHeader org, string pickupOrDelivery, Action<ZGuid> setCartage)
		{
			if (shipmentDetails != null && org != null)
			{
				ZGuid defaultCartageCompanyPK = ZGuid.Empty;
				OrgHeader cartage = null;

				if (shipmentDetails.TransportMode == Xsd.TransportMode.AIR)
				{
					cartage = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickupOrDelivery, Constants.TransportModes.Air, ZString.Empty);
					defaultCartageCompanyPK = FreightDataRegistry.Instance.AIRCartageCompany.Value;
				}
				else if (shipmentDetails.TransportMode == Xsd.TransportMode.SEA)
				{
					if (shipmentDetails.PackingMode == Xsd.ContainerMode.FCL)
					{
						cartage = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickupOrDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
						defaultCartageCompanyPK = FreightDataRegistry.Instance.FCLCartageCompany.Value;
					}
					else if (shipmentDetails.PackingMode == Xsd.ContainerMode.LCL)
					{
						cartage = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickupOrDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
						defaultCartageCompanyPK = FreightDataRegistry.Instance.LCLCartageCompany.Value;
					}
				}
				else
				{
					cartage = org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, pickupOrDelivery, shipment.TransportMode, shipment.PackingMode);
				}

				if (cartage != null)
				{
					setCartage(cartage.PK);
				}
				else if (!defaultCartageCompanyPK.IsEmpty && ShouldDefaultCartageCompanyFromRegistry(org.OH_RL_NKClosestPort))
				{
					setCartage(defaultCartageCompanyPK);
				}
			}
		}

		public void DefaultImportCartage(TBusinessObject shipment, Xsd.Shipment shipmentValue)
		{
			DefaultCartage(shipment, shipmentValue.ShipmentDetails, shipment.Consignee, RelatedPartyDirectionList.Codes.Delivery, c => shipment.DocsAndCartage.DeliveryCartageCoPK = c);
		}

		public void DefaultExportCartage(TBusinessObject shipment, Xsd.Shipment shipmentValue)
		{
			DefaultCartage(shipment, shipmentValue.ShipmentDetails, shipment.Consignor, RelatedPartyDirectionList.Codes.Pickup, c => shipment.DocsAndCartage.PickupCartageCoPK = c);
		}

		bool ShouldDefaultCartageCompanyFromRegistry(ZString closestPortUnloco)
		{
			bool result = (closestPortUnloco == GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			if (!result)
			{
				foreach (GlbBranchExtraPorts extraPort in GlbBranch.CurrentBranch.ExtraPorts)
				{
					if (closestPortUnloco == extraPort.GY_RL_NKAdditionalBranchRelatedPort)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		void ImportPickupAndDeliveryInformation(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			ImportDeliveryInformation(shipment, shipmentValue, context);
			ImportPickupInformation(shipment, shipmentValue, context);
		}

		void ImportDeliveryInformation(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.ShipmentDetails.Deliver.DeliveryFrom.IsValid)
			{
				shipment.DocsAndCartage.JP_EstimatedDelivery = shipmentValue.ShipmentDetails.Deliver.DeliveryFrom.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Deliver.DeliveryRequiredBy.IsValid)
			{
				shipment.DocsAndCartage.JP_DeliveryRequiredBy = shipmentValue.ShipmentDetails.Deliver.DeliveryRequiredBy.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Deliver.CartageAdvised.IsValid)
			{
				shipment.DocsAndCartage.JP_DeliveryCartageAdvised = shipmentValue.ShipmentDetails.Deliver.CartageAdvised.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Deliver.GoodsDelivered.IsValid)
			{
				shipment.DocsAndCartage.JP_DeliveryCartageCompleted = shipmentValue.ShipmentDetails.Deliver.GoodsDelivered.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Deliver.DeliveryAgent.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				shipment.JS_OH_DeliveryAgent = context.FindOrCreateTempOrganisationPK(shipmentValue.ShipmentDetails.Deliver.DeliveryAgent, shipment, OrganisationTypes.Forwarder);
			}
		}

		void ImportPickupInformation(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.ShipmentDetails.Pickup.PickupFrom.IsValid)
			{
				shipment.DocsAndCartage.JP_EstimatedPickup = shipmentValue.ShipmentDetails.Pickup.PickupFrom.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Pickup.PickupRequiredBy.IsValid)
			{
				shipment.DocsAndCartage.JP_PickupRequiredBy = shipmentValue.ShipmentDetails.Pickup.PickupRequiredBy.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Pickup.CartageAdvised.IsValid)
			{
				shipment.DocsAndCartage.JP_PickupCartageAdvised = shipmentValue.ShipmentDetails.Pickup.CartageAdvised.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Pickup.GoodsPickup.IsValid)
			{
				shipment.DocsAndCartage.JP_PickupCartageCompleted = shipmentValue.ShipmentDetails.Pickup.GoodsPickup.ToSmallDateTime();
			}

			if (shipmentValue.ShipmentDetails.Pickup.DateOfReceipt.IsValid)
			{
				shipment.JS_A_RCV = shipmentValue.ShipmentDetails.Pickup.DateOfReceipt.ToSmallDateTime();
			}
		}

		void ImportCustomAttributeInformation(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomAttrib1Info, shipmentValue.ShipmentDetails.Custom.CustomAttribute1, shipmentValue.ShipmentDetails.Custom.CustomAttribute1Specified, "CustomAttribute1");
			context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomAttrib2Info, shipmentValue.ShipmentDetails.Custom.CustomAttribute2, shipmentValue.ShipmentDetails.Custom.CustomAttribute2Specified, "CustomAttribute2");
			if (!shipmentValue.ShipmentDetails.Custom.Date1.IsEmpty)
			{
				context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomDate1Info, shipmentValue.ShipmentDetails.Custom.Date1.ToSmallDateTime().ToDateTime());
			}

			if (!shipmentValue.ShipmentDetails.Custom.Date2.IsEmpty)
			{
				context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomDate2Info, shipmentValue.ShipmentDetails.Custom.Date2.ToSmallDateTime().ToDateTime());
			}

			if (shipmentValue.ShipmentDetails.Custom.Decimal1Specified)
			{
				context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomDecimal1Info, shipmentValue.ShipmentDetails.Custom.Decimal1, JobDocsAndCartageSchema.JP_CustomDecimal1);
			}

			if (shipmentValue.ShipmentDetails.Custom.Decimal2Specified)
			{
				context.SetPropertyInfoValue(shipment.DocsAndCartage.JP_CustomDecimal2Info, shipmentValue.ShipmentDetails.Custom.Decimal2, JobDocsAndCartageSchema.JP_CustomDecimal2);
			}

			if (shipmentValue.ShipmentDetails.Custom.Flag1Specified)
			{
				shipment.DocsAndCartage.JP_CustomFlag1 = (shipmentValue.ShipmentDetails.Custom.Flag1 == Xsd.TrueFalse.@true);
			}

			if (shipmentValue.ShipmentDetails.Custom.Flag2Specified)
			{
				shipment.DocsAndCartage.JP_CustomFlag2 = (shipmentValue.ShipmentDetails.Custom.Flag2 == Xsd.TrueFalse.@true);
			}
		}

		void ImportDocAddresses(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentDetails.DocAddresses.IsSpecified)
			{
				DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("");
				helper.ImportFromValueObjectCollection(value.ShipmentDetails.DocAddresses.DocAddress, shipment.DocAddresses, context);
			}

			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRD) == null)
			{
				if (value.ShipmentDetails.Consignor.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.ConsignorPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Consignor, shipment, OrganisationTypes.Consignor);
				}
			}
			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CED) == null)
			{
				if (value.ShipmentDetails.Consignee.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					shipment.ConsigneePK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.Consignee, shipment, OrganisationTypes.Consignee);
				}
			}
			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRG) == null && value.ShipmentDetails.Pickup.Address.IsSpecified)
			{
				if (value.ShipmentDetails.Pickup.Address.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					SetDocAddressValuesFromPickupOrDeliveryAddress(value.ShipmentDetails.Pickup.Address, shipment.ConsignorPickupAddress, Res.GetString("470b99e4-3284-41b6-8d9b-99fe2291d1cf", "Pickup"), context);
				}
			}
			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CEG) == null && value.ShipmentDetails.Deliver.Address.IsSpecified)
			{
				if (value.ShipmentDetails.Deliver.Address.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
				{
					SetDocAddressValuesFromPickupOrDeliveryAddress(value.ShipmentDetails.Deliver.Address, shipment.ConsigneeDeliveryAddress, Res.GetString("a9cfdde0-d40f-4036-becd-d4a49e0219ba", "Delivery"), context);
				}
			}
			if (value.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.NPP) == null)
			{
				ImportNotifyParty(shipment, value, context);
			}
		}

		void ImportNotifyParty(TBusinessObject shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentDetails.NotifyParty.IsSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				ContactValueObjectHelper helper = new ContactValueObjectHelper("");

				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = OrganisationsSubTypeList.Descriptions.NotifyParty };
				shipment.NotifyPartyDocumentaryAddress.OrganisationPK = context.FindOrCreateTempOrganisationPK(value.ShipmentDetails.NotifyParty.Organisation, shipment, OrganisationTypes.None, unmatchOrgRecordCriteria);
				shipment.NotifyContact = helper.FromContactReferenceGetContactName(value.ShipmentDetails.NotifyParty, context);
			}
		}

		protected virtual void ImportBilling(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.Billing.IsSpecified && SystemDataRegistry.Instance.AllowBillingImportIntoShipment.Value)
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ImportFromValueObject(shipment, shipmentValue.Billing, context);
			}
		}

		void ImportConsignorCOD(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			if (shipmentValue.ShipmentDetails.ConsignorCODAmountSpecified)
			{
				context.SetPropertyInfoValue(shipment.JS_ShipperCODAmountInfo, shipmentValue.ShipmentDetails.ConsignorCODAmount, JobShipmentSchema.JS_ShipperCODAmount);
				shipment.JS_ShipperCODPayMethod = shipmentValue.ShipmentDetails.ConsignorCODType;
			}
		}

		protected void SetDocAddressValuesFromPickupOrDeliveryAddress(Xsd.OrgAddress address, JobDocAddress docAddress, string description, IValueObjectImportContext context)
		{
			docAddress.E2_AddressOverride = true;
			context.SetPropertyInfoValue(docAddress.E2_CompanyNameInfo, address.CompanyName, address.CompanyNameSpecified, description + " CompanyName");
			context.SetPropertyInfoValue(docAddress.E2_Address1Info, address.AddressLine1, address.AddressLine1Specified, description + " Address1");
			context.SetPropertyInfoValue(docAddress.E2_Address2Info, address.AddressLine2, address.AddressLine2Specified, description + " Address2");
			context.SetPropertyInfoValue(docAddress.E2_CityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified, Res.GetString("a3024ca2-e47b-44f5-83db-99f7b4fb037e", "{0} City", description));
			context.SetPropertyInfoValue(docAddress.E2_StateInfo, address.StateOrProvince, address.StateOrProvinceSpecified, Res.GetString("d3e96407-a8f3-452b-b179-10cb29dbdbd2", "{0} State", description));
			context.SetPropertyInfoValue(docAddress.E2_PostcodeInfo, address.PostCode, address.PostCodeSpecified, description + " PostCode");
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject shipment, Xsd.Shipment result, IValueObjectExportContext context)
		{
			string errorContext = Res.GetString("d7e58e93-ac69-47cc-89e0-99ba881ed0ac", "House Bill {0}", shipment.JS_HouseBill);
			result.Events = StmALogValueObjectDataAdapter.New(shipment, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);
			result.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			result.Declaration.IsSpecified = false;

			ExportHouseBillIdentifier(shipment, result, context);
			ExportCoLoadMaster(shipment, result, context);
			ExportBranchIdentifier(shipment, result, context);

			result.ShipmentDetailsSpecified = true;

			result.ShipmentDetails.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(shipment.JS_TransportMode, errorContext, context);

			if (!shipment.JS_ShipmentType.IsEmpty)
			{
				result.ShipmentDetails.ForwardingShipmentType = ForwardingShipmentTypeToXmlCodeMappings.Instance.GetExternalCode(shipment.JS_ShipmentType, errorContext, context);
			}
			result.ShipmentDetails.PortOfOrigin = XsdMovement.FromPortEstimatedActualDates(shipment.Factory, shipment.JS_RL_NKOrigin, shipment.JS_E_DEP, ZDateTime.Empty);
			result.ShipmentDetails.PortofDestination = XsdMovement.FromPortEstimatedActualDates(shipment.Factory, shipment.JS_RL_NKDestination, shipment.JS_E_ARV, ZDateTime.Empty);

			ExportShipmentOrganisations(shipment, result, context);

			result.ShipmentDetails.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(shipment.JS_PackingMode, errorContext, context);
			result.ShipmentDetails.PackingModeSpecified = true;
			result.ShipmentDetails.BookingReference = shipment.JS_BookingReference;
			result.ShipmentDetails.InterimReceipt = shipment.JS_InterimReceipt;
			result.ShipmentDetails.HBLIssueDate = shipment.JS_HouseBillIssueDate;
			result.ShipmentDetails.ShippedOnBoardDate = shipment.JS_ShippedOnBoardDate;
			if (!shipment.JS_ShippedOnBoard.IsEmpty)
			{
				result.ShipmentDetails.ShippedOnBoardType = ShippedOnBoardTypeCodeMappings.Instance.GetExternalCode(shipment.JS_ShippedOnBoard, errorContext, context);
				result.ShipmentDetails.ShippedOnBoardTypeSpecified = true;
			}
			result.ShipmentDetails.HBLContainerMode = shipment.JS_HBLContainerPackModeOverride;
			result.ShipmentDetails.NoOriginalBills = shipment.JS_NoOriginalBills.ToString();
			result.ShipmentDetails.NoCopyBills = shipment.JS_NoCopyBills.ToString();

			result.ShipmentDetails.TotalInnerPacksQty = ExportPackageType(shipment.JS_F3_NKTotalCountPackType, shipment.JS_TotalPackageCount, errorContext, context, shipment);
			result.ShipmentDetails.TotalOuterPacksQty = ExportPackageType(shipment.JS_F3_NKPackType, shipment.JS_OuterPacks, errorContext, context, shipment);

			result.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_ActualWeight, WeightUQXmlCodeMappings.Instance.GetExternalCode(shipment.JS_UnitOfWeight, errorContext, context));
			result.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_ActualVolume, VolumeUQXmlCodeMappings.Instance.GetExternalCode(shipment.JS_UnitOfVolume, errorContext, context));
			result.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_ActualChargeable, shipment.JS_ChargeableUnit);
			result.ShipmentDetails.GoodsValue = Xsd.FinancialValue.FromAmountAndCurrency(shipment.JS_GoodsValue, shipment.GoodsValueCurr);
			result.ShipmentDetails.InsuranceValue = Xsd.FinancialValue.FromAmountAndCurrency(shipment.JS_InsuranceValue, shipment.InsuranceCurrency);
			result.ShipmentDetails.FreightRate = Xsd.FinancialValue.FromAmountAndCurrency(shipment.JS_UnitFreightRate, shipment.FrtRateCurrency);
			result.ShipmentDetails.ReleaseType = ReleaseTypeXmlCodeMappings.Instance.GetExternalCode(shipment.JS_ReleaseType, errorContext, context);
			result.ShipmentDetails.ReleaseTypeSpecified = true;
			result.ShipmentDetails.BookedDate = shipment.JS_A_BKD;

			if (!shipment.JS_LoadingMeters.IsEmpty)
			{
				result.ShipmentDetails.LoadingMeters = shipment.JS_LoadingMeters;
			}

			result.ShipmentDetails.DeclarationStyle = shipment.DeclarationForDocuments != null ? (ZString)shipment.DeclarationForDocuments[JobDeclarationSchema.Constants.JE_MessageSubType] : ZString.Empty;

			if (!shipment.JS_MarksAndNumbers.IsEmpty)
			{
				result.ShipmentDetails.MarksAndNumbers = shipment.JS_MarksAndNumbers;
			}

			if (!shipment.JS_RS_NKServiceLevel.IsEmpty)
			{
				result.ShipmentDetails.ServiceLevel = shipment.JS_RS_NKServiceLevel;
			}

			if (!shipment.JS_INCO.IsEmpty)
			{
				result.ShipmentDetails.Incoterm = shipment.JS_INCO;
			}

			if (!shipment.JS_AdditionalTerms.IsEmpty)
			{
				result.ShipmentDetails.AdditionalTerms = shipment.JS_AdditionalTerms;
			}

			result.ShipmentDetails.AgentReference = shipment.JS_UniqueConsignRef;
			if (!shipment.Logs.CreatedDateUtc.IsEmpty)
			{
				result.ShipmentDetails.DateCreated = shipment.Logs.CreatedDateUtc.ToDateTime();
			}

			result.ShipmentDetails.CustomsEntryNumbers = XsdCustomEntryNumbersObjectHelper.ExportFromCusEntryNumCollection(shipment.CusEntryNumbersForAllCountries);

			result.ShipmentDetails.ExporterStatement = shipment.DocsAndCartage.JP_ExportStatement;
			ExportPickupAndDeliveryInformation(shipment, result, context);

			if (shipment.Job != null)
			{
				if (shipment.Job.RepSales != null)
				{
					result.ShipmentDetails.SalesRep = shipment.Job.RepSales.GS_Code;
				}
				if (shipment.Job.LocalCharges != null)
				{
					result.ShipmentDetails.LocalClient = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.Job.LocalCharges, context);
				}
			}

			result.ShipmentDetails.TEU = TotalTEU(shipment);
			result.ShipmentDetails.TEUSpecified = (result.ShipmentDetails.TEU > 0);
			ExportOuterPackages(shipment, result, context);
			ExportInnerPackages(shipment, result, context);
			ExportCustomAttributes(shipment, result, context);
			ExportTransportPlan(shipment, result, context);

			if (IncludeeDocs)
			{
				ExportStorageDocs(shipment, result, context);
			}

			result.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(shipment.Notes, context);
			result.ShipmentDetails.GoodsDescription = shipment.JS_GoodsDescription;

			ExportDocAddresses(shipment, result, context);
			ReferenceNumberDataAdapter.ExportReferenceNumbers(shipment.Numbers, result.ShipmentDetails.ReferenceNumbers, context);
			ExportBilling(shipment, result, context);
			ExportConsignorCOD(shipment, result, context);
			DocDataValueObjectDataAdapter.ExportData(shipment, result.DocData, context);

			OnAfterExportFromValueObjectCore(shipment, result, context);

			AddExportEvent(result, shipment, context, DataExportReference);
		}

		Xsd.DimensionValue ExportPackageType(ZString enterprisePackageType, ZInt packageCount, string errorContext, IValueObjectExportContext context, TBusinessObject shipment)
		{
			Xsd.DimensionValue dimensionValue = new Xsd.DimensionValue();

			string externalCode = PkgUnitXmlCodeMappings.Instance.GetExternalCode(enterprisePackageType, errorContext, context);
			if (externalCode != null)
			{
				dimensionValue = Xsd.DimensionValue.FromAmountAndUnit(packageCount, externalCode);
			}
			else if (!enterprisePackageType.IsEmpty)
			{
				string unknownExternalCodeErrorMessage = Res.GetString("f9159942-5f0e-4408-8698-078ab0ead80d", "A non-system defined package type ({0}) on shipment {1} has been exported. The organization that imports this XML file may not have that package type in their registry (they can add it in Maintain -> Reference Files -> Package Types). If they do not have the package type, their import will not fail, but the record will have an error on the package type field when they try to edit it.", enterprisePackageType, shipment.JS_UniqueConsignRef);

				context.Notify(new WarningNotification(unknownExternalCodeErrorMessage));
				dimensionValue = Xsd.DimensionValue.FromAmountAndUnit(packageCount, enterprisePackageType);
			}

			return dimensionValue;
		}

		protected virtual ZString DataExportReference
		{
			get { return ZString.Empty; }
		}

		protected virtual void OnAfterExportFromValueObjectCore(TBusinessObject shipment, Xsd.Shipment value, IValueObjectExportContext context)
		{
		}

		ZDecimal TotalTEU(TBusinessObject shipment)
		{
			ZDecimal result = 0;
			if (shipment != null && shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL)
			{
				foreach (CommonContainer container in shipment.Containers)
				{
					RefContainer refContainer = container.Container;
					if (refContainer != null)
					{
						result += refContainer.RC_TEU * Math.Min((short)1, container.JC_ContainerCount);
					}
				}
			}
			return result;
		}

		void ExportHouseBillIdentifier(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			if (!shipment.JS_HouseBill.IsEmpty)
			{
				identifier.Value = shipment.JS_HouseBill;
			}
		}

		void ExportCoLoadMaster(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (shipment.CoLoadMasterShipment != null && shipment.JS_HouseBill != shipment.CoLoadMasterShipment.JS_HouseBill)
			{
				Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
				identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;
				identifier.Value = shipment.CoLoadMasterShipment.JS_HouseBill;
			}
		}

		void ExportBranchIdentifier(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (shipment.Job != null)
			{
				if (shipment.Job.Branch != null)
				{
					Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
					identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Branch;
					identifier.Value = shipment.Job.Branch.GB_Code;
				}
			}
		}

		void ExportPickupAndDeliveryInformation(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			shipmentValue.ShipmentDetails.Deliver.DeliveryFrom = shipment.DocsAndCartage.JP_EstimatedDelivery;
			shipmentValue.ShipmentDetails.Deliver.DeliveryRequiredBy = shipment.DocsAndCartage.JP_DeliveryRequiredBy;
			shipmentValue.ShipmentDetails.Deliver.CartageAdvised = shipment.DocsAndCartage.JP_DeliveryCartageAdvised;
			shipmentValue.ShipmentDetails.Deliver.GoodsDelivered = shipment.DocsAndCartage.JP_DeliveryCartageCompleted;
			shipmentValue.ShipmentDetails.Deliver.CartageCompany = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.DocsAndCartage.DeliveryCartageCo, context);
			shipmentValue.ShipmentDetails.Deliver.DeliveryAgent = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.DeliveryAgent, context);
			SetAddressesWhereOrgIsDifferent(shipmentValue.ShipmentDetails.Deliver.Address, shipment.ConsigneeDeliveryAddress, shipment.ConsigneeDocumentaryAddress);
			shipmentValue.ShipmentDetails.Deliver.Address.CityOrSuburb = shipment.ConsigneeDeliveryAddress.E2_City;
			shipmentValue.ShipmentDetails.Deliver.Address.StateOrProvince = shipment.ConsigneeDeliveryAddress.E2_State;
			shipmentValue.ShipmentDetails.Deliver.Address.PostCode = shipment.ConsigneeDeliveryAddress.E2_Postcode;

			AddressValueObjectHelper helper = GetNewAddressValueObjectHelper(Res.GetString("67307886-992e-4c1a-a95c-4384a66b384d", "CFS Release Depot"));
			shipmentValue.ShipmentDetails.Deliver.CFS.Address = helper.ToAddressReference(shipment.ImportReleaseDepot, context);
			shipmentValue.ShipmentDetails.Deliver.CFS.Location = shipment.JS_WarehouseLocation;

			shipmentValue.ShipmentDetails.Pickup.PickupFrom = shipment.DocsAndCartage.JP_EstimatedPickup;
			shipmentValue.ShipmentDetails.Pickup.PickupRequiredBy = shipment.DocsAndCartage.JP_PickupRequiredBy;
			shipmentValue.ShipmentDetails.Pickup.CartageAdvised = shipment.DocsAndCartage.JP_PickupCartageAdvised;
			shipmentValue.ShipmentDetails.Pickup.GoodsPickup = shipment.DocsAndCartage.JP_PickupCartageCompleted;
			shipmentValue.ShipmentDetails.Pickup.CartageCompany = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.DocsAndCartage.PickupCartageCo, context);
			shipmentValue.ShipmentDetails.Pickup.DateOfReceipt = shipment.JS_A_RCV;
			SetAddressesWhereOrgIsDifferent(shipmentValue.ShipmentDetails.Pickup.Address, shipment.ConsignorPickupAddress, shipment.ConsignorDocumentaryAddress);
			shipmentValue.ShipmentDetails.Pickup.Address.CityOrSuburb = shipment.ConsignorPickupAddress.E2_City;
			shipmentValue.ShipmentDetails.Pickup.Address.StateOrProvince = shipment.ConsignorPickupAddress.E2_State;
			shipmentValue.ShipmentDetails.Pickup.Address.PostCode = shipment.ConsignorPickupAddress.E2_Postcode;

			helper = GetNewAddressValueObjectHelper(Res.GetString("490dad77-c102-4e07-854d-d9348a2cdfa3", "CFS Receiving Depot"));
			shipmentValue.ShipmentDetails.Pickup.CFS.Address = helper.ToAddressReference(shipment.ExportReceivingDepot, context);
			shipmentValue.ShipmentDetails.Pickup.CFS.Location = shipment.JS_WarehouseLocation;

			ExportDeliveryConfirms(shipmentValue, shipment, context);

			if (!shipment.JS_Calc_DeliveryCartageZone.IsEmpty)
			{
				Xsd.Zone zone = shipmentValue.ShipmentDetails.Deliver.Zones.AddNew();
				zone.Type = Xsd.ZoneType.CartageZone;
				zone.Code = shipment.JS_Calc_DeliveryCartageZone;
				zone.IsSpecified = true;
				shipmentValue.ShipmentDetails.Deliver.Zones.IsSpecified = true;
			}

			if (!shipment.JS_Calc_PickupCartageZone.IsEmpty)
			{
				Xsd.Zone zone = shipmentValue.ShipmentDetails.Pickup.Zones.AddNew();
				zone.Type = Xsd.ZoneType.CartageZone;
				zone.Code = shipment.JS_Calc_PickupCartageZone;
				zone.IsSpecified = true;
				shipmentValue.ShipmentDetails.Pickup.Zones.IsSpecified = true;
			}

			#region US & Canada Only.

			/* Virtual properties JS_Calc_ACIConsigneeDestinationZone & JS_Calc_ACIConsignorOriginZone are only calculated for United States and Canada
			 * only, so we don't need to second check. */

			if (!shipment.JS_Calc_ACIConsigneeDestinationZone.IsEmpty)
			{
				Xsd.Zone zone = shipmentValue.ShipmentDetails.Deliver.Zones.AddNew();
				zone.Type = Xsd.ZoneType.ACIZone;
				zone.Code = shipment.JS_Calc_ACIConsigneeDestinationZone;
				zone.IsSpecified = true;
				shipmentValue.ShipmentDetails.Deliver.Zones.IsSpecified |= zone.IsSpecified;
			}

			if (!shipment.JS_Calc_ACIConsignorOriginZone.IsEmpty)
			{
				Xsd.Zone zone = shipmentValue.ShipmentDetails.Pickup.Zones.AddNew();
				zone.Type = Xsd.ZoneType.ACIZone;
				zone.Code = shipment.JS_Calc_ACIConsignorOriginZone;
				zone.IsSpecified = true;
				shipmentValue.ShipmentDetails.Pickup.Zones.IsSpecified |= zone.IsSpecified;
			}

			#endregion
		}

		AddressValueObjectHelper GetNewAddressValueObjectHelper(string context)
		{
			return new AddressValueObjectHelper(context);
		}

		void ExportDeliveryConfirms(Xsd.Shipment shipmentValue, TBusinessObject shipment, IValueObjectExportContext context)
		{
			CommonPickupDeliveryConfirmValueObjectDataAdapter adapter = new CommonPickupDeliveryConfirmValueObjectDataAdapter();
			foreach (CommonPickupDeliveryConfirm leg in shipment.DeliveryConfirms)
			{
				shipmentValue.ShipmentDetails.Deliver.DeliveryLegs.Add(adapter.ExportToValueObject(leg, context));
			}

			foreach (CommonContainer container in shipment.Containers)
			{
				if (container.DestinationGetConfirm != null)
				{
					shipmentValue.ShipmentDetails.Deliver.DeliveryLegs.Add(adapter.ExportToValueObject(container.DestinationGetConfirm, context));
				}
			}
		}

		protected virtual void SetAddressesWhereOrgIsDifferent(Xsd.OrgAddress addressToSet, JobDocAddress setFromDocAddress, JobDocAddress documentaryAddress)
		{
			if (setFromDocAddress.E2_AddressOverride || setFromDocAddress.E2_CompanyName != documentaryAddress.E2_CompanyName)
			{
				addressToSet.CompanyName = setFromDocAddress.E2_CompanyNameTruncated;
			}

			addressToSet.AddressLine1 = setFromDocAddress.E2_Address1;
			addressToSet.AddressLine2 = setFromDocAddress.E2_Address2;
		}

		void ExportOuterPackages(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (ExistingConsol != null)
			{
				shipment.OuterPackLines.CurrentConsol = ExistingConsol;
			}

			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				if (shipmentValue.ShipmentDetails.Packages == null)
				{
					shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
				}
				Xsd.Package packageValue = ExportOuterPackagesCore(packLine, context);
				shipmentValue.ShipmentDetails.Packages.Add(packageValue);
			}
		}

		protected virtual Xsd.Package ExportOuterPackagesCore(PackLine packline, IValueObjectExportContext context)
		{
			return new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>().ExportToValueObject(packline, context);
		}

		void ExportInnerPackages(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			foreach (PackLine packLine in shipment.InnerPackLines)
			{
				if (shipmentValue.ShipmentDetails.InnerPackages == null)
				{
					shipmentValue.ShipmentDetails.InnerPackages = new Xsd.PackageBaseCollection();
				}
				Xsd.PackageBase packageValue = new InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>().ExportToValueObject(packLine, context);
				shipmentValue.ShipmentDetails.InnerPackages.Add(packageValue);
			}
		}

		#region Export Shipment Organisations

		void ExportShipmentOrganisations(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			ExportConsignee(shipment, shipmentValue, context);
			ExportConsignor(shipment, shipmentValue, context);

			shipmentValue.ShipmentDetails.ImportBroker = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.ImportBroker, context);
			shipmentValue.ShipmentDetails.ExportBroker = GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(shipment.ExportBroker, context);

			ExportNotifyParty(shipment, shipmentValue, context);
		}

		void ExportConsignee(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			shipmentValue.ShipmentDetails.Consignee = ExportOrganisation(shipment, shipment.ConsigneeDocumentaryAddress, context, false);
		}

		void ExportConsignor(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			shipmentValue.ShipmentDetails.Consignor = ExportOrganisation(shipment, shipment.ConsignorDocumentaryAddress, context, false);
		}

		void ExportNotifyParty(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (shipment.NotifyPartyDocumentaryAddress != null)
			{
				Xsd.ContactReference contactReference = new Xsd.ContactReference();
				contactReference.ContactSequenceRef = 1;
				contactReference.Organisation = ExportOrganisation(shipment, shipment.NotifyPartyDocumentaryAddress, context);

				shipmentValue.ShipmentDetails.NotifyParty = contactReference;
			}
		}

		Xsd.Organisation ExportOrganisation(TBusinessObject shipment, JobDocAddress jobDocAddress, IValueObjectExportContext context, bool shouldExportContact = true)
		{
			if (!jobDocAddress.E2_AddressOverride)
			{
				return GetNewOrganisationValueObjectDataAdapter(shipment).ExportToValueObject(jobDocAddress.Organisation, context);
			}

			return new DocAddressValueObjectHelper("").ExportToOrganisationValueObject(jobDocAddress, context, shouldExportContact);
		}

		#endregion

		void ExportStorageDocs(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			Type adapterTypeToCreate1 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageDocsValueObjectDataAdapter>());
			Type adapterTypeToCreate2 = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<IStorageFilesValueObjectDataAdapter>());
			IValueObjectDataAdapter storageDocsDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate1);
			IValueObjectDataAdapter storageFilesDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(adapterTypeToCreate2);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IStorageMainForPK documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(shipment.Factory);
			IDocumentsView storageMain = documentFactory.GetStorageMain(shipment.PK);

			if (storageMain != null)
			{
				foreach (BusinessObject storageDoc in storageMain.DocumentCollectionView)
				{
					shipmentValue.Documents.Add((Xsd.Document)storageDocsDataAdapter.ExportToValueObject(storageDoc, context));
				}
				foreach (BusinessObject storageDoc in storageMain.PDFFilesCollectionView)
				{
					shipmentValue.Documents.Add((Xsd.Document)storageFilesDataAdapter.ExportToValueObject(storageDoc, context));
				}
			}
		}

		bool IncludeeDocs
		{
			get { return SystemDataRegistry.Instance.IncludeShipmenteDocs.Value; }
		}

		void ExportCustomAttributes(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			shipmentValue.ShipmentDetails.Custom.CustomAttribute1 = shipment.DocsAndCartage.JP_CustomAttrib1;
			shipmentValue.ShipmentDetails.Custom.CustomAttribute2 = shipment.DocsAndCartage.JP_CustomAttrib2;
			shipmentValue.ShipmentDetails.Custom.Date1 = shipment.DocsAndCartage.JP_CustomDate1;
			shipmentValue.ShipmentDetails.Custom.Date2 = shipment.DocsAndCartage.JP_CustomDate2;

			shipmentValue.ShipmentDetails.Custom.Decimal1 = shipment.DocsAndCartage.JP_CustomDecimal1;
			shipmentValue.ShipmentDetails.Custom.Decimal2 = shipment.DocsAndCartage.JP_CustomDecimal2;
			shipmentValue.ShipmentDetails.Custom.Flag1 = (shipment.DocsAndCartage.JP_CustomFlag1) ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			shipmentValue.ShipmentDetails.Custom.Flag2 = (shipment.DocsAndCartage.JP_CustomFlag2) ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			shipmentValue.ShipmentDetails.Custom.Decimal1Specified = true;
			shipmentValue.ShipmentDetails.Custom.Decimal2Specified = true;
			shipmentValue.ShipmentDetails.Custom.Flag1Specified = true;
			shipmentValue.ShipmentDetails.Custom.Flag2Specified = true;
		}

		protected virtual void ExportTransportPlan(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			foreach (Transport transport in shipment.Transports)
			{
				Xsd.PlannedLeg plannedLeg = shipmentValue.ShipmentDetails.TransportPlan.AddNew();
				XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLeg, transport, context, Res.GetString("6783aed8-2172-42b8-9c14-3f0a84274683", "Transport Plan"));
			}
		}

		virtual protected void ExportDocAddresses(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			DocAddressValueObjectHelper helper = new DocAddressValueObjectHelper("");
			helper.ExportToValueObjectCollection(shipment.DocAddresses, shipmentValue.ShipmentDetails.DocAddresses.DocAddress, context);
		}

		void ExportBilling(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			JobHeader header = new JobHeader.Loader(shipment).Load();

			if (header == null || !IncludeBillingInfoInShipmentXML)
			{
				shipmentValue.Billing.IsSpecified = false;
			}
			else
			{
				IValueObjectDataAdapter dataAdapter = CreateBillingDataAdapter();
				dataAdapter.ExportToValueObject(shipment, shipmentValue.Billing, context);
			}
		}

		protected virtual bool IncludeBillingInfoInShipmentXML
		{
			get { return SystemDataRegistry.Instance.IncludeBillingInfoInShipmentXML.Value; }
		}

		void ExportConsignorCOD(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (shipment.IsDomestic() && !shipment.JS_ShipperCODAmount.IsEmpty)
			{
				shipmentValue.ShipmentDetails.ConsignorCODAmount = shipment.JS_ShipperCODAmount;
				shipmentValue.ShipmentDetails.ConsignorCODType = shipment.JS_ShipperCODPayMethod;
				shipmentValue.ShipmentDetails.ConsignorCODAmountSpecified = true;
			}
		}

		static IValueObjectDataAdapter CreateBillingDataAdapter()
		{
			return (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.IBillingDataAdapter>());
		}

		#endregion

		#region Implementation

		DocDataValueObjectDataAdapter DocDataValueObjectDataAdapter
		{
			get { return docDataValueObjectDataAdapter ?? (docDataValueObjectDataAdapter = new DocDataValueObjectDataAdapter()); }
		}
		DocDataValueObjectDataAdapter docDataValueObjectDataAdapter;

		#endregion
	}
}
