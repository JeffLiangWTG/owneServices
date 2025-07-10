using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.PL.Business.Constants;
using CusPollingTransaction = Enterprise.Customs.Business.CusPollingTransaction;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business;

sealed class CusPollingTransactionMessageSender : PLMessageSender<CusPollingTransaction>
{
	public CusPollingTransactionMessageSender(BusinessObjectFactory factory)
		: base(factory)
	{
		var polandUNLOCO = new RefUNLOCO.Loader(factory).Load(Constants.PolishTimeZoneUnloco);
		plTimeZone = polandUNLOCO?.TimeZoneSet?.GetCalculationTimeZone()
			?? throw new InvalidOperationException("Error getting the Polish time zone!");
	}

	readonly ITimeZone plTimeZone;

	protected override ZString MessageType => EdiMessageMessageType.CusPollingTransaction;

	protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.PLCustoms;

	protected override ZString ApplicationReference => string.Empty;

	protected override ZBool SendWithMessageErrors => ZBool.False;

	protected override void SendCoreInContextCore(CusPollingTransaction transaction, EDIMessage message)
	{
		base.SendCoreInContextCore(transaction, message);

		var dataOd = UTCToPLLocalTime(transaction.CPT_StatusTimeUtc).ToISO8601String();
		var dataDo = UTCToPLLocalTime(
			transaction.CPT_Type != CusPollingTransactionTypes.PLC
				? transaction.CPT_EarliestTimeOfNextAttemptUtc
				: transaction.CPT_EarliestTimeOfNextAttemptUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry).ToISO8601String();
		var source = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(
			$"""
			<GetDocumentsRequest xmlns="http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0">
				<pobrany>0</pobrany>
				<dataOd>{dataOd}</dataOd>
				<dataDo>{dataDo}</dataDo>
				<allEmployees>true</allEmployees>
			</GetDocumentsRequest>
			"""));
		message.SetEM_MessageTextOrDataSource(source);

		message.EM_MessageSubType = EDIMessageSubType.CusPollingTransaction;
		message.EM_IsActive = ZBool.True;
		message.EM_IsTestMessage = ZBool.False;
		message.EM_GP = transaction.CPT_ParentID;
		message.EM_LinkedObject = transaction;

		ZDateTime UTCToPLLocalTime(ZDateTime utcZDateTime)
			=> utcZDateTime.IsValid ? plTimeZone.ToLocalTime(utcZDateTime.ToDateTime()) : utcZDateTime;
	}
}
