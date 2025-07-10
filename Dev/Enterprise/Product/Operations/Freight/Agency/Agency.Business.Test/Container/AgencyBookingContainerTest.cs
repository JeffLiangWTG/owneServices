using System;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyBookingContainerTest : BaseFreightTest
	{
		public void TestValidation()
		{
			AgencyBookingContainer container = Factory.NewWithValidTestData<AgencyBookingContainer>();
			AssertEquals(typeof(AgencyBookingContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(typeof(AgencyRORContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(typeof(AgencyTopLevelPackValidation), container.Validation.GetType());
		}

		public void TestDefaultWeightUnit()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			CommonContainer container1 = shipment.RealContainers.AddNew();
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			CommonContainer container2 = shipment.RealContainers.AddNew();
			AssertEquals("Container1", Constants.Weight.Kilograms, container1.JC_GrossWeightUQ);
			AssertEquals("Container2", Constants.Weight.Tonnes, container2.JC_GrossWeightUQ);
		}

		public void TestDefaultVolumeUnit()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			CommonContainer container1 = shipment.RealContainers.AddNew();
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicFeet);
			CommonContainer container2 = shipment.RealContainers.AddNew();
			AssertEquals("Container1", Constants.Volume.CubicMetres, container1.JC_GrossVolumeUQ);
			AssertEquals("Container2", Constants.Volume.CubicFeet, container2.JC_GrossVolumeUQ);
		}

		public void TestCantSetContainerCountToANonPositiveValue()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			AgencyBookingContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerCount = 4;
			AssertEquals("Container.JC_ContainerCount", 4, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = -4;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 5;
			AssertEquals("Container.JC_ContainerCount", 5, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 0;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
		}
	}
}
