using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class RefFacilityNewSecurityCheckpointTest : TransactionedTestCase
	{
		RefFacilityNewSecurityCheckpoint checkpoint;

		RefFacilityNewSecurityCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new RefFacilityNewSecurityCheckpoint()); }
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "AddChild() is not supported by RefFacilityNewSecurityCheckpoint.")]
		public void TestAddChild()
		{
			Checkpoint.AddChild(null);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "ShowError() is not supported by RefFacilityNewSecurityCheckpoint.")]
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
			AssertEquals("To register a new facility with CargoWise, please raise a CR8 Compliance. Once this request has been processed the new record will be available for selection in your system.", Checkpoint.ErrorMessageForNotAllowed);
		}
	}
}

