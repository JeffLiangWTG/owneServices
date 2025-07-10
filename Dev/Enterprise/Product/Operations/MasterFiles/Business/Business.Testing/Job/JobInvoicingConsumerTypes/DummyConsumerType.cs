using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyConsumerType : JobInvoicingConsumerType
	{
		public DummyConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { throw new NotImplementedException(); }
		}

		public override Type BizoType
		{
			get { throw new NotImplementedException(); }
		}

		public override bool IsDirectionSupported
		{
			get { return isDirectionSupported; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
		bool isDirectionSupported;

		public void SetIsDirectionSupported(bool value)
		{
			isDirectionSupported = value;
		}

		public override bool IsTransportModeSupported
		{
			get { return isTransportModeSupported; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
		bool isTransportModeSupported;

		public void SetIsTransportModeSupported(bool value)
		{
			isTransportModeSupported = value;
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
