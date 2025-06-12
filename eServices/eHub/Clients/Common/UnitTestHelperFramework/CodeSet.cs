using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;


namespace CargoWise.eHub.Clients.Common.UnitTestHelperFramework
{

    public class CodeSet
    {
        private string codeSetName;
        private eHubClient sender;
        private eHubClient recipient;
        private eHubTransformationSet transforms;

        private List<string> fieldNames;
        private List<List<string>> rowData;

        public CodeSet(string codeSetName, eHubClient sender, eHubClient recipient, eHubTransformationSet transforms, bool isDefault)
        {
            this.codeSetName = codeSetName;
            this.sender = sender;
            this.recipient = recipient;
            this.transforms = transforms;

            IsDefault = isDefault;

            fieldNames = new List<string>();
            rowData = new List<List<string>>();
        }

        public bool IsDefault { get; set; }

        public void AddFields(params string[] list)
        {
            fieldNames.AddRange(list);
        }

        public void AddRecord(params string[] list)
        {
            rowData.Add(new List<string>(list));
        }

        public void AddToTestingContext(CodeMapsTestingContext ctx)
        {
            eHubCodeSet ehCodeSet = new eHubCodeSet
            {
                CS_Name = codeSetName,
                eHubClient_Sender = sender,
                eHubClient_Recipient = recipient,
                eHubTransformationSet = transforms
            };

            ctx.eHubCodeSets.Add(ehCodeSet);

            if (IsDefault)
            {
                Configure_Default(ctx, ehCodeSet);
            }
            else
            {
                Configure_Keyed(ctx, ehCodeSet);
            }
        }

        private void Configure_Default(CodeMapsTestingContext ctx, eHubCodeSet ehCodeSet)
        {
            List<string> row = rowData[0];

            for (int i = 0; i < fieldNames.Count; i++)
            {
                var ehResult = new eHubCodeSetResult
                {
                    eHubCodeSet = ehCodeSet,
                    CR_Order = 1,
                    CR_Name = fieldNames[i]
                };
                ctx.eHubCodeSetResults.Add(ehResult);

                var ehKey = new eHubCodeMapKey
                {
                    eHubCodeSet = ehCodeSet,
                    CK_Order = 1
                };
                ctx.eHubCodeMapKeys.Add(ehKey);

                var ehValue = new eHubCodeMapValue
                {
                    eHubCodeMapKey = ehKey,
                    eHubCodeSetResult = ehResult,
                    CV_OutputCode = row[i]
                };
                ctx.eHubCodeMapValues.Add(ehValue);
            }
        }

        private void Configure_Keyed(CodeMapsTestingContext ctx, eHubCodeSet ehCodeSet)
        {
            List<eHubCodeSetResult> ownResults = new List<eHubCodeSetResult>();

            for (int i = 1; i < fieldNames.Count; i++)
            {
                var ehResult = new eHubCodeSetResult
                {
                    eHubCodeSet = ehCodeSet,
                    CR_Order = i,
                    CR_Name = fieldNames[i]
                };

                ctx.eHubCodeSetResults.Add(ehResult);
                ownResults.Add(ehResult);
            }

            int order = 0;

            foreach (List<string> row in rowData)
            {
                order++;

                var ehKey = new eHubCodeMapKey
                {
                    eHubCodeSet = ehCodeSet,
                    CK_Order = ++order,
                    CK_Key1Value = row[0]
                };

                ctx.eHubCodeMapKeys.Add(ehKey);

                for (int i = 1; i < fieldNames.Count; i++)
                {
                    string outputVal = row[i];

                    var ehValue = new eHubCodeMapValue
                    {
                        eHubCodeMapKey = ehKey,
                        eHubCodeSetResult = ownResults[i - 1]
                    };

                    if (outputVal == "<PassThroughKey>")
                    {
                        ehValue.CV_PassThroughKey = 1;
                    }
                    else
                    {
                        ehValue.CV_OutputCode = outputVal;
                    }

                    ctx.eHubCodeMapValues.Add(ehValue);
                }
            }
        }

        public string GetCodeSetName()
        {
            return codeSetName;
        }

        public List<string> GetFields()
        {
            return fieldNames;
        }

        public List<List<string>> GetRowData()
        {
            return rowData;
        }
    }
}
