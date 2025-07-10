using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TR.Business
{
	public interface ITRCusStatmentHeaderDocumentSupporterConfigurator
	{
		DocumentSupporterDataState PrintStampDutyLedgerConfig(CusStatementHeader statementHeader, DocumentSupporterDataState dataState);
	}
}
