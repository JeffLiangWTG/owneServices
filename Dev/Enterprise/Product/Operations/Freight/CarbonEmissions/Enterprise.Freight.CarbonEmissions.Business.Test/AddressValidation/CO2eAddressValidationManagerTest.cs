using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using WTG.AddressCleansing.Common;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class CO2eAddressValidationManagerTest : TestCaseWithFactory
{
	public void TestAddressesNeedValidate()
	{
		var address1 = Factory.New<OrgAddressForTest>();
		address1.GeoLocation = ZGeography.Empty;
		var address2 = Factory.New<OrgAddressForTest>();
		address2.GeoLocation = ZGeography.CreatePoint(10, 10);

		var manager = new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1, address2 });
		AssertContainsExactElementsInAnyOrder(new[] { address1 }, manager.AddressesNeedValidate);
	}

	public void TestAddressesNeedValidate_ExcludeManuallyVerifiedAddresses()
	{
		var address1 = Factory.New<OrgAddressForTest>();
		address1.ValidationStatus = AddressValidationStatus.ManuallyVerified;
		var address2 = Factory.New<OrgAddressForTest>();
		address2.ValidationStatus = AddressValidationStatus.ToBeVerified;

		var manager = new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1, address2 });
		AssertContainsExactElementsInAnyOrder(new[] { address2 }, manager.AddressesNeedValidate);
	}

	public void TestValidateAsync()
	{
		// Arrange
		var address1 = Factory.New<OrgAddressForTest>();
		address1.GeoLocation = ZGeography.Empty;
		address1.ValidateAddressActionForTest = address => address.GeoLocation = ZGeography.CreatePoint(10, 10);

		// Act
		var tokenSource = new CancellationTokenSource();
		new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1 }).ValidateAsync(tokenSource).GetAwaiter().GetResult();

		// Assert
		AssertEquals(ZGeography.CreatePoint(10, 10), address1.GeoLocation);
	}

	public void TestValidateAsync_HandleException()
	{
		// Arrange
		var address1 = Factory.New<OrgAddressForTest>();
		address1.GeoLocation = ZGeography.Empty;
		address1.ValidateAddressActionForTest = address => address.GeoLocation = ZGeography.CreatePoint(10, 10);

		var address2 = Factory.New<OrgAddressForTest>();
		address2.GeoLocation = ZGeography.Empty;
		address2.ValidateAddressActionForTest = address =>
		{
			address.GeoLocation = ZGeography.CreatePoint(20, 20);
			throw new Exception();
		};

		// Act
		var tokenSource = new CancellationTokenSource();
		AssertNoExceptionThrown(() => new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1, address2 }).ValidateAsync(tokenSource).GetAwaiter().GetResult());

		// Assert
		AssertContains("Error validating addresses.", ErrorReporter.LastMessageReported);
		ErrorReporter.Clear();
		AssertEquals(ZGeography.CreatePoint(10, 10), address1.GeoLocation);
		AssertEquals(ZGeography.CreatePoint(20, 20), address2.GeoLocation);
		Assert(tokenSource.IsCancellationRequested);
	}

	public void TestValidateAsync_GeoLocationUpdated_WhenValid()
	{
		// Arrange
		var address1 = Factory.New<OrgAddressForTest>();
		address1.GeoLocation = ZGeography.Empty;
		address1.ValidateAddressActionForTest = address => address.GeoLocation = ZGeography.CreatePoint(10, 10);

		// Act
		var tokenSource = new CancellationTokenSource();
		new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1 }).ValidateAsync(tokenSource).GetAwaiter().GetResult();

		// Assert
		AssertEquals(ZGeography.CreatePoint(10, 10), address1.GeoLocation);
	}

	public void TestValidateAsync_GeoLocationStillUpdated_WhenInvalid()
	{
		// Arrange
		var address1 = Factory.New<OrgAddressForTest>();
		address1.GeoLocation = ZGeography.Empty;
		address1.ValidateAddressActionForTest = address => address.GeoLocation = ZGeography.CreatePoint(10, 10);
		address1.ValidationStatusForTest = AddressValidationStatus.Invalid;

		// Act
		var tokenSource = new CancellationTokenSource();
		new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1 }).ValidateAsync(tokenSource).GetAwaiter().GetResult();

		// Assert
		AssertEquals(ZGeography.CreatePoint(10, 10), address1.GeoLocation);
	}

	public void TestValidate()
	{
		try
		{
			// Arrange
			var address = Factory.New<OrgAddressForTest>();
			address.GeoLocation = ZGeography.Empty;
			CO2eAddressValidationManager.ValidateAddressActionForTesting = x => x.GeoLocation = ZGeography.CreatePoint(10, 10);

			// Act
			new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address }).Validate();

			// Assert
			AssertEquals(ZGeography.CreatePoint(10, 10), address.GeoLocation);
		}
		finally
		{
			CO2eAddressValidationManager.ValidateAddressActionForTesting = null;
		}
	}

	public void TestValidate_HandleException()
	{
		try
		{
			// Arrange
			var address1 = Factory.New<OrgAddressForTest>();
			address1.GeoLocation = ZGeography.Empty;

			var address2 = Factory.New<OrgAddressForTest>();
			address2.GeoLocation = ZGeography.Empty;
			CO2eAddressValidationManager.ValidateAddressActionForTesting = address =>
			{
				if (address.EntityPK == address1.PK)
				{
					address.GeoLocation = ZGeography.CreatePoint(10, 10);
				}
				else if (address.EntityPK == address2.PK)
				{
					address.GeoLocation = ZGeography.CreatePoint(20, 20);
					throw new Exception();
				}
			};

			// Act
			new CO2eAddressValidationManager(new ISupportWebAddressValidation[] { address1, address2 }).Validate();

			// Assert
			AssertContains("Error validating addresses.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertEquals(ZGeography.CreatePoint(10, 10), address1.GeoLocation);
			AssertEquals(ZGeography.CreatePoint(20, 20), address2.GeoLocation);
		}
		finally
		{
			CO2eAddressValidationManager.ValidateAddressActionForTesting = null;
		}
	}

	sealed class OrgAddressForTest : OrgAddress
	{
		public OrgAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			ValidateAddressActionForTest?.Invoke(this);
			ValidationStatus = ValidationStatusForTest;
			return await Task.FromResult(new WebAddressValidationResult
				{
					ResultAddress = new ValidationResultItem
					{
						ResultStatusCode = ValidationResultStatusCode.PointExact
					}
				});
		}

		internal Action<OrgAddressForTest> ValidateAddressActionForTest { get; set; }
		internal WebAddressValidationResult ValidationResultForTest { get; set; }
		internal ZString ValidationStatusForTest { get; set; } = AddressValidationStatus.Verified;
	}
}
