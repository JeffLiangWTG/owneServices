using System;
using System.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ADEntityProvider_RowNotInTable : IADEntityProvider
	{
		public IADEntity GetADGroup(IGlbGroup group)
		{
			throw new NotImplementedException();
		}

		public IADUser GetADUser(IGlbStaff staff)
		{
			throw new RowNotInTableException();
		}
	}
}
