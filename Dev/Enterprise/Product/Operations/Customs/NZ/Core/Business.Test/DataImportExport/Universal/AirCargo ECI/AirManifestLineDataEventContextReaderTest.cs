using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.Integration;
using EventContextTypes = Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	class AirManifestLineDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestAddHAWBEventContextValues()
		{
			var hawb = Factory.NewWithValidTestData<CusHAWB>();
			hawb.CS_MasterHouseBill = "TestShipmentHB";

			var eventContext = new List<KeyValuePair<TypeWithDescription, IZType>>();
			var reader = new AirManifestLineDataEventContextReader(hawb);
			InvokeAddHAWBEventContextValuesForTesting(reader, eventContext);
			AssertEquals("Master house bill should be populated", "TestShipmentHB", eventContext.Single(x => x.Key.Type == nameof(EventContextTypes.MasterHouseBill)).Value);
		}

		static void InvokeAddHAWBEventContextValuesForTesting(AirManifestLineDataEventContextReader reader, List<KeyValuePair<TypeWithDescription, IZType>> eventContext)
		{
			var methodInfo = reader.GetType().GetMethod("AddHAWBEventContextValues", BindingFlags.Instance | BindingFlags.NonPublic);
			methodInfo.Invoke(reader, new[] { eventContext });
		}
	}
}
