using System;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class ImportRelatedActivityPromptUserDeciderAttribute : Attribute
	{
		public ImportRelatedActivityPromptUserDeciderAttribute(string typeName)
		{
			this.typeName = typeName;
		}

		public string TypeName
		{
			get { return typeName; }
		}
		readonly string typeName;
	}
}
