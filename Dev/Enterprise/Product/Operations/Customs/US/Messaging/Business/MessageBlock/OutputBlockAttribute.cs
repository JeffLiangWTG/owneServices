using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class OutputBlockAttribute : BlockAttribute
	{
		public OutputBlockAttribute(string mandatoryCharacters, string version = "")
			: base(mandatoryCharacters, version)
		{
		}
	}
}
