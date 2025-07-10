using System;
using System.ServiceModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DDDUserNamePasswordValidatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidate_ShouldValidateBasedOnDDDRegistryValue()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test"))
			{
				var validator = new DDDUserNamePasswordValidator();
				validator.Validate("test", "test");
			}

			Assert(true);
		}

		public void TestValidate_ThrowsExceptionOnInvalidUsername()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test"))
			{
				try
				{
					var validator = new DDDUserNamePasswordValidator();
					validator.Validate("test1", "test");
				}
				catch (FaultException ex)
				{
					AssertEquals("Username or Password invalid.", ex.Message);
				}
			}

			Assert(true);
		}

		public void TestValidate_ThrowsExceptionOnInvalidPassword()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|test"))
			{
				try
				{
					var validator = new DDDUserNamePasswordValidator();
					validator.Validate("test", "test1");
				}
				catch (FaultException ex)
				{
					AssertEquals("Username or Password invalid.", ex.Message);
				}
			}

			Assert(true);
		}

		public void TestValidate_ThrowsExceptionWhenRegistryIsEmpty()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				try
				{
					var validator = new DDDUserNamePasswordValidator();
					validator.Validate("test", "test");
				}
				catch (FaultException ex)
				{
					AssertEquals("Credentials for authenticating user are not configured, please configure registry 'Delivery Due Date API Inbound Authentications'.", ex.Message);
				}
			}

			Assert(true);
		}

		public void TestValidate_ThrowsExceptionWhenRegistryIsNotComplete()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			{
				try
				{
					var validator = new DDDUserNamePasswordValidator();
					validator.Validate("test", "test");
				}
				catch (FaultException ex)
				{
					AssertEquals("Credentials for registry item 'Delivery Due Date API Inbound Authentications' is not configured correctly. Please specify both username and password and they should not contain '|' character.", ex.Message);
				}
			}

			Assert(true);
		}

		public void TestValidate_ThrowsExceptionWhenRegistryContainsInvalidCharacter()
		{
			using (FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test|te|st"))
			{
				try
				{
					var validator = new DDDUserNamePasswordValidator();
					validator.Validate("test", "test");
				}
				catch (FaultException ex)
				{
					AssertEquals("Credentials for registry item 'Delivery Due Date API Inbound Authentications' is not configured correctly. Please specify both username and password and they should not contain '|' character.", ex.Message);
				}
			}

			Assert(true);
		}
	}
}
