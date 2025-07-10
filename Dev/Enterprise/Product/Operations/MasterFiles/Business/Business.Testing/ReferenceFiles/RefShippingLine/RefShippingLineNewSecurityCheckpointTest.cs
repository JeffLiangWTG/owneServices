using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefShippingLineNewSecurityCheckpointTest : TransactionedTestCase
	{
		RefShippingLineNewSecurityCheckpoint checkpoint;

		RefShippingLineNewSecurityCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new RefShippingLineNewSecurityCheckpoint()); }
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "AddChild() is not supported by RefShippingLineNewSecurityCheckpoint.")]
		public void TestAddChild()
		{
			Checkpoint.AddChild(null);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "ShowError() is not supported by RefShippingLineNewSecurityCheckpoint.")]
		public void TestShowError()
		{
			Checkpoint.ShowError();
		}

		public void TestIsAllowed()
		{
			AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
		}

		public void TestErrorMessage()
		{
			AssertEquals("To register a new Carrier with CargoWise, please raise a CR8 Compliance, Ocean Carrier Integration request. Once this request has been processed the new record will be available for selection in your system.", Checkpoint.ErrorMessageForNotAllowed);
		}
	}
}
