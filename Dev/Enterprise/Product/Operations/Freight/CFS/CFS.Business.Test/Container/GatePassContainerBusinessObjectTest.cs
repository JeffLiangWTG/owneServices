using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassContainer))]
	public class GatePassContainerBusinessObjectTest : CFSBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<GatePassContainer>();
		}

		public void TestJobServices()
		{
			GatePassContainer container = Factory.New<GatePassContainer>();
			AssertNotNull("Container should contain a job services collection", container.Services);

			GatePassService fumigation = container.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GatePassContainer loadedContainer = newFactory.Load<GatePassContainer>(container.PK);
			AssertEquals("Job services of container were not loaded correctly", 1, loadedContainer.Services.Count);

			AssertEquals("Precondition - LoadedContainer.Services should be true.", true, container.Services.ReadOnly);
			container.SetReadOnlyIncludingChildren(false);
			AssertEquals("LoadedContainer.Services should be false (JobServices is registered editable).", false, container.Services.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			GatePassContainer container;
			container = Factory.New<GatePassContainer>();
			return container;
		}
	}
}
