using System.Collections.Generic;
using Hawking.eHub.Model.eHubTransactions;
using Unity;
using Xunit;

namespace Hawking.eHub.Model.Tests.eHubTransactions
{
    public class EHubStoredProcFixtures : FixturesBase
    {
        public EHubStoredProcFixtures() : base()
        {
            Container.RegisterType<IeHubStoredProc, eHubStoredProc>();
        }

        [Fact]
        public void TestCalculateTimeZoneOffset()
        {
            var ehubStoredProc = Container.Resolve<IeHubStoredProc>();

            var procedure = "CalculateTimeZoneOffset";
            var outputParm = "@offset";
            var inputParms = new List<string> {
                    "@UNLOCO", "AUMEL",
                    "@localtime", "2018-06-10T10:09:32.393"
                };

            var expectedResult = "+10:00";

            var result = ehubStoredProc.CallActionProcedure(procedure, outputParm, inputParms.ToArray());
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestGetStateFromUNLOCO()
        {
            var ehubStoredProc = Container.Resolve<IeHubStoredProc>();
            var result = ehubStoredProc.CallActionProcedureHelper("GetStateFromUNLOCO", "", "@UNLOCO", "AUSYD");

            Assert.Equal("NSW", result);
        }
    }
}
