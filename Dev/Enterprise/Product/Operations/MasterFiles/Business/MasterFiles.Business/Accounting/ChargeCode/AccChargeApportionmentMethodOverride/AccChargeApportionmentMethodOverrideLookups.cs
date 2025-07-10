using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeApportionmentMethodOverrideLookups : AutoAccChargeApportionmentMethodOverrideLookups
	{
		public AccChargeApportionmentMethodOverrideLookups(AutoAccChargeApportionmentMethodOverride parent) : base(parent)
		{
		}

		new AccChargeApportionmentMethodOverride Parent => (AccChargeApportionmentMethodOverride)base.Parent;

		public CodeDescriptionPairList ApportionmentList => ApportionmentMethodOverrideLookupsHelper.ApportionmentList(Parent.AAM_Module);

		public CodeDescriptionPairList ConsolTypeList => ApportionmentMethodOverrideLookupsHelper.ConsolTypeList(Parent.AAM_TransportMode, Parent.AAM_Module);

		public CodeDescriptionPairList DirectionList => ApportionmentMethodOverrideLookupsHelper.DirectionList(Parent.AAM_Module);

		public CodeDescriptionPairList ModuleList => ApportionmentMethodOverrideLookupsHelper.ModuleList;

		public CodeDescriptionPairList ContainerModeList => ApportionmentMethodOverrideLookupsHelper.ContainerModeList(Parent.AAM_ConsolType, Parent.AAM_TransportMode, Parent.AAM_Module);

		public CodeDescriptionPairList TransportModeList => ApportionmentMethodOverrideLookupsHelper.TransportModeList(Parent.AAM_Module);
	}
}
