using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Hawking.eHub.Model.eHubTransactions
{
    public class eHubStoredProc : IeHubStoredProc
    {
        DbContext dbContext;
        List<Tuple<string, string, int>> storedProcOutputParamSizes;

        public eHubStoredProc(IeHubTransactionsContext eHubTransactionsContext)
        {
            dbContext = eHubTransactionsContext as DbContext;
            this.eHubTransactionsContext = eHubTransactionsContext;

            storedProcOutputParamSizes = new List<Tuple<string, string, int>>();
            storedProcOutputParamSizes.Add(
                new Tuple<string, string, int>("CalculateTimeZoneOffset", "@offset", 6));
            storedProcOutputParamSizes.Add(
                new Tuple<string, string, int>("GetCounterInterfaceValue", "@StartValue", 8));
            storedProcOutputParamSizes.Add(
                new Tuple<string, string, int>("GetCounterValue", "@value", 20));
            storedProcOutputParamSizes.Add(
                new Tuple<string, string, int>("GetUserIDPasswordReference", "@Result", 160));
            storedProcOutputParamSizes.Add(
                new Tuple<string, string, int>("SelectSubscribedReference", "@reference", 8000));
        }

        public IeHubTransactionsContext eHubTransactionsContext { get; }

        public string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
        {
            try
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = procedure;

                        for (int i = 0; i < inputParms.Length; i += 2)
                        {
                            command.Parameters.Add(new SqlParameter(inputParms[i], inputParms[i + 1]));
                        }

                        if (string.IsNullOrEmpty(outputParm))
                        {
                            return Convert.ToString(command.ExecuteScalar());
                        }
                        else
                        {
                            if (!storedProcOutputParamSizes.Any(x => x.Item1 == procedure && x.Item2 == outputParm))
                            {
                                throw new Exception($"{procedure} output parameter {outputParm} has not been configured to have a size");
                            }

                            var output = new SqlParameter
                            {
                                ParameterName = outputParm,
                                Size = storedProcOutputParamSizes.First(x => x.Item1 == procedure && x.Item2 == outputParm).Item3,
                                Direction = ParameterDirection.Output
                            };

                            command.Parameters.Add(output);
                            command.ExecuteNonQuery();

                            return Convert.ToString(command.Parameters[outputParm].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("CallActionProcedure error: procedure = \"{0}\"; outputParm = \"{1}\"; inputParms = [ \"{2}\" ]\r\nException: {3}", procedure, outputParm, string.Join("\", \"", inputParms), ex.ToString()));
            }
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
    }
}
