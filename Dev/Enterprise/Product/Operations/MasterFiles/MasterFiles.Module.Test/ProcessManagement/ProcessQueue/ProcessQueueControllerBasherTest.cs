using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessQueueController))]
	sealed class ProcessQueueControllerBasherTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(ProcessQueueController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProcessQueue;
		}
	}
}
