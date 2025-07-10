using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class ContainerStockEventContextReaderTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ContainerStockEventContextReader(null));
			var container = Factory.New<RefContainerStock>();
			container.R6_ContainerNum = ZString.Empty;
			var reader = new ContainerStockEventContextReader(container);
			AssertNoExceptionThrown("", () => reader.AddContainerContextValues(null));
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			reader.AddContainerContextValues(contextValues);
			AssertMultilineASCIIEquals("contextValues", string.Empty, Format(contextValues));
			container.R6_ContainerNum = "ABCD123456";
			reader.AddContainerContextValues(contextValues);
			AssertMultilineASCIIEquals("contextValues", "ContainerNumber|ABCD123456", Format(contextValues));
		}

		string Format(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues != null ? string.Join(System.Environment.NewLine, contextValues.Select(val => string.Concat(val.Key, "|", val.Value)).ToArray()) : string.Empty;
		}
	}
}
