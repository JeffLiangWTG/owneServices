using System;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public static partial class ContainerMovementCountryProcessorFactory
	{
		public static ICMMCountryProcessor NewProcessor(CMMEmailGenerator emailBuilder, ICMMProcessingAdapter adapter)
		{
			Argument.NotNull(emailBuilder, "emailBuilder");
			Argument.NotNull(adapter, "adapter");

#if DEBUG
			if (Globals.IsTest && processorOverride != null)
			{
				return processorOverride(emailBuilder, adapter);
			}
#endif
			switch (adapter.CountryCode)
			{
				case Constants.CountryCodes.Australia:
					return new CMMAUCountryProcessor(emailBuilder, adapter);

				default:
					return new CMMNullCountryProcessor();
			}
		}
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	partial class ContainerMovementCountryProcessorFactory
	{
		public delegate ICMMCountryProcessor NewProcessorOverride(CMMEmailGenerator emailBuilder, ICMMProcessingAdapter adapter);

		public static IDisposable OverrideNewProcessor(NewProcessorOverride newProcessorOverride)
		{
			if (newProcessorOverride == null)
			{
				throw new ArgumentNullException(nameof(newProcessorOverride));
			}

			if (processorOverride != null)
			{
				throw new InvalidOperationException("NewProcessor has already been overridden");
			}

			processorOverride = newProcessorOverride;
			return new Reverter();
		}

		#region Implementation

		sealed class Reverter : IDisposable
		{
			public void Dispose()
			{
				processorOverride = null;
			}
		}

		[ThreadStatic]
		static NewProcessorOverride processorOverride;

		#endregion
	}
}

#endregion
#endif
#endregion
