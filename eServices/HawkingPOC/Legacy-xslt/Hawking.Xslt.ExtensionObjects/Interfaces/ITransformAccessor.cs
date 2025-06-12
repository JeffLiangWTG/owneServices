namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface ITransformAccessor
    {
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string code);
        string GetRecipientCodeUnkeyed(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField);
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1);
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2);
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3);
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4);
        string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5);
        string GetUNLOCOFromIATA(string IATACode);
        string GetStateFromUNLOCO(string UNLOCO);
        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1);
        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2);
        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3);
        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4);
        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4, string inputParmN5, string inputParmV5);
        string CallActionProcedure(string procedure, string outputParm, params string[] inputParms);
    }
}
