using System;
using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using System.IO;

namespace CargoWise.eHub.Clients.Common.UnitTestHelperFramework
{
    public class UnitTestHelper
    {
        private eHubClient sender;
        private eHubClient recipient;
        private eHubTransformationSet transforms;

        private Dictionary<string, eHubClient> eHubClients;
        private Dictionary<string, eHubTransformationSet> eHubTransforms;

        private List<CodeSet> codeSets;
        private List<Action_Procedure> actionProcedures;

        public UnitTestHelper()
        {
            eHubClients = new Dictionary<string, eHubClient>();
            eHubTransforms = new Dictionary<string, eHubTransformationSet>();

            codeSets = new List<CodeSet>();
            actionProcedures = new List<Action_Procedure>();
        }

        public void SetActiveTS(string senderId, string recipientId, string transformsName)
        {
            if (!eHubClients.TryGetValue(senderId, out sender))
            {
                eHubClients[senderId] = sender = new eHubClient { CC_ID = senderId };;
            }
            
            if (!eHubClients.TryGetValue(recipientId, out recipient))
            {
                eHubClients[recipientId] = recipient = new eHubClient { CC_ID = recipientId };;
            } 

            if (!eHubTransforms.TryGetValue(transformsName, out transforms))
            {
                eHubTransforms[transformsName] = transforms = new eHubTransformationSet
                {
                    TS_Name = transformsName,
                    eHubClient_Sender = sender,
                    eHubClient_Recipient = recipient
                };
            } 
        }
        
        public CodeSet NewCodeSet(string codeSetName, bool isDefault = false)
        {
            CodeSet cs = new CodeSet(codeSetName, sender, recipient, transforms, isDefault);

            codeSets.Add(cs);

            return cs;
        }
        
        public Action_Procedure NewActionProcedure()
        {
            var ap = new Action_Procedure();
            actionProcedures.Add(ap);
            return ap;
        } 

        public CodeMapsTestingContext GetCodeMapsTestingContext()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();

            ctx.eHubClients.AddRange(eHubClients.Values);
            ctx.eHubTransformationSets.AddRange(eHubTransforms.Values);

            codeSets.ForEach(cs => cs.AddToTestingContext(ctx));
            actionProcedures.ForEach(ap => ap.AddToTestingContext(ctx));

            return ctx;
        } 
        
        public string GetSQL()
        {
            SQLScriptHelper sqlHelper = new SQLScriptHelper(codeSets);
            return sqlHelper.GetSQL();
        } 
        
        public void OutputSQL()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(GetSQL());
            Console.WriteLine();
        }
        
        public void WriteSqlToFile(string FileName)
        {
            if (Environment.GetEnvironmentVariable("DAT_IS_TESTING") == "true")
            {
                return;
            }

            string sql = this.GetSQL();
            File.WriteAllText(FileName, sql);
        }
    }
} 