using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccInvMsgCollection))]
	sealed class AccInvMsgCollectionTest : ActiveBusinessObjectCollectionTestCase<AccInvMsgCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AccInvMsg message = Factory.NewWithValidTestData<AccInvMsg>();
			message.A9_LocalMsg = "LOCAL";
			message.A9_EnglishMsg = "english";
			message.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			return message;
		}

		public void TestCollectionFilter()
		{
			var uSMessage = Factory.NewWithValidTestData<AccInvMsg>();
			uSMessage.A9_EnglishMsg = "ENGLISH";
			uSMessage.A9_LocalMsg = "LOCAL";
			uSMessage.A9_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			var franceMessage = Factory.NewWithValidTestData<AccInvMsg>();
			franceMessage.A9_EnglishMsg = "FRENCH";
			franceMessage.A9_LocalMsg = "LOCAL";
			franceMessage.A9_RN_NKCountryCode = Constants.CountryCodes.France;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var collection = new AccInvMsgCollection(Factory);
				Assert("only the message from the current company should be found", collection.Contains(uSMessage));
				Assert("only the message from the current company should be found", !collection.Contains(franceMessage));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var collection = new AccInvMsgCollection(Factory, Constants.CountryCodes.UnitedStates);
				Assert("only the message from countryCode parameter should be found", collection.Contains(uSMessage));
				Assert("only the message from countryCode parameter should be found", !collection.Contains(franceMessage));
			}
		}
	}
}
