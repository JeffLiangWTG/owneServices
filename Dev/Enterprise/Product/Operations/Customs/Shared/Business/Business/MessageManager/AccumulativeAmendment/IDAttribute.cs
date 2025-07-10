using System;

namespace Enterprise.Customs.Business.AccumulativeAmendment
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class IDAttribute : Attribute
	{
	}
}
