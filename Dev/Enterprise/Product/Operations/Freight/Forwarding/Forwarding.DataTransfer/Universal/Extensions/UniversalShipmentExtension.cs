using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalDate = Enterprise.UniversalDataBuss.DataObjects.Universal.Date;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class UniversalShipmentExtension
	{
		#region Documentary Purpose

		public static bool IsAmendment(this UniversalShipment dataObject)
			=> GetDocumentaryPurpose(dataObject) == MessagePurposes.Codes.Amendment;

		public static bool IsOriginal(this UniversalShipment dataObject)
			=> GetDocumentaryPurpose(dataObject) == MessagePurposes.Codes.Original;

		public static string GetDocumentaryPurpose(this UniversalShipment dataObject)
			=> dataObject?.DataContext?.DocumentaryOverride?.Purpose?.Code.GetValueOrDefault() ?? string.Empty;

		#endregion

		#region Import Action

		public static bool IsLinkOnly(this UniversalShipment dataObject)
			=> dataObject?.CheckImportAction(ImportAction.LinkOnly) ?? false;

		public static bool IsMerge(this UniversalShipment dataObject)
			=> dataObject?.CheckImportAction(ImportAction.Merge) ?? false;

		public static bool CheckImportAction(this UniversalShipment dataObject, ImportAction importActionToCheck)
		{
			if (dataObject.GetImportAction() is ImportAction importAction)
			{
				return importAction == importActionToCheck;
			}

			return false;
		}

		#endregion

		#region Document Name

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string DocumentNameConsolidationAdvice = "Consolidation Advice";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string DocumentNameCargoReceiptAdvice = "Cargo Receipt Advice";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string DocumentNameShippingInstruction = "Shipping Instruction";

		public static bool IsConsolidationAdviceMessage(this UniversalShipment dataObject)
			=> dataObject?.CheckDocumentName(DocumentNameConsolidationAdvice) ?? false;

		public static bool IsCargoReceiptAdviceMessage(this UniversalShipment dataObject)
			=> dataObject?.CheckDocumentName(DocumentNameCargoReceiptAdvice) ?? false;

		public static bool IsShippingInstructionMessage(this UniversalShipment dataObject)
			=> dataObject?.CheckDocumentName(DocumentNameShippingInstruction) ?? false;

		public static bool CheckDocumentName(this UniversalShipment dataObject, string documentNameToCheck)
		{
			if (dataObject == null)
			{
				return false;
			}

			var documentName = dataObject.DataContext?.DocumentaryOverride?.DocumentName;

			return documentName.HasValue && string.Equals(documentName.Value, documentNameToCheck, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		public static bool IsNVOCC(this UniversalShipment dataObject)
			=> dataObject?.DataContext?.RecipientRoleCollection?.Any(c => c.Code == RecipientRoleType.NVO && (c.ServiceCode == ServiceCodeType.BRQ || c.ServiceCode == ServiceCodeType.SIN)) ?? false;

		public static ZString GetShipmentNumber(this UniversalShipment ushipment)
			=> ushipment?.GetMatchingDataTarget(DataContextType.ForwardingShipment)?.Key.GetValueOrDefault() ?? string.Empty;

		public static OrgAddress GetMatchedOrgAddress(this UniversalShipment dataObject, DocAddressType addressType, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType.ToString()) is UniversalOrganizationAddress matchingAddress
				? new OrganisationDataObjectReader(matchingAddress, logger, factory).GetMatched()
				: null;
		}

		#region try method

		public static void TryAddDateToCollection(this UniversalShipment ushipment, UniversalDateType dateType, ZDate date)
		{
			if (ushipment.DateCollection == null)
			{
				ushipment.SetDateCollection(() => new List<UniversalDate>());
			}

			if (date.IsValid)
			{
				ushipment.DateCollection?.Add(new UniversalDate { Type = dateType, Value = date });
			}
		}

		#endregion
	}
}
