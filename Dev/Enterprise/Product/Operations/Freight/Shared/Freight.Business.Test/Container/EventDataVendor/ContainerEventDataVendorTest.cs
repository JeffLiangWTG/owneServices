using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerEventDataVendorTest : TestCaseWithFactory
	{
		#region Factory Method

		public void TestNew_OneStopVendor()
		{
			GlbCompany.CurrentCompany.SetCountry("NZ");
			AssertEquals("SS clients should get the OneStop vendor", true, ContainerEventDataVendor.Instance is Integration.SailingDataVendor.IOneStopContainerEventDataVendor);
			GlbCompany.CurrentCompany.SetCountry("ER");
			AssertEquals("Any country without a sailing schedule vendor should return a no-action vendor", typeof(NoActionContainerEventDataVendor), ContainerEventDataVendor.Instance.GetType());
			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("Australian clients should get the OneStop vendor", true, ContainerEventDataVendor.Instance is Integration.SailingDataVendor.IOneStopContainerEventDataVendor);
		}
		#endregion

		#region NotifyContainerCreated / NotifyContainerNumberChanged

		public void TestNotifyContainerCreated()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyContainerCreated(Container);
				AssertEquals(true, MockContainerEventDataVendor.Instance.NotifyContainerCreatedCalled);
			}
		}

		public void TestNotifyContainerNumberChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyContainerNumberChanged(Container);
				AssertEquals(true, MockContainerEventDataVendor.Instance.NotifyContainerNumberChangedCalled);
			}
		}

		#endregion

		#region NotifyETDChanged / NotifyETAChanged

		public void TestNotifyETDChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyETDChanged(Container, null);
				AssertEquals(true, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers.Contains(Container));
			}
		}

		public void TestNotifyETAChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyETAChanged(Container, null);
				AssertEquals(true, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers.Contains(Container));
			}
		}

		#endregion

		#region NotifyVesselChanged / NotifyVoyageFlightChanged / NotifyDischargePortChanged

		public void TestNotifyVesselChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyVesselChanged(Container);
				AssertEquals(1, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers.Count);
			}
		}

		public void TestNotifyVoyageFlightChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyVoyageChanged(Container);
				AssertEquals(1, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers.Count);
			}
		}

		public void TestNotifyDischargePortChanged()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				MockContainerEventDataVendor.Instance.NotifyDischargePortChanged(Container);
				AssertEquals(1, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers.Count);
			}
		}

		#endregion

		#region Implementation

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
				}
				return container;
			}
		}
		CommonContainer container;

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		#endregion
	}
}
