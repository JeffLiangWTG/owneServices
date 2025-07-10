using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationTimeoutTest : TestCase
	{
		public void TestNotReturnNullWhenWorkFinishedImmediately()
		{
			var timeoutExecution = new DeduplicationTimeout<IEnumerable<string>>(TimeSpan.FromSeconds(10));
			var result = timeoutExecution.DoWork(Array.Empty<string>);
			AssertNotNull(result);
		}
	}
}
