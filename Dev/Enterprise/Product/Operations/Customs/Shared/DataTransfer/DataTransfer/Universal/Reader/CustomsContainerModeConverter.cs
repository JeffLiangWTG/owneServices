using System;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsContainerModeConverter<T>
		where T : IDataObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public CustomsContainerModeConverter(T dataObject, ICodeDescriptionPairList containerModeList, ContainerMode containerMode, Func<T, CustomsContainerModeConverter<T>> subConverterGetter, Func<T, bool> shouldApplyFallbackPredicate)
		{
			this.dataObject = Argument.NotNull(dataObject, nameof(dataObject));
			this.containerModeList = Argument.NotNull(containerModeList, nameof(containerModeList));
			this.containerMode = containerMode;
			this.subConverterGetter = subConverterGetter;
			this.shouldApplyFallbackPredicate = shouldApplyFallbackPredicate;
		}

		readonly T dataObject;
		readonly ICodeDescriptionPairList containerModeList;
		readonly ContainerMode containerMode;
		readonly Func<T, CustomsContainerModeConverter<T>> subConverterGetter;
		readonly Func<T, bool> shouldApplyFallbackPredicate;

		public ContainerMode Convert()
		{
			ContainerMode result = null;
			if (containerMode != null && containerMode.Code.HasValue)
			{
				var containerModeCode = containerMode.Code.Value;
				if (IsValidCustomsContainerMode(containerModeCode))
				{
					result = containerMode;
				}
				else if (containerModeCode == Core.Constants.ContainerModes.BuyersConsol && IsValidCustomsContainerMode(Core.Constants.ContainerModes.FCLMixedShipper))
				{
					result = new ContainerMode { Code = Core.Constants.ContainerModes.FCLMixedShipper };
				}
			}

			if (result == null && subConverterGetter != null)
			{
				var subConverter = subConverterGetter(dataObject);
				if (subConverter != null)
				{
					result = subConverter.Convert();
				}
			}

			if (result == null && (shouldApplyFallbackPredicate?.Invoke(dataObject) ?? false))
			{
				if (IsValidCustomsContainerMode(Core.Constants.ContainerModes.Containerised))
				{
					result = new ContainerMode { Code = Core.Constants.ContainerModes.Containerised };
				}
				else if (IsValidCustomsContainerMode(Core.Constants.ContainerModes.LCL))
				{
					result = new ContainerMode { Code = Core.Constants.ContainerModes.LCL };
				}
			}

			if (result == null)
			{
				result = containerMode;
			}

			return result;
		}

		bool IsValidCustomsContainerMode(ZString? containerModeCode)
		{
			return containerModeCode.HasValue && containerModeList.ContainsCode(containerModeCode.Value);
		}
	}
}
