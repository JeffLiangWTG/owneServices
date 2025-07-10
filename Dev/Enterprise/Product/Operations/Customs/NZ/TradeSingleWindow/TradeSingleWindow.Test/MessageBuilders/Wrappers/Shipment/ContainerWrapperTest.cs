using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class ContainerWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ContainerWrapper(null);
		}

		public void TestContainerWrapper()
		{
			Container.JC_ContainerNum = "HKFU0029385";
			Container.JC_SealNum = "F32904TR";
			Container.JC_StowagePosition = "15 07 30";
			AssertNotNull("wrappedContainer", ContainerWrapper);
			AssertEquals("ContainerNumber", "HKFU0029385", ContainerWrapper.ContainerNumber);
			AssertEquals("ContainerStatus", ContainerStatusList.Codes.C7, ContainerWrapper.Status);
			ZString sealNumbers = ZString.Empty;
			foreach (ZString sealNo in ContainerWrapper.SealNumbers)
			{
				sealNumbers += sealNo;
			}

			AssertEquals("ContainerSealsNumbers", "F32904TR", sealNumbers);
			AssertEquals("IsPallet", false, ContainerWrapper.IsPallet);
		}

		#region Implementation
		ITransportEquipment ContainerWrapper
		{
			get
			{
				if (fContainerWrapper == null)
				{
					fContainerWrapper = new ContainerWrapper(Container);
				}

				return fContainerWrapper;
			}
		}
		ITransportEquipment fContainerWrapper;

		ForwardingContainer Container
		{
			get
			{
				if (fcontainern == null)
				{
					fcontainern = Factory.NewWithValidTestData<ForwardingContainer>();
				}

				return fcontainern;
			}
		}
		ForwardingContainer fcontainern;
		#endregion
	}
}
