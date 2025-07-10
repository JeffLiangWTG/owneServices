using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks.Test
{
	sealed class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = (CommonConsol)factory.New<Integration.Forwarding.IForwardingConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		public CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
					container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				}
				return container;
			}
		}
		public CommonContainer container;

		public CommonContainer Container2
		{
			get
			{
				if (container2 == null)
				{
					container2 = Consol.Containers.AddNew();
					container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				}
				return container2;
			}
		}
		CommonContainer container2;
	}
}
