using System.IO;
using System.Text;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class JSONConvertHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXML2JSON_Array()
		{
			var xml = @"<fulfillment xmlns:json=""http://james.newtonking.com/projects/json"">
  <tracking_number></tracking_number>
  <tracking_company></tracking_company>
  <line_items json:Array=""true"">
    <id>8556084870</id>
    <quantity>1</quantity>
  </line_items>
</fulfillment>";
			var json = @"{
  ""fulfillment"": {
    ""tracking_number"": """",
    ""tracking_company"": """",
    ""line_items"": [
      {
        ""id"": ""8556084870"",
        ""quantity"": ""1""
      }
    ]
  }
}";
			Assert.AreEqual(json, JSONConvertHelper.XML2JSON(new MemoryStream(Encoding.UTF32.GetBytes(xml))));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestJSON2XML()
		{
			var json = @"
{
  ""fulfillment"": {
    ""tracking_number"": ""123456789010"",
    ""tracking_company"": ""fed ex"",
    ""date"": ""2017-08-28T07:00:00-07:00"",
    ""line_items"": [
      {
        ""id"": 466157049
      },
      {
        ""id"": 518995019
      },
      {
        ""id"": 703073504
      }
    ]
  }
}";
			var xml = @"<fulfillment>
  <tracking_number>123456789010</tracking_number>
  <tracking_company>fed ex</tracking_company>
  <date>2017-08-28T07:00:00-07:00</date>
  <line_items>
    <id>466157049</id>
  </line_items>
  <line_items>
    <id>518995019</id>
  </line_items>
  <line_items>
    <id>703073504</id>
  </line_items>
</fulfillment>";
			Assert.AreEqual(xml, JSONConvertHelper.JSON2XML(json).Beautify());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestJSON2XML_Array()
		{
			var json = @"{
  ""fulfillment"": {
    ""tracking_number"": """",
    ""tracking_company"": """",
    ""line_items"": [
      {
        ""id"": ""8556084870"",
        ""quantity"": ""1""
      }
    ]
  }
}";
			var xml = @"<fulfillment>
  <tracking_number></tracking_number>
  <tracking_company></tracking_company>
  <line_items json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <id>8556084870</id>
    <quantity>1</quantity>
  </line_items>
</fulfillment>";
			var result = JSONConvertHelper.JSON2XML(json).Beautify();
			Assert.AreEqual(xml, result);
			Assert.AreEqual(json, JSONConvertHelper.XML2JSON(new MemoryStream(Encoding.UTF32.GetBytes(result))));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXML2JSON_ComplexNamespaces_Array()
		{
			var xml = @"<ns0:fulfillment xmlns:ns0=""http://cargowise.com/ehub/clients/RIM/2016/11"" xmlns=""Test"" xmlns:ns1=""Test1"">
  <ns1:tracking_number attributeTest=""Test"">123456789010</ns1:tracking_number>
  <tracking_number ns0:attributeTest=""Test"">123456789010</tracking_number>
  <ns1:tracking_company>fed ex</ns1:tracking_company>
  <ns0:line_items json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <id>8556084870</id>
    <quantity>1</quantity>
  </ns0:line_items>
</ns0:fulfillment>";
			var json = @"{
  ""fulfillment"": {
    ""tracking_number"": [
      {
        ""@attributeTest"": ""Test"",
        ""#text"": ""123456789010""
      },
      {
        ""@attributeTest"": ""Test"",
        ""#text"": ""123456789010""
      }
    ],
    ""tracking_company"": ""fed ex"",
    ""line_items"": [
      {
        ""id"": ""8556084870"",
        ""quantity"": ""1""
      }
    ]
  }
}";
			Assert.AreEqual(json, JSONConvertHelper.XML2JSON(new MemoryStream(Encoding.UTF32.GetBytes(xml))));
		}
	}
}
