using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BizTalkBindingsManager.Tests
{
    [TestClass]
    public class TFSWorkspaceTest
    {
        [TestMethod]
        public void TestWorkspaceLoadElements()
        {
            var tempFolder = Path.GetTempPath() + "/" + Guid.NewGuid().ToString();
            var bindingsFolder = tempFolder + "/Operation/Bindings";

            Directory.CreateDirectory(bindingsFolder + "/SendPorts");
            var file = File.CreateText(bindingsFolder + "/SendPorts/SomeSendPort.xml");
            file.WriteLine("<FakeSendPortData></FakeSendPortData>");

            Directory.CreateDirectory(bindingsFolder + "/ReceivePorts");
            file = File.CreateText(bindingsFolder + "/ReceivePorts/SomeReceivePort.xml");
            file.WriteLine("<FakeReceivePortData></FakeReceivePortData>");

            Directory.CreateDirectory(bindingsFolder + "/Orchestrations");
            file = File.CreateText(bindingsFolder + "/Orchestrations/SomeOrchestration.xml");
            file.WriteLine("<FakeOrchestrationData></FakeOrchestrationData>");

            Directory.CreateDirectory(bindingsFolder + "/Agreements");
            file = File.CreateText(bindingsFolder + "/Agreements/SomeAgreement.xml");
            file.WriteLine("<FakeAgreementData></FakeAgreementData>");

            Directory.CreateDirectory(bindingsFolder + "/Parties");
            file = File.CreateText(bindingsFolder + "/Parties/SomeParty.xml");
            file.WriteLine("<FakePartyData></FakePartyData>");

            var workspace = new TFSWorkspace("Some Name", tempFolder);

            var sendPorts = workspace.LoadSendPorts();
            var receivePorts = workspace.LoadReceivePorts();
            var orchestrations = workspace.LoadOrchestrations();
            var parties = workspace.LoadParties();
            var agreements = workspace.LoadAgreements();

            Assert.AreEqual(1, sendPorts.Count);
            Assert.AreEqual(1, receivePorts.Count);
            Assert.AreEqual(1, orchestrations.Count);
            Assert.AreEqual(1, parties.Count);
            Assert.AreEqual(1, agreements.Count);

            Assert.AreEqual("SomeSendPort", sendPorts[0].Name);
            Assert.AreEqual("SomeReceivePort", receivePorts[0].Name);
            Assert.AreEqual("SomeOrchestration", orchestrations[0].Name);
            Assert.AreEqual("SomeParty", parties[0].Name);
            Assert.AreEqual("SomeAgreement", agreements[0].Name);
        }
    }
}
