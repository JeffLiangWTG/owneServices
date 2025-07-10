//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWorkProjectValidation
//
//    This class should be used for overriding validation in AutoWorkProjectValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProcessManagement.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using ZArchitecture.Environment;

	public class WorkProjectValidation : AutoWorkProjectValidation
	{
		public WorkProjectValidation(AutoWorkProject parent)
			: base(parent)
		{ }

		new Project Parent
		{
			get { return (Project)base.Parent; }
		}

		protected override void CheckWKP_Type()
		{
			CheckActiveCode(Parent.WKP_TypeInfo, Parent.Lookups.AllTypes, Parent.Lookups.ActiveTypes);
			CheckMandatoryField(ProcessManagementRegistry.Instance.ProjectTypeMandatory, Parent.WKP_TypeInfo, Parent.Lookups.ActiveTypes);
		}

		protected override void CheckWKP_SubType()
		{
			CheckActiveCode(Parent.WKP_SubTypeInfo, Parent.Lookups.AllSubtypes, Parent.Lookups.ActiveSubtypes);
			CheckMandatoryField(ProcessManagementRegistry.Instance.ProjectSubTypeMandatory, Parent.WKP_SubTypeInfo, Parent.Lookups.ActiveSubtypes);
		}

		protected override void CheckWKP_Module()
		{
			CheckActiveCode(Parent.WKP_ModuleInfo, Parent.Lookups.AllModules, Parent.Lookups.ActiveModules);
			CheckMandatoryField(ProcessManagementRegistry.Instance.ProjectModuleMandatory, Parent.WKP_ModuleInfo, Parent.Lookups.ActiveModules);
		}

		protected override void CheckWKP_Priority()
		{
			CheckActiveCode(Parent.WKP_PriorityInfo, Parent.Lookups.AllPriorities, Parent.Lookups.ActivePriorities);
			CheckMandatoryField(ProcessManagementRegistry.Instance.ProjectPriorityMandatory, Parent.WKP_PriorityInfo, Parent.Lookups.ActivePriorities);
		}

		protected override void CheckWKP_Status()
		{
			if (!Parent.WKP_Status.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WKP_StatusInfo);
			}
		}

		protected override void CheckWKP_Summary()
		{
			MandatoryValidation.CheckEntered(Parent.WKP_SummaryInfo);
		}

		void CheckActiveCode(ZPropertyInfo info, ICodeDescriptionPairList all, ICodeDescriptionPairList active)
		{
			if (!info.Value.IsEmpty)
			{
				if (!Parent.IsInDatabase || info.HasChanges || !all.ContainsCode(info.Value))
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(info, active, ListValidation.InactiveCodeMessage);
				}
			}
		}

		void CheckMandatoryField(BooleanRegistryItem registryItem, ZPropertyInfo info, ICodeDescriptionPairList valueList)
		{
			if (registryItem.Value && info.Value.IsEmpty && valueList.Count > 0)
			{
				MandatoryValidation.CheckEntered(info);
			}
		}
	}
}
