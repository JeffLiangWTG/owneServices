using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ExportDeclarationDataUpdaterProcessor : BatchProcess
	{
		public ExportDeclarationDataUpdaterProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected sealed override void Execute(CancellationToken canellationToken)
		{
			var exportDeclarationPKs = GetExportDeclarationPKsToProcess();
			foreach (var declarationPK in exportDeclarationPKs)
			{
				canellationToken.ThrowIfCancellationRequested();
				UpdateExportDeclarationAndSave(declarationPK);
			}
		}

		ZGuid[] GetExportDeclarationPKsToProcess()
		{
			var dateRange = ZDateTime.Today.AddDays(-7);
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "Export Declaration Data Update" // Factory Name For Debugging
			};
			var sqlText = new ZString($@"
			SELECT {JobDeclarationSchema.Constants.PK}
			FROM {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName}
			INNER JOIN {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} ON {JobDeclarationSchema.Constants.PK} = {CusEntryHeaderSchema.Constants.CH_JE} AND {CusEntryHeaderSchema.Constants.CH_MessageType} = @EntryMessageType
			INNER JOIN {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName} ON {CusEntryHeaderSchema.Constants.PK} = {CusEntryNumSchema.Constants.CE_ParentID} AND {CusEntryNumSchema.Constants.CE_Category} = @Category AND {CusEntryNumSchema.Constants.CE_EntryType} = @EntryType AND {CusEntryNumSchema.Constants.CE_RN_NKCountryCode} = @USCountryCode
			INNER JOIN {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName} ON {JobDeclarationSchema.Constants.JE_JS} = {JobShipmentSchema.Constants.PK}
			INNER JOIN {JobConShipLinkSchema.Constants.SqlSchemaName}.{JobConShipLinkSchema.Constants.TableName} ON {JobShipmentSchema.Constants.PK} = {JobConShipLinkSchema.Constants.JN_JS}
			INNER JOIN {JobConsolSchema.Constants.SqlSchemaName}.{JobConsolSchema.Constants.TableName} ON {JobConShipLinkSchema.Constants.JN_JK} = {JobConsolSchema.Constants.PK}
			INNER JOIN {JobConsolTransportSchema.Constants.SqlSchemaName}.{JobConsolTransportSchema.Constants.TableName} ON {JobConsolSchema.Constants.PK} = {JobConsolTransportSchema.Constants.JW_ParentGUID}
			INNER JOIN {OrgAddressSchema.Constants.SqlSchemaName}.{OrgAddressSchema.Constants.TableName} ON {JobConsolTransportSchema.Constants.JW_OA_CarrierAddress} = {OrgAddressSchema.Constants.PK}
			LEFT JOIN {GenAddOnColumnSchema.Constants.SqlSchemaName}.{GenAddOnColumnSchema.Constants.TableName} ON {GenAddOnColumnSchema.Constants.XA_ParentID} = {JobDeclarationSchema.Constants.PK} AND {GenAddOnColumnSchema.Constants.XA_Name} = @ExportDeclarationDataUpdateDateSchemaName
			WHERE {JobDeclarationSchema.Constants.JE_GC} = @CompanyPK
			AND {JobDeclarationSchema.Constants.JE_MessageType} = @MessageType
			AND {JobDeclarationSchema.Constants.JE_TransportMode} = @TransportMode
			AND {JobDeclarationSchema.Constants.JE_IsCancelled} = 0
			AND {CusEntryHeaderSchema.Constants.CH_Status} NOT IN (@AwaitingReplacementResponseCode,@AwaitingDeleteResponseCode)
			AND {CusEntryNumSchema.Constants.CE_EntryNum} <> ''
			AND {JobConsolSchema.Constants.JK_TransportMode} = @TransportMode
			AND
			(
				(SUBSTRING({JobConsolSchema.Constants.JK_RL_NKLoadPort}, 1, 2) = @USCountryCode AND SUBSTRING({JobConsolSchema.Constants.JK_RL_NKDischargePort}, 1, 2) <> @USCountryCode)
				OR
				(SUBSTRING({JobConsolSchema.Constants.JK_RL_NKLoadPort}, 1, 2) = @PRCountryCode AND SUBSTRING({JobConsolSchema.Constants.JK_RL_NKDischargePort}, 1, 2) <> @PRCountryCode)
			)
			AND
			(
				({JobConsolTransportSchema.Constants.JW_ETD} IS NOT NULL AND {JobConsolTransportSchema.Constants.JW_ETD} > @DateRange)
				OR
				({JobConsolTransportSchema.Constants.JW_ATD} IS NOT NULL AND {JobConsolTransportSchema.Constants.JW_ATD} > @DateRange)
			)
			AND
			(
				({GenAddOnColumnSchema.Constants.XA_Data} IS NULL)
				OR
				(CONVERT(SMALLDATETIME, {GenAddOnColumnSchema.Constants.XA_Data}) < (CASE WHEN {JobConsolSchema.Constants.JK_SystemLastEditTimeUtc} > {JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc} THEN {JobConsolSchema.Constants.JK_SystemLastEditTimeUtc} ELSE {JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc} END))
			)
			GROUP BY {JobDeclarationSchema.Constants.PK}");

			var sqlFilterParams = new ZSqlParameterCollection();
			sqlFilterParams.Add("@EntryMessageType", CusEntryHeaderMessageTypeList.Codes.Export, CusEntryHeaderSchema.CH_MessageType);
			sqlFilterParams.Add("@AwaitingReplacementResponseCode", AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse, CusEntryHeaderSchema.CH_Status);
			sqlFilterParams.Add("@AwaitingDeleteResponseCode", AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse, CusEntryHeaderSchema.CH_Status);
			sqlFilterParams.Add("@Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, CusEntryNumSchema.CE_Category);
			sqlFilterParams.Add("@EntryType", CusEntryNumberTypeList.Codes.ITN, CusEntryNumSchema.CE_EntryType);
			sqlFilterParams.Add("@USCountryCode", Core.Constants.CountryCodes.UnitedStates, CusEntryNumSchema.CE_RN_NKCountryCode);
			sqlFilterParams.Add("@PRCountryCode", Core.Constants.CountryCodes.PuertoRico, CusEntryNumSchema.CE_RN_NKCountryCode);
			sqlFilterParams.Add("@ExportDeclarationDataUpdateDateSchemaName", ExportDeclarationDataUpdateDateSchemaName, GenAddOnColumnSchema.XA_Name);
			sqlFilterParams.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, JobDeclarationSchema.JE_GC);
			sqlFilterParams.Add("@MessageType", JobMessageTypeList.Codes.Export, JobDeclarationSchema.JE_MessageType);
			sqlFilterParams.Add("@TransportMode", TransportTypeList.Codes.Air, JobDeclarationSchema.JE_TransportMode);
			sqlFilterParams.Add("@DateRange", dateRange, JobConsolTransportSchema.JW_ETD);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sqlText, sqlFilterParams);
			return collection.Select(x => (ZGuid)x[JobDeclarationSchema.Constants.PK]).ToArray();
		}

		void UpdateExportDeclarationAndSave(ZGuid declarationPK)
		{
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory()
				{
					RefreshEnabled = false,
					NameForDebugging = $"Export Declaration Data Update Processor" // Factory Name For Debugging
				};

				var declaration = factory.Load<JobDeclaration>(declarationPK);
				if (declaration != null)
				{
					var consol = declaration.RelevantConsol;
					if (consol != null)
					{
						var jobReference = ZString.Empty;
						jobReference = declaration.JE_DeclarationReference;
						Logger.Log($"System is synchronizing data from consol for Declaration {jobReference}.");

						var referenceChanges = new Dictionary<string, string>();
						var billNumberFromConsol = GetMasterBillNumber(consol);
						var masterBill = declaration.JE_MasterBill;
						if (masterBill != billNumberFromConsol)
						{
							declaration.JE_MasterBill = billNumberFromConsol;
							Logger.Log($"JE_MasterBill has been updated from {masterBill} to {billNumberFromConsol}.");
							const string MasterBillReferenceType = "MasterBill"; 
							AddReferenceChange(referenceChanges, MasterBillReferenceType, masterBill, billNumberFromConsol);
						}

						var mostInterestingInboundLeg = declaration.MostInterestingLegProvider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
						var mostInterestingOutboundLeg = declaration.MostInterestingLegProvider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
						var portOfLoadingFromConsol = GetPortOfLoading(declaration, mostInterestingInboundLeg);
						if (portOfLoadingFromConsol.HasValue)
						{
							var portOfLoading = declaration.JE_RL_NKPortOfLoading;
							if (portOfLoadingFromConsol.Value != portOfLoading)
							{
								declaration.JE_RL_NKPortOfLoading = portOfLoadingFromConsol.Value;
								Logger.Log($"JE_RL_NKPortOfLoading has been updated from {portOfLoading} to {portOfLoadingFromConsol.Value}.");
								const string PortOfLoadingReferenceType = "PortOfLoading";
								AddReferenceChange(referenceChanges, PortOfLoadingReferenceType, portOfLoading, portOfLoadingFromConsol.Value);
							}
						}

						var portOfExportFromConsol = consol.JK_RL_NKLoadForExportTransport;
						var portOfExport = declaration.US_RL_NKPortOfExport;
						if (portOfExportFromConsol != portOfExport)
						{
							declaration.US_RL_NKPortOfExport = portOfExportFromConsol;
							Logger.Log($"US_RL_NKPortOfExport has been updated from {portOfExport} to {portOfExportFromConsol}.");
							const string PortOfExportReferenceType = "PortOfExport";
							AddReferenceChange(referenceChanges, PortOfExportReferenceType, portOfExport, portOfExportFromConsol);
						}

						var exportDateFromConsol = GetExportDate(declaration, mostInterestingOutboundLeg);
						if (exportDateFromConsol.HasValue)
						{
							var dateOfExport = declaration.US_DateOfExport;
							if (exportDateFromConsol.Value.Date != dateOfExport.Date)
							{
								declaration.US_DateOfExport = exportDateFromConsol.Value;
								Logger.Log($"US_DateOfExport has been updated from {dateOfExport} to {exportDateFromConsol.Value}.");
								const string DateOfExportReferenceType = "DateOfExport";
								AddReferenceChange(referenceChanges, DateOfExportReferenceType, dateOfExport, exportDateFromConsol.Value);
							}
						}

						if (declaration.IsAir)
						{
							var voyageFlightNo = GetVoyageFlightNo(declaration, mostInterestingOutboundLeg);
							if (voyageFlightNo.HasValue)
							{
								var flightNo = declaration.JE_VoyageFlightNo;
								if (voyageFlightNo.Value.Left(2) != flightNo.Left(2))
								{
									declaration.JE_VoyageFlightNo = voyageFlightNo.Value;
									Logger.Log($"JE_VoyageFlightNo has been updated from {flightNo} to {voyageFlightNo.Value}.");
									const string FlightNoReferenceType = "FlightNo";
									AddReferenceChange(referenceChanges, FlightNoReferenceType, flightNo, voyageFlightNo.Value);
								}
							}
						}

						var carrierPK = GetCarrierPK(declaration, mostInterestingOutboundLeg, consol);
						if (carrierPK.HasValue)
						{
							var shippingLine = declaration.ShippingLine;
							if (carrierPK.Value != declaration.JE_OH_ShippingLine)
							{
								declaration.JE_OH_ShippingLine = carrierPK.Value;
								Logger.Log($"JE_OH_ShippingLine has been updated from {declaration.JE_OH_ShippingLineInfo.OriginalValue} to {carrierPK.Value}.");
								const string CarrierReferenceType = "Carrier";
								var newShippingLine = declaration.ShippingLine;
								AddReferenceChange(referenceChanges, CarrierReferenceType, shippingLine?.OH_Code, newShippingLine?.OH_Code);
							}
						}

						if (declaration.HasChanges)
						{
							declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().ForEach(x =>
							{
								if (x.IsExport && x.HasActiveTransactionsWithCustoms && !x.IsCurrentlyWithdrawn)
								{
									x.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired;
									x.US_ShouldBeReportToCustoms = true;
								}
							});

							declaration.DeriveDeclarationStatus();
							declaration.Logs.AddNew(Events.CustomsEntryStatus, AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, referenceChanges.ToArray());
						}

						var exportDeclarationDataUpdateDate = declaration.GetSystemDefinedValue<ZDateTime>(ExportDeclarationDataUpdateDateSchemaName);
						var maxLastEditTime = consol.JK_SystemLastEditTimeUtc > declaration.JE_SystemLastEditTimeUtc ? consol.JK_SystemLastEditTimeUtc : declaration.JE_SystemLastEditTimeUtc;
						if (declaration.HasChanges || !exportDeclarationDataUpdateDate.Equals(maxLastEditTime))
						{
							declaration.SetSystemDefinedValue(ExportDeclarationDataUpdateDateSchemaName, maxLastEditTime);
							factory.Save();

							Logger.Log($"Synchronise data from consol for Declaration {jobReference} has completed and saved successfully.");
						}
					}
				}
			}, () => Logger.LogWarning("An error occurred while synchronizing data from consol, retrying..."));
		}

		void AddReferenceChange(Dictionary<string, string> referenceChanges, string referenceType, IZType oldValue, IZType newValue)
		{
			referenceChanges.Add(referenceType, $"{oldValue}->{newValue}");
		}

		ZString GetMasterBillNumber(ForwardingConsol consol)
		{
			return consol.JK_MasterBillNum.KeepValidBillNumberCharacters();
		}

		ZString? GetPortOfLoading(JobDeclaration declaration, IMovementLeg mostInterestingInboundLeg)
		{
			if (mostInterestingInboundLeg != null)
			{
				return mostInterestingInboundLeg.Load;
			}
			else if (declaration.Shipment?.Consols.Count == 0 && declaration.Supplier is OrgHeader supplier)
			{
				return supplier.OH_RL_NKClosestPort;
			}

			return null;
		}

		ZDateTime? GetExportDate(JobDeclaration declaration, IMovementLeg mostInterestingOutboundLeg)
		{
			var result = ZDateTime.Empty;

			var consol = declaration.RelevantConsol;
			if (consol != null)
			{
				var transport = consol.Transports.ExportTransport;
				if (transport != null)
				{
					if (!transport.JW_ATD.IsEmpty)
					{
						result = transport.JW_ATD;
					}
					else if (!transport.JW_ETD.IsEmpty)
					{
						result = transport.JW_ETD;
					}
				}
			}

			if (result.IsEmpty)
			{
				if (mostInterestingOutboundLeg is Transport transport)
				{
					if (!transport.JW_ATD.IsEmpty)
					{
						result = transport.JW_ATD;
					}
					else if (!transport.JW_ETD.IsEmpty)
					{
						result = transport.JW_ETD;
					}
				}

				if (declaration.Shipment is ForwardingShipment shipment)
				{
					if (!shipment.JS_ShippedOnBoardDate.IsEmpty)
					{
						result = shipment.JS_ShippedOnBoardDate;
					}
					else
					{
						result = shipment.JS_E_DEP;
					}
				}
			}

			return result;
		}

		ZString? GetVoyageFlightNo(JobDeclaration declaration, IMovementLeg mostInterestingOutboundLeg)
		{
			if (mostInterestingOutboundLeg is Transport transport)
			{
				return transport.JW_VoyageFlight;
			}

			return null;
		}

		ZGuid? GetCarrierPK(JobDeclaration declaration, IMovementLeg mostInterestingOutboundLeg, ForwardingConsol consol)
		{
			if (mostInterestingOutboundLeg is Transport transport && !transport.CarrierPK.IsEmpty)
			{
				return transport.CarrierPK;
			}
			else
			{
				return consol.ShippingLinePK;
			}
		}

		const string ExportDeclarationDataUpdateDateSchemaName = "ExportDeclarationDataUpdateDate";
	}
}
