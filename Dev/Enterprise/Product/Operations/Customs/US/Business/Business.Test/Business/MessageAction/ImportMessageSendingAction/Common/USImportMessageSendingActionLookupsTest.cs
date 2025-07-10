using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	class USImportMessageSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLIsts()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			var action = mock.Object;
			AssertEquals(typeof(AIITitleOfDeclarant), action.Lookups.TitleOfDeclarantList.GetType());
			AssertEquals(Factory.GetCachedValue<AIITitleOfDeclarant>(), action.Lookups.TitleOfDeclarantList);
			AssertEquals(typeof(CollectionBillInformationCodesList), action.Lookups.CollectionBillInformationCodesList.GetType());
			AssertEquals(Factory.GetCachedValue<CollectionBillInformationCodesList>(), action.Lookups.CollectionBillInformationCodesList);
			AssertEquals(typeof(ReasonCodeList), action.Lookups.ReasonCodeList.GetType());
			AssertEquals(typeof(ACEPNActionCodeList), action.Lookups.ACEPNActionCodeList.GetType());
		}

		public void TestReasonCodeList()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.CargoRelease, Actions });
			mock.CallBase = true;
			var action = mock.Object;
			AssertEquals(11, action.Lookups.ReasonCodeList.Count);
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(12, action.Lookups.ReasonCodeList.Count);
		}

		ImportMessageSendingActionCollection actions;
		ImportMessageSendingActionCollection Actions => actions ?? (actions = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return declaration;
			}
		}
	}
}
