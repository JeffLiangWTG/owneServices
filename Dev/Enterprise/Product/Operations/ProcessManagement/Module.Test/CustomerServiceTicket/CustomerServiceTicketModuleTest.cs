using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(CustomerServiceTicketModule))]
	class CustomerServiceTicketModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CustomerServiceTicket;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(factory);
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Licence.ProductivityTools, module.LicenceCheckPoint);
			}
		}
	}
}
