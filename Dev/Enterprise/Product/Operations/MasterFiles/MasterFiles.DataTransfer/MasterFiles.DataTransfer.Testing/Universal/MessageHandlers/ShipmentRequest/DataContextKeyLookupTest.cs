using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class DataContextKeyLookupTest : TestCaseWithFactory
	{
		public void TestAllDataContextTypesAreMapped()
		{
			var request = new ShipmentRequest();
			var dataContext = new DataContext();
			request.DataContext = dataContext;
			var dataTarget = new DataTarget();
			dataContext.DataTargetCollection = new List<DataTarget>(new[] { dataTarget });

			List<string> dataContextsThatBlewUp = new List<string>();
			foreach (var dataContextType in Enum.GetValues(typeof(DataContextType)))
			{
				dataTarget.Type = dataContextType.ToString();
				try
				{
					var lookup = new DataContextKeyLookup(Factory, new XmlSessionTracker(new ServiceTaskLogForTesting())).Match(request);
				}
				catch (Exception e)
				{
					dataContextsThatBlewUp.Add(dataContextType.ToString() + " - " + e.Message);
				}
			}

			AssertMultilineASCIIEquals("All these should be fine. No Exceptions.", "", String.Join("\r\n", dataContextsThatBlewUp));
		}

		public void TestMatch()
		{
			var logger = new ServiceTaskLogForTesting();

			var consol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C123456789";

			Factory.Save();

			var request = new ShipmentRequest();
			var dataContext = DataContextFactory.New();
			request.DataContext = dataContext;
			dataContext.AddDataTarget(DataContextType.ForwardingConsol, "C123456789");

			var lookup = new DataContextKeyLookup(Factory, new XmlSessionTracker(logger));
			AssertEquals(consol, lookup.Match(request).FirstOrDefault());
		}

		public void TestUnknownContextType()
		{
			var request = new ShipmentRequest();
			var lookup = new DataContextKeyLookup(Factory, new XmlSessionTracker(new ServiceTaskLogForTesting()));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<BusinessObject>(), lookup.Match(request));

			var dataContext = DataContextFactory.New();
			request.DataContext = dataContext;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<BusinessObject>(), lookup.Match(request));
		}
	}
}
