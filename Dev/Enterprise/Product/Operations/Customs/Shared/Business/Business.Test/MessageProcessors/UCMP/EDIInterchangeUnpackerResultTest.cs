using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	sealed class EDIInterchangeUnpackerResultTest : TestCaseWithFactory
	{
		public void TestFailUnpackerResult()
		{
			var result = new EDIInterchangeUnpackerResult("Error unpacking");
			AssertEquals("Fail", expected: false, result.IsSuccess);
			AssertEquals("ErrorReason", "Error unpacking", result.ErrorReason);
			AssertExceptionThrown<InvalidOperationException>("Mutually Exclusive Exception", "EdiMessages cannot be accessed when IsSuccess is false.", () => _ = result.EdiMessages);
		}

		public void TestSuccessUnpackerResult()
		{
			var result = new EDIInterchangeUnpackerResult(new EDIMessage[] { Factory.New<EDIMessage>() } );
			Assert("Success", result.IsSuccess);
			AssertEquals("EdiMessages", 1, result.EdiMessages.Count);
			AssertExceptionThrown<InvalidOperationException>("Mutually Exclusive Exception", "ErrorReason cannot be accessed when IsSuccess is true.", () => _ = result.ErrorReason);
		}

		public void TestConstructor_WithNullEdiMessages()
		{
			ICollection<EDIMessage> nullMessages = null;
			AssertExceptionThrown<ArgumentNullException>("Null ediMessage", () => _ = new EDIInterchangeUnpackerResult(nullMessages));
		}

		public void TestConstructor_WithEmptyEdiMessages()
		{
			ICollection<EDIMessage> emptyMessages = new List<EDIMessage>();
			var result = new EDIInterchangeUnpackerResult(emptyMessages);
			Assert("Success", result.IsSuccess);
			AssertEquals("EdiMessages", 0, result.EdiMessages.Count);
		}

		public void TestConstructor_WithNullErrorReason()
		{
			string errorReason = null;
			AssertExceptionThrown<ArgumentNullException>("Null errorReason", () => _ = new EDIInterchangeUnpackerResult(errorReason));
		}

		public void TestConstructor_WithEmptyErrorReason()
		{
			string errorReason = "   ";
			AssertExceptionThrown<ArgumentNullException>("Empty errorReason", () => _ = new EDIInterchangeUnpackerResult(errorReason));
		}
	}
}
