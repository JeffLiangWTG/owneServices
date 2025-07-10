using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UniversalXmlCommunicationModeProvider : IUniversalXmlCommunicationModeProvider, IMessageProcessorCommunicationModesResult
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public UniversalXmlCommunicationModeProvider(Func<(IEDICommunicationsMode[] modes, MultilingualString failureReason)> func)
		{
			this.func = func;
		}

		#region

		Func<(IEDICommunicationsMode[] modes, MultilingualString failureReason)> func;
		IList<IEDICommunicationsMode> communicationsModes;
		MultilingualString reasonForNoCommunicationModes;

		public IList<IEDICommunicationsMode> CommunicationModes
		{
			get
			{
				EnsureInit();
				return communicationsModes;
			}
		}

		public MultilingualString ReasonForNoCommunicationModes
		{
			get
			{
				EnsureInit();
				return reasonForNoCommunicationModes;
			}
		}

		IList<IMessageDestinationSource> IMessageProcessorCommunicationModesResult.Destinations => CommunicationModes.Cast<IMessageDestinationSource>().ToArray();

		MultilingualString IMessageProcessorCommunicationModesResult.ConfigurationLogging => ReasonForNoCommunicationModes;

		#endregion

		void EnsureInit()
		{
			if (func != null)
			{
				(communicationsModes, reasonForNoCommunicationModes) = func();
				func = null;
			}
		}
	}
}
