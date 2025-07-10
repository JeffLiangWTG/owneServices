using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefRateCodeModule))]
	sealed class CusRefRateCodeModuleTest : ZModuleBasherTest
	{
		public void TestShowEditForm()
		{
			var rateCode = Factory.New<CusRefRateCode>();
			rateCode.CR7_RateCode = "123";
			rateCode.CR7_Description = "123 DESC";
			rateCode.CR7_RateType = Constants.RateTypes.Duty;
			rateCode.CR7_RN_NKCountryCode = "AU";
			Factory.Save();
			using (var module = new CusRefRateCodeModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetEditForm(rateCode))
					{
						AssertNotNull(form);
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				{
					using (var form = module.GetEditForm(rateCode))
					{
						AssertNull(form);
						AssertEquals("Rate Codes that do not belong to your country cannot be edited.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusRefRateCode;

		protected override bool HasController() => true;

		sealed class CusRefRateCodeModuleForUnitTest : CusRefRateCodeModule
		{
			public IZForm GetEditForm(CusRefRateCode rateCode) => ShowEditForm(rateCode);
		}
	}
}
