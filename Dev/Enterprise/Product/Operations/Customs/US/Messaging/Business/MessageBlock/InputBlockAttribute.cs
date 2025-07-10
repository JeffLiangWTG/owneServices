using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class InputBlockAttribute : BlockAttribute
	{
		public InputBlockAttribute(string mandatoryCharacters, string version = "")
			: base(mandatoryCharacters, version)
		{
		}
	}
}
