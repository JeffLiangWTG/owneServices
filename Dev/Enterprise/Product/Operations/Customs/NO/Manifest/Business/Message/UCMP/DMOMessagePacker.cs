using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.NO.Manifest.Business.DMOMessageConstants;

[assembly: UniversalCustomsEDIMessagePacker(ApplicationCodeList.Codes.NOCustomsDMO, typeof(Enterprise.Customs.NO.Manifest.Business.DMOMessagePacker))]

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class DMOMessagePacker : IUniversalCustomsEDIMessagePacker
{
	bool IUniversalCustomsEDIMessagePacker.AllowEmptyMessageBody => false;

	ZString IUniversalCustomsEDIMessagePacker.Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		Argument.NotNull(message, nameof(message));
		Argument.NotNull(interchange, nameof(interchange));
		Argument.NotNull(logger, nameof(logger));

		logger.Log(LogType.Information, FormattableString.Invariant($"Started Processing EDIMessage with PK: [{message.PK}]"));

		var currentCompany = GetCurrentCompany(message);
		if (currentCompany == null)
		{
			var unknownComapnyError = $"EDIMessage with PK: [{message.PK}], failed to identify compnay with branch PK [{message.EM_GB}].";
			logger.LogError(unknownComapnyError);
			return unknownComapnyError;
		}

		var glbCompanyWrapper = new NO.Business.GlbCompanyWrapper(currentCompany);
		var credential = glbCompanyWrapper.Credential;
		if (credential == null)
		{
			var companyCredentialNotFound = $"EDIMessage with PK: [{message.PK}], failed to fetch compnay's credentials PK [{currentCompany.PK}].";
			logger.LogError(companyCredentialNotFound);
			return companyCredentialNotFound;
		}

		var interchangeData = DMOInterchangeDataBuilder.BuildData(message);
		var errors = interchangeData.ErrorBuilder.ToStringWithNewLineBetweenAppends();
		if (!string.IsNullOrWhiteSpace(errors))
		{
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
			logger.LogError(FormattableString.Invariant($"Failed to process EDIMessage with PK: [{message.PK}]"));
			return errors;
		}

		EDIMessagePackerUtils.PopulateInterchange(interchange,
			ApplicationCodeList.Codes.NOCustomsDMO,
			message.EM_MessageType,
			Env.CurrentCompany.GetLicenceCode(),
			DMOSupportedTargets.Default,
			message.EM_GB,
			message.EM_GP,
			ZGuid.NewZGuid());

		interchange.EI_GP = credential.PK;
		interchange.EI_BodyText = interchangeData.MessageText;
		interchange.EI_InterchangeType = interchangeData.InterchangeType;

		interchangeData.MessageAttributes[DMOMessageAttributes.CertificateIssuerAttribute] = credential.GP_UserID;

		interchange.SetHeaderTextWithAttributeDictionary(interchangeData.MessageAttributes);
		logger.Log(LogType.Information, FormattableString.Invariant($"Message Information Packed successfully for EDIMessage with PK: [{message.PK}]"));

		return ZString.Empty;
	}

	GlbCompany GetCurrentCompany(EDIMessage message)
	{
		var currentBranch = GetCurrentBranch(message);
		if (currentBranch == null)
		{
			return null;
		}

		var query = new ZQuery(GlbCompanySchema.PK, currentBranch.GB_GC);
		return message.Factory.LoadTop1<GlbCompany>(query);
	}

	GlbBranch GetCurrentBranch(EDIMessage message)
	{
		var branckPK = message.EM_GB;
		if (branckPK.IsEmpty || !branckPK.IsValid)
		{
			return null;
		}

		var query = new ZQuery(GlbBranchSchema.PK, branckPK);
		return message.Factory.LoadTop1<GlbBranch>(query);
	}
}
