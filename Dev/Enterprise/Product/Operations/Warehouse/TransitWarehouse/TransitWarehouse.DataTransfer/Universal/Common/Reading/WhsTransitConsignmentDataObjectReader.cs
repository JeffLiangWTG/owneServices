using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class SingleOrMultiConsignmentDataObjectManager
	{
		public SingleOrMultiConsignmentDataObjectManager(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseFromConsol)
		{
			WarehouseFromDataObject = warehouseFromConsol;
			var stdOrMasterShipment = UniversalShipment.GetSourceDataObject(dataObject);

			if (WarehouseFromDataObject == null)
			{
				WhsTransitLogHelper.LogRecipientRole(dataObject, logger);
				WarehouseFromDataObject = WarehouseMatchingHelper.GetWarehouse(stdOrMasterShipment, factory, logger)
					?? WarehouseMatchingHelper.GetWarehouse(dataObject, factory, logger);
			}

			if (TransitUniversalHelper.IsMasterShipmentRepresentingAllChildShipments(stdOrMasterShipment))
			{
				if (WarehouseFromDataObject == null)
				{
					throw new DataObjectReadFailureException(Res.GetString("41317986-e270-4570-b4b3-89b228f67e8d", "Cannot find a matched Transit Warehouse"));
				}

				var branchPK = WarehouseFromDataObject.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);

				Master = stdOrMasterShipment;

				var shipmentsToCreateConsignments = !branchPK.IsEmpty
					? TransitUniversalHelper.GetShipmentsToCreateConsignments(new[] { stdOrMasterShipment }, branchPK.ToGuid())
					: new[] { stdOrMasterShipment };
				var hasMasterWithNoSubs = !shipmentsToCreateConsignments.Any(); // If no shipments returned it must be master with no subs.
				if (hasMasterWithNoSubs)
				{
					var forwardingShipmentDS = Master.GetMatchingDataSource(DataContextType.ForwardingShipment);
					throw new DataObjectReadFailureException(Res.GetString("fe478fd5-4cec-4422-b10a-0f3cc6a11dc8", "Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment '{0}'.", forwardingShipmentDS?.Key ?? ZString.Empty));
				}

				StandardOrFirstSubShipment = PickAShipmentToCreateConsignment(shipmentsToCreateConsignments, branchPK);
				OtherSiblingsToCreateConsignmentsFor = shipmentsToCreateConsignments.Where(s => s != StandardOrFirstSubShipment);
			}
			else
			{
				Master = null;
				StandardOrFirstSubShipment = stdOrMasterShipment;
				OtherSiblingsToCreateConsignmentsFor = Enumerable.Empty<UniversalShipment>();
			}

			IsTWX = dataObject.IsTransitWarehouseCombined();

			IsTWP = dataObject.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWP) ||
					dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWP);
		}

		static UniversalShipment PickAShipmentToCreateConsignment(IEnumerable<UniversalShipment> shipments, ZGuid branchPK)
		{
			var stdShipment = shipments.FirstOrDefault(c => TransitUniversalHelper.CanShipmentCreateConsignment(c, branchPK));
			if (stdShipment != null)
			{
				return stdShipment;
			}
			return shipments.FirstOrDefault();
		}

		public UniversalShipment Master;
		public bool IsTWX;
		public bool IsTWP;
		public UniversalShipment StandardOrFirstSubShipment;
		public IColumnIndexer WarehouseFromDataObject; // which gets from the Master (or Consol)
		public IEnumerable<UniversalShipment> OtherSiblingsToCreateConsignmentsFor;
	}

	public abstract class WhsTransitConsignmentDataObjectReader<T> : WhsTransitDataObjectReader<T>
		where T : BusinessObject, IDocAddresses, IUniversalXMLNoteParent, IHaveServices, IHaveCusEntryNumReferences
	{
		#region Constructor

		protected WhsTransitConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseFromConsol = null, ZString? masterShipmentId = null)
			: this(new SingleOrMultiConsignmentDataObjectManager(consignmentDataObject, logger, factory, warehouseFromConsol), logger, factory, masterShipmentId: masterShipmentId)
		{
			LogDataSourceOrTarget();
		}

		WhsTransitConsignmentDataObjectReader(SingleOrMultiConsignmentDataObjectManager manager, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseFromConsol = null, ZString? masterShipmentId = null)
			: base(manager.StandardOrFirstSubShipment, logger, factory)
		{
			Manager = manager;
			WarehouseFromSource = warehouseFromConsol ?? manager.WarehouseFromDataObject;

			var consolDO = logger.GetConsolDataObject(factory);
			var isSeaCargoOutturn = consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
			BookingParty = TransitUniversalHelper.GetBookingParty(factory, logger, isRequired: !isSeaCargoOutturn);
			MasterShipmentId = masterShipmentId;
		}

		protected readonly SingleOrMultiConsignmentDataObjectManager Manager;
		protected readonly IColumnIndexer WarehouseFromSource;
		protected IOrgHeader BookingParty;
		protected ZString ConsignmentDirection;
		protected ZString? MasterShipmentId;

		#endregion

		#region Warehouse

		protected IColumnIndexer GetWarehouse(IColumnIndexer consignmentRow)
		{
			Argument.NotNull(consignmentRow, "consignmentRow");

			return GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(WhsWarehouseSchema.Constants.TableName, consignmentRow.GetValue(WarehouseSchemaColumn)));
		}

		IColumnIndexer GetNotNullWarehouse(UniversalShipment sourceDO)
		{
			if (WarehouseFromSource == null)
			{
				WarehouseMatchingHelper.ThrowForNoMatchingWarehouse(sourceDO, logger);
			}

			return WarehouseFromSource;
		}

		#endregion

		#region Logging

		protected abstract ResourceString GetJobTypeForLogs();

		protected abstract ResourceString GetLogForMatching(IDataSourceDataObject dataSource);

		protected ZString GetCodeDescriptionForLogs(IColumnIndexer consignment)
			=> WhsTransitLogHelper.FormatCodeDescriptionForLogging(consignment.GetValue(JobIDSchemaColumn), consignment.GetValue(ConsignmentIDSchemaColumn));

		void LogDataSourceOrTarget()
		{
			var dataTarget = GetDataTarget(dataObject);
			var dataSource = GetDataSource(dataObject);

			if (!string.IsNullOrEmpty(dataTarget?.Key))
			{
				logger.Log(LogType.Information, Res.GetString("c8a367ca-af98-4154-9637-96c6e6a8bf20", "Data Target is '{0} - {1}'.", dataTarget.Type, dataTarget.Key));
			}
			else if (!string.IsNullOrEmpty(dataSource?.Key))
			{
				logger.Log(LogType.Information, Res.GetString("6e8219ac-9e5a-485b-995c-f0b7b9def5d6", "Data Source is '{0} - {1}'.", dataSource.Type, dataSource.Key));
			}
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override T GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var consignmentID = GetConsignmentID();
			var houseBill = GetHouseBill();
			var sourceDO = GetSourceDataObject();
			var dataSource = GetDataSource(sourceDO);

			logger.Log(LogType.Information, GetLogForMatching(dataSource));
			return GetMatchingHelper(sourceDO).MatchConsignment(houseBill, consignmentID, sourceDO);
		}

		protected static IDataSourceDataObject GetDataSource(UniversalShipment sourceDO) => sourceDO.FirstDataSource();

		protected static IDataTargetDataObject GetDataTarget(UniversalShipment sourceDO) => sourceDO.FirstDataTarget();

		WhsTransitConsignmentMatchingHelper<T> GetMatchingHelper(UniversalShipment sourceDO)
		{
			return GetMatchingHelperCore(sourceDO, GetNotNullWarehouse(sourceDO));
		}

		protected abstract WhsTransitConsignmentMatchingHelper<T> GetMatchingHelperCore(UniversalShipment sourceDO, IColumnIndexer warehouseFromConsol);

		#endregion

		#region GetCombinedReferenceMatcher

		protected override sealed IMatchingBusinessEntityFinder<T> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region PopulateBusinessObject

		protected override sealed void PopulateBusinessObject(T consignment)
		{
			var sourceDataObject = GetSourceDataObject();
			var consignmentRow = GetColumnIndexer(consignment);

			ThrowExceptionIfDataSourceContainsRadioactivePackages(sourceDataObject);
			PopulateJobID(consignmentRow);
			PopulateConsignmentReference(consignmentRow);
			PopulateWarehouse(sourceDataObject, consignmentRow);
			PopulateConsignmentDirection(consignment, sourceDataObject);
			PopulateServiceLevel(sourceDataObject, consignmentRow);
			PopulateBookingParty(consignmentRow);
			PopulateHouseBillNumber(consignmentRow, sourceDataObject);
			PopulateConsignmentAdditionalReferences(consignment, sourceDataObject);
			PopulateBusinessObjectCore(sourceDataObject, consignment, consignmentRow);
			PopulateAddresses(consignment, sourceDataObject);
			PopulateNotes(consignment, sourceDataObject);
			PopulateDestination(consignment, sourceDataObject);
			PopulateAdditionalServices(sourceDataObject, consignment, consignmentRow);
			LinkParentShipment(sourceDataObject, consignmentRow);
			AfterSuccessfullyPopulatingBizO(consignment);
		}

		void ThrowExceptionIfDataSourceContainsRadioactivePackages(UniversalShipment sourceDataObject)
		{
			var undgList = sourceDataObject.PackingLineCollection?.Where(p => p.UNDGCollection?.Count > 0).SelectMany(p => p.UNDGCollection).ToList();

			if (undgList != null && undgList.Count > 0)
			{
				var undgCodeList = undgList.Where(u => u.UNDGCode.HasValue).Select(u => u.UNDGCode.Value).ToList();
				var undgClassList = undgList.Where(u => u.IMOClass.HasValue).Select(u => u.IMOClass.Value).Distinct().ToList();
				var undgCodeStandardList = undgList.Where(u => u.UNDGCode.HasValue && u.Standard.HasValue).Select(u => u.UNDGCode.Value + u.Standard.Value).Distinct().ToList();

				var undgCountryReferenceBizoList = new DynamicBusinessObjectCollection(factory.BOFactory);

				var sqlParameter = string.Join(", ", undgCodeStandardList.Select(u => "'" + u + "'"));
				if (sqlParameter.Length > 0)
				{
					undgCountryReferenceBizoList.Load(
$@"
SELECT
	DCP_DCR
FROM
	dbo.UNDGCountryReferencePivot
	JOIN dbo.UNDGSubstance ON DCP_UNNO = DG_UNNO AND DCP_Variant = DG_Variant AND DCP_Standard = DG_Standard
WHERE
	CONCAT(DCP_UNNO, DCP_Variant, DCP_Standard) IN ({sqlParameter})
");
				}
				var undgCountryReferenceList = undgCountryReferenceBizoList.Select(bo => Guid.Parse(bo["DCP_DCR"].ToString())).ToList();

				var warehouseFromSource = GetNotNullWarehouse(sourceDataObject);
				var warehouse = (WhsWarehouse)factory.Load(typeof(WhsWarehouse), warehouseFromSource.GetValue(WhsWarehouseSchema.PK));
				var enableLimit = warehouse.WW_IsDangerousGoodsManagementEnabled;

				if (enableLimit)
				{
					var undgLimitQuery = new ZQuery(WhsUNDGLimitSchema.WWD_WW_Warehouse, warehouse.PK);
					undgLimitQuery.AddToFilter(WhsUNDGLimitSchema.WWD_TotalVolumeLimit, ZDecimal.Zero);
					undgLimitQuery.AddToFilter(WhsUNDGLimitSchema.WWD_TotalWeightLimit, ZDecimal.Zero);
					var undgLimits = factory.Load<WhsUNDGLimit>(undgLimitQuery);

					if (undgLimits.Length > 0)
					{
						var disallowedUNDGSubstances = undgLimits.Where(undgLimit => undgLimit.UNDGSubstance != null
						&& undgCodeStandardList.Any(dg => dg == undgLimit.UNDGSubstance.DG_Code + undgLimit.UNDGSubstance.DG_Standard)).Select(undgLimit => undgLimit.UNDGSubstance.DG_Code).ToList();
						if (disallowedUNDGSubstances.Count > 0)
						{
							throw new DataObjectReadFailureException(WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGSubstanceLimit, string.Join(", ", disallowedUNDGSubstances)));
						}

						var disallowedUNDGClasses = undgLimits.Where(undgLimit => !undgLimit.WWD_UNDGClass.IsEmpty
							&& undgClassList.Any(dgClass => dgClass.StartsWith(undgLimit.WWD_UNDGClass))).Select(undgLimit => undgLimit.WWD_UNDGClass).ToList();
						if (disallowedUNDGClasses.Count > 0)
						{
							throw new DataObjectReadFailureException(WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGClassLimit, string.Join(", ", disallowedUNDGClasses)));
						}

						var disallowedUNDGCountryReferences = undgLimits.Where(undgLimit => undgLimit.UNDGCountryReference != null
							&& undgCountryReferenceList.Any(dcr => dcr == undgLimit.WWD_DCR_UNDGCountryReference)).Select(undgLimit => undgLimit.UNDGCountryReference.DCR_Code).ToList();

						if (disallowedUNDGCountryReferences.Count > 0)
						{
							throw new DataObjectReadFailureException(WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGCountryReferenceLimit, string.Join(", ", disallowedUNDGCountryReferences)));
						}
					}
				}
			}
		}

		void AfterSuccessfullyPopulatingBizO(T consignment)
		{
			PopulateLinks(consignment);
			AddBookingConfirmedLog(consignment);
			ProcessSubShipments(Manager);
			AfterSuccessfullyPopulatingBizOCore();
			ExecuteStoreProcedure();
		}

		protected void PopulateLinks(T bizO)
		{
			if (BookingParty != null)
			{
				var sourceDataContext = GetSourceDataObject().DataContext;
				if (!logger.TopLevelDataContext.DataSourceCollection.IsNullOrEmpty() && logger.TopLevelDataContext.DataSourceCollection.Any(d => d.Type.Value == nameof(DataContextType.UnderBond)))
				{
					sourceDataContext = logger.TopLevelDataContext;
				}

				var linkCreator = new UniversalJobLinkCreator(bizO.Factory, bizO, BookingParty, sourceDataContext, logger);
				linkCreator.TryCreateJobLink(DataContextType.ForwardingShipment);
				linkCreator.TryCreateJobLink(DataContextType.LandTransportConsignment);
				linkCreator.TryCreateJobLink(DataContextType.AirManifestLine);
				linkCreator.TryCreateJobLink(DataContextType.UnderBond);
			}
		}

		void ProcessSubShipments(SingleOrMultiConsignmentDataObjectManager manager)
		{
			var shipments = manager.OtherSiblingsToCreateConsignmentsFor;
			var hasMasterShipment = manager?.Master != null && TransitUniversalHelper.IsMasterShipmentRepresentingAllChildShipments(manager.Master);

			foreach (var shipment in shipments)
			{
				ReadSubShipment(shipment, hasMasterShipment ? manager.Master.WayBillNumber : null);
			}
		}

		protected abstract void PopulateConsignmentDirection(T consignment, UniversalShipment sourceDO);

		protected abstract void PopulateDestination(T consignment, UniversalShipment sourceDataObject);

		protected abstract void ReadSubShipment(UniversalShipment shipment, ZString? masterShipmentId = null);

		public void AddBookingConfirmedLog(T consignment)
		{
			var warehouseCode = WarehouseFromSource?.GetValue(WhsWarehouseSchema.WW_WarehouseCode) ?? string.Empty;
			var warehouseAddressPK = WarehouseFromSource?.GetValue(WhsWarehouseSchema.WW_OA_WarehouseAddress);
			var address = warehouseAddressPK.HasValue ? GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(OrgAddressSchema.Constants.TableName, warehouseAddressPK.Value)) : null;
			var addressCode = address != null ? address.GetValue(OrgAddressSchema.OA_City) : ZString.Empty;

			if (IsNewBO)
			{
				var reference = FormattableString.Invariant($"New|FAC=CFS|LOC={addressCode}|TYP={TransitJobType}|RFN={consignment[JobIDSchemaColumn]}|WHS={warehouseCode}");  // event reference parameters
				WhsTransitLogHelper.AddStmALog(factory, logger, consignment.PK.ToGuid(), consignment.TableName, reference, Events.BookingConfirmedCode);
			}
			else
			{
				var reference = FormattableString.Invariant($"Updated|FAC=CFS|LOC={addressCode}|TYP={TransitJobType}|RFN={consignment[JobIDSchemaColumn]}|WHS={warehouseCode}");  // event reference parameters
				WhsTransitLogHelper.AddStmALog(factory, logger, consignment.PK.ToGuid(), consignment.TableName, reference, Events.BookingConfirmedCode);
			}
		}

		protected virtual void AfterSuccessfullyPopulatingBizOCore()
		{
		}

		protected abstract string TransitJobType { get; }

		void PopulateJobID(IColumnIndexer consignmentRow)
		{
			if (IsNewBO)
			{
				SetValue(consignmentRow, JobIDSchemaColumn, NumberFountainID);
			}
		}

		void PopulateConsignmentReference(IColumnIndexer consignmentRow)
		{
			var reference = GetHouseBill();
			if (reference.IsEmpty)
			{
				reference = GetConsignmentID();
			}

			if (!reference.IsEmpty)
			{
				SetValue(consignmentRow, ConsignmentIDSchemaColumn, reference);
			}
			else if (IsNewBO)
			{
				SetValue(consignmentRow, ConsignmentIDSchemaColumn, consignmentRow.GetValue(JobIDSchemaColumn));
			}
		}

		void PopulateWarehouse(UniversalShipment sourceDataObject, IColumnIndexer consignmentRow)
		{
			if (IsNewBO)
			{
				var warehouseFromSource = GetNotNullWarehouse(sourceDataObject);
				if (warehouseFromSource != null)
				{
					var warehousePK = warehouseFromSource.GetValue(WhsWarehouseSchema.PK);
					SetValue(consignmentRow, WarehouseSchemaColumn, warehousePK);
				}
			}
			else
			{
				var consignmentWarehouse = GetWarehouse(consignmentRow);
				if (WarehouseFromSource != null && WarehouseFromSource.GetValue(WhsWarehouseSchema.PK) != consignmentWarehouse.GetValue(WhsWarehouseSchema.PK))
				{
					throw new DataObjectReadFailureException(Res.GetString("ea998132-8849-4086-9de2-95abc01633db", "Consignment cannot be updated as the Warehouse specified '{0}' differs from the Consignment Warehouse '{1}'.",
						WarehouseFromSource.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
						consignmentWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode)));
				}
			}
		}

		void PopulateServiceLevel(UniversalShipment sourceDataObject, IColumnIndexer consignmentRow)
		{
			SetValue(consignmentRow, ServiceLevelSchemaColumn, sourceDataObject.ServiceLevel);
		}

		void LinkParentShipment(UniversalShipment sourceDataObject, IColumnIndexer consignmentRow)
		{
			if (logger.IsInternalImport())
			{
				var shipmentDataSource = sourceDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
				var shipment = shipmentDataSource?.GetLoadedJobFromDataContextType(sourceDataObject, factory.BOFactory);

				if (shipment != null)
				{
					SetValue(consignmentRow, ParentIDSchemaColumn, shipment.PK);
					SetValue(consignmentRow, ParentTableCodeSchemaColumn, JobShipmentSchema.Constants.Prefix);

					if (consignmentRow is WhsItemReceiveConsignment)
					{
						logger.Log(Enterprise.Integration.LogType.Information, ResString.GetMultilingualString("dbaa4011-9c10-46df-a53f-a537f91004bd", "Receive Consignment {0} linked to Shipment {1}.", consignmentRow.GetValue(ConsignmentIDSchemaColumn), shipmentDataSource.Key));
					}
					else if (consignmentRow is WhsItemDispatchConsignment)
					{
						logger.Log(Enterprise.Integration.LogType.Information, ResString.GetMultilingualString("23b5ee24-963b-4ef7-b5e4-9faeeea8178b", "Dispatch Consignment {0} linked to Shipment {1}.", consignmentRow.GetValue(ConsignmentIDSchemaColumn), shipmentDataSource.Key));
					}
				}
			}
		}

		protected abstract void PopulateBusinessObjectCore(UniversalShipment sourceDataObject, T consignment, IColumnIndexer consignmentRow);

		protected abstract ZString NumberFountainID { get; }

		protected abstract SchemaStringColumn ConsignmentIDSchemaColumn { get; }
		protected abstract SchemaStringColumn ConsignmentHouseBillColumn { get; }
		protected abstract SchemaStringColumn JobIDSchemaColumn { get; }
		protected abstract SchemaStringColumn ServiceLevelSchemaColumn { get; }
		protected abstract SchemaGuidColumn WarehouseSchemaColumn { get; }

		protected abstract SchemaGuidColumn ParentIDSchemaColumn { get; }
		protected abstract SchemaStringColumn ParentTableCodeSchemaColumn { get; }

		protected abstract SchemaGuidColumn PKSchemaColumn { get; }
		protected abstract string TablePrefix { get; }

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(T consignment, UniversalShipment sourceDataObject)
		{
			logger.Log(LogType.Information, Res.GetString("8b171c53-1b1e-4869-afe1-b21eb82d3bd0", "Populating Addresses for {0}...", GetJobTypeForLogs()));

			if (sourceDataObject.OrganizationAddressCollection != null)
			{
				foreach (var organisationAddressDataObject in sourceDataObject.OrganizationAddressCollection)
				{
					var addressType = organisationAddressDataObject.AddressType.GetValueOrDefault();
					var isInvalidAddressType = addressType == nameof(DocAddressType.ArrivalCFSAddress)
												|| addressType == nameof(DocAddressType.DepartureCFSAddress)
												|| addressType == nameof(DocAddressType.LocalCartageCFS)
												|| addressType == nameof(DocAddressType.BookingPartyDocumentaryAddress);
					if (!isInvalidAddressType)
					{
						var preventUpdatingRCNConsignorInfoWhenPackagesHaveBeenUnloaded = (string errorMessage, JobDocAddress existingAddress) => {
							if (existingAddress != null && consignment is WhsItemReceiveConsignment rcn && rcn.PackageStates.Any(pkg => !pkg.WPS_UnloadedTime.IsEmpty))
							{
								if ((existingAddress.Address1.ToString() is var existingAdress1 && existingAdress1 != null && existingAdress1 != organisationAddressDataObject.Address1.GetValueOrDefault()) ||
									(existingAddress.Address2.ToString() is var existingAdress2 && existingAdress2 != null && existingAdress2 != organisationAddressDataObject.Address2.GetValueOrDefault()))
								{
									throw new DataObjectReadFailureException(errorMessage);
								}
							}
						};

						switch (addressType)
						{
							case nameof(DocAddressType.ConsigneePickupDeliveryAddress):
								if (DataContextType == DataContextType.TransitDispatch)
								{
									new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment);
								}
								if (WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.Value == DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED && (consignment is WhsItemReceiveConsignment || consignment is WhsItemDispatchConsignment))
								{
									new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ClientRequestedBillingParty);
								}
								break;
							case nameof(AddressTypes.DeliveryLocalCartage):
								if (DataContextType == DataContextType.TransitDispatch)
								{
									new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.TransportCompanyDocumentaryAddress);
								}
								break;
							case nameof(DocAddressType.ConsignorPickupDeliveryAddress):
								{
									var existingDocAddress = consignment.DocAddresses?.Where(address => address.DocAddressType == DocAddressType.ConsignorPickupDeliveryAddress).SingleOrDefault();
									preventUpdatingRCNConsignorInfoWhenPackagesHaveBeenUnloaded(Res.GetString("80d2d0ad-cdee-48d2-8023-d723b9834cf8", "Cannot update the RCN Consignor Pickup Delivery Address. Some packages have already been unloaded."), existingDocAddress);
								}
								if (DataContextType == DataContextType.TransitReceive)
								{
									new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment);
								}
								break;
							case nameof(DocAddressType.ConsigneeDocumentaryAddress):
								new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ConsigneeDocumentaryAddress);
								break;
							case nameof(DocAddressType.ConsignorDocumentaryAddress):
								{
									var existingDocAddress = consignment.DocAddresses?.Where(address => address.DocAddressType == DocAddressType.LocalCartageExporter).SingleOrDefault();
									preventUpdatingRCNConsignorInfoWhenPackagesHaveBeenUnloaded(Res.GetString("550119d9-2799-487b-bb1f-24428856c8ea", "Cannot update the RCN Consignor Documentary Address. Some packages have already been unloaded."), existingDocAddress);
								}
								new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.LocalCartageExporter);
								break;
							case nameof(AddressTypes.SendersLocalClient) when (WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.Value == DefaultBilltoPartyForTWConsignmentCodeList.Codes.LCN && (consignment is WhsItemReceiveConsignment || consignment is WhsItemDispatchConsignment)):
								new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ClientRequestedBillingParty);
								break;
							default:
								new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment);
								break;
						}
					}
				}
			}

			if (WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.Value == DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD)
			{
				var consignmentRow = GetColumnIndexer(consignment);
				var bookingPartyDocAddress = GetBookingPartyDocAddress(consignmentRow);
				var organisationAddressDataObject = sourceDataObject.OrganizationAddressCollection[0];
				new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, bookingPartyDocAddress.Address, DocAddressType.ClientRequestedBillingParty);
			}

			var consolDO = logger.GetConsolDataObject(factory);
			if (consolDO?.OrganizationAddressCollection != null)
			{
				foreach (var organisationAddressDataObject in consolDO.OrganizationAddressCollection)
				{
					var addressType = organisationAddressDataObject.AddressType.GetValueOrDefault();
					if (addressType == nameof(DocAddressType.ArrivalCTOAddress) || addressType == nameof(DocAddressType.DepartureCTOAddress))
					{
						if (consignment is WhsItemReceiveConsignment && addressType == nameof(DocAddressType.ArrivalCTOAddress))
						{
							new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ArrivalCTOAddress);
						}
						else if (consignment is WhsItemDispatchConsignment && addressType == nameof(DocAddressType.DepartureCTOAddress))
						{
							new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.DepartureCTOAddress);
						}
					}
					if (addressType == nameof(AddressTypes.SendingForwarderAddress) && WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.Value == DefaultBilltoPartyForTWConsignmentCodeList.Codes.SFA)
					{
						new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ClientRequestedBillingParty);
					}
					if (addressType == nameof(AddressTypes.ReceivingForwarderAddress) && WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.Value == DefaultBilltoPartyForTWConsignmentCodeList.Codes.RFA)
					{
						new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment, DocAddressType.ClientRequestedBillingParty);
					}
				}
			}

			if (sourceDataObject.InstructionCollection != null)
			{
				foreach (var address in sourceDataObject.InstructionCollection.Where(i => i.Address != null && i.Address.AddressType.HasValue).Select(i => i.Address))
				{
					new OrganisationDataObjectReader(address, logger, factory).GetMatchedOrNew(consignment);
				}
			}

			if (sourceDataObject.GetMatchingDataSource(DataContextType.Outturn) != null)
			{
				PopulateConsigneeForOutturn(GetConsigneeDocAddress(consignment));
			}
		}

		#region PopulateConsigneeForOutturn

		void PopulateConsigneeForOutturn(JobDocAddress consigneeDocAddress)
		{
			var consignee = GetConsigneeNameFromAddInfos(dataObject.AddInfoCollection);

			if (!string.IsNullOrEmpty(consignee))
			{
				var defaultText = ResString.GetMultilingualString("fe4d742a-ba5b-453a-96ad-8cb4ea0fb9d2", "not specified");
				consigneeDocAddress.E2_AddressOverride = true;
				consigneeDocAddress.CompanyName = consignee;
				consigneeDocAddress.Address1 = defaultText;
				consigneeDocAddress.City = defaultText;
			}
		}

		protected abstract JobDocAddress GetConsigneeDocAddress(T consignment);

		#endregion

		#endregion

		#region PopulateBookingParty

		void PopulateBookingParty(IColumnIndexer consignmentRow)
		{
			var consolDO = logger.GetConsolDataObject(factory);
			var isSeaCargoOutturn = consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
			if (!isSeaCargoOutturn && BookingParty != null)
			{
				var bookingPartyDocAddress = GetBookingPartyDocAddress(consignmentRow);
				new OrganisationDataObjectReader(new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance), logger, factory).PopulateJobDocAddress(BookingParty.MainAddress, bookingPartyDocAddress); // needs BizOs to work
			}
		}

		JobDocAddress GetBookingPartyDocAddress(IColumnIndexer consignmentRow)
		{
			var jobDocAddressQuery = new ZQuery();
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, consignmentRow.GetValue(PKSchemaColumn));
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			jobDocAddressQuery.MaximumRows = 1;

			var addresses = factory.RowFactory.Load(JobDocAddressSchema.Constants.TableName, jobDocAddressQuery);
			var jobDocAddressRow = addresses.Length == 1 ? GetColumnIndexerFromRow(addresses[0]) : null;
			return jobDocAddressRow != null ? factory.Load<JobDocAddress>(jobDocAddressRow.GetValue(JobDocAddressSchema.PK)) : GetNewBookingPartyDocAddress(consignmentRow);
		}

		JobDocAddress GetNewBookingPartyDocAddress(IColumnIndexer consignmentRow)
		{
			var jobDocAddress = factory.New<JobDocAddress>();
			SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			SetValue(jobDocAddress, JobDocAddressSchema.E2_ParentTableCode, TablePrefix);
			SetValue(jobDocAddress, JobDocAddressSchema.E2_ParentID, consignmentRow.GetValue(PKSchemaColumn));

			return jobDocAddress;
		}

		#endregion

		#region House bill number

		void PopulateHouseBillNumber(IColumnIndexer consignmentRow, UniversalShipment sourceDO)
		{
			SetValue(consignmentRow, ConsignmentHouseBillColumn, sourceDO.WayBillNumber);
		}

		#endregion

		#region PopulateAdditionalReferences

		protected void PopulateConsignmentAdditionalReferences(T consignment, UniversalShipment sourceDO, bool ignoreMatchedAdditionalReferences = false)
		{
			var forwardingShipmentDS = sourceDO.GetMatchingDataSource(DataContextType.ForwardingShipment);

			var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);
			var additionalReferences = new List<TransitAdditionalReferenceInfo>();

			var tableName = consignment.TableName;
			// forwarding references
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, forwardingShipmentDS?.Key ?? "");
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription, sourceDO.GoodsDescription);

			PopulateConsignmentAdditionalReferencesCore(consignment, sourceDO, referenceHelper, additionalReferences);

			PopulateMasterBillNumberFromCustoms(consignment, sourceDO, referenceHelper, additionalReferences);

			if (ignoreMatchedAdditionalReferences)
			{
				new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), consignment, logger, factory).ReadIntoCollectionUnmatchedElementsOnly();
			}
			else
			{
				new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), consignment, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
			}
		}

		void PopulateMasterBillNumberFromCustoms(T consignment, UniversalShipment sourceDO, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			if (sourceDO.GetMatchingDataSource(DataContextType.Outturn) != null)
			{
				FillAdditionalBills(consignment, sourceDO, referenceHelper, additionalReferences);
			}
		}

		void FillAdditionalBills(T consignment, UniversalShipment dataObject, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			var tableName = consignment.TableName;
			var additionalBills = dataObject.AdditionalBillCollection;
			if (additionalBills != null && additionalBills.Count > 0)
			{
				var additionalBill = additionalBills.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House);
				if (additionalBill != null)
				{
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, AdditionalReferenceTypes.Codes.MasterBill, additionalBill.ParentBillNumber);
				}
			}
		}

		protected virtual void PopulateConsignmentAdditionalReferencesCore(T consignment, UniversalShipment sourceDO, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
		}

		#endregion

		#region GetConsigneeNameFromAddInfos

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Add Info type to get consignee name")]
		protected string GetConsigneeNameFromAddInfos(IEnumerable<AddInfo> addInfos)
		{
			var addInfo = addInfos?.FirstOrDefault(x => x.Key.GetValueOrDefault() == "Consignee");
			if (addInfo != null && !string.IsNullOrEmpty(addInfo.Value))
			{
				return addInfo.Value;
			}
			return string.Empty;
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(T consignment, UniversalShipment sourceDataObject)
		{
			if (sourceDataObject?.NoteCollection != null)
			{
				var notesToImport = WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, sourceDataObject.NoteCollection, StmNoteDescription.Pub, StmNoteDescription.Agv, StmNoteDescription.Prv, StmNoteDescription.Int);
				if (notesToImport.Any())
				{
					new NotesCollectionReader(notesToImport, logger, factory, consignment).ReadIntoCollection();
				}
			}
		}

		#endregion

		#region PopulateAdditionalServices

		void PopulateAdditionalServices(UniversalShipment sourceDataObject, T consignment, IColumnIndexer consignmentRow)
		{
			var rcnServicesRemaining = GetRCNServices(sourceDataObject, consignment, consignmentRow).ToList();
			var dcnServicesRemaining = GetDCNServices(consignment).ToList();
			var additionalServiceCollection = sourceDataObject?.LocalProcessing?.AdditionalServiceCollection;

			if (additionalServiceCollection != null)
			{
				var warehouseAddressPK = GetWarehouse(consignmentRow).GetValue(WhsWarehouseSchema.WW_OA_WarehouseAddress);
				foreach (var additionalService in additionalServiceCollection)
				{
					var serviceAddress = additionalService.Location != null ? new OrganisationDataObjectReader(additionalService.Location, logger, factory).GetMatched() : null;
					if (serviceAddress != null && warehouseAddressPK == serviceAddress.PK)
					{
						var matchingRCNService = new AdditionalServiceBusinessObjectFinder<WhsJobService>(additionalService).Find(rcnServicesRemaining?.Cast<WhsJobService>());
						if (matchingRCNService != null)
						{
							UpdateJobServiceIfNotCompleted(matchingRCNService, additionalService, consignment);
							rcnServicesRemaining.Remove(matchingRCNService);
						}
						else
						{
							var matchingDCNService = new AdditionalServiceBusinessObjectFinder<WhsJobService>(additionalService).Find(dcnServicesRemaining?.Cast<WhsJobService>());
							if (matchingDCNService != null)
							{
								UpdateJobServiceIfNotCompleted(matchingDCNService, additionalService, consignment);
								dcnServicesRemaining.Remove(matchingDCNService);
							}
							else
							{
								UpdateJobServiceIfNotCompleted(null, additionalService, consignment);
							}
						}
					}
				}
			}

			DeleteExistingServicesBookedBySender(rcnServicesRemaining);
			DeleteExistingServicesBookedBySender(dcnServicesRemaining);
		}

		void DeleteExistingServicesBookedBySender(IEnumerable<WhsJobService> jobServices)
		{
			var remainingServicesBookedBySender = jobServices.Where(s => s.ES_Completed.IsEmpty && IsServiceBookedBySender(s));
			foreach (var service in remainingServicesBookedBySender)
			{
				var jobServiceRow = GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(JobServiceSchema.Constants.TableName, service.PK));
				factory.DeleteRowAndSetHasChanges<WhsJobService>(jobServiceRow, JobServiceSchema.PK);
				// This is required to trigger BusinessObject.OnSaving() to handle StmALogs
				service.Delete();
			}
		}

		bool IsServiceBookedBySender(WhsJobService service)
		{
			var senderID = GetSourceDataObject()?.FirstDataSource()?.Key ?? ZString.Empty;
			return TransitUniversalHelper.GetAddOnValuesByName(factory, service.PK, "JobNumber").Any(a => a.XV_Data == senderID);
		}

		protected abstract IEnumerable<WhsJobService> GetRCNServices(UniversalShipment sourceDataObject, T consignment, IColumnIndexer consignmentRow);

		protected abstract IEnumerable<WhsJobService> GetDCNServices(T consignment);

		protected void UpdateJobServiceIfNotCompleted(WhsJobService jobServiceBO, AdditionalService additionalService, T consignment)
		{
			if (jobServiceBO == null)
			{
				if (new AdditionalServiceBusinessObjectFinder<WhsJobService>(additionalService).Find(consignment.Services.Cast<WhsJobService>()) == null)
				{
					jobServiceBO = new AdditionalServiceDataObjectReader<WhsJobService>(additionalService, logger, factory, consignment).ReadIntoBusinessObject();
					consignment.Services.Add(jobServiceBO);
				}
			}
			else if (jobServiceBO.ES_Completed.IsEmpty)
			{
				jobServiceBO = new AdditionalServiceDataObjectReader<WhsJobService>(additionalService, logger, factory, consignment).ReadIntoBusinessObject();
			}
			if (jobServiceBO != null && !jobServiceBO.IsInDatabase)
			{
				var senderID = GetSourceDataObject()?.FirstDataSource()?.Key ?? ZString.Empty;
				if (!senderID.IsEmpty)
				{
					var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);
					referenceHelper.PopulateAddOnValue(jobServiceBO.PK, jobServiceBO.TablePrefix, "JobNumber", "STR", senderID);
				}
			}
		}

		#endregion
	}
}
