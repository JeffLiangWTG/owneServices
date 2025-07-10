using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class SecurityTestHelper
	{
		public static SecurityCore CreateSecurityInstance(BusinessObjectFactory factory)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;

			var securityCollection = new GlbSecurityCollection(factory);
			securityCollection.Load();

			return new SecurityCore(securityCollection, staff, Guid.Empty, Guid.Empty, Guid.Empty);
		}
	}
}
