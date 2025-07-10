using System;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class CannotOverrideAddressSecurityCheckpointTest : TransactionedTestCase
	{
		public void TestAddChild()
		{
			AssertExceptionThrown<NotSupportedException>(() => checkpoint.AddChild(null));
		}

		public void TestIsAllowed()
		{
			AssertEquals("IsAllowed", false, checkpoint.IsAllowed);
		}

		public void TestShowError()
		{
			AssertExceptionThrown<NotSupportedException>(() => checkpoint.ShowError());
		}

		public void TestVisible()
		{
			AssertEquals("Visible", false, checkpoint.Visible);
		}

		public void TestErrorMessage()
		{
			AssertEquals("Overriding this address is not allowed", checkpoint.ErrorMessageForNotAllowed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			checkpoint = new CannotOverrideAddressSecurityCheckpoint();
		}

		CannotOverrideAddressSecurityCheckpoint checkpoint;
	}
}
