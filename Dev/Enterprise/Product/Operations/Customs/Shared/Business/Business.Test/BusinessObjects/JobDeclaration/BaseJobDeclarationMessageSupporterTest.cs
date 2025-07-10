using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class BaseJobDeclarationMessageSupporterTest : JobDeclarationMessageSupporterTest<BaseJobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var countriesSupportedAutoSendMessages = new List<ZString>() {
														Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.UnitedStates,
														Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.SouthAfrica,
														Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.France,
														Core.Constants.CountryCodes.FrenchGuyana, Core.Constants.CountryCodes.Guadeloupe,
														Core.Constants.CountryCodes.Martinique, Core.Constants.CountryCodes.Mayotte,
														Core.Constants.CountryCodes.Reunion, Core.Constants.CountryCodes.SaintBarthelemy,
														Core.Constants.CountryCodes.SaintMartin,Core.Constants.CountryCodes.Spain,
														Core.Constants.CountryCodes.Ireland };
			var countryCollection = new RefCountryCollection(Factory).Select(x => x.RN_Code).Distinct().Except(countriesSupportedAutoSendMessages);
			foreach (var countryCode in countryCollection)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)Factory.New<BaseJobDeclaration>();
					AssertEquals(false, supporter.SupportEntryDeclarationMessage);
					AssertEquals(string.Format(BaseJobDeclaration.SendEntryDeclarationTriggerNotSupportedMessage, countryCode), supporter.GetReasonForNotSupportEntryDeclarationMessage);
					AssertNull(supporter.CreateEntryDeclarationMessageProcessor());

					AssertEquals(false, supporter.SupportReleaseMessage);
					AssertEquals(string.Format(BaseJobDeclaration.SendReleaseMessageTroggerNotSupportedMessage, countryCode), supporter.GetReasonForNotSupportReleaseMessage);
					AssertNull(supporter.CreateReleaseMessageProcessor());
				}
			}

			Assert(true);
		}

		public void TestJobDecarationMessageSupporterInDifferentJobs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

				var supporter1 = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration1;
				var supporter2 = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)declaration2;
				AssertNotEquals(supporter1, supporter2);
			}
		}
	}
}
