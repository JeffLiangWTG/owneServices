using System;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Integration
{
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class FlattenPropertiesFromInheritanceForMacroEvaluationAttribute : Attribute
	{
		public FlattenPropertiesFromInheritanceForMacroEvaluationAttribute(string interfaceName)
		{
			InterfaceName = Argument.NotNullOrEmpty(interfaceName, nameof(interfaceName));
		}

		public string InterfaceName { get; }
	}
}
