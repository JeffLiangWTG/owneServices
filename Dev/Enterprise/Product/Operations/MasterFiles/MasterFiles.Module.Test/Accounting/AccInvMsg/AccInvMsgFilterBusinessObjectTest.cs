using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccInvMsgFilterBusinessObject))]
	sealed class AccInvMsgFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEnglishFilter()
		{
			AccInvMsg message1 = Factory.NewWithValidTestData<AccInvMsg>();
			message1.A9_EnglishMsg = "ENGLISH";
			message1.A9_LocalMsg = "LOCAL";
			message1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message1.A9_IsActive = true;
			AccInvMsg message2 = Factory.NewWithValidTestData<AccInvMsg>();
			message2.A9_EnglishMsg = "DIFFERENT ENGLISH";
			message2.A9_LocalMsg = "LOCAL";
			message2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message2.A9_IsActive = true;
			Factory.Save();

			AccInvMsgFilterBusinessObject filter = new AccInvMsgFilterBusinessObject();
			((ModuleTextFilter)filter["English Description"]).Property = "engli";
			((ModuleTextFilter)filter["English Description"]).IsActive = true;

			AccInvMsgCollection messages = new AccInvMsgCollection(Factory);
			messages.AdditionalFilter = filter.Filter;

			AssertCollectionContains(message1, messages);
			AssertCollectionNotContains(message2, messages);
		}

		public void TestLocalFilter()
		{
			AccInvMsg message1 = Factory.NewWithValidTestData<AccInvMsg>();
			message1.A9_EnglishMsg = "ENGLISH";
			message1.A9_LocalMsg = "LOCAL";
			message1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message1.A9_IsActive = true;
			AccInvMsg message2 = Factory.NewWithValidTestData<AccInvMsg>();
			message2.A9_EnglishMsg = "ENGLISH";
			message2.A9_LocalMsg = "DIFFERENT LOCAL";
			message2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message2.A9_IsActive = true;
			Factory.Save();

			AccInvMsgFilterBusinessObject filter = new AccInvMsgFilterBusinessObject();
			((ModuleTextFilter)filter["Local Language Description"]).Property = "Loc";
			((ModuleTextFilter)filter["Local Language Description"]).IsActive = true;

			AccInvMsgCollection messages = new AccInvMsgCollection(Factory);
			messages.AdditionalFilter = filter.Filter;

			AssertCollectionContains(message1, messages);
			AssertCollectionNotContains(message2, messages);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccInvMsgFilterBusinessObject();
		}
	}
}
