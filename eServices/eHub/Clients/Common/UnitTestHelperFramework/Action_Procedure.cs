using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;


namespace CargoWise.eHub.Clients.Common.UnitTestHelperFramework
{
    public class Action_Procedure
    {
        public string Procedure { get; set; }
        public string OutputParm { get; set; }
        public string Result { get; set; }

        private List<string> inputParams;

        public Action_Procedure()
        {
            inputParams = new List<string>();
        }

        public void AddInputParms(params string[] list)
        {
            inputParams.AddRange(list);
        }

        public void AddToTestingContext(CodeMapsTestingContext ctx)
        {
            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = Procedure,
                OutputParm = OutputParm,
                InputParms = inputParams,
                Result = Result
            });
        }
    }
}