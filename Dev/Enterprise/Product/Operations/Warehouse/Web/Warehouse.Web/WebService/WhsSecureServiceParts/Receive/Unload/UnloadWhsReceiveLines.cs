using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region UnloadWhsReceiveLines

		[WebMethod(Description = "Unload Receive Lines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public WhsInventoryWebServiceResponse UnloadWhsReceiveLines(WhsLightDocketLineInfo[] lines, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabledForUnloadLine)
		{
			return HandleWebServiceRequest<WhsInventoryWebServiceResponse>(r => UnloadWhsReceiveLinesCore(r, lines, productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine));
		}

		WhsInventoryWebServiceResponse UnloadWhsReceiveLinesCore(WhsInventoryWebServiceResponse response, WhsLightDocketLineInfo[] lines, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabledForUnloadLine)
		{
			if (AllowedToRunService(response))
			{
				lock (Lock)
				{
					var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
					if (warehouse == null)
					{
						response.LogBusinessValidationError(Res.GetString("73D3FFA7-9956-471A-96B5-ECC557D0112D", "Warehouse should not be null."));
					}
					else
					{
						var referenceDocket = Factory.Load<WhsReceive>(new ZGuid(lines[0].DocketPK));
						if (referenceDocket == null)
						{
							response.LogBusinessValidationError(Res.GetString("b0cac925-ae8b-474b-af8c-83c8dc79df5e", "Receive record could not be found."));
						}
						else
						{
							if (referenceDocket.IsFinalised)
							{
								response.LogBusinessValidationError(Res.GetString("fe934266-ff11-4143-b409-c049e64dadc6", "Receive record has been finalized."));
							}
							else if (referenceDocket.WD_ArrivalDate.IsEmpty)
							{
								response.LogBusinessValidationError(Res.GetString("9c6d58e2-535f-460e-8f95-bc9e2a386a61", "Receive record arrival date is blank. Please suspend and reload this job."));
							}
							else
							{
								var factoryFactory = FactoryService.Value.GetFactory<Func<BusinessObjectFactory>>();

								var success = BatchValidateAndSave(response, lines, productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine, warehouse, referenceDocket, factoryFactory);

								if (!success)
								{
									LineByLineValidateAndSave(response, lines, productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine, warehouse, referenceDocket, factoryFactory);
								}
								else
								{
									response.InventoryErrorInfos = Array.Empty<ErrorInfo>();
								}
							}
						}
					}
				}
			}

			return response;
		}

		static string SaveFactoryExceptionMessage => Res.GetString("d2d64f14-2884-4359-8600-fac7b83f1497", "Another user has made changes to the receive while you have been working on it. Please restart the operation and try again.");

		void LineByLineValidateAndSave(WhsInventoryWebServiceResponse response, WhsLightDocketLineInfo[] lines, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabledForUnloadLine, WhsWarehouse warehouse, WhsReceive referenceDocket, Func<BusinessObjectFactory> factoryFactory)
		{
			var sequence = 0;
			var errorInfos = new List<ErrorInfo>();
			var addedErrors = new HashSet<string>();

			var productSummary = referenceDocket.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToDictionary(ps => ps.ProductPk, ps => ps);

			foreach (var line in lines)
			{
				var newFactory = factoryFactory.Invoke();
				var receive = newFactory.Load<WhsReceive>(referenceDocket.PK);
				var validationError = ValidateLineInfoAndCreateReceiveLine(response, productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine, warehouse, productSummary, receive, line);

				var hadReceiveError = false;

				if (string.IsNullOrEmpty(validationError))
				{
					var receiveErrors = GetValidationError(new[] { receive });
					validationError = receiveErrors;
					hadReceiveError = !string.IsNullOrEmpty(receiveErrors);
				}

				if (string.IsNullOrEmpty(validationError))
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(receive.Factory, response,
						ex => SaveFactoryExceptionMessage);
				}
				else
				{
					// This is dodgy should be added to result.InventoryErrorInfos when we start handling those errors on RF side.
					// We should not return same error more than once
					if (addedErrors.Add(validationError))
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage += validationError;
						errorInfos.Add(new ErrorInfo(sequence, validationError, nameof(ErrorTypes.BusinessValidationError)));
					}

					// no point processing further if the receive has an error
					if (hadReceiveError)
					{
						break;
					}
				}
				sequence++;
			}
			response.InventoryErrorInfos = errorInfos.ToArray();
		}

		bool BatchValidateAndSave(WhsInventoryWebServiceResponse response, WhsLightDocketLineInfo[] lines, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabledForUnloadLine, WhsWarehouse warehouse, WhsReceive referenceDocket, Func<BusinessObjectFactory> factoryFactory)
		{
			var success = false;

			var newFactory = factoryFactory.Invoke();
			var receive = newFactory.Load<WhsReceive>(referenceDocket.PK);
			var productSummary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().ToDictionary(ps => ps.ProductPk, ps => ps);

			foreach (var line in lines)
			{
				var validationError = ValidateLineInfoAndCreateReceiveLine(response, productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine, warehouse, productSummary, receive, line);

				success = string.IsNullOrEmpty(validationError) && string.IsNullOrEmpty(GetValidationError(new[] { receive }));

				if (!success)
				{
					break;
				}
			}
			if (success)
			{
				WebServiceHelper.SaveFactoryWithExceptionHandling(receive.Factory, response,
							ex => SaveFactoryExceptionMessage);
			}
			return success;
		}

		string ValidateLineInfoAndCreateReceiveLine(WhsInventoryWebServiceResponse response, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabledForUnloadLine, WhsWarehouse warehouse, Dictionary<ZGuid, WhsReceiveProductSummary> productSummary, WhsReceive receive, WhsLightDocketLineInfo line)
		{
			var validationError = ValidatePalletDuringUnload(receive, line.PalletID);

			OrgSupplierPart part = null;
			ZDecimal conversionFactor = 0m;

			if (string.IsNullOrEmpty(validationError))
			{
				(part, validationError) = GetProductByProductCode(receive, line);
			}

			if (string.IsNullOrEmpty(validationError) && line.Packs > 0 && line.PackUQ != part.OP_StockKeepingUnit)
			{
				(conversionFactor, validationError) = GetConversionFactor(line, part);
			}

			if (string.IsNullOrEmpty(validationError))
			{
				validationError = ValidateProductOverReceiving(receive, line, productSummary, part, conversionFactor);
			}

			if (string.IsNullOrEmpty(validationError))
			{
				var receiveLine = CreateWhsReceiveLine(warehouse, receive, line.ProductCode, line.Packs, line.PackUQ,
					line.Attribute1, line.Attribute2, line.Attribute3, line.SerialNumber, line.ExpiryDate, line.PackingDate,
					line.InventoryHeldCode, line.PalletID, 0, 0, line.Location, line.DockDoorLocationPK,
					productsWhichMayFulfillAsnLinesWithStockUnit, isEmptyAsnPalletIdMatchingEnabledForUnloadLine,
					part, conversionFactor, out validationError);
				if (receiveLine != null)
				{
					response.InventoryLinePK = receiveLine.PK.ToGuid();
				}
			}

			return validationError;
		}

		(ZDecimal ConversionFactor, string Error) GetConversionFactor(WhsLightDocketLineInfo line, OrgSupplierPart part)
		{
			var error = string.Empty;
			var conversionFactor = part.UnitConverter.ConversionFactor(line.PackUQ, part.OP_StockKeepingUnit);
			if (conversionFactor == 0m)
			{
				error = Res.GetString("71fa143e-d7d3-4a6f-b4a7-7605df0966b1", "Cannot convert '{0}' to '{1}' for the product '{2}'. Please add this conversion and try again.", line.PackUQ, part.OP_StockKeepingUnit, part.OP_PartNum);
			}

			return (conversionFactor, error);
		}

		(OrgSupplierPart Part, string Error) GetProductByProductCode(WhsReceive receive, WhsLightDocketLineInfo line)
		{
			OrgSupplierPart part = null;
			var error = string.Empty;
			if (!string.IsNullOrEmpty(line.ProductCode.Trim()))
			{
				part = WebServiceHelper.GetPartByPartNum(Factory, line.ProductCode, receive.WD_OH_Client);
			}

			if (part == null)
			{
				error = Res.GetString("ef2b414a-3ae0-43fa-86f9-aa7a6d617fe8", "Product {0} could not be found for client {1}.", line.ProductCode, receive.Client.OH_Code);
			}

			return (part, error);
		}

		string ValidateProductOverReceiving(WhsReceive receive, WhsLightDocketLineInfo line, Dictionary<ZGuid, WhsReceiveProductSummary> productSummary, OrgSupplierPart product, ZDecimal conversionFactor)
		{
			var error = string.Empty;
			var unloadQuantity = line.Packs;
			if (unloadQuantity > 0 && line.PackUQ != product.OP_StockKeepingUnit)
			{
				unloadQuantity = Utilities.Round(unloadQuantity * conversionFactor, product.OP_CountDecimalPlaces);
			}

			if (string.IsNullOrEmpty(error))
			{
				if (!productSummary.TryGetValue(product.PK, out var summary))
				{
					var isBlindReceive = !(receive.AsnLines.Count > 0);
					summary = new WhsReceiveProductSummary(Factory, receive.Client.PK, receive.WD_WW_Whs, product.PK, isBlindReceive, product.OP_PartNum, product.OP_Desc, receive.WD_ReceiveCategory, 0m, 0m);
					productSummary.Add(product.PK, summary);
				}

				summary.ReceivedQuantity += unloadQuantity;
				error = GetValidationError(new[] { summary });
				if (!string.IsNullOrEmpty(error))
				{
					summary.ReceivedQuantity -= unloadQuantity;
				}
			}

			return error;
		}

		readonly static object Lock = new object();

		WhsReceiveLine CreateWhsReceiveLine(WhsWarehouse warehouse, WhsReceive receive, string productCode, decimal packs, string packUQ, string attr1, string attr2, string attr3, string serial, DateTime expiryDate, DateTime packingDate,
			string heldCode, string palletID, int lineNo, int subLineNo, string directPutawayLocationString, Guid dockDoorLocationPK, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit, bool isEmptyAsnPalletIdMatchingEnabled, OrgSupplierPart part, ZDecimal? conversionFactor, out string validationError)
		{
			WhsLocation directPutawayLocation = null;
			if (!string.IsNullOrEmpty(directPutawayLocationString))
			{
				directPutawayLocation = WebServiceHelper.GetLocationByLocationString(Factory, warehouse, directPutawayLocationString);

				if (directPutawayLocation == null)
				{
					validationError = Res.GetString("490ea65b-46b5-40cf-abd0-83ca644c0532", "Location {0} could not be found for Warehouse {1}.", directPutawayLocationString, SecurityHeader.WarehouseCode);
					return null;
				}
			}

			var unloadedTime = ZDateTimeOffset.Now;
			if (!receive.DocketRelatedEntityOperationsStrategy.EventLogExists(ZArchitecture.Business.AutoEvents.WarehouseReceiptUnloadedCode))
			{
				receive.CreateUnloadTime(unloadedTime);
			}

			var (receiveLines, updateOrCreateError) = UpdateOrCreateReceiveLine(receive, productCode, packs, packUQ,
				attr1, attr2, attr3, serial, expiryDate, packingDate, unloadedTime.ToDateTime(), heldCode, palletID,
				directPutawayLocation, dockDoorLocationPK, lineNo, subLineNo, productsWhichMayFulfillAsnLinesWithStockUnit,
				isEmptyAsnPalletIdMatchingEnabled, part, conversionFactor);
			validationError = updateOrCreateError;

			if (string.IsNullOrEmpty(validationError))
			{
				// need to check if unique serial number
				receiveLines.ForEach(rl => rl.RunPreSaveValidation());
				InvalidReceiveLineDataForTest(receiveLines.First());

				validationError = GetValidationError(receiveLines);
				if (string.IsNullOrEmpty(validationError))
				{
					receiveLines.ForEach(receiveLine => receiveLine.Validation.ValidateWE_WL()); // currently we care only for location capacity check (to avoid trigger blow up)
					validationError = GetValidationError(receiveLines);
				}
			}

			return receiveLines.FirstOrDefault();
		}

		string GetValidationError(IEnumerable<BusinessObject> bizoList)
		{
			var validationErrorOnBizO = string.Empty;
			var notifications = bizoList.SelectMany(bizo => bizo.Notifications);
			var errorList = notifications.GetErrors().GetUniqueMessageList();
			if (errorList.Length > 0)
			{
				validationErrorOnBizO = errorList[0];
			}
			else
			{
				var messageErrorsList = notifications.GetMessageErrors().GetUniqueMessageList();
				if (messageErrorsList.Length > 0)
				{
					validationErrorOnBizO = messageErrorsList[0];
				}
			}

			return validationErrorOnBizO;
		}

		(IEnumerable<WhsReceiveLine> ReceiveLines, string Error) UpdateOrCreateReceiveLine(WhsReceive receive, string productCode, decimal packs, string packUQ, string attr1, string attr2, string attr3, string serial, DateTime expiryDate,
			DateTime packingDate, DateTime unloadedTime, string heldCode, string palletID, WhsLocation directPutawayLocation, Guid dockDoorLocationPK, int lineNo, int subLineNo, Guid[] productsWhichMayFulfillAsnLinesWithStockUnit,
			bool isEmptyAsnPalletIdMatchingEnabled, OrgSupplierPart part, ZDecimal? conversionFactor)
		{
			var error = string.Empty;
			var isDirectPutawayLocation = directPutawayLocation != null;
			var locationPK = isDirectPutawayLocation ? directPutawayLocation.PK : dockDoorLocationPK;
			var existingEligibleReceiveLines = new List<WhsReceiveLine>();

			if (packs == 0)
			{
				var newReceiveLine = CreateNewReceiveLine(receive, packs, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, unloadedTime, heldCode, palletID, locationPK, isDirectPutawayLocation, part);
				existingEligibleReceiveLines = new List<WhsReceiveLine> { newReceiveLine };
			}
			else
			{
				var remainingPacksToUnload = packs;

				existingEligibleReceiveLines = FindExistingEligibleReceiveLines(receive, productCode, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, locationPK, palletID, heldCode, lineNo, subLineNo, isEmptyAsnPalletIdMatchingEnabled).ToList();
				if (existingEligibleReceiveLines.Any())
				{
					var referenceDocketLine = existingEligibleReceiveLines.First();
					var unloadQty = referenceDocketLine.GetOrderedQuantityFromPackageQuantity(packs);
					remainingPacksToUnload = UpdateReceiveLinesAndGetRemainingQty(receive, unloadQty, packUQ, palletID, isEmptyAsnPalletIdMatchingEnabled, isDirectPutawayLocation, locationPK, existingEligibleReceiveLines);
				}

				if (remainingPacksToUnload > 0 && packUQ != part.OP_StockKeepingUnit && productsWhichMayFulfillAsnLinesWithStockUnit.Any(p => p == part.PK))
				{
					var existingEligibleReceiveLinesWithStockUnit = FindExistingEligibleReceiveLines(receive, productCode, part.OP_StockKeepingUnit, attr1, attr2, attr3, serial, expiryDate, packingDate, locationPK, palletID, heldCode, lineNo, subLineNo, isEmptyAsnPalletIdMatchingEnabled).ToList();
					if (existingEligibleReceiveLinesWithStockUnit.Any())
					{
						Argument.NotNull(conversionFactor, nameof(conversionFactor));
						var remainingPacksInStockUnit = Utilities.Round(remainingPacksToUnload * conversionFactor.Value, part.OP_CountDecimalPlaces);

						remainingPacksInStockUnit = UpdateReceiveLinesAndGetRemainingQty(receive, remainingPacksInStockUnit, packUQ, palletID, isEmptyAsnPalletIdMatchingEnabled, isDirectPutawayLocation, locationPK, existingEligibleReceiveLinesWithStockUnit);

						existingEligibleReceiveLines.AddRange(existingEligibleReceiveLinesWithStockUnit);
						remainingPacksToUnload = Utilities.Round(remainingPacksInStockUnit / conversionFactor.Value, part.OP_CountDecimalPlaces);
					}
				}

				if (remainingPacksToUnload > 0 && string.IsNullOrEmpty(error))
				{
					var newReceiveLine = CreateNewReceiveLine(receive, remainingPacksToUnload, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, unloadedTime, heldCode, palletID, locationPK, isDirectPutawayLocation, part);
					existingEligibleReceiveLines.Add(newReceiveLine);
				}
			}

			return (existingEligibleReceiveLines, error);
		}

		static WhsReceiveLine CreateNewReceiveLine(WhsReceive receive, decimal packs, string packUQ, string attr1, string attr2, string attr3, string serial, DateTime expiryDate, DateTime packingDate, DateTime unloadedTime, string heldCode, string palletID, ZGuid locationPK, bool isDirectPutawayLocation, OrgSupplierPart part)
		{
			var newReceiveLine = receive.Factory.New<WhsReceiveLine>();
			var inventory = newReceiveLine.Inventory[0];
			using (new SemaphoreManager(inventory.AddToDocketInventoryCollectionSemaphore))
			{
				inventory.WI_WD = receive.PK; // setup through inventory is more performan as it will setup WI_WD before WE_WD and therefore bypass some collection loading logic. A bit of hack, but to be addressed in the future.
			}
			newReceiveLine.WE_OP = part.PK;
			newReceiveLine.WE_F3_NKPackType = packUQ;
			newReceiveLine.WE_PartAttrib1 = attr1.ToUpper();
			newReceiveLine.WE_PartAttrib2 = attr2.ToUpper();
			newReceiveLine.WE_PartAttrib3 = attr3.ToUpper();
			newReceiveLine.WE_SerialNumber = serial.ToUpper();

			if (expiryDate == new DateTime())
			{
				newReceiveLine.WE_ExpiryDate = ZDate.Empty;
			}
			else
			{
				newReceiveLine.WE_ExpiryDate = new ZDate(expiryDate);
			}

			if (packingDate == new DateTime() || packingDate == DateTime.MinValue)
			{
				newReceiveLine.WE_PackingDate = ZDate.Empty;
			}
			else
			{
				newReceiveLine.WE_PackingDate = new ZDate(packingDate);
			}
			newReceiveLine.WE_PalletID = palletID.ToUpper();
			newReceiveLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
			newReceiveLine.WE_WL = locationPK;

			if (isDirectPutawayLocation)
			{
				WebServiceHelper.GenerateWarehouseConfirmedPutAwayEvent(newReceiveLine);
			}

			newReceiveLine.WE_UnloadedTime = unloadedTime;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			newReceiveLine.Logs.AddNew(ZArchitecture.Business.Events.AddedARecordToTheSystem, "RF");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			newReceiveLine.WE_PackQuantity += packs;

			return newReceiveLine;
		}

		ZDecimal UpdateReceiveLinesAndGetRemainingQty(WhsReceive receive, decimal unloadQty, string packUQ, string palletID, bool isEmptyAsnPalletIdMatchingEnabled, bool isDirectPutawayLocation, ZGuid locationPK, List<WhsReceiveLine> existingEligibleReceiveLines)
		{
			var groupedReceiveLines = GetGroupedReceiveLines(existingEligibleReceiveLines);

			var receiveLinesUpdater = GetReceiveLinesUpdater(false);
			var palletIDKey = palletID.ToUpperInvariant();
			if (groupedReceiveLines.TryGetValue((locationPK, palletIDKey), out var receiveLines))
			{
				unloadQty = receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(receive, receiveLines, unloadQty, packUQ, locationPK, palletID, isDirectPutawayLocation);
			}

			if (unloadQty > 0 && groupedReceiveLines.TryGetValue((ZGuid.Empty, palletIDKey), out receiveLines))
			{
				unloadQty = receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(receive, receiveLines, unloadQty, packUQ, locationPK, palletID, isDirectPutawayLocation);
			}

			if (unloadQty > 0 && isEmptyAsnPalletIdMatchingEnabled && groupedReceiveLines.TryGetValue((ZGuid.Empty, string.Empty), out receiveLines))
			{
				receiveLinesUpdater = GetReceiveLinesUpdater(true);
				var receiveLinesToUnloadTo = receiveLines.Where(rl => rl.WE_ClientOrderedUnits > 0 && rl.WE_TransactionQuantity == 0).ToList();
				if (receiveLinesToUnloadTo.Any())
				{
					unloadQty = receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(receive, receiveLinesToUnloadTo, unloadQty, packUQ, locationPK, palletID, isDirectPutawayLocation);
				}

				existingEligibleReceiveLines.AddRange(receiveLinesToUnloadTo.Except(receiveLines)); // to add the new receive lines from splits into the collection
			}

			var matchingReceiveLinesAfterUpdate = existingEligibleReceiveLines.Where(rl => rl.WE_PalletID.EqualsIgnoringCase(palletID) && rl.WE_F3_NKPackType.EqualsIgnoringCase(packUQ));
			if (unloadQty > 0 && matchingReceiveLinesAfterUpdate.Any())
			{
				UnloadExcessQtyToMatchingReceiveLines(isDirectPutawayLocation, locationPK, unloadQty, matchingReceiveLinesAfterUpdate);
				unloadQty = 0;
			}

			return unloadQty;
		}

		static void UnloadExcessQtyToMatchingReceiveLines(bool isDirectPutawayLocation, ZGuid locationPK, ZDecimal unloadQty, IEnumerable<WhsReceiveLine> matchingReceiveLines)
		{
			var receiveLinesToUpdateWithExcessQty = matchingReceiveLines.OrderBy(rl => rl.WE_LineNo).First();
			receiveLinesToUpdateWithExcessQty.WE_TransactionQuantity += unloadQty;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receiveLinesToUpdateWithExcessQty.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "RF");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			if (receiveLinesToUpdateWithExcessQty.WE_WL.IsEmpty)
			{
				receiveLinesToUpdateWithExcessQty.WE_WL = locationPK;
			}

			if (isDirectPutawayLocation)
			{
				WebServiceHelper.GenerateWarehouseConfirmedPutAwayEvent(receiveLinesToUpdateWithExcessQty);
			}
		}

		static Dictionary<(ZGuid, string), List<WhsReceiveLine>> GetGroupedReceiveLines(IEnumerable<WhsReceiveLine> existingReceiveLines)
		{
			var groupedReceiveLines = new Dictionary<(ZGuid, string), List<WhsReceiveLine>>();
			foreach (var receiveLine in existingReceiveLines)
			{
				var locationPK = receiveLine.WE_WL;
				var palletID = receiveLine.WE_PalletID.ToUpperInvariant();
				if (!groupedReceiveLines.TryGetValue((locationPK, palletID), out var receiveLines))
				{
					receiveLines = new List<WhsReceiveLine>();
					groupedReceiveLines.Add((locationPK, palletID), receiveLines);
				}
				receiveLines.Add(receiveLine);
			}

			return groupedReceiveLines;
		}

		static WhsReceiveLine[] FindExistingEligibleReceiveLines(WhsReceive receive, string productCode, string packUQ, string attr1, string attr2, string attr3, string serial, DateTime expiryDate, DateTime packingDate,
			ZGuid locationPK, string palletID, string heldCode, int lineNo, int subLineNo, bool isEmptyAsnPalletIdMatchingEnabled)
		{
			var query = new ZDBOnlyQuery(typeof(WhsReceiveLine));
			var receiveLinesWithEmptyLocationSubQuery = GetReceiveLinesSubQuery(receive.PK, productCode, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, palletID, heldCode, lineNo, subLineNo, isEmptyAsnPalletIdMatchingEnabled);
			receiveLinesWithEmptyLocationSubQuery.AddToFilter(WhsDocketLineSchema.WE_WL, DBNull.Value);

			var receiveLinesWithLocationSubQuery = GetReceiveLinesSubQuery(receive.PK, productCode, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, palletID, heldCode, lineNo, subLineNo, isEmptyAsnPalletIdMatchingEnabled);
			receiveLinesWithLocationSubQuery.AddToFilter(WhsDocketLineSchema.WE_WL, locationPK);

			receiveLinesWithEmptyLocationSubQuery.AddAsUnionQuery(receiveLinesWithLocationSubQuery, addAsUnionAll: true);
			query.AddSubQuery(receiveLinesWithEmptyLocationSubQuery, JoinCondition.And);

			return receive.Factory.Load<WhsReceiveLine>(query);
		}

		static ZDBOnlySubQuery GetReceiveLinesSubQuery(ZGuid receivePK, string productCode, string packUQ, string attr1, string attr2, string attr3, string serial, DateTime expiryDate, DateTime packingDate, string palletID, string heldCode, int lineNo, int subLineNo, bool isEmptyAsnPalletIdMatchingEnabled)
		{
			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
			productSubQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);

			var query = new ZDBOnlySubQuery(typeof(WhsReceiveLine), WhsDocketLineSchema.PK);
			query.AddToFilter(WhsDocketLineSchema.WE_WD, receivePK);
			query.AddToFilter(WhsDocketLineSchema.WE_F3_NKPackType, packUQ);
			query.AddToFilter(WhsDocketLineSchema.WE_PartAttrib1, attr1);
			query.AddToFilter(WhsDocketLineSchema.WE_PartAttrib2, attr2);
			query.AddToFilter(WhsDocketLineSchema.WE_PartAttrib3, attr3);
			query.AddToFilter(WhsDocketLineSchema.WE_SerialNumber, serial);

			if (isEmptyAsnPalletIdMatchingEnabled)
			{
				query.AddToFilter(WhsDocketLineSchema.WE_PalletID, new[] { palletID, string.Empty });
			}
			else
			{
				query.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletID);
			}

			if (expiryDate == DateTime.MinValue)
			{
				query.AddToFilter(WhsDocketLineSchema.WE_ExpiryDate, DBNull.Value);
			}
			else
			{
				query.AddToFilter(WhsDocketLineSchema.WE_ExpiryDate, expiryDate);
			}

			if (packingDate == DateTime.MinValue)
			{
				query.AddToFilter(WhsDocketLineSchema.WE_PackingDate, DBNull.Value);
			}
			else
			{
				query.AddToFilter(WhsDocketLineSchema.WE_PackingDate, packingDate);
			}

			query.AddToFilter(WhsDocketLineSchema.WE_WHC_NKOriginalInventoryHeldCode, heldCode);
			if (lineNo != 0)
			{
				query.AddToFilter(WhsDocketLineSchema.WE_LineNo, (short)lineNo);
				query.AddToFilter(WhsDocketLineSchema.WE_SubLineNo, (short)subLineNo);
			}

			query.AddSubQuery(productSubQuery, JoinCondition.And);
			query.ReLoadExistingRows = true;

			return query;
		}

		IReceiveLinesUpdater GetReceiveLinesUpdater(bool withEmptyPalletIdMatchingEnabled)
		{
			return withEmptyPalletIdMatchingEnabled
				? new ReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching()
				: new ReceiveLinesUpdaterForUnload();
		}

		partial void InvalidReceiveLineDataForTest(WhsReceiveLine receiveLine);

		#endregion
	}

#if DEBUG
	partial class WhsSecureService
	{
		partial void InvalidReceiveLineDataForTest(WhsReceiveLine receiveLine)
		{
			CreateInvalidReceiveLineDataForTest?.Invoke(receiveLine);
		}
		public Action<WhsReceiveLine> CreateInvalidReceiveLineDataForTest;
	}
#endif
}
