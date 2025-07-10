using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class IJobAddressAdditionalInfoSupportExtensions
	{
		public static string GetTransportModeOrDefault(this IJobAddressAdditionalInfoSupport support, string addressType, bool isOverride = false)
		{
			var (addressInfo, isDependantAddressEmpty) = GetJobAddressAdditionalInfo(support, addressType, isOverride);

			if ((isDependantAddressEmpty && !isOverride) || addressInfo is null)
			{
				return string.Empty;
			}

			return addressInfo.TransportMode;
		}

		public static void SetTransportMode(this IJobAddressAdditionalInfoSupport support, string addressType, string transporMode)
		{
			if (IsAddressTypeValid(support, addressType) && IsTransportModeValid(transporMode))
			{
				var addressAdditionalInfoItem = support.JobAddressAdditionalInfoCollection.GetOrCreate(addressType);
				if (string.IsNullOrEmpty(transporMode))
				{
					addressAdditionalInfoItem.Delete();
				}
				else
				{
					addressAdditionalInfoItem.TransportMode = transporMode;
				}
			}
		}

		public static void ValidateAdressType(this IJobAddressAdditionalInfoSupport support, string type)
		{
			var typeAttribute = support.GetJobAddressAdditionalInfoAddressTypesAttribute();

			if (typeAttribute != null)
			{
				if (!typeAttribute.Types.Contains(type))
				{
					ErrorReporter.ReportOnce("ValidateAdressType - type not allowed",
						new ArgumentException($"Type '{type}' is not a valid Address type for the AddressAdditionalInfoCollection of {support.GetType().Name}."));
				}
			}
			else if (!string.IsNullOrEmpty(type))
			{
				ErrorReporter.ReportOnce("ValidateAdressType - type not allowed",
					new ArgumentException($"Type '{type}' is not allowed. Only the default type is acceptable for the AddressAdditionalInfoCollection of {support.GetType().Name} without a AddressTypes attribute."));
			}
		}

		public static void HookEventsToDependentAddresses(this IJobAddressAdditionalInfoSupport support)
		{
			var typeAttribute = support.GetJobAddressAdditionalInfoAddressTypesAttribute();
			if (typeAttribute != null)
			{
				typeAttribute.Types.ForEach(addressType =>
				{
					var dependentAddress = support.GetDependentAddress(addressType);
					if (dependentAddress != null)
					{
						dependentAddress.ValueChanged += (s, e) =>
						{
							var jobDocAddress = (s as JobDocAddress);
							if (jobDocAddress == null)
							{
								DeleteBindedAddressAdditionalInfo(support, addressType);
							}
							else if (!jobDocAddress.E2_AddressOverride && !((ValueChangedEventArgs)e).OldValue.IsEmpty)
							{
								DeleteBindedAddressAdditionalInfo(support, addressType);
							}
						};
					}
				});
			}
		}

		static JobAddressAdditionalInfoAddressTypesAttribute GetJobAddressAdditionalInfoAddressTypesAttribute(this IJobAddressAdditionalInfoSupport parent)
		{
			var addressAdditionalInfoCollectionProperty = parent.GetType().GetProperty(nameof(IJobAddressAdditionalInfoSupport.JobAddressAdditionalInfoCollection));
			return addressAdditionalInfoCollectionProperty?.GetCustomAttributes(typeof(JobAddressAdditionalInfoAddressTypesAttribute), false).FirstOrDefault() as JobAddressAdditionalInfoAddressTypesAttribute;
		}

		static bool IsAddressEmpty(ZPropertyInfo dependentAddress)
		{
			return (dependentAddress is null) || dependentAddress.Value.IsDefault;
		}

		static (IJobAddressAdditionalInfo, bool) GetJobAddressAdditionalInfo(IJobAddressAdditionalInfoSupport support, string addressType, bool isOverride)
		{
			var dependentAddress = support.GetDependentAddress(addressType);
			var isDependantAdressEmpty = IsAddressEmpty(dependentAddress);
			var result = isDependantAdressEmpty && !isOverride ? null : support.JobAddressAdditionalInfoCollection.Get(addressType);
			return (result, isDependantAdressEmpty);
		}

		static void DeleteBindedAddressAdditionalInfo(IJobAddressAdditionalInfoSupport support, string addressType)
		{
			var addressAdditionalInfo = support.JobAddressAdditionalInfoCollection.GetOrCreate(addressType);
			(addressAdditionalInfo).Delete();
		}

		static bool IsTransportModeValid(ZString transportMode)
		{
			return string.IsNullOrEmpty(transportMode) || new JobAddressAdditionalInfoTransportModeCodeDescriptionPairList().ContainsCode(transportMode);
		}

		static bool IsAddressTypeValid(IJobAddressAdditionalInfoSupport support, string addressType)
		{
			var typeAttribute = support.GetJobAddressAdditionalInfoAddressTypesAttribute()?.Types.Contains(addressType);

			return typeAttribute ?? false;
		}
	}
}
