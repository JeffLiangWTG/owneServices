using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.NO.Registry;
using Enterprise.Edifact.Generic;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

[assembly: UniversalCustomsEDIMessagePacker(ApplicationCodeList.Codes.NOCustoms, typeof(Enterprise.Customs.NO.Business.EDIFactMessagePacker))]

namespace Enterprise.Customs.NO.Business;

sealed class EDIFactMessagePacker : IUniversalCustomsEDIMessagePacker
{
	bool IUniversalCustomsEDIMessagePacker.AllowEmptyMessageBody => false;

	ZString IUniversalCustomsEDIMessagePacker.Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		Argument.NotNull(message, nameof(message));
		Argument.NotNull(message.EM_LinkedObject as CusEntryHeader, nameof(message), "EM_LinkedObject is not a CusEntryHeader");
		Argument.NotNull(interchange, nameof(interchange));
		Argument.NotNull(logger, nameof(logger));
		using var traceLogger = new TraceLogger($"Processing EDIMessage with PK: [{message.PK}]", logger);

		return PackCore(message, interchange, logger);
	}

	ZString PackCore(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		PopulateInterchangeBody(interchange, message);

		EDIMessagePackerUtils.PopulateInterchange(
			interchange,
			ApplicationCodeList.Codes.NOCustoms,
			message.EM_MessageType,
			message.ExternalPassword?.Company?.LicenceKeyIdentifier ?? GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			MessageSupportedTargetFTP,
			message.EM_GB,
			message.EM_GP,
			ZGuid.NewZGuid());

		SetInterchangeHeaderTextWithAttributeDictionary(message, interchange);

		message.EM_Status = EDIMessageStatusList.Codes.Sent;
		logger.Log(LogType.Information, FormattableString.Invariant($"Message Information Packed successfully for EDIMessage with PK: [{message.PK}]"));

		return ZString.Empty;
	}

	void PopulateInterchangeBody(EDIInterchange interchange, EDIMessage message)
	{
		var cusEntryHeader = message.EM_LinkedObject as CusEntryHeader;
		var interchangeControlReference = GetReferenceNumber(cusEntryHeader.Factory);

		interchange.ContainedMessages.Add(message);

		var headerSegment = GetHeaderSegment(cusEntryHeader, interchangeControlReference);
		var footerSegment = GetFooterSegment(interchange, interchangeControlReference);

		var interchangeTextBuilder = new ZStringBuilder();
		interchangeTextBuilder.Append(headerSegment.ToString(CharacterSet));
		interchange.ContainedMessages
			.Select(x => x.EM_MessageText)
			.ForEach(x => interchangeTextBuilder.Append(x));
		interchangeTextBuilder.Append(footerSegment.ToString(CharacterSet));

		interchange.EI_BodyText = interchangeTextBuilder.ToStringWithNewLineBetweenAppends();
	}

	static UNBSegment GetHeaderSegment(CusEntryHeader cusEntryHeader, string interchangeControlReference)
	{
		var unb = new UNBSegment();

		unb.SyntaxIdentifier.SyntaxIdentifier = UNOA;
		unb.SyntaxIdentifier.SyntaxVersionNumber = SystemVersionNumber;

		unb.InterchangeSender.SenderIdentification = GetSenderIdentification(cusEntryHeader);
		unb.InterchangeSender.PartnerIdentificationCodeQualifier = NEP;

		unb.InterchangeRecipient.RecipientIdentification = GetReceiverIdentification();
		unb.InterchangeRecipient.PartnerIdentificationCodeQualifier = NEP;

		var preparedDateTime = ZDateTime.Now;
		unb.DateTimeOfPreparation.Date = preparedDateTime.ToShortCustomsFormatDateString(yearDigits: 4);
		unb.DateTimeOfPreparation.Time = preparedDateTime.ToCustomsFormatTimeString();

		unb.InterchangeControlReference = interchangeControlReference;

		unb.TestIndicator = GetTestIndicator();

		return unb;
	}

	static UNZSegment GetFooterSegment(EDIInterchange interchange, string interchangeControlReference)
	{
		var numberOfMessages = interchange.ContainedMessages.Count;
		var unz = new UNZSegment();
		unz.InterchangeControlCount = numberOfMessages.ToString();
		unz.InterchangeControlReference = interchangeControlReference;
		return unz;
	}

	static ZString GetReferenceNumber(BusinessObjectFactory factory)
	{
		Argument.NotNull(factory, nameof(factory));

		var date = ZDateTime.Now.ToShortCustomsFormatDateString(yearDigits: 4);
		using var transactionManager = Db.Connection.BeginTransactionWithManager();
		var sequence = Env.NumberFountains.NODecReferenceNumber($"{date}").GetNextFormatted(factory);
		transactionManager.CommitTransaction();

		return $"{date}{sequence}";
	}

	static ZString GetReceiverIdentification() => IsTestEnvironment ? NodiRegistry.CurrentCustomsTestNodiNumber : NodiRegistry.CurrentCustomsProductionNodiNumber;

	static ZString GetSenderIdentification(CusEntryHeader cusEntryHeader)
	{
		var declaration = cusEntryHeader.Declaration;
		if (declaration.JE_OA_DeclarantAddress.IsEmpty)
		{
			return ZString.Empty;
		}

		var dateOfValuation = declaration?.DateOfValuation ?? ZDateTime.Today;
		var header = declaration.DeclarantAddress?.Header;

		if (header != null)
		{
			return CusAuthorisationHeader.Loader.GetAuthorisationNumber(header.Factory,
				Core.Constants.CountryCodes.Norway,
				CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration,
				dateOfValuation,
				header.PK);
		}

		return ZString.Empty;
	}

	static void SetInterchangeHeaderTextWithAttributeDictionary(EDIMessage message, EDIInterchange interchange)
	{
		var dictionary = new Dictionary<string, string>()
		{
			{ FileNameAttribute, GenerateFileName(message) }
		};
		interchange.SetHeaderTextWithAttributeDictionary(dictionary);
	}

	static string GenerateFileName(EDIMessage message)
	{
		var messageType = string.Empty;
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			messageType = entryHeader.IsImport ? (NoResString)"I" : (NoResString)"E";
		}

		return FormattableString.Invariant($"{messageType}{message.EM_MessageNum}.txt");
	}

	static bool IsTestEnvironment => NOCustomsDataRegistry.Instance.EnableTestMessages.Value;

	static ZString GetTestIndicator() => IsTestEnvironment ? TestIndicator : ZString.Empty;

	NOCharacterSet CharacterSet => characterSet ??= new NOCharacterSet();

	NOCharacterSet characterSet;

	const string FileNameAttribute = "custom.NO.FileName";
	const string MessageSupportedTargetFTP = "NOCustomsFTP";
	const string UNOA = "UNOA";
	const string SystemVersionNumber = "1";
	const string TestIndicator = "2";
	const string NEP = "NEP";
}
