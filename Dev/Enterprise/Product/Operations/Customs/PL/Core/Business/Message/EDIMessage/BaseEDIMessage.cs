using System.Data;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public abstract class BaseEDIMessage(BusinessObjectFactory factory, DataRow row) : EnterpriseEDIMessage(factory, row), Integration.Customs.PL.IEDIMessage
{
	public const string PLMessageNumberPlaceHolder = "_PL_MSGNO_PLACEHOLDER_";

	protected override string MessageNumberPlaceHolderOverride => PLMessageNumberPlaceHolder;

	public override bool UsesPlaceHolders => true;

	protected override string GetMessageReferenceNumber()
	{
		var messagePrefix = GetMessagePrefix();
		var numberFountain = Env.NumberFountains.PLMessageControlNumber(EM_ApplicationCode, messagePrefix.Year2Digits, messagePrefix.MessageCode5Symbols);
		return numberFountain.GetNextFormatted(Factory);
	}

	protected sealed record MessageNumberPrefix(string Year2Digits, string MessageCode5Symbols = null);

	protected virtual MessageNumberPrefix GetMessagePrefix() => new(Year2Digits: ZDateTime.Now.ToString("yy", CultureInfo.InvariantCulture));

	protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

	public new static readonly EDIMessageTypeDecider TypeDecider = new();

	public override void OnSaving()
	{
		base.OnSaving();

		if (!IsInDatabase)
		{
			OnSavingNewRecord();
		}
	}

	void OnSavingNewRecord()
	{
		if (EM_MessageNum.IsEmpty)
		{
			PopulateMessageNumber();
		}

		if (IsTransmitMessage &&
			EM_MessageType != EdiMessageMessageType.Attachment &&
			EM_MessageType != EdiMessageMessageType.CusPollingTransaction)
		{
			SignByCertificate();
		}
	}

	protected override bool ClearMessageNumberOnFailureToSaveCore => true;

	void SignByCertificate()
	{
		if (EM_MessageText.IsEmpty)
		{
			return;
		}

		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		var certificate = new X509Certificate2(glbStaffCertificate.GP_Certificate, glbStaffCertificate.CurrentDecryptedCertificatePassphrase);
		EM_MessageText = XadesBesSigner.Sign(EM_MessageText, certificate);
		EM_IsActive = true;
	}
}
