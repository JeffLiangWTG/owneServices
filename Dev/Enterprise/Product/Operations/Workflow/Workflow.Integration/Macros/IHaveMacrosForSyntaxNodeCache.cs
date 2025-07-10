using System;

namespace Enterprise.Workflow.Integration
{
	public interface IHaveMacrosForSyntaxNodeCache
	{
		Guid Identifier { get; }
		string ParentTable { get; }
		string[] GetAllMacrosForSyntaxNodeCache();
	}
}
