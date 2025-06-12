using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.NZCustoms.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	[TestClass]
	public class NZTransformationTests
	{
		private MapTester _mapTester;
		private Assembly _testFixture;

		[TestInitialize]
		public void TestSetup()
		{
			_testFixture = Assembly.GetExecutingAssembly();
			_mapTester = new MapTester(_testFixture, Comparer);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZTransformation_NZCustoms2MessageGateway()
		{
			string source = "TestFiles.NZCustoms.xml";
			string expected = "TestFiles.NZCustomsMessageGateway.xml";
			_mapTester.Execute<NZCustoms2MessageGateway>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZTransformation_NZCustoms2MessageGatewayAuthenticationEmpty()
		{
			string source = "TestFiles.NZCustomsAuthenticationEmpty.xml";
			string expected = "TestFiles.NZCustomsMessageGatewayAuthenticationEmpty.xml";
			_mapTester.Execute<NZCustoms2MessageGateway>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZTransformation_NZCustoms2MessageGatewayNoAuthentication()
		{
			string source = "TestFiles.NZCustomsNoAuthentication.xml";
			string expected = "TestFiles.NZCustomsMessageGatewayAuthenticationEmpty.xml";
			_mapTester.Execute<NZCustoms2MessageGateway>(source, expected);
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZTransformation_SendMessage2NZCustomsReply()
		{
			string source = "TestFiles.SendMessage.xml";
			string expected = "TestFiles.NZCustomsReply.xml";
			_mapTester.Execute<SendMessage2NZCustomsReply>(source, expected);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;


	}
}
