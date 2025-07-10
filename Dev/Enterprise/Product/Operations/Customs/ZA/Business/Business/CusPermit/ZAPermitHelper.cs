using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business
{
	internal static class ZAPermitHelper
	{
		public static Action<AutoCusPermitLineTransaction> GetLoggingAction(CusEntryHeader entryHeader, EDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger)
			=> (transaction => logger.Log(Res.GetString("C0E62678-FF1C-4999-873E-F3242CF3E053", "Permit Transactions added/updated for Permit {3} linked to Job {2} for Message: #{0}/{1}",
				incomingMessage.EM_MessageNum, outgoingMessage.Interchange?.EI_InterchangeNum, entryHeader.Declaration?.JE_DeclarationReference, (transaction as BaseCusPermitLineTransaction)?.PermitHeader?.HumanReadableName)));
		#region Static Methods

		public static void UpdatePendingTransactions(CusEntryHeader entryHeader, EDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger, ZString newStatus)
		{
			PermitHelper.UpdatePendingTransactions(incomingMessage,
														outgoingMessage,
														GetPermitAppIdForMessage,
														Core.Constants.CountryCodes.SouthAfrica,
														true,
														newStatus,
														GetLoggingAction(entryHeader, incomingMessage, outgoingMessage, logger));
		}

		#region Rollback Permit Transactions

		public static void RollbackPermitTransactionsForMessage(CusEntryHeader entryHeader, EDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger, string status)
		{
			PermitHelper.RollbackPermitTransactions(incomingMessage,
												outgoingMessage,
												GetLoggingAction(entryHeader, incomingMessage, outgoingMessage, logger),
												GetPermitAppIdForMessage,
												ZString.Empty,
												Core.Constants.CountryCodes.SouthAfrica,
												true,
												status);
		}

		public static void RollbackPermitTransactionsForEntry(CusEntryHeader entryHeader, EDIMessage incomingMessage, EDIMessage outgoingMessage, LoggingInformation logger, string status)
		{
			PermitHelper.RollbackPermitTransactions(incomingMessage,
												outgoingMessage,
												GetLoggingAction(entryHeader, incomingMessage, outgoingMessage, logger),
												GetPermitAppIdForMessage,
												ZString.Empty,
												Core.Constants.CountryCodes.SouthAfrica,
												false,
												status);
		}

		#endregion

		#region Reference Formatting

		public static ZInt GetPermitReferenceLineNumberForEntry(CusEntryHeader entryHeader) => entryHeader?.CH_EntryNumber ?? 0;

		public static ZString GetPermitAppIdForMessage(EDIMessage message)
		{
			var result = ZString.Empty;

			switch (message?.EM_MessageType ?? ZString.Empty)
			{
				case SARSEDIMessage.MessageTypes.CUSDEC:
					result = message.EM_MessageNum; // Equal to RFF+ACD for CUSDEC
					break;
				case SARSEDIMessage.MessageTypes.CUSRES:
					var cusresHelper = CUSRESMessageHelper.New((CUSRESEDIMessage)message);
					result = cusresHelper?.OutgoingMessageNumber ?? ZString.Empty;
					break;
				case SARSEDIMessage.MessageTypes.CONTRL:
					var contrlHelper = CONTRLMessageHelper.New((CONTRLEDIMessage)message);
					result = contrlHelper?.MessageReferenceNumber ?? ZString.Empty;
					break;
			}

			return result;
		}

		#endregion

		#region Get Permit Records

		public static IList<PermitRecord> GetPermitRecords(CusEntryHeader entryHeader, IEnumerable<ILineLevelInformation> lines)
		{
			IList<PermitRecord> result = null;
			if (entryHeader != null)
			{
				var permitRecords = new Dictionary<string, PermitRecord>();
				if (lines != null)
				{
					foreach (var lineInfo in lines)
					{
						var addInfos = lineInfo.AdditionalInformations;
						AddRebatePermits(entryHeader, addInfos, permitRecords, UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, (ZString a) => a);
						AddRebatePermits(entryHeader, addInfos, permitRecords, UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate, UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue, (ZString a) => a.SubstringSafe(3));
						AddImportExportPermit(entryHeader, addInfos, permitRecords, lineInfo);
					}
				}
				result = permitRecords.Values.ToList();
				PermitHelper.AddPermitRecordsToCancel(entryHeader.Factory, Core.Constants.CountryCodes.SouthAfrica, PermitHelper.GetPermitReferenceForEntry(entryHeader), PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader), result);
			}
			return result;
		}

		public static IList<PermitRecord> GetPermitRecords(CusEntryHeader entryHeader)
		{
			return GetPermitRecords(entryHeader, entryHeader?.MergedLines?.OfType<ILineLevelInformation>());
		}

		public static IList<PermitRecord> GetPermitRecords(CUSDECEDIMessage cusdecMessage)
		{
			var cusdecHelper = MessageProcessor.CUSDECMessageHelper.New(cusdecMessage);
			var entryHeader = cusdecMessage?.EM_LinkedObject as CusEntryHeader;
			return GetPermitRecords(entryHeader, cusdecHelper?.LineLevelInformations?.OfType<ILineLevelInformation>());
		}

		static void AddRebatePermits(CusEntryHeader entryHeader, IEnumerable<IAdditionalInformation> addInfos, Dictionary<string, PermitRecord> permitRecords, ZString certificateCode, ZString valueCode, Func<ZString, ZString> getCertificateNumberAction)
		{
			var rebateCertificates = addInfos.OfType<IAdditionalInformation>().Where(x => x.Code == certificateCode);
			if (rebateCertificates.Any())
			{
				var rebateValues = addInfos.OfType<IAdditionalInformation>().Where(x => x.Code == valueCode);
				var index = 0;
				foreach (var rebateCertificate in rebateCertificates)
				{
					var rebateValue = rebateValues.ElementAtOrDefault(index);
					if (rebateValue != null)
					{
						var certificateNum = getCertificateNumberAction(rebateCertificate.Value);
						var permitHeader = LoadCusPermitHeader(entryHeader, certificateNum);
						var value = ZDecimal.ParseSafe(rebateValue.Value, ZDecimal.Zero);
						value = (permitHeader?.CPH_Type ?? ZString.Empty) == PermitTypeList.Codes.PRC && value != 0m ? (ZDecimal)(value / 100) : value;
						if (!certificateNum.IsEmpty && !value.IsEmpty)
						{
							if (permitRecords.ContainsKey(certificateNum))
							{
								permitRecords[certificateNum].Value += value;
							}
							else
							{
								permitRecords.Add(certificateNum, new PermitRecord()
								{
									PermitHeader = permitHeader,
									Value = value,
									Quantity = ZDecimal.Zero
								});
							}
						}
					}
					index++;
				}
			}
		}

		static void AddImportExportPermit(CusEntryHeader entryHeader, IEnumerable<IAdditionalInformation> addInfos, Dictionary<string, PermitRecord> permitRecords, ILineLevelInformation lineInfo)
		{
			var customsValue = lineInfo.CustomsValue;

			var addInfoCode = ZString.Empty;
			var permitType = ZString.Empty;

			if (entryHeader.IsImport)
			{
				addInfoCode = UniversalReferenceConstants.AdditionalInformation.ImportPermitControl;
				permitType = PermitTypeList.Codes.IMP;
			}
			else
			{
				addInfoCode = UniversalReferenceConstants.AdditionalInformation.ExportPermitControl;
				permitType = PermitTypeList.Codes.EXP;
			}

			var addInfo = addInfos.FirstOrDefault(x => x.Code == addInfoCode);
			if (addInfo != null)
			{
				var permitNumber = addInfo.Value;
				if (permitRecords.ContainsKey(permitNumber))
				{
					var permitRecord = permitRecords[permitNumber];
					permitRecord.Value += customsValue;
					permitRecord.Quantity += GetCustomsQuantity(permitNumber, permitRecord, lineInfo);
				}
				else
				{
					var permitRecord = new PermitRecord()
					{
						PermitHeader = LoadCusPermitHeader(entryHeader, permitNumber, permitType),
						Value = customsValue
					};
					permitRecord.Quantity = GetCustomsQuantity(permitNumber, permitRecord, lineInfo);
					permitRecords.Add(permitNumber, permitRecord);
				}
			}
		}

		static ZDecimal GetCustomsQuantity(ZString permitNumber, PermitRecord permitRecord, ILineLevelInformation lineInfo)
		{
			var result = ZDecimal.Zero;
			var unitOfMeasure = permitRecord.PermitHeader?.CPH_UnitOfMeasure ?? ZString.Empty;
			if (!unitOfMeasure.IsEmpty)
			{
				if (unitOfMeasure == lineInfo.CustomsUnitQty)
				{
					result = lineInfo.CustomsQuantity;
				}
				else if (unitOfMeasure == lineInfo.AdditionalUnitQty)
				{
					result = lineInfo.AdditionalQuantity;
				}
				else if (unitOfMeasure == lineInfo.ClassificationUnitQty)
				{
					result = lineInfo.ClassificationQuantity;
				}
				else
				{
					permitRecord.ErrorMessages.Add(UnitOfMeasureNotValidForTariff(permitNumber, unitOfMeasure, lineInfo.TariffCode));
				}
			}
			else if (permitRecord.PermitHeader?.IsQTY ?? false)
			{
				permitRecord.ErrorMessages.Add(UnitOfMeasureNotSetupOnPermit(permitNumber));
			}
			return result;
		}

		internal static string UnitOfMeasureNotValidForTariff(ZString permitNumber, ZString unitOfMeasure, ZString tariffCode)
		{
			return Res.GetString("C4ED0F16-139E-4450-82F6-8E31D192DD13", "The Unit of Measure '{1}' setup against Permit '{0}' is not valid for tariff '{2}'.", permitNumber, unitOfMeasure, tariffCode);
		}

		internal static string UnitOfMeasureNotSetupOnPermit(ZString permitNumber)
		{
			return Res.GetString("A34B3F92-B33E-4AC8-9941-4B7DC2088D4A", "No Unit of Measure has been setup against Permit '{0}'.", permitNumber);
		}

		#endregion

		static BaseCusPermitHeader LoadCusPermitHeader(CusEntryHeader entryHeader, ZString permitNumber, string permitType = "")
		{
			ZGuid permitHolder;
			if (permitType == PermitTypeList.Codes.EXP)
			{
				permitHolder = entryHeader.Declaration.Supplier.PK;
			}
			else
			{
				permitHolder = entryHeader.Declaration.Importer.PK;
			}
			return new CusPermitHeader.Loader(entryHeader.Factory).Load(Core.Constants.CountryCodes.SouthAfrica, permitNumber, permitHolder, entryHeader.EntryInstructionAssessmentDate, "", permitType, "");
		}

		#endregion
	}
}
