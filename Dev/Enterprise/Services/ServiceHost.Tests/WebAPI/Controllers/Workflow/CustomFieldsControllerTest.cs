using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static Enterprise.Core.SharedConstants;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class CustomFieldsControllerTest : TestCaseWithFactory
	{
		public void TestGetCustomFieldDefinitionsByGuid_NonExistentRecord()
		{
			var result = controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, Guid.NewGuid());
			result.AssertResultContains(HttpStatusCode.NotFound);
		}

		public void TestGetCustomFieldDefinitionsByGuid_NonSupportedRecord()
		{
			Assert("Sanity check", !typeof(ICustomFieldProvider).IsAssignableFrom(typeof(DummyBusinessObject)));

			var dummy = DummyBusinessObject.New(Factory);
			Factory.Save();

			var result = controller.GetCustomFieldDefinitionsByGuid(DummyBusinessObject.Schema.TablePrefix, dummy.PK.ToGuid());
			result.AssertResultContains(HttpStatusCode.NotFound);
		}

		public void TestGetCustomFieldDefinitionsByGuid_Empty()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var result = controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, staff.PK.ToGuid());
			result.AssertJsonResultEquals(Array.Empty<object>());
		}

		public void TestGetCustomFieldDefinitionsByGuid()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
			processTaskTemplate.P0_IsActive = true;

			var stringRules = Factory.New<GenCustomAddOnRule>();
			stringRules.XR_Code = nameof(stringRules);
			var list = new CodeDescriptionPairList();
			list.AddPair("UNO", "Numero Uno");
			list.AddPair("TRES", "Nomor Tiga");
			stringRules.SetRules(
				new InvalidCodeRule { List = list, IsEnabled = true },
				new CheckEnteredRule { IsEnabled = true });
			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "My Text";
			genCustomColumnString.XC_Type = "STR";
			genCustomColumnString.XC_DisplaySequence = 5;
			genCustomColumnString.XC_XR = stringRules.PK;
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var dateRules = Factory.New<GenCustomAddOnRule>();
			dateRules.XR_Code = nameof(dateRules);
			dateRules.SetRules(
				new InvalidCodeRule { List = list, IsEnabled = false },
				new CheckEnteredRule { IsEnabled = true },
				new CreateEventRule { IsEnabled = true },
				new DateTimeFormatRule { Format = KDateTimeFormat.Short, IsEnabled = true });
			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "My Date";
			genCustomColumnDate.XC_Type = "DTE";
			genCustomColumnDate.XC_DisplaySequence = 2;
			genCustomColumnDate.XC_XR = dateRules.PK;
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var result = controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, staff.PK.ToGuid());
			var expected = new object[]
			{
				new {
					Name = "My Date",
					Type = "DTE",
					Caption = "My Date",
					MetaData = new {
						CustomFieldOrigin = "GenCustomColumnDefinition",
						Mandatory = true,
						DateTimeFormat = nameof(KDateTimeFormat.Short)
					}
				},
				new
				{
					Name = "My Text",
					Type = "STR",
					Caption = "My Text",
					MetaData = new
					{
						CustomFieldOrigin = "GenCustomColumnDefinition",
						MaxLength = 4,
						List = new[] { new { Code = "UNO", Desc = "Numero Uno" }, new { Code = "TRES", Desc = "Nomor Tiga" } },
						Mandatory = true
					}
				}
			};
			var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			result.AssertJsonResultEquals(expected, jsonSerializerSettings);

			staff.SetUserDefinedValue("CustomFieldBool", ZBool.True);
			Factory.Save();
			expected = expected.Prepend(new {
				Name = "CustomFieldBool",
				Type = "BOO",
				Caption = "CustomFieldBool",
				MetaData = new {
					CustomFieldOrigin = "GenCustomColumnDefinition",
				}
			}).ToArray();
			result = controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, staff.PK.ToGuid());
			result.AssertJsonResultEquals(expected, jsonSerializerSettings);
		}

		public void TestGetCustomFieldDefinitionsByGuid_DifferentCustomFieldOrigins()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "My Date";
			genCustomColumnDate.XC_Type = "DTE";
			genCustomColumnDate.XC_DisplaySequence = 1;
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "some full name";
			orgHeader.OH_Code = "TST";
			orgHeader.MainAddress.OA_Address1 = "some place";

			var orgCustomLabel = Factory.New<OrgCustomLabels>();
			orgCustomLabel.OT_FieldName = "OrderHeader.CustomAttrib1";
			orgCustomLabel.OT_Position = 1;
			orgCustomLabel.OT_Caption = "My Text";
			orgCustomLabel.OT_IsMandatory = true;
			orgCustomLabel.OT_OH = orgHeader.PK;

			Order order = Factory.New<Order>();
			order.JD_OA_BuyerAddress = orgHeader.MainAddress.PK;

			Factory.Save();

			var result = controller.GetCustomFieldDefinitionsByGuid(JobOrderHeaderSchema.Constants.Prefix, order.PK.ToGuid());
			var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			var expected = new object[]
			{
				new {
					Name = "My Date",
					Type = "DTE",
					Caption = "My Date",
					MetaData = new {
						CustomFieldOrigin = "GenCustomColumnDefinition",
					}
				},
				new {
					Name = "JD_CustomAttrib1",
					Type = "STR",
					Caption = "My Text",
					MetaData = new {
						CustomFieldOrigin = "OrgCustomLabels",
					}
				},
			};
			result.AssertJsonResultEquals(expected, jsonSerializerSettings);
		}

		public void TestFactoryRefreshIsDisabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest(new[] { GenCustomColumnDefinitionSchema.Constants.TableName }))
			{
				controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, staff.PK.ToGuid());
				var factory = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.FirstOrDefault(x => x.NameForDebugging == "Custom Fields Web Service");
				Assert(!factory.RefreshEnabled);
			}
		}

		public void TestGetCustomFieldDefinitionsByGuid_InvalidCodeRuleWithoutList()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
			processTaskTemplate.P0_IsActive = true;

			var stringRules = Factory.New<GenCustomAddOnRule>();
			stringRules.XR_Code = nameof(stringRules);
			stringRules.SetRules(new InvalidCodeRule { IsEnabled = true });
			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "My Text";
			genCustomColumnString.XC_Type = "STR";
			genCustomColumnString.XC_DisplaySequence = 5;
			genCustomColumnString.XC_XR = stringRules.PK;
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var result = controller.GetCustomFieldDefinitionsByGuid(GlbStaffSchema.Constants.Prefix, staff.PK.ToGuid());
			var expected = new object[]
			{
				new
				{
					Name = "My Text",
					Type = "STR",
					Caption = "My Text",
					MetaData = new
					{
						CustomFieldOrigin = "GenCustomColumnDefinition",
						MaxLength = 0,
						List = Array.Empty<object>()
					}
				}
			};
			var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			result.AssertJsonResultEquals(expected, jsonSerializerSettings);
		}

		public void TestGetCustomFieldDefinitions_GivenNoDefinitions_ReturnsEmptyArray()
		{
			var result = controller.GetCustomFieldDefinitions(GlbStaffSchema.Constants.Prefix, Languages.English);
			var expected = Array.Empty<object>();
			var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			result.AssertJsonResultEquals(expected, jsonSerializerSettings);
		}

		public void TestGetCustomFieldDefinitions()
		{
			void TestCaseGetCustomFieldDefinitions(int testCaseIndex, string languageCode)
			{
				using (Res.TemporarilySwitchLanguage("DE-DE"))
				using (var mockRes = Res.UseMockData())
				{
					// Add Custom Field Rules
					var list = new CodeDescriptionPairList();
					list.AddPair("UNO", "Numero Uno");
					list.AddPair("TRES", "Nomor Tiga");

					var stringRules = Factory.New<GenCustomAddOnRule>();
					stringRules.XR_Code = $"{nameof(stringRules)}{testCaseIndex}";
					stringRules.SetRules(
						new InvalidCodeRule { List = list, IsEnabled = true },
						new CheckEnteredRule { IsEnabled = true });

					var dateRules = Factory.New<GenCustomAddOnRule>();
					dateRules.XR_Code = $"{nameof(dateRules)}{testCaseIndex}";
					dateRules.SetRules(
						new InvalidCodeRule { List = list, IsEnabled = false },
						new CheckEnteredRule { IsEnabled = true },
						new CreateEventRule { IsEnabled = true },
						new DateTimeFormatRule { Format = KDateTimeFormat.Short, IsEnabled = true });

					// Add Custom Field Definitions
					var genCustomColumnDefinition1 = Factory.New<GenCustomColumnDefinition>();
					genCustomColumnDefinition1.XC_Name = "My Text";
					genCustomColumnDefinition1.XC_Type = "STR";
					genCustomColumnDefinition1.XC_DisplaySequence = 2;
					genCustomColumnDefinition1.XC_XR = stringRules.PK;

					var genCustomColumnDefinition2 = Factory.New<GenCustomColumnDefinition>();
					genCustomColumnDefinition2.XC_Name = "My Date";
					genCustomColumnDefinition2.XC_Type = "DTE";
					genCustomColumnDefinition2.XC_DisplaySequence = 1;
					genCustomColumnDefinition2.XC_XR = dateRules.PK;

					var genCustomColumnDefinition3 = Factory.New<GenCustomColumnDefinition>();
					genCustomColumnDefinition3.XC_Name = "Base Company Tariff";
					genCustomColumnDefinition3.XC_Type = "STR";
					genCustomColumnDefinition3.XC_DisplaySequence = 1;
					genCustomColumnDefinition3.XC_XR = stringRules.PK;

					var basicTariffInGermany = "Basis Unternehmenstarif";
					var resKey = genCustomColumnDefinition3
										.XC_NameInfo
										.CustomizableDataResourceStrings
										.GetMultilingualString(genCustomColumnDefinition3.XC_NameInfo, "Base Company Tariff")
										.ResourceKey;
					mockRes.Put(resKey, new ResourceStringData(resKey, basicTariffInGermany));

					var genCustomColumnDefinition4 = Factory.New<GenCustomColumnDefinition>();
					genCustomColumnDefinition4.XC_Name = "My Date 2";
					genCustomColumnDefinition4.XC_Type = "DTE";
					genCustomColumnDefinition4.XC_DisplaySequence = 2;
					genCustomColumnDefinition4.XC_XR = dateRules.PK;

					// Save Process Task Templates
					var processTaskTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					processTaskTemplate1.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
					processTaskTemplate1.P0_IsActive = true;
					processTaskTemplate1.GenCustomColumnDefinitions.Add(genCustomColumnDefinition1);
					processTaskTemplate1.GenCustomColumnDefinitions.Add(genCustomColumnDefinition2);
					Factory.Save();

					var processTaskTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					processTaskTemplate2.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
					processTaskTemplate2.P0_IsActive = true;
					processTaskTemplate2.GenCustomColumnDefinitions.Add(genCustomColumnDefinition3);
					processTaskTemplate2.GenCustomColumnDefinitions.Add(genCustomColumnDefinition4);
					Factory.Save();

					Factory.NewWithValidTestData<GlbStaff>();
					Factory.Save();

					var result = controller.GetCustomFieldDefinitions(GlbStaffSchema.Constants.Prefix, languageCode);
					var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

					result.AssertJsonResultContains(
						new
						{
							Name = "My Date",
							Type = "DTE",
							Caption = "My Date",
							MetaData = new
							{
								CustomFieldOrigin = "GenCustomColumnDefinition",
								Mandatory = true,
								DateTimeFormat = nameof(KDateTimeFormat.Short) }
						},
						jsonSerializerSettings);
					result.AssertJsonResultContains(
						new
						{
							Name = "My Text",
							Type = "STR",
							Caption = "My Text",
							MetaData = new
							{
								CustomFieldOrigin = "GenCustomColumnDefinition",
								MaxLength = 4,
								List = new[] { new { Code = "UNO", Desc = "Numero Uno" }, new { Code = "TRES", Desc = "Nomor Tiga" } },
								Mandatory = true
							}
						},
						jsonSerializerSettings);
					result.AssertJsonResultContains(
						new
						{
							Name = "Base Company Tariff",
							Type = "STR",
							Caption = languageCode == Languages.German ? basicTariffInGermany : "Base Company Tariff",
							MetaData = new
							{
								CustomFieldOrigin = "GenCustomColumnDefinition",
								MaxLength = 4,
								List = new[] { new { Code = "UNO", Desc = "Numero Uno" }, new { Code = "TRES", Desc = "Nomor Tiga" } },
								Mandatory = true
							}
						},
						jsonSerializerSettings);
					result.AssertJsonResultContains(
						new
						{
							Name = "My Date 2",
							Type = "DTE",
							Caption = "My Date 2",
							MetaData = new
							{
								CustomFieldOrigin = "GenCustomColumnDefinition",
								Mandatory = true,
								DateTimeFormat = nameof(KDateTimeFormat.Short) }
						},
						jsonSerializerSettings);

					var resultAsJArray = result.GetJsonResult() as JArray;
					AssertEquals(4, resultAsJArray.Count);
				}
			}

			TestCaseGetCustomFieldDefinitions(1, Languages.German);
			TestCaseGetCustomFieldDefinitions(2, Languages.French);
			TestCaseGetCustomFieldDefinitions(3, Languages.English);
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new CustomFieldsController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		CustomFieldsController controller;
	}
}
