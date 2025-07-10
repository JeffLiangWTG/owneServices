//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDeliveryOrderContainerAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDeliveryOrderContainerAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderContainerAddInfoValidation : AutoUSDeliveryOrderContainerAddInfoValidation
	{
		public USDeliveryOrderContainerAddInfoValidation(AutoUSDeliveryOrderContainerAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_ContainerNumber()
		{
			base.CheckUS_ContainerNumber();
			MandatoryValidation.WarnIfNotEntered(Parent.US_ContainerNumberInfo);
			ListValidation.WarnIfInvalidCode(Parent.US_ContainerNumberInfo, Parent.Lookups.ContainerList, ContainerNumberShouldBeInList);
		}
		internal const string ContainerNumberShouldBeInList = "Please enter a valid Container Number. The number you have selected is not in the Container List.";

		protected override void CheckUS_ContainerType()
		{
			base.CheckUS_ContainerType();
			ListValidation.WarnIfInvalidCode(Parent.US_ContainerTypeInfo, Parent.Lookups.ContainerTypeList, (NoResString)ContainerTypeShouldBeInList);
		}
		internal const string ContainerTypeShouldBeInList = "Please enter a valid Container Type. The type you have selected is not in the Container Type List.";

		protected override void CheckUS_ContainerMode()
		{
			base.CheckUS_ContainerMode();
			ListValidation.WarnIfInvalidCode(Parent.US_ContainerModeInfo, Parent.Lookups.ContainerModeList, ContainerModeShouldBeInList);
		}
		internal const string ContainerModeShouldBeInList = "Please enter a valid Container Mode. The mode you have selected is not in the Container Mode List.";

		protected override void CheckUS_NoOfPackages()
		{
			base.CheckUS_NoOfPackages();
			ValidateUS_PackageType();
		}

		protected override void CheckUS_PackageType()
		{
			base.CheckUS_PackageType();
			if (Parent.US_PackageType.IsEmpty)
			{
				if (Parent.US_NoOfPackages > ZInt.Zero)
				{
					Parent.US_PackageTypeInfo.AddWarning(PackageTypeShouldBeEnteredIfValueIsEntered);
				}
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_PackageTypeInfo, Parent.Lookups.PackageTypeList, PackageTypeShouldBeInList);
			}
		}
		internal const string PackageTypeShouldBeInList = "Please enter a valid Package Type. The type you have selected is not in the Package Type List.";
		internal const string PackageTypeShouldBeEnteredIfValueIsEntered = "A Package Type is required when a quantity is specified.";

		protected override void CheckUS_Weight()
		{
			base.CheckUS_Weight();
			ValidateUS_WeightUQ();
		}

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();
			if (Parent.US_WeightUQ.IsEmpty)
			{
				if (Parent.US_Weight > ZDecimal.Zero)
				{
					Parent.US_WeightUQInfo.AddWarning(WeightUQShouldBeEnteredIfWeightIsEntered);
				}
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_WeightUQInfo, Parent.Lookups.WeightUQList, WeightUQShouldBeInList);
			}
		}
		internal const string WeightUQShouldBeInList = "Please enter a valid Weight UQ. The UQ you have selected is not in the Weight UQ List.";
		internal const string WeightUQShouldBeEnteredIfWeightIsEntered = "A Weight UQ is required when a weight value is specified.";
	}
}
