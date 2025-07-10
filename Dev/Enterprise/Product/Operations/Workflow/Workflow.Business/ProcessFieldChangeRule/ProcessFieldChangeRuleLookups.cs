//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessFieldChangeRuleLookups
//
//    This class should be used for overriding collections in AutoProcessFieldChangeRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleLookups : AutoProcessFieldChangeRuleLookups
	{
		public ProcessFieldChangeRuleLookups(AutoProcessFieldChangeRule parent) : base(parent)
		{
		}

		public new ProcessFieldChangeRule Parent => (ProcessFieldChangeRule)base.Parent;

		public CodeDescriptionPairList Types => Factory.GetCachedValue<WorkflowDescriptorList>();

		public GlbStaffCollection Staffs => Factory.GetCachedValue("PFRLookups.Staffs", () => new GlbStaffCollection(Factory));

		public new StmCustomizableEventCodeDescriptionPairList Events => Factory.GetCachedValue("StmCustomizableEventCodeDescriptionPairList", () => new StmCustomizableEventCodeDescriptionPairList(Factory));

		public CodeDescriptionPairList FieldAndTableNames => ConfigurationManager.GetFieldAndTableNames(Parent.PFR_ProcessType);

		public CodeDescriptionPairList Fields => ConfigurationManager.GetFields(Parent.PFR_ProcessType);

		public CodeDescriptionPairList Tables => ConfigurationManager.GetTables(Parent.PFR_ProcessType);

		ProcessFieldChangeRuleConfigurationManager ConfigurationManager
		{
			get
			{
				if (configurationManager == null)
				{
					configurationManager = new ProcessFieldChangeRuleConfigurationManager();
				}
				return configurationManager;
			}
		}
		ProcessFieldChangeRuleConfigurationManager configurationManager;
	}
}

