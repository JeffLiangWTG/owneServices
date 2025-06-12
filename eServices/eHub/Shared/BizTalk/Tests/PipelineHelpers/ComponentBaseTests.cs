using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;

namespace CargoWise.eHub.Shared.BizTalk.Tests.PipelineHelpers
{
	[TestClass]
	public class ComponentBaseTests
	{
		class TestComponent : ComponentBase
		{
			protected override string DisplayName { get { return "Test Component"; } }
			protected override Guid ClassID { get { return new Guid("00000000-0000-0000-0000-000000000000"); } }

			public string TestString { get; set; }
			public bool TestBoolTrue { get; set; }
			public bool TestBoolFalse { get; set; }
			public int TestInt { get; set; }
			public long TestLong { get; set; }
			public short TestShort { get; set; }
			public SchemaWithNone TestSchema { get; set; }
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ComponentBase_LoadSave()
		{
			var testComponent = new TestComponent
			{
				TestString = "TESTSTRING",
				TestBoolTrue = true,
				TestBoolFalse = false,
				TestInt = int.MaxValue,
				TestLong = long.MaxValue,
				TestShort = short.MaxValue,
				TestSchema = new SchemaWithNone("Schema")
			};

			Assert.AreEqual("Test Component", testComponent.Name);
			Assert.AreEqual("Test Component", testComponent.Description);
			Assert.AreEqual("1.0", testComponent.Version);
			Assert.AreEqual(IntPtr.Zero, testComponent.Icon);
			Assert.AreEqual(null, testComponent.Validate(new object()));
			
			Guid classId;
			testComponent.GetClassID(out classId);
			Assert.AreEqual(new Guid("00000000-0000-0000-0000-000000000000"), classId);

			var xmlRdr = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes("<Properties/>")));
			xmlRdr.Read();
			IPropertyBag testPropertyBag = new InstConfigPropertyBag(xmlRdr);
			testComponent.Save(testPropertyBag, false, true);

			var resultComponent = new TestComponent();
			resultComponent.Load(testPropertyBag, 0);

			Assert.AreEqual(testComponent.TestString, resultComponent.TestString);
			Assert.AreEqual(testComponent.TestBoolTrue, resultComponent.TestBoolTrue);
			Assert.AreEqual(testComponent.TestBoolFalse, resultComponent.TestBoolFalse);
			Assert.AreEqual(testComponent.TestInt, resultComponent.TestInt);
			Assert.AreEqual(testComponent.TestLong, resultComponent.TestLong);
			Assert.AreEqual(testComponent.TestShort, resultComponent.TestShort);
			Assert.AreEqual(testComponent.TestSchema, resultComponent.TestSchema);
		}
	}
}
