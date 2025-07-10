using System.Diagnostics;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Moq;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation.Test
{
	sealed class ConstructorStackTraceExtensionsTest : TestCaseWithFactory
	{
		public void TestGetStackTrace()
		{
			AssertEquals(false, ObjectFactory.Get<IAccounting>().CollectConstructorCallStackDetails);
			IHaveConstructorStackTraceDummy dummy = new IHaveConstructorStackTraceDummy();
			dummy.SetConstructorStackTrace();
			AssertNull(dummy.ConstructorStackTrace);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				StackTrace trace = new StackTrace();
				dummy.SetConstructorStackTrace();
				AssertNotNull(dummy.ConstructorStackTrace);
				AssertEquals("Trace should be as expected", trace.ToString(), dummy.ConstructorStackTrace.ToString());
			}
		}

		class IHaveConstructorStackTraceDummy : IHaveConstructorStackTrace
		{
			public StackTrace ConstructorStackTrace { get; set; }
		}
	}
}
