using System;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property)]
	internal sealed class SkipForRefreshAttribute : Attribute
	{
	}
}
