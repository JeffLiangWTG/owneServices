using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class CusEntryHeaderCustomsMessengerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<NotSupportedException>("Invalid message type", () => { var crash = CusEntryHeaderCustomsMessenger.New(cusEntryHeader, "CRASH"); });
				var messenger = CusEntryHeaderCustomsMessenger.New(cusEntryHeader, TRMessageTypes.Codes.DTE);
				AssertType<TRCustomsMessenger>(messenger);
				messenger = CusEntryHeaderCustomsMessenger.New(cusEntryHeader, TRMessageTypes.Codes.EUT);
				AssertType<TRCustomsMessenger>(messenger);
			});
		}

		public void TestConstruction()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			var trMessenger = CusEntryHeaderCustomsMessenger.New(cusEntryHeader, TRMessageTypes.Codes.DTE);
			var messenger = trMessenger as ICustomsMessenger;
			AssertSame(cusEntryHeader, messenger.Owner);

			trMessenger = CusEntryHeaderCustomsMessenger.New(cusEntryHeader, TRMessageTypes.Codes.EUT);
			var messengerEUT = trMessenger as ICustomsMessenger;
			AssertSame(cusEntryHeader, messengerEUT.Owner);
		}
	}
}
