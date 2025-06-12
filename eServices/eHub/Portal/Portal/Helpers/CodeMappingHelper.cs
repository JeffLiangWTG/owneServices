using CargoWise.eHub.Portal.Models.eHubTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Helpers
{
    public class CodeMappingHelper
    {

        public static bool ShouldUpdateDatabase(List<eHubCodeMapKey> keysToDelete, List<eHubCodeMapKey> keysToAdd, List<eHubCodeMapValue> valuesToDelete, List<eHubCodeMapValue> valuesToAdd)
        {
            var result = valuesToDelete.Count != valuesToAdd.Count || keysToDelete.Count != keysToAdd.Count;

            if (!result)
            {
                for (int valuesIndex = 0; valuesIndex < valuesToDelete.Count; valuesIndex++)
                {
                    if (valuesToAdd[valuesIndex].CV_CR != valuesToDelete[valuesIndex].CV_CR ||
                        valuesToAdd[valuesIndex].CV_OutputCode != valuesToDelete[valuesIndex].CV_OutputCode ||
                        valuesToAdd[valuesIndex].CV_PassThroughKey != valuesToDelete[valuesIndex].CV_PassThroughKey)
                    {
                        result = true;
                        break;
                    }
                }
                if (!result)
                {
                    for (int keysIndex = 0; keysIndex < keysToDelete.Count; keysIndex++)
                    {
                        if (keysToAdd[keysIndex].CK_Key1Value != keysToDelete[keysIndex].CK_Key1Value ||
                            keysToAdd[keysIndex].CK_Key2Value != keysToDelete[keysIndex].CK_Key2Value ||
                            keysToAdd[keysIndex].CK_Key3Value != keysToDelete[keysIndex].CK_Key3Value ||
                            keysToAdd[keysIndex].CK_Key4Value != keysToDelete[keysIndex].CK_Key4Value ||
                            keysToAdd[keysIndex].CK_Key5Value != keysToDelete[keysIndex].CK_Key5Value ||
                            keysToAdd[keysIndex].CK_Order != keysToDelete[keysIndex].CK_Order)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }
}