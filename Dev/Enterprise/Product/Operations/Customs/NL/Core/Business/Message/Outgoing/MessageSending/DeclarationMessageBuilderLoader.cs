using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class DeclarationMessageBuilderLoader
{
	DeclarationMessageBuilderLoader()
	{
	}

	public static DeclarationMessageBuilderLoader Instance => instance ?? (instance = new DeclarationMessageBuilderLoader());
	[ThreadStatic]
	static DeclarationMessageBuilderLoader instance;

	public IXmlMessageBuilder GetMessageBuilder(JobDeclarationMessageSendingObject sendingObject)
	{
		IXmlMessageBuilder result = null;
		var outgoingMessageDetails = GetOutgoingMessageDetails(sendingObject);
		if (outgoingMessageDetails?.WrapperType is Type wrapperType && outgoingMessageDetails.MessageBuilderType is Type messageBuilderType)
		{
			var wrapper = (IMetaData)Activator.CreateInstance(wrapperType, sendingObject);
			result = (IXmlMessageBuilder)Activator.CreateInstance(messageBuilderType, wrapper);
		}
		return result;
	}

	OutgoingMessageDetails GetOutgoingMessageDetails(JobDeclarationMessageSendingObject sendingObject)
	{
		OutgoingMessageDetails result;
		if (sendingObject.Declaration.IsImport)
		{
			result = GetMessageBuilderTypeFromDictionary(importMessageDetailsDic);
		}
		else
		{
			result = GetMessageBuilderTypeFromDictionary(exportMessageDetailsDic);
		}
		return result;

		OutgoingMessageDetails GetMessageBuilderTypeFromDictionary(ImmutableDictionary<ZString, OutgoingMessageDetails> dictionary)
		{
			var messageType = sendingObject.MessageType;
			var success = dictionary.TryGetValue(messageType, out var outgoingMessageDetails);
			if (!success)
			{
				var style = sendingObject.Header.EntryInstruction?.CEI_Style;
				_ = dictionary.TryGetValue(messageType + style, out outgoingMessageDetails);
			}
			return outgoingMessageDetails;
		}
	}

	readonly ImmutableDictionary<ZString, OutgoingMessageDetails> importMessageDetailsDic = new Dictionary<ZString, OutgoingMessageDetails>
	{
		{ ImportSendMessageTypes.Codes.AMD, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMSAmendment.Import.ImportAmendmentMessageBuilder), typeof(ImportAmendmentMessageWrapper)) },
		{ ImportSendMessageTypes.Codes.CRI, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Import.CRIMessageBuilder) , typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationForEndUse, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationForCustWarehouse, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationTemporaryAdmission, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H3MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationInwardProcessing, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H4MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.ImportSpecialFiscalTerritoriesDeclaration, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H5MessageBuilder) ,typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationFreeCirculation,new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H6MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.ImportSimplifiedDeclaration, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.I1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.PRE, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.I2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ImportSendMessageTypes.Codes.CAN, new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Import.ImportInvalidationMessageBuilder), typeof(MetaDataWrapper)) },
	}.ToImmutableDictionary();

	readonly ImmutableDictionary<ZString, OutgoingMessageDetails> exportMessageDetailsDic = new Dictionary<ZString, OutgoingMessageDetails>
	{
		{ ExportSendMessageTypes.Codes.AMD,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMSAmendment.Export.ExportAmendmentMessageBuilder), typeof(ExportAmendmentMessageWrapper)) },
		{ ExportSendMessageTypes.Codes.CRE,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.CREMessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.ExportReExport,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.SpecialProcessing,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.UnionGoods,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B3MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.SpecialFiscalTerritory,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B4MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.ExportDeclarationC1,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.C1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.ExportDeclarationC2,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.C2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.DEC + NLConstants.EntryStyles.DeclarationForCustWarehouse,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.EXT,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.ExitInfoMessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.PRE,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.I2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.CAN,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.ExportInvalidationMessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.ExportReExport,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.SpecialProcessing,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.UnionGoods,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B3MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.SpecialFiscalTerritory,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.B4MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.ExportDeclarationC1,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.C1MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.ExportDeclarationC2,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.Export.C2MessageBuilder), typeof(MetaDataWrapper)) },
		{ ExportSendMessageTypes.Codes.FBK + NLConstants.EntryStyles.DeclarationForCustWarehouse,  new OutgoingMessageDetails(typeof(CargoWise.Customs.NL.MessageContracts.DMS.H2MessageBuilder), typeof(MetaDataWrapper)) },
	}.ToImmutableDictionary();
}
