using System;
using Enterprise.Environment;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingTmplModule))]
	public class DtbBookingTmplModuleTest : ZModuleBasherTest
	{
		public void TestFilterControl()
		{
			using (var template = new DtbBookingTmplModuleForTest())
			using (var filterControl = (IDisposable)template.GetNewFilterControlForTest())
			{
				Assert(filterControl is DtbBookingTmplFilterControl);
			}
		}

		public void TestGridCollection()
		{
			using (var template = new DtbBookingTmplModuleForTest())
			{
				Assert(template.GetNewGridCollectionForTest() is DtbBookingTmplCollection);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var template = new DtbBookingTmplModuleForTest())
			{
				Assert(template.GetNewFilterBusinessObjectForTest() is DtbBookingTmplFilterBusinessObject);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = new DtbBookingTmplModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbBookingTmpl;
		}
	}
}
