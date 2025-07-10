using System;

namespace Enterprise.MasterData.Business.Tests
{
	using NUnit.Framework;

	class GenShapeGeographyNewSecurityCheckpointTest : TransactionedTestCase
	{
		GenShapeGeographyNewSecurityCheckpoint checkpoint;

		GenShapeGeographyNewSecurityCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new GenShapeGeographyNewSecurityCheckpoint()); }
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "AddChild() is not supported by GenShapeGeographyNewSecurityCheckpoint.")]
		public void TestAddChild()
		{
			Checkpoint.AddChild(null);
		}

		public void TestIsAllowed()
		{
			AssertIsAllowed(true);
			AssertIsAllowed(false);

			void AssertIsAllowed(bool expectedValue)
			{
				using (Registry.Business.SystemDataRegistry.Instance.GeographyUnreleasedFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
				{
					AssertEquals("IsAllowed", expectedValue, Checkpoint.IsAllowed);
				}
			}
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "ShowError() is not supported by GenShapeGeographyNewSecurityCheckpoint.")]
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
			AssertEquals("Functionality to allow the import of user defined geography will be introduced shortly.", Checkpoint.ErrorMessageForNotAllowed);
		}
	}
}
