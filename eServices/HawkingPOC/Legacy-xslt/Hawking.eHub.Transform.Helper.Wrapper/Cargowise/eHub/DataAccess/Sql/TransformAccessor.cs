using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hawking.eHub.Model.DataAccess.Integration;
using Hawking.eHub.Model.eHubTransactions;
using Hawking.eHub.Model.Services;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.eHub.Transform.Helper.Wrapper.Cargowise.eHub.DataAccess.Sql
{
    public class TransformAccessor : ITransformAccessor, IMappingConfigService
    {
        IeHubStoredProc eHubStoredProc;
        IUNLOCOAccessor UNLOCOAccessor;
        public TransformAccessor(IeHubStoredProc storedProcHelper, IUNLOCOAccessor UNLOCOAccessor)
        {
            eHubStoredProc = storedProcHelper;
            this.UNLOCOAccessor = UNLOCOAccessor;
        }

        #region IMappingConfigService

        public List<TransformType> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, out Guid? bestMatchingSetID)
        {
            throw new NotImplementedException();
        }

        public List<TransformSet> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType)
        {
            List<TransformSet> transformSets = new List<TransformSet>();
            Guid transformationSetPK = Guid.Empty;
            using (var connection = eHubStoredProc.eHubTransactionsContext.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = @"[dbo].[SelectTransformsByPartiesMessage]";

                    command.Parameters.Add(new SqlParameter("@SenderID", senderID));
                    command.Parameters.Add(new SqlParameter("@RecipientID", recipientID));
                    command.Parameters.Add(new SqlParameter("@SourceType", sourceMessageType));

                    var output = new SqlParameter
                    {
                        ParameterName = "@TransformationSet",
                        Size = 36,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(output);

                    using (var queryResult = command.ExecuteReader())
                    {
                        var lastSetId = Guid.NewGuid();
                        var transforms = new List<TransformDetail>();
                        while (queryResult.Read())
                        {
                            var setId = queryResult.GetGuid(queryResult.GetOrdinal("TransformSetId"));
                            if (setId != lastSetId)
                            {
                                transforms = new List<TransformDetail>();
                                var xpath = queryResult.IsDBNull(queryResult.GetOrdinal("XPathPredicate")) ?
                                    string.Empty :
                                    (string)queryResult["XPathPredicate"];
                                transformSets.Add(new TransformSet(transforms, xpath));
                                lastSetId = setId;
                            }

                            transforms.Add(new TransformDetail(
                                (string)queryResult["TransformType"],
                                (string)queryResult["TargetMessageType"]));
                        }
                    }
                }
            }

            return transformSets;
        }

        #endregion

        #region CallActionProcedureHelper

        public string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
        {
            return eHubStoredProc.CallActionProcedure(procedure, outputParm, inputParms);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1)
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

        #endregion


        #region GetRecipientCode

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

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
        {
            using (var connection = eHubStoredProc.eHubTransactionsContext.GetDbConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = @"[dbo].[GetRecipientCode]";

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@senderClientCode", senderClientCode));
                    command.Parameters.Add(new SqlParameter("@recipientClientCode", recipientClientCode));
                    command.Parameters.Add(new SqlParameter("@transformationName", transformationName));
                    command.Parameters.Add(new SqlParameter("@codeSetName", codeSet));
                    command.Parameters.Add(new SqlParameter("@resultField", resultField));
                    command.Parameters.Add(new SqlParameter("@key1Value", key1));
                    command.Parameters.Add(new SqlParameter("@key2Value", key2));
                    command.Parameters.Add(new SqlParameter("@key3Value", key3));
                    command.Parameters.Add(new SqlParameter("@key4Value", key4));
                    command.Parameters.Add(new SqlParameter("@key5Value", key5));

                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                        else
                        {
                            throw new Exception("Error mapping code '" +
                                                String.Join("+", (new[] { key1, key2, key3, key4, key5 }).Where(a => a != null).ToArray()) +
                                                "'. Check that the database entries exist for code set:" +
                                                "\nSenderClientCode=" + senderClientCode +
                                                "\nRecipientClientCode=" + recipientClientCode +
                                                "\nTransformationName=" + transformationName +
                                                "\nCodeSetName=" + codeSet +
                                                "\nResultField=" + resultField);
                        }
                    }
                }
            }
        }
        
        #endregion

        public string GetStateFromUNLOCO(string UNLOCO)
        {
            throw new NotImplementedException();
        }

        public string GetUNLOCOFromIATA(string IATACode)
        {
            throw new NotImplementedException();
        }
    }
}
