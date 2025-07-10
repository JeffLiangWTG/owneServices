using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.GUI.DangerousGoods;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing.DangerousGoods
{
	public class UrlGeneratorTest : TestCaseWithFactory
	{
		public void Test_EndPointShouldBeSelectedBasedOnMappingAndTheTypeOfTheBusinessEntity()
		{
			var mapping = new Dictionary<string, string>
			{
				{ "CargoWise.EntityFramework.Testing.DummyBusinessObject", "Goto/DummyEndPoint" }
			};

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var testBusinessObject = Factory.New<DummyBusinessObject>();
			var urlGenerator = new UrlGenerator(mapping);
			var url = urlGenerator.GenerateUrl(testBusinessObject);
			AssertContains("https://address/Goto/DummyEndPoint", url);
		}

		public void Test_ExceptionShouldBeThrownIfTheMappingDoesNotContainTheTypeOfTheBusinessEntity()
		{
			var mapping = new Dictionary<string, string>()
			{
				{ "Some type other than the type of the business entity", "Goto/DummyEndPoint" }
			};

			var testBusinessObject = Factory.New<DummyBusinessObject>();
			var urlGenerator = new UrlGenerator(mapping);
			AssertExceptionThrown<ArgumentException>(() => urlGenerator.GenerateUrl(testBusinessObject));
		}
	}
}
