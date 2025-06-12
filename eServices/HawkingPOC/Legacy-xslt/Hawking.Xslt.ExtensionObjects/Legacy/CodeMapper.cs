using System;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class CodeMapper : ICodeMapper
    {
        ITransformAccessor transformAccessor;
        public CodeMapper(ITransformAccessor transformAccessor)
        {
            this.transformAccessor = transformAccessor;
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string code)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, "Output Code", code, null, null, null, null);
        }

        public virtual string GetRecipientCodeUnkeyed(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, null, null, null, null, null);
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, null, null, null, null);
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, null, null, null);
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, null, null);
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4)
        {
            return GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, null);
        }

        public virtual string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
        {
            return transformAccessor.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5) ?? String.Empty;
        }

        public virtual string GetUNLOCOfromIATA(string IATACode)
        {
            return transformAccessor.GetUNLOCOFromIATA(IATACode) ?? String.Empty;
        }

        public virtual string GetStateFromUNLOCO(string UNLOCO)
        {
            return transformAccessor.GetStateFromUNLOCO(UNLOCO) ?? String.Empty;
        }

        public virtual string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1)
        {
            return CallActionProcedure(procedure, outputParm, inputParmN1, inputParmV1);
        }

        public virtual string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2)
        {
            return CallActionProcedure(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2);
        }

        public virtual string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3)
        {
            return CallActionProcedure(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3);
        }

        public virtual string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4)
        {
            return CallActionProcedure(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3, inputParmN4, inputParmV4);
        }

        public virtual string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4, string inputParmN5, string inputParmV5)
        {
            return CallActionProcedure(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3, inputParmN4, inputParmV4, inputParmN5, inputParmV5);
        }

        public virtual string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
        {
            
            return transformAccessor.CallActionProcedure(procedure, outputParm, inputParms) ?? String.Empty;
        }
    }
}
