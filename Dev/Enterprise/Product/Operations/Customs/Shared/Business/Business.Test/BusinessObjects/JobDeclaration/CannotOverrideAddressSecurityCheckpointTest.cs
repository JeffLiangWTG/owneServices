using System;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CannotOverrideAddressSecurityCheckpointTest : TestCase
	{
		CannotOverrideAddressSecurityCheckpoint checkpoint;

		CannotOverrideAddressSecurityCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new CannotOverrideAddressSecurityCheckpoint()); }
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "AddChild() is not supported by CannotOverrideAddressSecurityCheckpoint.")]
		public void TestAddChild()
		{
			Checkpoint.AddChild(null);
		}

		public void TestIsAllowed()
		{
			AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "ShowError() is not supported by CannotOverrideAddressSecurityCheckpoint.")]
		public void TestShowError()
		{
			Checkpoint.ShowError();
		}

		public void TestVisible()
		{
			AssertEquals("Visible", false, Checkpoint.Visible);
		}

		public void TestErrorMessage()
		{
			AssertEquals("Overriding this address is not allowed", Checkpoint.ErrorMessageForNotAllowed);
		}
	}
}
