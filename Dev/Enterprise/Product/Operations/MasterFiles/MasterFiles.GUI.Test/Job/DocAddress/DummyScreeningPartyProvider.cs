using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	class DummyScreeningPartyProvider : DummyBizObj, IScreeningPartyProvider
	{
		#region IScreeningPartyProvider Members

		public ScreeningParty[] ScreeningParties
		{
			get { throw new NotImplementedException(); }
		}

		public ZString ScreeningStatus
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		public ZString GetWorstScreeningStatus()
		{
			throw new NotImplementedException();
		}

		public ZString GetWorstScreeningStatusUnlessManuallyCleared()
		{
			throw new NotImplementedException();
		}
	}
}
