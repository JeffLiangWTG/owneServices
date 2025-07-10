using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeApportionmentMethodOverrideValidation : AutoAccChargeApportionmentMethodOverrideValidation
	{
		public AccChargeApportionmentMethodOverrideValidation(AutoAccChargeApportionmentMethodOverride parent) : base(parent)
		{
		}

		protected override void CheckAAM_ApportionmentMethod()
		{
			base.CheckAAM_ApportionmentMethod();
			MandatoryValidation.CheckEntered(Parent.AAM_ApportionmentMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_ApportionmentMethodInfo, Parent.Lookups.ApportionmentList);
		}

		protected override void CheckAAM_ConsolType()
		{
			base.CheckAAM_ConsolType();
			MandatoryValidation.CheckEntered(Parent.AAM_ConsolTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_ConsolTypeInfo, Parent.Lookups.ConsolTypeList);
			CheckItIsNotDuplicate(Parent.AAM_ConsolTypeInfo);
		}

		protected override void CheckAAM_ContainerMode()
		{
			base.CheckAAM_ContainerMode();
			MandatoryValidation.CheckEntered(Parent.AAM_ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_ContainerModeInfo, Parent.Lookups.ContainerModeList);
			CheckItIsNotDuplicate(Parent.AAM_ContainerModeInfo);
		}

		protected override void CheckAAM_Direction()
		{
			base.CheckAAM_Direction();
			MandatoryValidation.CheckEntered(Parent.AAM_DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_DirectionInfo, Parent.Lookups.DirectionList);
			CheckItIsNotDuplicate(Parent.AAM_DirectionInfo);
		}

		protected override void CheckAAM_Module()
		{
			base.CheckAAM_Module();
			MandatoryValidation.CheckEntered(Parent.AAM_ModuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_ModuleInfo, Parent.Lookups.ModuleList);
			CheckItIsNotDuplicate(Parent.AAM_ModuleInfo);
		}

		protected override void CheckAAM_TransportMode()
		{
			base.CheckAAM_TransportMode();
			MandatoryValidation.CheckEntered(Parent.AAM_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AAM_TransportModeInfo, Parent.Lookups.TransportModeList);
			CheckItIsNotDuplicate(Parent.AAM_TransportModeInfo);
		}

		void CheckItIsNotDuplicate(ZPropertyInfo propertyInfo)
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			if (propertyInfo.HasErrors())
			{
				return;
			}

			var parentCollection = Parent.ChargeCode.ApportionmentMethodOverrides;
			if (parentCollection.Cast<AccChargeApportionmentMethodOverride>().Any(c => c != Parent && ((AccChargeApportionmentMethodOverride)Parent).IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		internal static string IsDuplicateErrorString => Res.GetString("0cf2c9fe-9219-447d-b428-f872dc9bcbbe", "At least one more record already sets apportionment method for the same Job parameters.");
	}
}
