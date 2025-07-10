using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(TradeGroupsModule))]
	sealed class TradeGroupsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TradeGroups;
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			var o = Factory.NewWithValidTestData<CusRefTradeGroupView>();
			o.ZZA_IsSystem = false;
			o = Factory.NewWithValidTestData<CusRefTradeGroupView>();
			o.ZZA_IsSystem = false;
			o = Factory.NewWithValidTestData<CusRefTradeGroupView>();
			o.ZZA_IsSystem = false;
			o = Factory.NewWithValidTestData<CusRefTradeGroupView>();
			o.ZZA_IsSystem = false;
			Factory.Save();
		}

		public void TestLicenseAndSecurityCheckpoint()
		{
			using (var module = new TradeGroupsModule())
			{
				AssertEquals("License Checkpoint", Env.Licence.Core, module.LicenceCheckPoint);
				AssertEquals("Security Checkpoint", Env.Security.GlobalTariffs, module.SecurityCheckpoint);
			}
		}

		public void TestModuleAllows()
		{
			using (var module = new TradeGroupsModule())
			{
				AssertEquals("Allows new", true, module.AllowNew);
				AssertEquals("Allows edit", true, module.AllowEdit);
				AssertEquals("Allows delete", true, module.AllowDelete);
				AssertEquals("Allows view", true, module.AllowView);
			}
		}

		public void TestShowEditForm()
		{
			var tradeGroup = CreateTradeGroup(Core.Constants.CountryCodes.Australia, "ANZ");
			using (var module = new TradeGroupsModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetEditForm(tradeGroup))
					{
						AssertNotNull(form);
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				{
					using (var form = module.GetEditForm(tradeGroup))
					{
						AssertNull(form);
						AssertEquals("Trade Groups that do not belong to your country cannot be edited.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestShowDeleteForm()
		{
			var tradeGroup = CreateTradeGroup(Core.Constants.CountryCodes.Australia, "ANZ");
			using (var module = new TradeGroupsModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetDeleteForm(tradeGroup))
					{
						AssertNotNull(form);
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				{
					using (var form = module.GetDeleteForm(tradeGroup))
					{
						AssertNull(form);
						AssertEquals("Trade Groups that do not belong to your country cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestFilterBusinessObjectDefaultCountryCode()
		{
			using (var module = new TradeGroupsModule())
			{
				var expected = new ZString(Core.Constants.CountryCodes.China);
				var defaults = new FilterBusinessObjectDefaults
				{
					new FilterBusinessObjectDefault(CusRefTradeGroupCollection.FilterConstants.CountryCode, "Property", expected, false)
				};
				module.FilterBusinessObject.SetExternalDefaults(defaults);
				module.FilterBusinessObject.Filter.AddToFilter(new ZQuery(CusRefTradeGroupSchema.CR9_RN_NKCountryCode, SQLComparisonOperator.Equal, expected));
				AssertEquals(expected, module.FilterBusinessObjectDefaultCountryCode);
			}
		}

		public void TestFilterBusinessObjectDefaultCountryCode_WithoutExternalDefaults()
		{
			using (var module = new TradeGroupsModule())
			{
				AssertEquals(ZString.Empty, module.FilterBusinessObjectDefaultCountryCode);
			}
		}

		CusRefTradeGroup CreateTradeGroup(ZString countryCode, ZString tradeGroup, ZDate? startDate = null, ZDate? endDate = null, string description = null)
		{
			var refTradeGroup = Factory.New<CusRefTradeGroup>();
			refTradeGroup.CR9_TradeGroup = tradeGroup;
			refTradeGroup.CR9_Description = description ?? (tradeGroup + " DESC");
			refTradeGroup.CR9_RN_NKCountryCode = countryCode;
			refTradeGroup.CR9_StartDate = startDate != null && startDate.HasValue ? startDate.Value : ZDateTime.MinSmallDateTimeValue.Date;
			refTradeGroup.CR9_EndDate = endDate != null && endDate.HasValue ? endDate.Value : ZDateTime.MaxSmallDateTimeValue.Date;
			Factory.Save();
			return refTradeGroup;
		}

		sealed class TradeGroupsModuleForUnitTest : TradeGroupsModule
		{
			public IZForm GetEditForm(CusRefTradeGroup tradeGroup) => ShowEditForm(tradeGroup);

			public IZForm GetDeleteForm(CusRefTradeGroup tradeGroup) => ShowDeleteForm(tradeGroup);
		}
	}
}
