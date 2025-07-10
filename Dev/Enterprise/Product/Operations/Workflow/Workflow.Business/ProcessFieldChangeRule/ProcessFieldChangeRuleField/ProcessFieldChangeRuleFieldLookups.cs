//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessFieldChangeRuleFieldLookups
//
//    This class should be used for overriding collections in AutoProcessFieldChangeRuleFieldLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleFieldLookups : AutoProcessFieldChangeRuleFieldLookups
	{
		public ProcessFieldChangeRuleFieldLookups(AutoProcessFieldChangeRuleField parent) : base(parent)
		{
			this.parent = (ProcessFieldChangeRuleField)parent;
		}
		readonly ProcessFieldChangeRuleField parent;

		public CodeDescriptionPairList FieldAndTableNames
		{
			get
			{
				if (parent.Parent != null)
				{
					return parent.Parent.Lookups.FieldAndTableNames;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList Fields
		{
			get
			{
				if (parent.Parent != null)
				{
					return parent.Parent.Lookups.Fields;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList Tables
		{
			get
			{
				if (parent.Parent != null)
				{
					return parent.Parent.Lookups.Tables;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}
	}
}

