using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	sealed class ContainerWrapperTest : TestCaseWithFactory
	{
		public void TestContainerWrapper()
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerNum = "HKFU0029385";
			container.JC_GrossWeight = 3.85m;
			container.JC_GrossWeightUQ = "T";
			container.JC_SealNum = "F32904TR";
			container.JC_AdditionalSealNum = "27389Z";
			container.JC_ContainerMode = "BLK";

			Consol.JK_MasterBillNum = "TESTBILL123";
			var shipment1 = Consol.Shipments.AddNew();
			var outerPack = shipment1.OuterPackLines.AddNew();
			outerPack.Containers.Add(container);

			var wrappedContainer = new ContainerWrapper(outerPack.PK, container);
			AssertEquals("ContainerNumber", "HKFU0029385", wrappedContainer.ContainerNumber);
			AssertEquals("ContainerMode", "BLK", wrappedContainer.ContainerMode);
			AssertEquals("ContainerStatus", ContainerStatusList.Codes.C8, wrappedContainer.Status);
			var sealNumbers = ZString.Empty;
			foreach (ZString sealNo in wrappedContainer.SealNumbers)
			{
				sealNumbers += sealNo;
			}
			AssertEquals("ContainerSealsNumbers", "F32904TR27389Z", sealNumbers);
			AssertEquals("IsPallet", false, wrappedContainer.IsPallet);
		}

		#region Implementation

		public ForwardingConsol Consol
		{
			get { return consol ?? (consol = Factory.NewWithValidTestData<ForwardingConsol>()); }
		}
		ForwardingConsol consol;

		#region Container
		public ForwardingContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Consol.Containers.AddNew();
				}
				return fContainer;
			}
		}
		ForwardingContainer fContainer;
		#endregion

		#endregion
	}
}
