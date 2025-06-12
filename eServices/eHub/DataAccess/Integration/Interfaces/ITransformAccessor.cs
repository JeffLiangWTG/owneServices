using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface ITransformAccessor
	{
		List<TransformSet> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, bool? tsDirection);
		string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5);
		void SetCodeMapsTestingContext(CodeMapsTestingContext ctx);
		bool IsFlatFile(string messageType, out string charset);
		bool IsEDI(string messageType);
        bool IsJson(string messageType);
		string CallActionProcedure(string procedure, string outputParm, params string[] inputParms);
		bool IsPostAssembleMapping(string messageType, out string postAssembleWrapper);
	}
}
