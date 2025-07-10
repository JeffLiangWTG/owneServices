using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public abstract class BaseMessageInterpreter<T> where T : class
{
	public abstract string Interpret(T dataProvider, EDIMessage ediMessage);

	protected string GetValidStatementTypeCode(string statementTypeCode)
	{
		var result = statementTypeCode;
		if (!string.IsNullOrEmpty(statementTypeCode) && ResponseStatementTypes.ContainsCode(statementTypeCode))
		{
			result = ResponseStatementTypes.GetDescriptionFromCode(statementTypeCode);
		}
		return result;
	}

	ResponseStatementTypes ResponseStatementTypes => responseStatementTypes ?? (responseStatementTypes = new ResponseStatementTypes());
	ResponseStatementTypes responseStatementTypes;
}
