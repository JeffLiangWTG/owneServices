using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	class TWIncomingMessageKeyInfomation : MessageKeyInfomation
	{
		public TWIncomingMessageKeyInfomation(string type, ZString xml) : base(type, xml)
		{
		}

		#region Create

		protected override void InitData()
		{
			GoodsShipmentNameCode = new List<ZString>();
			GoodsShipmentValidationCode = new List<ZString>();
			StatementCode = new List<ZString>();
		}

		protected override ImmutableDictionary<string, Action<ZString>> CreateDataGeneratorMap()
		{
			return ImmutableDictionary.CreateRange(new Dictionary<string, Action<ZString>>
					{
						{ MessageTypeList.Codes.ARM, CreateByNX5106 },
						{ MessageTypeList.Codes.ERM, CreateByN5204 },
						{ MessageTypeList.Codes.FHR, CreateByN5108 },
						{ MessageTypeList.Codes.IEM, CreateByN5109 },
						{ MessageTypeList.Codes.IRM, CreateByN5116 },
						{ MessageTypeList.Codes.RFM, CreateByN5107 },
						{ MessageTypeList.Codes.UHC, CreateByN5168 },
						{ MessageTypeList.Codes.TPC, CreateByN5110 },
						{ MessageTypeList.Codes.TAD, CreateByN5111 },
						{ MessageTypeList.Codes.TRN, CreateByN5302 },
						{ MessageTypeList.Codes.TRA, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.ECD, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.ICD, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.ADM, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.IEA, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.FCF, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes.FHM, CreateByTWCustomsDeliveryNotification },
						{ MessageTypeList.Codes._101, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._201, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._207, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._301, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._31A, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._31D, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._401, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._601, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._603, CreateByTWControllingDeliveryNotification },
						{ MessageTypeList.Codes._102, CreateByNX102 },
						{ MessageTypeList.Codes._202, CreateByNX202 },
						{ MessageTypeList.Codes._302, CreateByNX302 },
						{ MessageTypeList.Codes._32A, CreateByNX302_AX },
						{ MessageTypeList.Codes._32D, CreateByNX302_DN },
						{ MessageTypeList.Codes._402, CreateByNX402 },
						{ MessageTypeList.Codes._602, CreateByNX602 },
						{ MessageTypeList.Codes._901, CreateByNX901 },
						{ MessageTypeList.Codes._902, CreateByNX902 },
						{ MessageTypeList.Codes._903, CreateByNX903 },
					});
		}

		void CreateByN5111(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5111.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByN5110(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5110.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
				DutyDueDateForTPC = new ZDateTime(result?.Declaration?.DutyTaxFee?.Payment?.DueDateTime);
			}
		}

		void CreateByN5107(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5107.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				GovernmentProcedure = result?.Declaration?.GovernmentProcedure?.TwTransportTypeCode?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
				BorderTransportMeans = result?.Declaration?.BorderTransportMeans?.TypeCode?.Value;
				result?.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.ForEach(goodsItem =>
				{
					goodsItem.Error?.ForEach(error =>
					{
						var validationCode = error?.ValidationCode?.Value;
						if (!string.IsNullOrEmpty(validationCode))
						{
							GoodsShipmentValidationCode.Add(validationCode);
						}
					});
				});
			}
		}

		void CreateByN5116(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5116.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
				ReleaseDateTime = new ZDateTime(result?.Status?.ReleaseDateTime);
				result?.AdditionalInformation?.ForEach(additionalInformation => AddStatementCode(additionalInformation.StatementCode?.Value ?? string.Empty));
			}
		}

		void CreateByN5109(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5109.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				GovernmentProcedure = result?.Declaration?.GovernmentProcedure?.TwTransportTypeCode?.Value;
			}
		}

		void CreateByN5204(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5204.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
				BorderTransportMeans = result?.Declaration?.BorderTransportMeans?.TypeCode?.Value;
				ReleaseDateTime = new ZDateTime(result?.Status?.ReleaseDateTime);
				result?.AdditionalInformation?.ForEach(additionalInformation => AddStatementCode(additionalInformation.StatementCode?.Value ?? string.Empty));
			}
		}

		void AddStatementCode(ZString statementCode)
		{
			if (!statementCode.IsEmpty)
			{
				StatementCode.Add(statementCode);
			}
		}

		void CreateByNX5106(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX5106.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				GovernmentProcedure = result?.Declaration?.GovernmentProcedure?.TwTransportTypeCode?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
				BorderTransportMeans = result?.Declaration?.BorderTransportMeans?.TypeCode?.Value;
				result?.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.ForEach(goodsItem =>
				{
					goodsItem.Status?.ForEach(status =>
					{
						var nameCode = status?.NameCode?.Value;
						if (!string.IsNullOrEmpty(nameCode))
						{
							GoodsShipmentNameCode.Add(nameCode);
						}
					});
				});
			}
		}

		void CreateByN5108(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5108.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.Consignment?.ConsignmentItem?.PreviousDocument?.TwFunctionalReferenceId?.Value;
			}
		}

		void CreateByN5168(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5168.Declaration>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Id?.Value;
				GovernmentProcedure = result?.GovernmentProcedure?.TwTransportTypeCode?.Value;
			}
		}

		void CreateByN5302(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.N5302.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				DeclarationID = result?.Declaration?.Id?.Value;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX102(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX102.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX202(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX202.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX302(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX302.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX302_AX(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX302_AX.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX302_DN(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX302_DN.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX402(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX402.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX602(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX602.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				StatusNameCode = result?.Status?.NameCode?.Value;
			}
		}

		void CreateByNX901(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX901.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
			}
		}

		void CreateByNX902(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX902.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				Result = result;
				FunctionalReferenceID = result?.Declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
			}
		}

		void CreateByNX903(ZString xml)
		{
			var reader = new SafeXmlReader<CargoWise.Customs.TW.MessageDefinitions.NX903.Response>(this);
			if (reader.TryReadFromXML(xml))
			{
				var result = reader.Result;
				FunctionalReferenceID = result?.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty;
				DeclarationID = result?.Declaration?.Id?.Value ?? ZString.Empty;
				Result = result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xml element name")]
		void CreateByTWCustomsDeliveryNotification(ZString xml)
		{
			try
			{
				var doc = XDocument.Parse(xml, LoadOptions.None);
				EventType = doc.XPathSelectElement((NoResString)"//*[local-name()='Event']/*[local-name()='EventType']")?.Value;
				fEntryType = doc.XPathSelectElement((NoResString)"//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumberType']/*[local-name()='Value']")?.Value;
				DeclarationID = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumber']/*[local-name()='Value']")?.Value;
				InterchangeNumber = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='InterchangeNumber']/*[local-name()='Value']")?.Value;
				Result = doc;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorText = ex.Message;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml strings")]
		void CreateByTWControllingDeliveryNotification(ZString xml)
		{
			try
			{
				var doc = XDocument.Parse(xml, LoadOptions.None);
				EventType = doc.XPathSelectElement("//*[local-name()='Event']/*[local-name()='EventType']")?.Value;
				fEntryType = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumberType']/*[local-name()='Value']")?.Value;
				FunctionalReferenceID = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='EntryNumber']/*[local-name()='Value']")?.Value;
				InterchangeNumber = doc.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context'][./*[local-name()='Type']='InterchangeNumber']/*[local-name()='Value']")?.Value;
				Result = doc;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorText = ex.Message;
			}
		}

		#endregion

		public bool IsTranshipment
		{
			get
			{
				switch (MessageType)
				{
					case MessageTypeList.Codes.TRN:
						return true;
					case MessageTypeList.Codes.IEM:
					case MessageTypeList.Codes.ARM:
					case MessageTypeList.Codes.RFM:
					case MessageTypeList.Codes.UHC:
						return GovernmentProcedure == TransportTypeCodes.Transfer;
					default:
						return false;
				}
			}
		}

		public ZString EntryStatus
		{
			get
			{
				var result = ZString.Empty;
				switch (MessageType)
				{
					case MessageTypeList.Codes.ARM:
					case MessageTypeList.Codes.ERM:
					case MessageTypeList.Codes.IRM:
					case MessageTypeList.Codes.RFM:
					case MessageTypeList.Codes.IEM:
					case MessageTypeList.Codes.UHC:
					case MessageTypeList.Codes.TAD:
					case MessageTypeList.Codes.TPC:
						result = MessageType;
						break;
				}
				return result;
			}
		}

		public ZString ClearanceStatus
		{
			get
			{
				var result = ZString.Empty;
				switch (MessageType)
				{
					case MessageTypeList.Codes.ERM:
					case MessageTypeList.Codes.IRM:
						result = StatusNameCode;
						break;
				}
				return result;
			}
		}

		#region EntryType
		public ZString EntryType
		{
			get
			{
				var result = fEntryType;
				if (result.IsEmpty)
				{
					var messageType = MessageType;
					if (EntryTypeMap.ContainsKey(messageType))
					{
						result = EntryTypeMap[messageType].Invoke();
					}
				}
				return result;
			}
		}
		ZString fEntryType;

		ImmutableDictionary<string, Func<ZString>> EntryTypeMap
		{
			get
			{
				if (entryTypeMap == null)
				{
					entryTypeMap = ImmutableDictionary.CreateRange(new Dictionary<string, Func<ZString>>
					{
						{ MessageTypeList.Codes.IEM, GetEntryTypeByGovernmentProcedure },
						{ MessageTypeList.Codes.ARM, GetEntryTypeByGovernmentProcedure },
						{ MessageTypeList.Codes.RFM, GetEntryTypeByGovernmentProcedure },
						{ MessageTypeList.Codes.UHC, GetEntryTypeByGovernmentProcedure },
						{ MessageTypeList.Codes.ERM, GetExportEntryType },
						{ MessageTypeList.Codes.IRM, GetImportEntryType },
						{ MessageTypeList.Codes.TPC, GetImportEntryType },
						{ MessageTypeList.Codes.TAD, GetImportEntryType }
					});
				}
				return entryTypeMap;
			}
		}
		ImmutableDictionary<string, Func<ZString>> entryTypeMap;

		ZString GetEntryTypeByGovernmentProcedure()
		{
			var result = ZString.Empty;
			var rGovernmentProdecure = GovernmentProcedure;
			if (rGovernmentProdecure == TransportTypeCodes.Import)
			{
				result = SharedJobMessageTypeList.Codes.Import;
			}
			else if (rGovernmentProdecure == TransportTypeCodes.Export)
			{
				result = SharedJobMessageTypeList.Codes.Export;
			}

			return result;
		}

		ZString GetExportEntryType() => SharedJobMessageTypeList.Codes.Export;

		ZString GetImportEntryType() => SharedJobMessageTypeList.Codes.Import;
		#endregion

		public ZString ReleaseStatus
		{
			get
			{
				var result = ZString.Empty;
				switch (MessageType)
				{
					case MessageTypeList.Codes.ARM:
						result = EntryStatusCodeList.Codes.ARM;
						break;
					case MessageTypeList.Codes.RFM:
						result = EntryStatusCodeList.Codes.RFM;
						break;
					case MessageTypeList.Codes.TRN:
						result = StatusNameCode;
						break;
				}
				return result;
			}
		}

		public ZDateTime ReleaseDateTime { get; set; }

		public ZString DeclarationID { get; set; }

		public ZString GovernmentProcedure { get; set; }

		public ZString StatusNameCode { get; set; }

		public ZString BorderTransportMeans { get; set; }

		public List<ZString> GoodsShipmentValidationCode { get; set; }

		public List<ZString> GoodsShipmentNameCode { get; set; }

		public List<ZString> StatementCode { get; set; }

		public ZDateTime DutyDueDateForTPC { get; set; }

		public ZString EventType { get; set; }

		public ZString InterchangeNumber { get; set; }

		public ZString FunctionalReferenceID { get; set; }
	}
}
