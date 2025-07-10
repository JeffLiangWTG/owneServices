using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DisbursementBillingHelperTests : BaseFreightTest
	{
		public void TestDisbursementBillingHelper_BothDisabled()
		{
			var mockAccounting = SetupAccountingMock(registryEnabled: false, configEnabled: false);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var helper = new DisbursementBillingHelper();
				AssertEquals(expected: false, helper.IsDisbursementBillingEnabled);
			}
		}

		public void TestDisbursementBillingHelper_RegistryEnabledOnly()
		{
			var mockAccounting = SetupAccountingMock(registryEnabled: true, configEnabled: false);

			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var helper = new DisbursementBillingHelper();
				AssertEquals(expected: false, helper.IsDisbursementBillingEnabled);
			}
		}

		public void TestDisbursementBillingHelper_ConfigurationEnabledOnly()
		{
			var mockAccounting = SetupAccountingMock(registryEnabled: false, configEnabled: true);
			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var helper = new DisbursementBillingHelper();
				AssertEquals(expected: false, helper.IsDisbursementBillingEnabled);
			}
		}

		public void TestDisbursementBillingHelper_BothEnabled()
		{
			var mockAccounting = SetupAccountingMock(registryEnabled: true, configEnabled: true);
			using (ObjectFactory.Substitute(mockAccounting.Object))
			{
				var helper = new DisbursementBillingHelper();
				AssertEquals(expected: true, helper.IsDisbursementBillingEnabled);
			}
		}

		Mock<IAccounting> SetupAccountingMock(bool registryEnabled, bool configEnabled)
		{
			var registryItem = ObjectFactory.Get<IAccounting>().Registry.EnableElectronicProcessingChargeFunctionality as BooleanRegistryItem;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnabled);

			var mockRegistry = new Mock<IRegistry>();
			mockRegistry.Setup(r => r.EnableElectronicProcessingChargeFunctionality).Returns(registryItem);

			var mockAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			mockAccounting.Setup(a => a.Registry).Returns(mockRegistry.Object);
			mockAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>()))
						  .Returns(configEnabled);

			return mockAccounting;
		}
	}
}
