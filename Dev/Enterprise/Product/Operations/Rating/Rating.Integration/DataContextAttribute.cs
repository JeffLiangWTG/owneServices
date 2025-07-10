using System;

using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Integration
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DataContextAttribute : Attribute
	{
		public DataContextAttribute(params DataContext[] dataContexts)
		{
			this.DataContexts = dataContexts;
		}

		public readonly DataContext[] DataContexts;
	}
}

