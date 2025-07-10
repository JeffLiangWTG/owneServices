namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsExceptionsTest : WhsTestCaseWithFactoryEnv
	{
		public void TestWhsException()
		{
			WhsException e = new WhsException();
			AssertEquals("General Warehouse Exception", e.Message);
			e = new WhsException("Test");
			AssertEquals("Test", e.Message);
		}

		public void TestMutexLockDeniedException()
		{
			MutexLockDeniedException e = new MutexLockDeniedException();
			AssertEquals("Mutex Lock Denied", e.Message);
			e = new MutexLockDeniedException("Test");
			AssertEquals("Test", e.Message);
		}
	}
}
