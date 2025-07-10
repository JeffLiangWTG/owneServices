using System;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	class ExceptionEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExceptionEventArgs(null));
		}

		public void TestException()
		{
			var ex = new Exception("Test");
			var args = new ExceptionEventArgs(ex);
			AssertEquals(ex, args.Exception);
		}
	}
}
