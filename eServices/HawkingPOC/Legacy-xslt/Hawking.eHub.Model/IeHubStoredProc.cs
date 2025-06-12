namespace Hawking.eHub.Model.eHubTransactions
{
    public interface IeHubStoredProc
    {
        IeHubTransactionsContext eHubTransactionsContext { get; }

        string CallActionProcedure(string procedure, string outputParm, params string[] inputParms);

        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1);

        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2);

        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3);

        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4);

        string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4, string inputParmN5, string inputParmV5);
    }
}
