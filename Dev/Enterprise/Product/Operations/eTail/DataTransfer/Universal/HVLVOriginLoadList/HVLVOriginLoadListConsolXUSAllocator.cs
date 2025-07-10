using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVOriginLoadListConsolXUSAllocator : IHVLVOriginLoadListConsolAllocator
	{
		public HVLVOriginLoadListConsolXUSAllocator(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		public IForwardingConsol AllocatedConsol { get; private set; }

		public bool TryAttachToConsol(IForwardingConsol consol, IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage)
		{
			var processingSucceed = true;
			errorMessage = string.Empty;

			foreach (var loadlist in loadLists)
			{
				processingSucceed = TryAttachToConsolCore(consol, loadlist, out errorMessage);

				if (!processingSucceed)
				{
					break;
				}
			}

			return processingSucceed;
		}

		public bool TryCreateConsolAndAttachLoadLists(IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage)
		{
			var firstLoadList = loadLists.FirstOrDefault();
			var processingSucceed = TryAttachToConsolCore(null, firstLoadList, out errorMessage);

			if (processingSucceed)
			{
				processingSucceed = TryAttachToConsol(AllocatedConsol, loadLists.Skip(1), out errorMessage);
			}

			if (!processingSucceed)
			{
				AllocatedConsol = null;
			}

			return processingSucceed;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		bool TryAttachToConsolCore(IForwardingConsol consol, IHVLVOriginLoadList loadList, out string errorMessage)
		{
			var originLoadList = (HVLVOriginLoadList)loadList;
			errorMessage = null;
			var processingSucceedSoFar = true;

			if (originLoadList.Items.Count <= 0)
			{
				errorMessage = Res.GetString("4e076996-2638-4fba-a4f2-449cd0d67d3b", "HVLV Origin Load List '{0}' does not have any items attached", originLoadList.HVL_UniqueReference);
				logger.Error($"HVLV Origin Load List '{originLoadList.HVL_UniqueReference}' does not have any items attached");

				processingSucceedSoFar = false;
			}

			if (processingSucceedSoFar)
			{
				using (originLoadList.Split())
				{
					SetBookingHeadersIsProcessedAtOriginDepot(originLoadList);

					var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, originLoadList));
					var xusFactory = new UniversalObjectFactory(originLoadList.Factory);
					var topXUS = new HVLVOriginLoadListDataObjectWriter(writingManager).ExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly(originLoadList);
					var xmlLogger = new ConvertTracker(1, default, null, logger);
					xmlLogger.TopLevelDataObject = topXUS;

					logger.Log(LogType.Debug, string.Format("Try to attach a matching consol for origin loadlist {0}", originLoadList.HVL_UniqueReference));

					if (consol == null)
					{
						consol = new ConsolDataObjectReader(topXUS, xmlLogger, xusFactory).ReadIntoBusinessObject();

						if (consol == null)
						{
							ThrowExceptionForDataTransfer(xmlLogger, "Failed to read into consol.", logger);
						}
					}

					AllocatedConsol = consol;

					if (consol is BusinessObject consolBO && consolBO.IsInDatabase)
					{
						logger.Log(LogType.Debug, string.Format("Get a matching consol {0} for origin loadlist {1}", consol.JK_UniqueConsignRef, originLoadList.HVL_UniqueReference));
					}
					else
					{
						logger.Log(LogType.Debug, string.Format("No matching consol found, successfully created a new consol for loadlist {0}...", originLoadList.HVL_UniqueReference));
					}

					var hvmShipment = default(ForwardingShipment);
					var forwardingConsol = consol as ForwardingConsol;

					if (originLoadList.HVL_IsMasterHouse)
					{
						var masterHouseLoadListWriter = new HLPProcessingMasterHouseLoadListDataObjectWriter(writingManager);
						var masterHouseXUS = masterHouseLoadListWriter.GetDataObject(originLoadList);
						hvmShipment = new HLPProcessingHVMShipmentDataObjectReader(masterHouseXUS, xmlLogger, xusFactory, forwardingConsol, new ContainerLinkManager<ForwardingConsol>(forwardingConsol)).ReadIntoBusinessObject();
						if (hvmShipment == null)
						{
							ThrowExceptionForDataTransfer(xmlLogger, "Failed to read into HVM shipment.", logger);
						}

						hvmShipment.UpdateShipmentFromOuterPackLines();

						foreach (ForwardingPackLine packLine in hvmShipment.OuterPackLines)
						{
							if (!packLine.JL_RefNumber.IsEmpty)
							{
								var matchingOuterPackages = originLoadList.OuterPackages.Where(o => o.HVO_PackageReference == packLine.JL_RefNumber);
								if (matchingOuterPackages.Count() == 1)
								{
									matchingOuterPackages.Single().HVO_JL_PackLine = packLine.PK;
								}
								else if (matchingOuterPackages.Count() > 1)
								{
									logger.Log(LogType.Warning, string.Format("Unable to match outer pack line with outer package, the outer package reference {0} repeated on load list {1}.", packLine.JL_RefNumber, string.Join(",", matchingOuterPackages.Select(o => o.LoadList.HVL_MasterBillNumber))));
								}
								else
								{
									logger.Log(LogType.Warning, string.Format("Unable to match outer pack line with outer package, no matching outer package with reference {0} was found.", packLine.JL_RefNumber));
								}
							}
							else
							{
								logger.Log(LogType.Warning, "Unable to match outer pack line with outer package, the outer package reference is empty.");
							}
						}
					}

					foreach (HVLVOuterPackage outerPackage in originLoadList.OuterPackages)
					{
						outerPackage.HVO_JK_LoadedOnConsol = consol.PK;
						outerPackage.HVO_Status = HVLVOuterPackageStatus.Codes.Consolidated;
					}

					foreach (var subLoadList in originLoadList.SubLoadLists)
					{
						if (subLoadList.DestinationPort.IsEmpty)
						{
							logger.Warning(Res.GetString("AEA9581D-99DD-4FEC-A0D9-A6A621C95406", "Cannot determine suitable Destination Port for HVL Shipment(s) on Consol '{0}'. There are zero or multiple UNLOCOs for country(s) '{1}'.", consol.JK_MasterBillNum, subLoadList.DestinationCountry));
						}

						var subWriter = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, subLoadList.ActualLoadList)));
						var subXUS = subWriter.GetDataObject(subLoadList.ActualLoadList);

						var shipmentReader = hvmShipment != null
							? new HLPProcessingHVLShipmentDataObjectReader(subXUS, xmlLogger, xusFactory, hvmShipment, subLoadList)
							: new HLPProcessingHVLShipmentDataObjectReader(subXUS, xmlLogger, xusFactory, forwardingConsol, subLoadList);

						var shipment = shipmentReader.ReadIntoBusinessObject();
						if (shipment == null)
						{
							ThrowExceptionForDataTransfer(xmlLogger, "Failed to read into HVL shipment.", logger);
						}

						if (shipment.JS_JS_ColoadMasterShipment.IsEmpty && hvmShipment != null)
						{
							shipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;
						}

						shipment.UpdateShipmentFromOuterPackLines();
						shipment.OuterPackLines.SetDefaultCommodityForCollection();

						var consignmentHeaderPK = shipment.GetOrCreateHVLVConsignmentHeader().PK;
						var shipmentPK = shipment.PK;
						foreach (var item in subLoadList.ActiveItems)
						{
							item.Consignment.HVC_HCH_Header = consignmentHeaderPK;
						}

						errorMessage = ReportErrorIfAnyDuplicatedWayBillNumberOrShipperReference(subLoadList);

						if (!errorMessage.IsNullOrEmpty())
						{
							logger.Error(errorMessage);
							processingSucceedSoFar = false;

							break;
						}
					}

					if (xmlLogger.HasError)
					{
						ThrowExceptionForDataTransfer(xmlLogger, "Error detected during data transfer process", logger);
					}

					if (processingSucceedSoFar)
					{
						originLoadList.HVL_Status = ELoadListStatuses.Consolidated;

						using (Logs.DeferFiringWorkflow(true))
						{
							var eventParameters = new[]
							{
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, originLoadList.HVL_UniqueReference)
							};

							forwardingConsol.Logs.CreateOrRecreateEventLog(AutoEvents.ELoadListConsolidated, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParameters);
						}

						PopulateMasterBillNumberForNeutralMasterLoadListService.AddPopulateMasterBillNumberForNeutralMasterLoadListServiceIfRequired(forwardingConsol, originLoadList);
					}
				}
			}

			return processingSucceedSoFar;
		}

		static void SetBookingHeadersIsProcessedAtOriginDepot(HVLVOriginLoadList loadList)
		{
			var bookingHeaders = GetLoadListBookingHeaders(loadList);
			bookingHeaders.ForEach(x => x.HVH_IsProcessedAtOriginDepot = true);
		}

		static HVLVBookingHeader[] GetLoadListBookingHeaders(HVLVOriginLoadList loadList)
		{
			var itemsSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVC_Consignment);
			itemsSubQuery.AddToFilter(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);

			var consignmentsSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.HVC_HVH_BookingHeader);
			consignmentsSubQuery.AddSubQuery(HVLVConsignmentSchema.PK, itemsSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(HVLVBookingHeader));
			query.AddSubQuery(HVLVBookingHeaderSchema.PK, consignmentsSubQuery, JoinCondition.And);

			return loadList.Factory.Load<HVLVBookingHeader>(query);
		}

		static string ReportErrorIfAnyDuplicatedWayBillNumberOrShipperReference(SubLoadList subLoadList)
		{
			var errorMessage = string.Empty;
			var duplicatedWayBillNumbers = GetDuplicatedReferences(subLoadList.ActiveItems, consignment => consignment.HVC_WaybillNumber);
			var duplicatedShipperReferences = GetDuplicatedReferences(subLoadList.ActiveItems, consignment => consignment.HVC_ShipperReference);

			if (duplicatedWayBillNumbers.Any() || duplicatedShipperReferences.Any())
			{
				var errorMessageBuilder = new ZStringBuilder();

				foreach (var duplicatedWayBillNumberInfo in duplicatedWayBillNumbers)
				{
					errorMessageBuilder.AppendLine(Res.GetString("98A8EC7C-4222-485C-BD87-789C143FAA01",
						"Error processing the HVLV Origin Load List '{0}': Consignment Waybill number '{1}' is repeated on booking headers: {2}",
						subLoadList.ActualLoadList.HVL_UniqueReference, duplicatedWayBillNumberInfo.DuplicatedReference, duplicatedWayBillNumberInfo.BookingReferences));
				}

				foreach (var duplicatedShipperReferenceInfo in duplicatedShipperReferences)
				{
					errorMessageBuilder.AppendLine(Res.GetString("E79B4548-02C2-4875-8A61-77084101EBDC",
						"Error processing the HVLV Origin Load List '{0}': Consignment Shipper Reference '{1}' is repeated on booking headers: {2}",
						subLoadList.ActualLoadList.HVL_UniqueReference, duplicatedShipperReferenceInfo.DuplicatedReference, duplicatedShipperReferenceInfo.BookingReferences));
				}

				errorMessage = errorMessageBuilder.ToString().Trim();
			}

			return errorMessage;
		}

		static IEnumerable<(ZString DuplicatedReference, string BookingReferences)> GetDuplicatedReferences(IEnumerable<HVLVItem> items, Func<HVLVConsignment, ZString> referenceSelector)
		{
			var duplicatedReferences = items
				.Select(i => i.Consignment)
				.Where(c => !referenceSelector(c).IsEmpty)
				.GroupBy(referenceSelector)
				.Where(group => group.Count() > 1)
				.Select(group =>
					(DuplicatedReference: group.Key,
					BookingReferences: string.Join(", ", group.Select(consignment => consignment.BookingHeader.HVH_BookingReference))));

			return duplicatedReferences;
		}

		static void ThrowExceptionForDataTransfer(ConvertTracker tracker, string errorMessage, ILogger logger)
		{
			Exception exception;

			if (tracker.HasError)
			{
				exception = new Exception($"Data transfer processing failure. Detail: {errorMessage}.\r\nErrors collected from convertion tracker:\r\n{tracker.GetErrorDetail()}");
			}
			else
			{
				exception = new Exception($"Data transfer processing failure due to unknown reason. Detail: {errorMessage}");
			}

			logger.Log(LogType.Debug, errorMessage, exception);

			throw exception;
		}

		#region Application Locks

		public bool TryAcquireApplicationLocks(IList<ZGuid> pks, Action<IEnumerable<ZGuid>> actionForLockedRows, out string errorMessage)
		{
			return pks.TryAcquireApplicationLocks<HVLVOriginLoadList>("HVLVOriginLoadListProcessingQueue", actionForLockedRows, out errorMessage);
		}

		#endregion
	}
}
