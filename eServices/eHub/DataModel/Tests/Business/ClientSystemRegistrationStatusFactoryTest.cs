using System;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.DataModel.Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.DataModel.Tests.Business
{
	[TestClass]
	public class ClientSystemRegistrationStatusFactoryTest
	{
		[TestMethod]
		public void TestGetClientSystemRegistrationStatus()
		{
			try
			{
				var statusList = ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus("Sample1");
				var dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

				Assert.AreEqual(4, statusList.Count, "The dictionary will have 4 pair of status code and description for IT customs.");
				Assert.AreEqual("0: Invalid; 1: Valid; 2: Unknown; 255: Disabled", string.Join("; ", dictionaryPair));

				statusList = ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus("Sample2");
				dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));

				Assert.AreEqual(2, statusList.Count, "The dictionary will have 2 pair of status code and description for SG Customs.");
				Assert.AreEqual("0: Invalid; 1: Valid", string.Join("; ", dictionaryPair));

				statusList = ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus("NewProduct");
				Assert.AreEqual(0, statusList.Count, "The dictionary will have no key and value pair for new product.");

				statusList = ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus("NEXDOC_Client");
				dictionaryPair = statusList.OrderBy(k => k.Key).Select(k => k.Key + ": " + string.Join(", ", k.Value));
				Assert.AreEqual(2, statusList.Count, "The dictionary will have 2 pair of status code and description for NEXDOC Client.");
				Assert.AreEqual("0: Invalid; 1: Valid", string.Join("; ", dictionaryPair));
			}
			catch (ReflectionTypeLoadException ex)
			{
				Assert.AreEqual("", string.Join(Environment.NewLine, ex.LoaderExceptions.Select(e => e.ToString())));
			}
		}
	}
}
