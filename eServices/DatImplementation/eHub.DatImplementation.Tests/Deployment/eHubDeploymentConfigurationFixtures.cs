using NUnit.Framework;
using System.Linq;

namespace eHub.DatImplementation.Deployment.Tests
{
    [TestFixture]
    public class eHubDeploymentConfigurationFixtures
    {
        [Test]
        public void TestParse()
        {
            const string configString = @"Server=sydcowlsy2-srv1.sand.wtg.zone;Database=eHubTransactions;User Id=DevAdmin;Password=3hubP@$$w0rd;Project=Deployment\BizTalk\Deploy.proj;p:Profile=Test;t:dat:deploy";
            var config = eHubDeploymentConfig.Parse(configString);

            Assert.True(config.Settings.ContainsKey(eHubDeploymentConfig.Keys.Server));
            Assert.True(config.Settings.ContainsKey(eHubDeploymentConfig.Keys.Database));
            Assert.True(config.Settings.ContainsKey(eHubDeploymentConfig.Keys.UserId));
            Assert.True(config.Settings.ContainsKey(eHubDeploymentConfig.Keys.Password));
            Assert.True(config.Settings.ContainsKey(eHubDeploymentConfig.Keys.Project));
            Assert.True(config.Settings.ContainsKey("t:dat:deploy"));

            Assert.That("sydcowlsy2-srv1.sand.wtg.zone", Is.EqualTo(config.Settings[eHubDeploymentConfig.Keys.Server]));
            Assert.That("eHubTransactions", Is.EqualTo(config.Settings[eHubDeploymentConfig.Keys.Database]));
            Assert.That("DevAdmin", Is.EqualTo(config.Settings[eHubDeploymentConfig.Keys.UserId]));
            Assert.That("3hubP@$$w0rd", Is.EqualTo(config.Settings[eHubDeploymentConfig.Keys.Password]));
            Assert.That(@"Deployment\BizTalk\Deploy.proj", Is.EqualTo(config.Settings[eHubDeploymentConfig.Keys.Project]));
            Assert.That(string.Empty, Is.EqualTo(config.Settings["t:dat:deploy"]));
            Assert.True(config.SettingsByPrefix(eHubDeploymentConfig.Prefix.Target).Any());
        }

        [Test]
        public void TestParseProject()
        {
            var configs = new[]
            {
                @"Project=eHub\Deploy\BizTalk\DAT\LastestBuild\Deploy.proj;p:Profile=Dev",
                @"Project=eHub\Gateway\Gateway.Host\CargoWise.eHub.Gateway.Host.csproj;p:Profile=NewProdServer1;t:bat:deploy",
                @"Project=eHub\Gateway\Gateway.Host\CargoWise.eHub.Gateway.Host.csproj;p:Profile=NewProdServer2;t:bat:deploy",
                @"Project=eHub\Gateway\Gateway.Host\CargoWise.eHub.Gateway.Host.csproj;p:Profile=Production;t:bat:deploy",
                @"Project=eHub\Gateway\Gateway.Host\CargoWise.eHub.Gateway.Host.csproj;p:Profile=Test;t:bat:deploy",
                @"Project=eHub\Gateway\Gateway.Host\CargoWise.eHub.Gateway.Host.csproj;p:Profile=TestVM1;t:bat:deploy",
                @"Project=eHub\Deploy\BizTalk\DAT\LastestBuild\Deploy.proj;p:Profile=Dev",
                @"Project=eHub\Deploy\BizTalk\DAT\LastestBuild\Deploy.proj;p:Profile=Test",
                @"Project=eHub\Deploy\BizTalk\DAT\LastestBuild\Deploy.proj;p:Profile=Production"
            };

            foreach (var configString in configs)
            {
                var config = eHubDeploymentConfig.Parse(configString);
                Assert.NotNull(config.Settings[eHubDeploymentConfig.Keys.Project]);
            }
        }

        [Test]
        public void TestParseBAT()
        {
            var configs = new[]
            {
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.eHubAdmin.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.eHubAdmin.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Gateway.Host.zip;p:Profile=NewProdServer1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Gateway.Host.zip;p:Profile=NewProdServer2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Gateway.Host.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Gateway.Host.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Gateway.Host.zip;p:Profile=TestVM1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Nudge.NudgeWindowsService.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Nudge.NudgeWindowsService.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.zip;p:Profile=ProductionOne;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.zip;p:Profile=ProductionTwo;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.zip;p:Profile=ProductionOne;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.zip;p:Profile=ProductionTwo;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.zip;p:Profile=TestOne;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.zip;p:Profile=TestTwo;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.zip;p:Profile=Production1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.zip;p:Profile=Production2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.zip;p:Profile=Test1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.zip;p:Profile=Test2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.zip;p:Profile=Production1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.zip;p:Profile=Production2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.zip;p:Profile=ProductionTest1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.zip;p:Profile=ProductionTest2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.zip;p:Profile=Production1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.zip;p:Profile=Production2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.zip;p:Profile=ProductionTest1;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.zip;p:Profile=ProductionTest2;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.Billing.Collector.eHub.WindowsService.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.Billing.Collector.eHub.WindowsService.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceABIACE.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceABIACE.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceABIACE.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAES.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAES.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAES.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMS.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMS.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMS.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMA.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMA.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceAMA.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceMAN.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceMAN.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceMAN.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceUEM.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceUEM.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.InboundServiceUEM.zip;p:Profile=Test;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.OutboundProcessingService.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\CargoWise.eServices.USCustoms.OutboundProcessingService.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\eBondWebService.zip;p:Profile=ProductionOne;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\eBondWebService.zip;p:Profile=ProductionTest;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\eBondWebService.zip;p:Profile=ProductionTwo;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\Portal.zip;p:Profile=Production;t:bat:deploy",
                @"BAT=$(BinPath)\BAT\Portal.zip;p:Profile=Test;t:bat:deploy"
            };

            foreach (var configString in configs)
            {
                var config = eHubDeploymentConfig.Parse(configString);
                Assert.NotNull(config.Settings[eHubDeploymentConfig.Keys.BAT]);
            }
        }
    }
}
