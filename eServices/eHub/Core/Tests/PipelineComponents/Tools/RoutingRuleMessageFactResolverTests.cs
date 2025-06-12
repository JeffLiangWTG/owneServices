using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.DataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;

using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using System.Xml.XPath;

namespace CargoWise.eHub.Core.Tests.PipelineComponents.Tools
{
    [TestClass]
    public class RoutingRuleMessageFactResolverTests
    {
        protected MessageFactory MessageFactory
        {
            get { return messageFactory ?? (messageFactory = new MessageFactory()); }
        }
        MessageFactory messageFactory;

        #region Implementation

        protected Stream GetEmbeddedResource(string resourceName)
        {
            return GetEmbeddedResource(resourceName, Assembly.GetExecutingAssembly());
        }

        protected Stream GetEmbeddedResource(string resourceName, Assembly executingAssembly)
        {
            string fullResourceName = executingAssembly.GetName().Name + '.' + resourceName;
            Stream resource = executingAssembly.GetManifestResourceStream(fullResourceName);
            if (resource == null)
            {
                throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
            }
            return resource;
        }
        #endregion

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void RoutingRuleMessageFactResolver_Uppercase()
        {
            var mockResult = MockRepository.GenerateMock<IEnumerable<string>>();
            var result = new List<string> { "RESULT" }.AsEnumerable<string>().GetEnumerator();
            mockResult.Stub(x => x.GetEnumerator()).Return(result);
            var facts = new[]
			{
				new Fact { Type = "XPATHNAVFUNC", Name = "SCAC", Query = "translate(/*[local-name()='SCACTest']/*[local-name()='Value'], 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')"  },
                new Fact { Type = "XPATH", Name = "CountryCode", Query = "/*[local-name()='SCACTest']/*[local-name()='CountryCode']"},
				new Fact { Type = "XPATHNAVFUNC", Name = "SCACCount", Query = "count(/*[local-name()='SCACTest']/*[local-name()='Value'])"  },
			};

            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MessageFact_SCAC_input.xml");
            message.Context = MessageFactory.CreateMessageContext();

            var messageFactResolver = new RoutingRuleMessageFactResolver(message);
            messageFactResolver.Resolve(facts);

            Assert.AreEqual("RESULT", facts.First(f => f.Name == "SCAC").Value);
            Assert.AreEqual("AU", facts.First(f => f.Name == "CountryCode").Value);
			Assert.AreEqual("1", facts.First(f => f.Name == "SCACCount").Value);
        }
    }

}
