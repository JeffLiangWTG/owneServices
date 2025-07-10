using System;

namespace Enterprise.eTail.DataTransfer;

[AttributeUsage(AttributeTargets.Class)]
public sealed class RequiredFeatureControlCodeAttribute : Attribute
{
	public RequiredFeatureControlCodeAttribute(string featureControlCode = default, string parameters = default)
	{
		FeatureControlCode = featureControlCode;
		Parameters = parameters;
	}

	public readonly string FeatureControlCode;
	public readonly string Parameters;
}

