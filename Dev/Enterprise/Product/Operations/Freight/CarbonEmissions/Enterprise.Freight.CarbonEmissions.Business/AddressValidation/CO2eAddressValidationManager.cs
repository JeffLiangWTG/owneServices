using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.AddressCleansing.Common;

namespace Enterprise.Freight.CarbonEmissions.Business;

public class CO2eAddressValidationManager : IAddressesValidationManager
{
	public CO2eAddressValidationManager(ISupportWebAddressValidation[] addressesToValidate)
	{
		this.addressesToValidate = Argument.NotNull(addressesToValidate, nameof(addressesToValidate));
	}

	readonly ISupportWebAddressValidation[] addressesToValidate;

	public IEnumerable<ISupportWebAddressValidation> AddressesNeedValidate =>
		addressesToValidate.Where(address => address is not null && address.ValidationStatus != AddressValidationStatus.ManuallyVerified && (address.GeoLocation.IsEmpty || !address.GeoLocation.IsValid));

	public async Task ValidateAsync(CancellationTokenSource cancellationTokenSource)
	{
		try
		{
			var addressTaskPairs = AddressesNeedValidate
				.Select(address =>
				{
					var taskTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
					return (address, address.ValidateAddressAsync(taskTokenSource, CleanseAction.QuickValidate));
				})
				.ToArray();

			if (addressTaskPairs.Length > 0)
			{
				await Task.WhenAll(addressTaskPairs.Select(pair => pair.Item2));
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			cancellationTokenSource.Cancel();
			ErrorReporter.ReportDeveloperExceptionOnce("Error validating addresses.", ex);
		}
	}

	public void Validate()
	{
		using (var cancellationToken = new CancellationTokenSource())
		{
			try
			{
				foreach (var address in AddressesNeedValidate)
				{
#if DEBUG
					if (Globals.IsTest)
					{
						ValidateAddressActionForTesting?.Invoke(address);
						continue;
					}
#endif
					var taskTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token);
					var result = AddressValidationService.ValidateAddress(address, taskTokenSource, CleanseAction.QuickValidate);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				cancellationToken.Cancel();
				ErrorReporter.ReportDeveloperExceptionOnce("Error validating addresses.", ex);
			}
		}
	}

#if DEBUG
	[ThreadStatic] internal static Action<ISupportWebAddressValidation> ValidateAddressActionForTesting;
#endif
}
