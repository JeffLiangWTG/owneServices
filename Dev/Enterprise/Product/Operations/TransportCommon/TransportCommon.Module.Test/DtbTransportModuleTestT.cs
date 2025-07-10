using System;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Module.Testing
{
	[TestsSubclassesOf(typeof(DtbTransportModule))]
	public abstract class DtbTransportModuleTest<T> : ZModuleBasherTest
			where T : DtbTransportModule, new()
	{
		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new T())
			{
				AssertEquals(true, module.FilterBusinessObject.GetType().IsAssignableFrom(ExpectedFilterBusinessObjectType));
			}
		}

		protected abstract Type ExpectedFilterBusinessObjectType { get; }

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new T())
			{
				using (var controlForTest = (IDisposable)module.GetNewFilterControlForGrid())
				{
					AssertEquals(true, controlForTest.GetType().IsAssignableFrom(ExpectedFilterControlType));
				}
			}
		}

		protected abstract Type ExpectedFilterControlType { get; }

		#endregion

		#region TestFlags

		public void TestFlags()
		{
			using (var module = new T())
			{
				AssertEquals(true, module.SupportsWorkflow);
				AssertEquals(ExpectedAllowNew, module.AllowNew);
				AssertEquals(ExpectedAllowDelete, module.AllowDelete);
			}
		}

		protected abstract bool ExpectedAllowDelete { get; }
		protected abstract bool ExpectedAllowNew { get; }

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = new T())
			{
				AssertEquals(true, module.GridCollection.GetType().IsAssignableFrom(ExpectedCollectionType));
			}
		}

		protected abstract Type ExpectedCollectionType { get; }

		#endregion

		#region TestLicenseCheckpoint

		public void TestLicenseCheckpoint()
		{
			using (var module = new T())
			{
				AssertEquals(module.LicenceCheckPoint, ExpectedLicenceCheckpoint);
			}
		}

		protected abstract LicenceCheckpoint ExpectedLicenceCheckpoint { get; }

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new T())
			{
				AssertEquals(module.SecurityCheckpoint, ExpectedSecurityCheckpoint);
			}
		}

		protected abstract SecurityCheckpoint ExpectedSecurityCheckpoint { get; }

		#endregion
	}
}
