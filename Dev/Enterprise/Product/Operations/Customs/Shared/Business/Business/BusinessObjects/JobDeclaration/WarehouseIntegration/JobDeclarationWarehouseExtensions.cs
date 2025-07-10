using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public static class JobDeclarationWarehouseExtensions
	{
		public static Warehouse.Integration.IWhsWarehouse GetWhsWarehouse(this OrgAddress warehouseAddress)
		{
			Warehouse.Integration.IWhsWarehouse result = null;
			if (warehouseAddress != null)
			{
				var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK);
				query.FetchOnlyFromLocalCache = !warehouseAddress.IsInDatabase;
				result = warehouseAddress.Factory.LoadTop1<Warehouse.Integration.IWhsWarehouse>(query);
			}
			return result;
		}

		#region HasWHSTransaction
		public static bool HasWHSTransaction(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.IsActive && WarehouseTransactionStatusList.HasWHSTransaction(supporter.WarehouseTransactionStatus);
		}

		public static bool HasWHSChangeOfOwnershipTransactionAndNotCreatedPending(this IWarehouseIntegrationSupporter supporter)
		{
			var result = supporter.HasWHSTransaction();
			if (result)
			{
				var whsStatus = supporter.WarehouseTransactionStatus;
				result = whsStatus != WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending && WarehouseTransactionStatusList.IsChangeOfOwnershipCode(whsStatus);
			}
			return result;
		}

		public static bool HasWHSChangeOfRegimeTransactionAndNotCreatedPending(this IWarehouseIntegrationSupporter supporter)
		{
			var result = supporter.HasWHSTransaction();
			if (result)
			{
				var whsStatus = supporter.WarehouseTransactionStatus;
				result = whsStatus != WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending && WarehouseTransactionStatusList.IsChangeOfRegimeCode(whsStatus);
			}
			return result;
		}

		public static bool HasWHSInwardTransactionAndNotCreatedPending(this IWarehouseIntegrationSupporter supporter)
		{
			var result = supporter.HasWHSTransaction();
			if (result)
			{
				var whsStatus = supporter.WarehouseTransactionStatus;
				result = whsStatus != WarehouseTransactionStatusList.Codes.InwardCreatedPending && whsStatus != WarehouseTransactionStatusList.Codes.InwardCreationHeld && WarehouseTransactionStatusList.IsInwardCode(whsStatus);
			}
			return result;
		}

		public static bool HasWHSOutwardTransactionAndNotCreatedPending(this IWarehouseIntegrationSupporter supporter)
		{
			var result = supporter.HasWHSTransaction();
			if (result)
			{
				var whsStatus = supporter.WarehouseTransactionStatus;
				result = whsStatus != WarehouseTransactionStatusList.Codes.OutwardCreatedPending && WarehouseTransactionStatusList.IsOutwardCode(whsStatus);
			}
			return result;
		}
		#endregion

		#region Outward

		public static PublishToUniversalResult PublishShipmentForWHSOutwardFromLatestClearedData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLatestClearedData(supporter, DataContextType.WarehouseOrder, RecipientRoleType.BWR);
		}

		public static void PublishAcceptEventForWHSOutwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true));
		}

		public static void CancelPublishShipmentForWHSOutward(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardSecondTryPending())
			{
				supporter.PublishUniversalShipmentOfPreviouslyHoldFollowByDeletingModificationForWHSOutwardInADifferentFactory();
			}
			else
			{
				supporter.PublishCancelEventForWHSOutwardInADifferentFactory();
			}
		}

		static void PublishUniversalShipmentOfPreviouslyHoldFollowByDeletingModificationForWHSOutwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishUniversalShipmentOfPreviouslyHoldFollowByDeletingModificationForWHSOutward(true));
		}

		public static void PublishCancelEventForWHSOutwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true));
		}

		public static void RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			supporter.PublishShipmentInADifferentFactory((x) => x.PublishShipmentForWHSOutwardFromLatestClearedData(), (x) => x.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true));
		}

		public static PublishToUniversalResult PublishCancelEventForWHSOutwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.CancelTheWarehouseJob, Res.GetString("a361bed1-29b5-4c9f-9f08-259dbe9067e1", "Cancel Warehouse Order"), RecipientRoleType.BWR, DataContextType.WarehouseOrder, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
					supporter.DoActionOnOutwardCanceled(result.FindJobIfExists());
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = false;
					}
				}
			});
		}

		public static PublishToUniversalResult PublishHoldEventForWHSOutwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.HoldTheWarehouseOrder, Res.GetString("874324ee-bc11-4c49-bcea-d7c1def1a274", "Hold The Warehouse Order"), RecipientRoleType.BWR, DataContextType.WarehouseOrder, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					var whsStatus = supporter.WarehouseTransactionStatus;
					supporter.WarehouseTransactionStatus = whsStatus == WarehouseTransactionStatusList.Codes.OutwardCanceled ? WarehouseTransactionStatusList.Codes.OutwardCanceled : WarehouseTransactionStatusList.Codes.OutwardHolding;
				}
			});
		}

		public static PublishToUniversalResult PublishAcceptEventForWHSOutwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.WarehouseJobCanNowBeFinalised, Res.GetString("a211d6ad-1149-4f45-a7ad-f377af6f75a1", "Warehouse Job Can Now Be Finalized"), RecipientRoleType.BWR, DataContextType.WarehouseOrder, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = supporter.HasWHSOutwardTransactionAndNotCreatedPending() ? WarehouseTransactionStatusList.Codes.OutwardUpdated : WarehouseTransactionStatusList.Codes.OutwardCreated;
					supporter.DoActionOnOutwardAccepted(result.FindJobIfExists());
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = true;
					}
				}
			});
		}

		public static PublishToUniversalResult PublishShipmentForWHSOutwardFromLastHoldOrLatestData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLastHoldOrLatestData(supporter, DataContextType.WarehouseOrder, RecipientRoleType.BWR);
		}

		static PublishToUniversalResult PublishShipmentFromLastHoldOrLatestData(this IWarehouseIntegrationSupporter supporter, DataContextType dataContextType, RecipientRoleType recipientRoleType)
		{
			var company = supporter.Company;
			var recipients = new[] { new RecipientRoleDetail() { Type = recipientRoleType, ServiceCode = ServiceCodeType.AMD } };
			var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();

			return GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, recipients, supporter, (outboundSessionTracker) =>
			{
				ITopLevelDataObjectWriter writer = null;
				if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
				{
					writer = GetHoldWriterProvider(outboundSessionTracker, recipientRoleType);
					writerDictionary.Add(outboundSessionTracker, writer);
				}
				return writer;
			}));
		}

		public static PublishToUniversalResult PublishUniversalShipmentOfPreviouslyHoldFollowByDeletingModificationForWHSOutward(this IWarehouseIntegrationSupporter supporter, bool shouldSave)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, _ => PublishToUniversalResult.Empty, shouldSave, universalResult =>
			{
				var result = supporter.PublishShipmentForWHSOutwardFromLastHoldOrLatestData();
				if (result.ResultType != UniversalResult.HadErrors)
				{
					supporter.DeleteModificationShipmentNotes();
				}
			});
		}

		public static PublishToUniversalResult PublishUniversalShipmentOfLatestModificationFollowByElevatingModificationToHoldForWHSOutward(this IWarehouseIntegrationSupporter supporter)
		{
			var result = supporter.PublishShipmentForWHSOutwardFromLastModification();
			if (result.ResultType != UniversalResult.HadErrors)
			{
				supporter.ElevateModificationToHold();
			}
			return result;
		}

		static PublishToUniversalResult PublishShipmentForWHSOutwardFromLastModification(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLastModification(supporter, DataContextType.WarehouseOrder, RecipientRoleType.BWR);
		}

		static PublishToUniversalResult PublishShipmentFromLastModification(this IWarehouseIntegrationSupporter supporter, DataContextType dataContextType, RecipientRoleType recipientRoleType)
		{
			var company = supporter.Company;
			var recipients = new[] { new RecipientRoleDetail() { Type = recipientRoleType, ServiceCode = ServiceCodeType.AMD } };
			var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();

			return GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, recipients, supporter, (outboundSessionTracker) =>
			{
				ITopLevelDataObjectWriter writer = null;
				if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
				{
					writer = GetModificationWriterProvider(outboundSessionTracker, recipientRoleType);
					writerDictionary.Add(outboundSessionTracker, writer);
				}
				return writer;
			}));
		}

		public static PublishToUniversalResult PublishShipmentForWHSOutward(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
			{
				var company = dec.Company;

				var factory = new BusinessObjectFactory();
				PublishUniversalXmlResult events;
				using (factory.AddDisposableService())
				{
					events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, company.OrgProxy, new RecipientRoleType[] { RecipientRoleType.BWR }, dec);
					factory.Save();
				}

				var result = GetResultFromEventsFor(DataContextType.WarehouseOrder, events);
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					if (supporter.SupportModificationState)
					{
						PublishShipmentForWHSInwardOrOutward(supporter, RecipientRoleType.BWR, false);
					}
					dec.WarehouseTransactionStatus = supporter.HasWHSOutwardTransactionAndNotCreatedPending() ? WarehouseTransactionStatusList.Codes.OutwardUpdatedPending : WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
				}
				return result;
			}, shouldSave);
		}

		public static PublishToUniversalResult PublishShipmentForWHSOutwardWithPreAmendmentData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentWithPreAmendmentData(supporter, new RecipientRoleDetail() { Type = RecipientRoleType.BWR }, DataContextType.WarehouseOrder, WarehouseTransactionStatusList.Codes.OutwardUpdatedPending);
		}

		public static PublishToUniversalResult PublishShipmentForWHSOutwardFromLastDataAndUpdateEntryDetails(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLastDataAndUpdateEntryDetails(supporter, DataContextType.WarehouseOrder, new RecipientRoleDetail() { Type = RecipientRoleType.BWR });
		}

		#endregion

		#region Inward

		public static PublishToUniversalResult ElevateInwardModificationToHold(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, dec =>
			{
				if (WarehouseTransactionStatusList.IsPendingInward(supporter.WarehouseTransactionStatus) && supporter.HasModificationShipmentNote())
				{
					dec.DeleteHoldShipmentNotes();
					dec.ElevateModificationShipmentNotesToHold();
				}
				return PublishToUniversalResult.Empty;
			}, true);
		}

		public static PublishToUniversalResult DeleteInwardModificationNotes(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, dec =>
			{
				if (WarehouseTransactionStatusList.IsPendingInward(supporter.WarehouseTransactionStatus))
				{
					dec.DeleteModificationShipmentNotes();
				}
				return PublishToUniversalResult.Empty;
			}, true);
		}

		public static PublishToUniversalResult PublishAcceptEventForWHSInwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.WarehouseJobCanNowBeFinalised, Res.GetString("a211d6ad-1149-4f45-a7ad-f377af6f75a1", "Warehouse Job Can Now Be Finalized"), RecipientRoleType.BWI, DataContextType.WarehouseReceive, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = supporter.HasWHSInwardTransactionAndNotCreatedPending() ? WarehouseTransactionStatusList.Codes.InwardUpdated : WarehouseTransactionStatusList.Codes.InwardCreated;
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = true;
					}
				}
			});
		}

		public static PublishToUniversalResult PublishShipmentForWHSInwardFromLastHoldOrLatestData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLastHoldOrLatestData(supporter, DataContextType.WarehouseReceive, RecipientRoleType.BWI);
		}

		public static void RestoreBondedWarehouseInwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null && supporter.IsInDatabase)
			{
				var factory = new BusinessObjectFactory();
				var supporterInDifferentFactory = (IWarehouseIntegrationSupporter)factory.Load(supporter.GetType(), supporter.Identifier);

				var warehouseTransactionStatus = supporterInDifferentFactory?.WarehouseTransactionStatus ?? ZString.Empty;
				if (!warehouseTransactionStatus.IsEmpty)
				{
					if (warehouseTransactionStatus.EqualsIgnoringCase(WarehouseTransactionStatusList.Codes.InwardCreationHeld))
					{
						supporterInDifferentFactory.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, ZString.Empty, isManualWhsUpdate: false);
					}
					else if (WarehouseTransactionStatusList.IsPendingInward(warehouseTransactionStatus))
					{
						var messageInitiator = supporter.MessageInitiator;
						var result = supporterInDifferentFactory.PublishShipmentForWHSInwardFromLatestClearedData();
						if (messageInitiator.IsPublishToUniversalTransactionOK(result))
						{
							messageInitiator.IsPublishToUniversalTransactionOK(supporterInDifferentFactory.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true));
						}
					}
				}
			}
		}

		public static void RestoreLatestClearedBondedWarehouseInwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			supporter.PublishShipmentInADifferentFactory((x) => x.PublishShipmentForWHSInwardFromLatestClearedData(), (x) => x.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true));
		}

		public static PublishToUniversalResult PublishShipmentForWHSInwardFromLatestClearedData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentFromLatestClearedData(supporter, DataContextType.WarehouseReceive, RecipientRoleType.BWI);
		}

		public static void PublishAcceptEventForWHSInwardInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true));
		}

		public static PublishToUniversalResult PublishHoldEventForWHSInwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.HoldTheWarehouseOrder, Res.GetString("6F3F8AB3-D042-4F20-8D93-F7375153DAEC", "Hold The Warehouse Receive"), RecipientRoleType.BWI, DataContextType.WarehouseReceive, shouldSave);
		}

		public static PublishToUniversalResult PublishCancelEventForWHSInwardAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, string warehouseTransactionStatus = null, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.CancelTheWarehouseJob, Res.GetString("06f222ee-748c-4e38-84c2-08c816b98976", "Cancel Warehouse Receive"), RecipientRoleType.BWI, DataContextType.WarehouseReceive, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = warehouseTransactionStatus ?? WarehouseTransactionStatusList.Codes.InwardCanceled;
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = false;
					}
				}
			});
		}

		public static void CancelHoldShipmentForWHSInwardInDifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null && supporter.IsInDatabase)
			{
				var factory = new BusinessObjectFactory();
				var supporterInDifferentFactory = (IWarehouseIntegrationSupporter)factory.Load(supporter.GetType(), supporter.Identifier);
				if (supporterInDifferentFactory != null)
				{
					supporterInDifferentFactory.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, ZString.Empty, isManualWhsUpdate: false);
				}
			}
		}

		public static PublishToUniversalResult PublishShipmentForWHSInward(this IWarehouseIntegrationSupporter supporter, bool isHold, bool shouldSave = true)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
			{
				var company = dec.Company;
				var hasWHSTransaction = dec.HasWHSInwardTransactionAndNotCreatedPending();

				var result = isHold && !hasWHSTransaction ? PublishShipmentForWHSInwardOrOutward(dec, RecipientRoleType.BWI, true) : GetResultFromEventsFor(DataContextType.WarehouseReceive, PublishUniversalShipment(company.OrgProxy, new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWI, ServiceCode = isHold ? ServiceCodeType.HLD : ServiceCodeType.AMD } }, dec));

				if (isHold && result != null && result.ResultType != UniversalResult.HadErrors)
				{
					dec.WarehouseTransactionStatus = hasWHSTransaction ? WarehouseTransactionStatusList.Codes.InwardUpdatedPending : WarehouseTransactionStatusList.Codes.InwardCreationHeld;
				}
				return result;
			}, isHold && shouldSave);
		}

		static PublishToUniversalResult PublishShipmentForWHSInwardOrOutward(IWarehouseIntegrationSupporter supporter, RecipientRoleType recipientRoleType, bool shouldPublishXml)
		{
			PublishToUniversalResult result = null;
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardFirstTryPending())
			{
				CreateModificationUniversalShipment(supporter, recipientRoleType);
			}
			else
			{
				var holderWriter = CreateHoldUniversalShipment(supporter, recipientRoleType);
				if (shouldPublishXml)
				{
					result = GetResultFromEventsFor(holderWriter.TopLevelDataContextType, PublishUniversalShipment(supporter.Company.OrgProxy, new[] { new RecipientRoleDetail() { Type = recipientRoleType, ServiceCode = ServiceCodeType.HLD } }, supporter, (outboundSessionTracker) => holderWriter));
				}
			}
			return result ?? PublishToUniversalResult.Empty;
		}

		#endregion

		#region Change Of Ownership
		public static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnershipWithPreAmendmentData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentWithPreAmendmentData(supporter, new RecipientRoleDetail() { Type = RecipientRoleType.BCO, ServiceCode = ServiceCodeType.HLD }, DataContextType.WarehouseBondedChangeOfInventory, WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending);
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnershipFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.PublishShipmentForWHSChangeOfOwnership((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLastDataAndUpdateEntryDetails(dec, DataContextType.WarehouseBondedChangeOfInventory, new RecipientRoleDetail() { Type = RecipientRoleType.BCO, ServiceCode = ServiceCodeType.AMD });
			});
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnershipFromLastHoldOrLatestDataAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.PublishShipmentForWHSChangeOfOwnership((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLastHoldOrLatestData(dec, DataContextType.WarehouseBondedChangeOfInventory, RecipientRoleType.BCO);
			});
		}

		static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnership(this IWarehouseIntegrationSupporter supporter, Func<IWarehouseIntegrationSupporter, PublishToUniversalResult> publish, bool isHold = false, string holdStatus = WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, bool isManualWhsUpdate = false)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
			{
				var result = publish(dec);
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					if (isHold)
					{
						dec.WarehouseTransactionStatus = dec.HasWHSChangeOfOwnershipTransactionAndNotCreatedPending() ? holdStatus : WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending;
					}
					else
					{
						dec.WarehouseTransactionStatus = dec.HasWHSChangeOfOwnershipTransactionAndNotCreatedPending() ? WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated : WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated;
						dec.DeleteUniversalShipmentNotes();
					}
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = true;
					}
				}
				return result;
			}, true);
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnership(this IWarehouseIntegrationSupporter supporter, bool isHold, bool isManualWhsUpdate = false)
		{
			return supporter.PublishShipmentForWHSChangeOfOwnership((IWarehouseIntegrationSupporter dec) =>
			{
				return GetResultFromEventsFor(DataContextType.WarehouseBondedChangeOfInventory, PublishUniversalShipment(dec.Company.OrgProxy, new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BCO, ServiceCode = isHold ? ServiceCodeType.HLD : ServiceCodeType.AMD } }, dec));
			}, isHold, WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, isManualWhsUpdate);
		}

		public static void PublishCancelEventForWHSChangeOfOwnershipInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true));
		}

		public static PublishToUniversalResult PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.CancelTheWarehouseJob, Res.GetString("{5EFF59FA-4BCF-4FAD-BB72-EA6837D37F49}", "Cancel Bonded Warehouse Change of Inventory"), RecipientRoleType.BCO, DataContextType.WarehouseBondedChangeOfInventory, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled;
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = false;
					}
				}
			});
		}

		public static void RestoreLatestClearedBondedWarehouseChangeOfOwnershipInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			supporter.PublishShipmentInADifferentFactory((x) => x.PublishShipmentForWHSChangeOfOwnership((IWarehouseIntegrationSupporter dec) =>
			{
				return dec.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded();
			}));
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool isHold = false)
		{
			return supporter.PublishShipmentForWHSChangeOfOwnership((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLatestClearedData(dec, DataContextType.WarehouseBondedChangeOfInventory, new RecipientRoleDetail() { Type = RecipientRoleType.BCO, ServiceCode = isHold ? ServiceCodeType.HLD : ServiceCodeType.AMD });
			}, isHold);
		}

		#endregion

		#region Change Of Regime
		public static PublishToUniversalResult PublishShipmentForWHSChangeOfRegimeWithPreAmendmentData(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishShipmentWithPreAmendmentData(supporter, new RecipientRoleDetail() { Type = RecipientRoleType.BCR, ServiceCode = ServiceCodeType.HLD }, DataContextType.WarehouseBondedChangeOfInventory, WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending);
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfRegimeFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.PublishShipmentForWHSChangeOfRegime((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLastDataAndUpdateEntryDetails(dec, DataContextType.WarehouseBondedChangeOfInventory, new RecipientRoleDetail() { Type = RecipientRoleType.BCR, ServiceCode = ServiceCodeType.AMD });
			});
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfRegimeFromLastHoldOrLatestDataAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.PublishShipmentForWHSChangeOfRegime((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLastHoldOrLatestData(dec, DataContextType.WarehouseBondedChangeOfInventory, RecipientRoleType.BCR);
			});
		}

		static PublishToUniversalResult PublishShipmentForWHSChangeOfRegime(this IWarehouseIntegrationSupporter supporter, Func<IWarehouseIntegrationSupporter, PublishToUniversalResult> publish, bool isHold = false, string holdStatus = WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding, bool isManualWhsUpdate = false)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
			{
				var result = publish(dec);
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					if (isHold)
					{
						dec.WarehouseTransactionStatus = dec.HasWHSChangeOfRegimeTransactionAndNotCreatedPending() ? holdStatus : WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending;
					}
					else
					{
						dec.WarehouseTransactionStatus = dec.HasWHSChangeOfRegimeTransactionAndNotCreatedPending() ? WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated : WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated;
						dec.DeleteUniversalShipmentNotes();
					}
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = true;
					}
				}
				return result;
			}, true);
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfRegime(this IWarehouseIntegrationSupporter supporter, bool isHold, bool isManualWhsUpdate = false)
		{
			return supporter.PublishShipmentForWHSChangeOfRegime((IWarehouseIntegrationSupporter dec) =>
			{
				return GetResultFromEventsFor(DataContextType.WarehouseBondedChangeOfInventory, PublishUniversalShipment(dec.Company.OrgProxy, new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BCR, ServiceCode = isHold ? ServiceCodeType.HLD : ServiceCodeType.AMD } }, dec));
			}, isHold, WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending, isManualWhsUpdate);
		}

		public static void PublishCancelEventForWHSChangeOfRegimeInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			PublishShipmentInADifferentFactory(supporter, (IWarehouseIntegrationSupporter dec) => dec.PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(true));
		}

		public static PublishToUniversalResult PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool shouldSave = false, bool isManualWhsUpdate = false)
		{
			return PublishEventForWHSAndSaveIfNeeded(supporter, Events.CancelTheWarehouseJob, Res.GetString("{D06F5A78-2D5E-4FA5-9BEE-A01A3600529F}", "Cancel Inventory Change of Regime"), RecipientRoleType.BCR, DataContextType.WarehouseBondedChangeOfInventory, shouldSave, (PublishToUniversalResult result) =>
			{
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled;
					supporter.DeleteUniversalShipmentNotes();
					if (isManualWhsUpdate)
					{
						supporter.HasManualWhsUpdate = false;
					}
				}
			});
		}

		public static void RestoreLatestClearedBondedWarehouseChangeOfRegimeInADifferentFactory(this IWarehouseIntegrationSupporter supporter)
		{
			supporter.PublishShipmentInADifferentFactory((x) => x.PublishShipmentForWHSChangeOfRegime((IWarehouseIntegrationSupporter dec) =>
			{
				return dec.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded();
			}));
		}

		public static PublishToUniversalResult PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(this IWarehouseIntegrationSupporter supporter, bool isHold = false)
		{
			return supporter.PublishShipmentForWHSChangeOfRegime((IWarehouseIntegrationSupporter dec) =>
			{
				return PublishShipmentFromLatestClearedData(dec, DataContextType.WarehouseBondedChangeOfInventory, new RecipientRoleDetail() { Type = RecipientRoleType.BCR, ServiceCode = isHold ? ServiceCodeType.HLD : ServiceCodeType.AMD });
			}, isHold);
		}

		#endregion

		#region Common Methods

		public static PublishToUniversalResult ElevateModificationToHold(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, dec =>
			{
				if (WarehouseTransactionStatusList.IsPendingInwardOrOutward(supporter.WarehouseTransactionStatus) && supporter.HasModificationShipmentNote())
				{
					dec.DeleteHoldShipmentNotes();
					dec.ElevateModificationShipmentNotesToHold();
				}
				return PublishToUniversalResult.Empty;
			}, true);
		}

		public static PublishToUniversalResult DeleteModificationNotes(this IWarehouseIntegrationSupporter supporter)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, dec =>
			{
				if (WarehouseTransactionStatusList.IsPendingInwardOrOutward(supporter.WarehouseTransactionStatus))
				{
					dec.DeleteModificationShipmentNotes();
				}
				return PublishToUniversalResult.Empty;
			}, true);
		}

		static PublishToUniversalResult PublishShipmentWithPreAmendmentData(IWarehouseIntegrationSupporter supporter, RecipientRoleDetail recipientRoleDetail, DataContextType dataContextType, string warehouseTransactionStatusUpdatedPending)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
			{
				var company = dec.Company;
				var recipients = new[] { recipientRoleDetail };
				var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();
				var result = GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, recipients, dec, (outboundSessionTracker) =>
				{
					ITopLevelDataObjectWriter writer = null;
					if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
					{
						writer = GetPreAmendmentWriter(outboundSessionTracker, recipientRoleDetail.Type);
						writerDictionary.Add(outboundSessionTracker, writer);
					}
					return writer;
				}));
				if (result != null && result.ResultType != UniversalResult.HadErrors)
				{
					CreateHoldUniversalShipment(supporter, recipientRoleDetail.Type);
					dec.WarehouseTransactionStatus = warehouseTransactionStatusUpdatedPending;
				}
				return result;
			}, true);
		}

		static ITopLevelDataObjectWriter GetPreAmendmentWriter(IDataWritingManager outboundSessionTracker, RecipientRoleType recipientRoleType)
		{
			var provider = ObjectFactory.Get<IAmendmentWriterProvider>();
			return provider.GetPreAmendmentWriter(outboundSessionTracker, recipientRoleType);
		}

		static PublishToUniversalResult PublishShipmentFromLastDataAndUpdateEntryDetails(IWarehouseIntegrationSupporter supporter, DataContextType dataContextType, RecipientRoleDetail recipientRoleDetail)
		{
			var company = supporter.Company;
			var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();
			return GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, new[] { recipientRoleDetail }, supporter, (outboundSessionTracker) =>
			{
				ITopLevelDataObjectWriter writer = null;
				if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
				{
					writer = GetLastDataEntryNumberWriterProvider(outboundSessionTracker, recipientRoleDetail.Type);
					writerDictionary.Add(outboundSessionTracker, writer);
				}
				return writer;
			}));
		}

		static ITopLevelDataObjectWriter GetLastDataEntryNumberWriterProvider(IDataWritingManager outboundSessionTracker, RecipientRoleType recipientRoleType)
		{
			var provider = ObjectFactory.Get<ILastDataEntryNumberWriterProvider>();
			return provider.GetLastDataWriter(outboundSessionTracker, recipientRoleType);
		}

		public static Shipment GetLastClearedUniversalShipment(this IWarehouseIntegrationSupporter warehouseIntegrationSupporter, RecipientRoleType recipientRoleType)
		{
			Shipment result = null;
			var foundClearedEvent = false;

			var collection = warehouseIntegrationSupporter.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();

			foreach (var exportLog in collection)
			{
				exportLog.Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, exportLog.PK);
			}

			IEDIMessage lastUXMLShipmentMessage = null;
			StmALog lastUXMShipmentDEX = null;
			foreach (var exportLog in collection)
			{
				var relatedEDIMessage = exportLog.RelatedEDIMessage;
				if (relatedEDIMessage != null && relatedEDIMessage.DataContext?.RecipientRoleCollection?.FirstOrDefault(x => x.Code.HasValue && x.Code.Value == recipientRoleType) != null)
				{
					if (relatedEDIMessage.Message is IEDIMessage message)
					{
						var messageSubType = message.EM_MessageSubType.ToUpperInvariant();
						if (messageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent)
						{
							if (foundClearedEvent)
							{
								continue;
							}
							var eventDataObject = message.GetEM_MessageTextReader().Parse<UniversalDataBuss.DataObjects.Universal.Event>();
							foundClearedEvent = eventDataObject != null && eventDataObject.EventType.GetValueOrDefault() == Events.WarehouseJobCanNowBeFinalisedCode;
							if (foundClearedEvent && lastUXMShipmentDEX != null && lastUXMShipmentDEX.SL_PostedTimeUtc == exportLog.SL_PostedTimeUtc)
							{
								result = lastUXMLShipmentMessage?.GetEM_MessageTextReader().Parse<Shipment>();
								if (result != null)
								{
									break;
								}
							}
						}
						else if (messageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
						{
							if (foundClearedEvent || (relatedEDIMessage.DataContext?.RecipientRoleCollection?.FirstOrDefault(x => x.Code.HasValue && x.Code.Value == recipientRoleType && x.ServiceCode.HasValue && x.ServiceCode.Value == ServiceCodeType.AMD) != null))
							{
								result = message.GetEM_MessageTextReader().Parse<Shipment>();
								if (result != null)
								{
									break;
								}
							}
							else
							{
								lastUXMLShipmentMessage = message;
								lastUXMShipmentDEX = exportLog;
							}
						}
					}
				}
			}
			return result;
		}

		public static Shipment GetLastHoldUniversalShipment(this IWarehouseIntegrationSupporter warehouseIntegrationSupporter, RecipientRoleType recipientRoleType)
		{
			Shipment result = warehouseIntegrationSupporter.GetLastHoldUniversalShipmentFromNote()
				?? warehouseIntegrationSupporter.GetLastUniversalShipmentFromDataExport(recipientRoleType, ServiceCodeType.HLD);
			return result;
		}

		public static Shipment GetLastModificationUniversalShipment(this IWarehouseIntegrationSupporter warehouseIntegrationSupporter, RecipientRoleType recipientRoleType)
		{
			return warehouseIntegrationSupporter.GetLastModificationUniversalShipmentFromNote();
		}

		public static Shipment GetLastUniversalShipmentFromDataExport(this IWarehouseIntegrationSupporter warehouseIntegrationSupporter, RecipientRoleType recipientRoleType, ServiceCodeType? serviceCodeType)
		{
			Shipment result = null;
			var collection = warehouseIntegrationSupporter.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).Where(x => !x.SL_IsCancelled && !x.SL_IsEstimate).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
			if (collection.Length > 0)
			{
				foreach (var exportLog in collection)
				{
					exportLog.Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, exportLog.PK);
				}

				var checkServiceCodeType = serviceCodeType.HasValue;
				foreach (var exportLog in collection)
				{
					var relatedEDIMessage = exportLog.RelatedEDIMessage;
					if (relatedEDIMessage != null && (relatedEDIMessage.DataContext.RecipientRoleCollection?.Any(x => (!checkServiceCodeType || (x.ServiceCode.HasValue && x.ServiceCode.Value == serviceCodeType.Value)) && x.Code.HasValue && x.Code.Value == recipientRoleType) ?? false))
					{
						var message = relatedEDIMessage.Message;
						if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
						{
							result = relatedEDIMessage.Message.GetEM_MessageTextReader().Parse<Shipment>();
							break;
						}
					}
				}
			}
			return result;
		}

		public static Shipment GetLastHoldUniversalShipmentFromNote(this IWarehouseIntegrationSupporter supporter)
		{
			Shipment result = null;
			if (supporter != null)
			{
				var note = supporter.Notes?.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).OrderBy(x => x.ST_CreatedDateUtc).FirstOrDefault();
				if (note != null)
				{
					result = new StreamReader(note.GetST_NoteDataReader(), MessageEncoding.UTF8WithoutBOM).Parse<Shipment>();
				}
			}
			return result;
		}

		static ITopLevelDataObjectWriter GetHoldWriterProvider(IDataWritingManager outboundSessionTracker, RecipientRoleType recipientRoleType)
		{
			var provider = ObjectFactory.Get<IHoldWriterProvider>();
			return provider.GetLastHoldWriter(outboundSessionTracker, recipientRoleType);
		}

		public static Shipment GetLastModificationUniversalShipmentFromNote(this IWarehouseIntegrationSupporter supporter)
		{
			Shipment result = null;
			if (supporter != null)
			{
				var note = supporter.Notes?.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).OrderBy(x => x.ST_CreatedDateUtc).FirstOrDefault();
				if (note != null)
				{
					result = new StreamReader(note.GetST_NoteDataReader(), MessageEncoding.UTF8WithoutBOM).Parse<Shipment>();
				}
			}
			return result;
		}

		static ITopLevelDataObjectWriter GetModificationWriterProvider(IDataWritingManager outboundSessionTracker, RecipientRoleType recipientRoleType)
		{
			var provider = ObjectFactory.Get<IModificationWriterProvider>();
			return provider.GetLastHoldWriter(outboundSessionTracker, recipientRoleType);
		}

		static PublishToUniversalResult PublishShipmentFromLatestClearedData(IWarehouseIntegrationSupporter supporter, DataContextType dataContextType, RecipientRoleType recipientRoleType)
		{
			var company = supporter.Company;
			var recipients = new RecipientRoleType[] { recipientRoleType };
			var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();
			return GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, recipients, supporter, (outboundSessionTracker) =>
			{
				ITopLevelDataObjectWriter writer = null;
				if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
				{
					writer = GetPreviousClearedWriter(outboundSessionTracker, recipientRoleType);
					writerDictionary.Add(outboundSessionTracker, writer);
				}
				return writer;
			}));
		}

		static PublishToUniversalResult PublishShipmentFromLatestClearedData(IWarehouseIntegrationSupporter supporter, DataContextType dataContextType, RecipientRoleDetail recipientRoleDetail)
		{
			var company = supporter.Company;
			var recipients = new RecipientRoleDetail[] { recipientRoleDetail };
			var writerDictionary = new Dictionary<IDataWritingManager, ITopLevelDataObjectWriter>();
			return GetResultFromEventsFor(dataContextType, PublishUniversalShipment(company.OrgProxy, recipients, supporter, (outboundSessionTracker) =>
			{
				ITopLevelDataObjectWriter writer = null;
				if (!writerDictionary.TryGetValue(outboundSessionTracker, out writer))
				{
					writer = GetPreviousClearedWriter(outboundSessionTracker, recipientRoleDetail.Type);
					writerDictionary.Add(outboundSessionTracker, writer);
				}
				return writer;
			}));
		}

		static ITopLevelDataObjectWriter GetPreviousClearedWriter(IDataWritingManager outboundSessionTracker, RecipientRoleType recipientRoleType)
		{
			var provider = ObjectFactory.Get<IAmendmentWriterProvider>();
			return provider.GetPreviousClearedWriter(outboundSessionTracker, recipientRoleType);
		}

		static void PublishShipmentInADifferentFactory(this IWarehouseIntegrationSupporter supporter, Func<IWarehouseIntegrationSupporter, PublishToUniversalResult> publish1, Func<IWarehouseIntegrationSupporter, PublishToUniversalResult> publish2 = null)
		{
			if (supporter != null && supporter.IsInDatabase)
			{
				var factory = new BusinessObjectFactory();
				var supporterInDifferentFactory = (IWarehouseIntegrationSupporter)factory.Load(supporter.GetType(), supporter.Identifier);
				if (supporterInDifferentFactory != null)
				{
					var messageInitiator = supporter.MessageInitiator;
					var result = publish1(supporterInDifferentFactory);
					if (messageInitiator.IsPublishToUniversalTransactionOK(result) && publish2 != null)
					{
						messageInitiator.IsPublishToUniversalTransactionOK(publish2(supporterInDifferentFactory));
					}
				}
			}
		}

		public static void DeleteUniversalShipmentNotes(this IWarehouseIntegrationSupporter supporter)
		{
			DeleteHoldShipmentNotes(supporter);
			DeleteModificationShipmentNotes(supporter);
		}

		static void DeleteHoldShipmentNotes(this IWarehouseIntegrationSupporter supporter)
		{
			var notes = supporter.Notes;
			if (notes != null)
			{
				notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).DeleteAll();
			}
		}

		public static bool IsInwardOrOutwardFirstTryPending(this IWarehouseIntegrationSupporter supporter)
		{
			return (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCreationHeld || supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending) && supporter.HasHoldShipmentNote() && !supporter.HasModificationShipmentNote();
		}

		public static bool IsInwardOrOutwardSecondTryPending(this IWarehouseIntegrationSupporter supporter)
		{
			return (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCreationHeld || supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending) && supporter.HasHoldShipmentNote() && supporter.HasModificationShipmentNote();
		}

		static bool HasHoldShipmentNote(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.Notes?.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription)?.Any() ?? false;
		}

		static bool HasModificationShipmentNote(this IWarehouseIntegrationSupporter supporter)
		{
			return supporter.Notes?.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription)?.Any() ?? false;
		}

		public static void DeleteModificationShipmentNotes(this IWarehouseIntegrationSupporter supporter)
		{
			var notes = supporter.Notes;
			if (notes != null)
			{
				notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).DeleteAll();
			}
		}

		static void ElevateModificationShipmentNotesToHold(this IWarehouseIntegrationSupporter supporter)
		{
			var notes = supporter.Notes;
			if (notes != null)
			{
				var modificationNotes = notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription);
				if (modificationNotes != null)
				{
					foreach (var note in modificationNotes)
					{
						note.ST_Description = WarehouseConstants.UniversalHoldShipmentNoteDescription;
					}
				}
			}
		}

		static ITopLevelDataObjectWriter CreateHoldUniversalShipment(IWarehouseIntegrationSupporter supporter, RecipientRoleType recipientRoleType)
		{
			return CreateUniversalShipment(supporter, recipientRoleType, WarehouseConstants.UniversalHoldShipmentNoteDescription);
		}

		static void CreateModificationUniversalShipment(IWarehouseIntegrationSupporter supporter, RecipientRoleType recipientRoleType)
		{
			CreateUniversalShipment(supporter, recipientRoleType, WarehouseConstants.UniversalModificationShipmentNoteDescription);
		}

		static ITopLevelDataObjectWriter CreateUniversalShipment(IWarehouseIntegrationSupporter supporter, RecipientRoleType recipientRoleType, string noteDescription)
		{
			var bizObj = (BusinessObject)supporter;
			IExternalFetchHintSupporter externalFetchHintSupporter = bizObj.Factory;
			ITopLevelDataObjectWriter holderWriter = null;
			using (externalFetchHintSupporter.SetupCreator())
			{
				var manager = (IShipmentDataContextManager)bizObj.GetUniversalDataContextManager();
				var actionInfo = new ActionInfo(new[] { new RecipientRoleDetail() { Type = recipientRoleType, ServiceCode = ServiceCodeType.HLD } }, bizObj)
				{
					ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML
				};
				var outboundSessionTracker = new DataWritingManager(actionInfo);
				var writer = manager.GetShipmentDataObjectWriter(outboundSessionTracker);
				var shipment = writer.GetDataObject(bizObj);
				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, bizObj, shipment.DataContext);
				var stream = new CargoWise.IO.Shim.SubStreamableStream();
				new XmlWriter().WriteXML(shipment, stream);
				if (supporter.Notes != null)
				{
					var notes = supporter.Notes.FindByDescription(noteDescription).ToList();
					StmNote note;
					if (notes.Count == 0)
					{
						note = supporter.Notes.AddNew(true, noteDescription, "."); //Need this here,in SetST_NoteDataSource will be removed.
					}
					else
					{
						note = notes[0];
						notes.Remove(note);
						notes.DeleteAll();
					}
					note.ST_IsCustomDescription = true;
					note.SetST_NoteDataSource(new StreamSource(stream));
				}
				holderWriter = new HolderTopLevelDataObjectWriter(writer.TopLevelDataContextType, writer.EDIMessageSubType, writer.RootElementName, shipment);
			}

			return holderWriter;
		}

		class HolderTopLevelDataObjectWriter : ITopLevelDataObjectWriter
		{
			public HolderTopLevelDataObjectWriter(DataContextType topLevelDataContextType, ZString ediMessageSubType, ZString rootElementName, ITopLevelDataObject dataObject)
			{
				this.TopLevelDataContextType = topLevelDataContextType;
				this.EDIMessageSubType = ediMessageSubType;
				this.RootElementName = rootElementName;
				this.dataObject = dataObject;
			}

			public DataContextType TopLevelDataContextType { get; private set; }

			public ZString EDIMessageSubType { get; private set; }

			public ZString RootElementName { get; private set; }

			public ITopLevelDataObject GetDataObject(BusinessObject sourceBO) => dataObject;
			readonly ITopLevelDataObject dataObject;
		}

		public static PublishUniversalXmlResult PublishUniversalShipment(OrgHeader recipientOrganisation, RecipientRoleType[] recipientRoleTypes, IWorkflowProviderCore workflowAndDataProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null)
		{
			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, recipientOrganisation, recipientRoleTypes, workflowAndDataProvider, dataWriterGetter);
				factory.Save();
			}
			return events;
		}

		public static PublishUniversalXmlResult PublishUniversalShipment(OrgHeader recipientOrganisation, RecipientRoleDetail[] recipientRoleDetails, IWorkflowProviderCore workflowAndDataProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = null)
		{
			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, recipientOrganisation, recipientRoleDetails, workflowAndDataProvider, dataWriterGetter);
				factory.Save();
			}
			return events;
		}

		public static PublishUniversalXmlResult PublishUniversalEvent(RecipientRoleType[] recipientRoleTypes, IWorkflowProvider workflowAndDataProvider, OrgHeader[] recipientOrganisations, StmALog eventBO)
		{
			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, recipientRoleTypes, workflowAndDataProvider, recipientOrganisations, eventBO);
				factory.Save();
			}
			return events;
		}

		public static PublishToUniversalResult GetResult(this PublishUniversalXmlResult events, DataContextType contextType)
		{
			PublishToUniversalResult result = null;
			if (events != null && events.Length > 0)
			{
				result = PublishToUniversalResult.New(events, contextType, ZString.Empty);
			}
			return result;
		}

		static PublishToUniversalResult GetResultFromEventsFor(DataContextType contextType, PublishUniversalXmlResult events)
		{
			return events.GetResult(contextType);
		}

		static PublishToUniversalResult PublishEventForWHSAndSaveIfNeeded(IWarehouseIntegrationSupporter supporter, ZArchitecture.Business.Event eventType, ZString eventRefence, RecipientRoleType recipientRoleType, DataContextType contextType, bool shouldSave = false, Action<PublishToUniversalResult> preSaveAction = null)
		{
			return PublishUniversalXMLForWHSAndSaveIfNeeded(supporter, (IWarehouseIntegrationSupporter dec) =>
				{
					var company = dec.Company;
					var newEvent = dec.Logs.AddNew(eventType, eventRefence);
					return GetResultFromEventsFor(contextType, PublishUniversalEvent(new RecipientRoleType[] { recipientRoleType }, dec, new OrgHeader[] { company.OrgProxy }, newEvent));
				}, shouldSave, preSaveAction);
		}

		static PublishToUniversalResult PublishUniversalXMLForWHSAndSaveIfNeeded(IWarehouseIntegrationSupporter supporter, Func<IWarehouseIntegrationSupporter, PublishToUniversalResult> publishUniversalXML, bool shouldSave = false, Action<PublishToUniversalResult> preSaveAction = null)
		{
			using (shouldSave ? supporter.Factory.AddDisposableService() : null)
			{
				PublishToUniversalResult result = null;
				try
				{
					result = publishUniversalXML(supporter);
					if (preSaveAction != null)
					{
						preSaveAction(result);
					}
					if (shouldSave)
					{
						supporter.Factory.Save();
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					result = null;
				}
				return result;
			}
		}

		#endregion
	}
}
