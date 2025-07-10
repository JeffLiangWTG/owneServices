using System;

namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Use this attribute to signal that there could be triggers defined against a business object despite the fact that it doesn't implement IWorkflowProvider.
	/// <para>Needing this attribute is probably an indication that your test setup data is wrong, or your design doesn't use standard Workflow architecture (which it really should).</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CanHaveTriggersDespiteNotImplementingIWorkflowProviderAttribute : Attribute
	{
	}
}
