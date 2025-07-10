using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class CUSDECCUSRESMessagePairDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider
	{
		#region ctor

		public CUSDECCUSRESMessagePairDocumentWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader.Factory)
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			this.entryHeader = cusEntryHeader;
			this.declaration = cusEntryHeader.Declaration;
		}

		CUSDECCUSRESMessagePairDocumentWrapper(CUSRESEDIMessage cusresMessage) : base(cusresMessage.Factory)
		{
			this.entryHeader = cusresMessage.EM_LinkedObject as CusEntryHeader;
			this.declaration = entryHeader.Declaration;
			this.cusresMessage = cusresMessage.CUSRESHelper;
		}

		public static CUSDECCUSRESMessagePairDocumentWrapper NewForMessage(CUSRESEDIMessage cusresMessage)
		{
			CUSDECCUSRESMessagePairDocumentWrapper result = null;
			var linkedEntryHeader = cusresMessage?.EM_LinkedObject as CusEntryHeader;
			if (linkedEntryHeader != null)
			{
				result = new CUSDECCUSRESMessagePairDocumentWrapper(cusresMessage);
			}
			return result;
		}

		#endregion

		#region Fields

		readonly CusEntryHeader entryHeader;
		bool outgoingMessageDataInitialised;

		#endregion

		public JobDeclaration Declaration => declaration;
		readonly JobDeclaration declaration;

		public CUSRESMessageHelper CUSRESMessage => cusresMessage ?? (cusresMessage = entryHeader.LastReceivedCUSRESMessageHelper);
		CUSRESMessageHelper cusresMessage;

		public CUSDECMessageHelper CUSDECMessage
		{
			get
			{
				if (!outgoingMessageDataInitialised)
				{
					InitialiseOutgoingMessageData();
				}
				return cusdecMessage;
			}
		}
		CUSDECMessageHelper cusdecMessage;

		ZAMessage OutgoingCUSDECMessage
		{
			get
			{
				if (!outgoingMessageDataInitialised)
				{
					InitialiseOutgoingMessageData();
				}
				return outgoingCUSDECMessage;
			}
		}
		ZAMessage outgoingCUSDECMessage;

		public ZDecimal TotalDutiesAndTaxes
		{
			get
			{
				var result = ZDecimal.Zero;
				if (OutgoingCUSDECMessage != null)
				{
					result = OutgoingCUSDECMessage.CustomsDutyNoS1P2BAfter + OutgoingCUSDECMessage.S1P2BDutyAfter + OutgoingCUSDECMessage.ValueAddedTaxAfter + OutgoingCUSDECMessage.ProvisionalPaymentAmountAfter + OutgoingCUSDECMessage.PenaltyAmountAfter;
				}
				return result;
			}
		}

		public ZDecimal TotalCustomsDutyExcluding12B => OutgoingCUSDECMessage?.CustomsDutyNoS1P2BAfter ?? ZDecimal.Zero;

		public ZDecimal TotalS1P2BDuty => OutgoingCUSDECMessage?.S1P2BDutyAfter ?? ZDecimal.Zero;

		public ZDecimal TotalValueAddedTax => OutgoingCUSDECMessage?.ValueAddedTaxAfter ?? ZDecimal.Zero;

		public ZDecimal TotalPPs => (OutgoingCUSDECMessage != null) ? (OutgoingCUSDECMessage.ProvisionalPaymentAmountAfter + OutgoingCUSDECMessage.PenaltyAmountAfter) : decimal.Zero;

		public System.Drawing.Image JobBranchLogo => SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(entryHeader.RegistryCompanyPK, entryHeader.RegistryBranchPK, Guid.Empty);

		public ZString ExporterTIN
		{
			get
			{
				var result = ZString.Empty;
				if (!CUSDECMessage?.Exporter?.OrganizationCode.IsEmpty ?? false)
				{
					result = CUSDECMessage.Exporter.OrganizationCode;
				}
				else if (!CUSDECMessage?.Supplier.OrganizationCode.IsEmpty ?? false)
				{
					result = CUSDECMessage.Supplier.OrganizationCode;
				}
				else if (entryHeader?.Declaration?.Supplier?.CustomsCodes != null)
				{
					result = entryHeader.Declaration.Supplier.CustomsCodes.GetCustomsRegNo(
						OrgCusCode.CodeTypes.SupplierCode,
						Core.Constants.CountryCodes.SouthAfrica
					);
				}

				return result;
			}
		}

		public ZString ExporterName
		{
			get
			{
				var result = ZString.Empty;
				if (!CUSDECMessage?.Exporter.OrganizationCode.IsEmpty ?? false)
				{
					result = CUSDECMessage.Exporter.Name;
				}
				else if (!CUSDECMessage?.Supplier.OrganizationCode.IsEmpty ?? false)
				{
					result = CUSDECMessage.Supplier.Name;
				}
				else
				{
					result = entryHeader?.Declaration?.Supplier?.OH_FullName ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString ImporterTIN
		{
			get
			{
				var result = ZString.Empty;
				if (!CUSDECMessage?.Importer.OrganizationCode.IsEmpty ?? false)
				{
					result = CUSDECMessage.Importer.OrganizationCode;
				}
				else if (entryHeader?.Declaration?.Importer?.CustomsCodes != null)
				{
					result = entryHeader.Declaration.Importer.CustomsCodes.GetCustomsRegNo(
						OrgCusCode.CodeTypes.CustomsClientCode,
						Core.Constants.CountryCodes.SouthAfrica
					);
				}

				return result;
			}
		}

		public ZString ImporterName =>
			CUSDECMessage?.Importer.Name.IsEmpty ?? true
			? entryHeader?.Declaration?.Importer?.OH_FullName ?? ZString.Empty
			: CUSDECMessage.Importer.Name;

		public ZString UniqueConsignmentReference => entryHeader.UniqueConsignmentReference;

		#region Implementation

		void InitialiseOutgoingMessageData()
		{
			var messageNumber = CUSRESMessage?.OutgoingMessageNumber ?? ZString.Empty;
			if (!messageNumber.IsEmpty)
			{
				outgoingCUSDECMessage = (ZAMessage)entryHeader.Messages.GetMatchingMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC, messageNumber, EDIMessage.Direction.Transmit);
			}
			else
			{
				outgoingCUSDECMessage = (ZAMessage)entryHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC, EDIMessage.Direction.Transmit);
			}
			if (outgoingCUSDECMessage != null)
			{
				cusdecMessage = CUSDECMessageHelper.New(outgoingCUSDECMessage);
			}
			outgoingMessageDataInitialised = true;
		}

		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => entryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => entryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
