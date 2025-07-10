using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DummyUSDec : DummyScreeningPartyProviderWithJobDeclaration, Enterprise.Integration.Customs.US.IJobDeclaration
	{
		public ZDateTime US_DateOfExport
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
		public ZString US_EntryType
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
		public ZBool US_EnableENS
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
	}
}
