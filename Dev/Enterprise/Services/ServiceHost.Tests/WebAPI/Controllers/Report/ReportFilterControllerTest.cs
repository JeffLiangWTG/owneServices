using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ReportFilterControllerTest : BaseReportControllerTest<ReportFilterController>
	{
		public void TestGetSecurityRights_Staff()
		{
			var expectedResult = new List<SecurityRightNodeData>();
			expectedResult.Add(new SecurityRightNodeData() { Name = "TestName", Code = "TEST CODE", ChildRights = new List<SecurityRightNodeData>() });
			mockService.Setup(x => x.GetSecurityRights()).Returns(expectedResult);
			var actionResult = controller.GetSecurityRights();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetOrganisationRegistrationCodeTypes()
		{
			var expectedResult = new List<CodeDescription> { new CodeDescription { Code = "ABCCode1", Description = "ABCCode1Description" } };
			mockService.Setup(x => x.GetOrganisationRegistrationCodeTypes("ABC")).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetOrganisationRegistrationCodeTypes("ABC");
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetLookupDataRetrieveCodeDescription()
		{
			var filter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.StartsWith, "Z");

			var collection = new RefCountryCollection(new BusinessObjectFactory(), filter);
			var expectedResult = new
			{
				count = collection.Count,
				results = collection.Select(o => new CodeDescription { Pk = o.PK.ToGuid(), Code = o.RN_Code, Description = o.RN_Desc }).ToList()
			};

			var param = new LookupFilterSearchArgs() { LookupType = "organisation", SearchTerms = "code", Top = 50 };
			mockService.Setup(x => x.GetLookupData(It.IsAny<LookupFilterSearchArgs>(), It.IsAny<Func<ModuleIdentifier, IBusinessObjectCollection, LookupFilterSearchArgs, IBusinessObjectCollection>>())).Returns(collection);

			AssertEquals("Pre condition: current user is not a web user.", false, Env.CurrentUser.IsWebUser);
			AssertJsonResult(expectedResult, controller.SearchLookupData(param));

			using (Env.SetTemporaryUserContext(new Environment.UserContext(User.WebUserName, Guid.Empty, Guid.Empty)))
			{
				AssertEquals("Pre condition: current user is a web user.", true, Env.CurrentUser.IsWebUser);
				AssertJsonResult(expectedResult, controller.SearchLookupData(param));
			}
		}

		public void TestCalculateDateSchedule()
		{
			var expected = new DateScheduleData();
			var runningError = new ReportRunningError();
			mockService.Setup(x => x.CalculateDateSchedule(It.IsAny<DateScheduleData>(), It.IsAny<ReportScheduleTaskData>())).Returns(expected).Verifiable();
			mockService.Setup(x => x.RunningError).Returns(runningError);

			var actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule = new DateScheduleData(), scheduleTask = new ReportScheduleTaskData() }));
			AssertJsonResult(expected, actual);

			actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule1 = new DateScheduleData(), scheduleTask1 = new ReportScheduleTaskData() }));
			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "dateSchedule and scheduleTask are mandatory.");

			runningError.Errors.Clear();

			actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule = new DateScheduleData(), scheduleTask1 = new ReportScheduleTaskData() }));
			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "dateSchedule and scheduleTask are mandatory.");

			runningError.Errors.Clear();

			actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule1 = new DateScheduleData(), scheduleTask = new ReportScheduleTaskData() }));
			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "dateSchedule and scheduleTask are mandatory.");

			runningError.Errors.Clear();

			actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule = "test", scheduleTask = new ReportScheduleTaskData() }));

			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "Error converting value \"test\" to type 'Enterprise.DocumentEngine.DateScheduleData'. Path 'dateSchedule'.");

			runningError.Errors.Clear();
			actual = controller.CalculateDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { dateSchedule = new DateScheduleData(), scheduleTask = "test" }));

			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "Error converting value \"test\" to type 'Enterprise.DocumentEngine.ReportScheduleTaskData'. Path 'scheduleTask'.");
		}

		public void TestGetDateSchedule()
		{
			var expected = new DateScheduleData();
			mockService.Setup(x => x.GetDateSchedule(It.IsAny<DateTime>(), It.IsAny<ReportScheduleTaskData>())).Returns(expected).Verifiable();

			var actual = controller.GetDateSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { storageValue = DateTime.Now, scheduleTask = new ReportScheduleTaskData() }));
			AssertJsonResult(expected, actual);
		}

		public void TestCalculateAccPeriodSchedule()
		{
			var expected = new AccPeriodScheduleData();
			var runningError = new ReportRunningError();
			mockService.Setup(x => x.CalculateAccPeriodSchedule(It.IsAny<AccPeriodScheduleData>(), It.IsAny<ReportScheduleTaskData>())).Returns(expected).Verifiable();
			mockService.Setup(x => x.RunningError).Returns(runningError);

			var actual = controller.CalculateAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { periodSchedule = new AccPeriodScheduleData(), scheduleTask = new ReportScheduleTaskData() }));
			AssertJsonResult(expected, actual);

			actual = controller.CalculateAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { periodSchedule = new AccPeriodScheduleData() }));
			AssertJsonResult(expected, actual);

			actual = controller.CalculateAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { periodSchedule1 = new AccPeriodScheduleData(), scheduleTask1 = new ReportScheduleTaskData() }));
			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "periodSchedule is mandatory.");

			runningError.Errors.Clear();
			actual = controller.CalculateAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { periodSchedule = "test", scheduleTask = new ReportScheduleTaskData() }));

			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "Error converting value \"test\" to type 'Enterprise.DocumentEngine.AccPeriodScheduleData'. Path 'periodSchedule'.");

			runningError.Errors.Clear();
			actual = controller.CalculateAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { periodSchedule = new AccPeriodScheduleData(), scheduleTask = "test" }));

			AssertNull(actual);
			AssertEquals(runningError.ErrorType, ReportServiceErrorType.ValidationError);
			AssertEquals(runningError.Errors.Count, 1);
			AssertEquals(runningError.Errors[0], "Error converting value \"test\" to type 'Enterprise.DocumentEngine.ReportScheduleTaskData'. Path 'scheduleTask'.");
		}

		public void TestGetAccPeriodSchedule()
		{
			var expected = new AccPeriodScheduleData();
			mockService.Setup(x => x.GetAccPeriodSchedule(It.IsAny<DateTime>(), It.IsAny<ReportScheduleTaskData>())).Returns(expected).Verifiable();

			var actual = controller.GetAccPeriodSchedule(Newtonsoft.Json.Linq.JObject.FromObject(new { storageValue = DateTime.Now, scheduleTask = new ReportScheduleTaskData() }));
			AssertJsonResult(expected, actual);
		}

		protected override string[] StaffOnlyAuthorizationFilterMethods => new string[]
		{
			nameof(ReportFilterController.GetSecurityRights),
			nameof(ReportFilterController.CalculateAccPeriodSchedule),
			nameof(ReportFilterController.GetAccPeriodSchedule),
		};
	}

	class ReportFilterControllerTransactionTest : TestCaseWithFactory
	{
		public void TestGetLookupDataForStaff()
		{
			AssertEquals("Pre condition: current user is not a web user.", false, Env.CurrentUser.IsWebUser);
			var reportFilterController = new ReportFilterController();

			CombineAssertions(() =>
			{
				var allLookupTypes = CollectionAndModuleIDBuilder.Lookup_Exposed.Except(unSupportedLookUpType).Except(countrySpecificLookupType.SelectMany(o => o.Value));
				foreach (var lookupType in allLookupTypes)
				{
					var searchParames = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "1", Top = 5 };
					AssertNoExceptionThrown($"Error happened, lookup type {lookupType}", () =>
					{
						reportFilterController.SearchLookupData(searchParames);
					});
				}
				AssertEquals("There should be no silent error.", 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestBuildCodeDescriptionQueryForStaff()
		{
			string[] exceptLookupTypes =
			{
				CollectionProviderTypeCodeDescriptionList.Codes.Location,
				CollectionProviderTypeCodeDescriptionList.Codes.CASublocation,
				CollectionProviderTypeCodeDescriptionList.Codes.USEntryHeader,
				CollectionProviderTypeCodeDescriptionList.Codes.EUCusTempStorageRegPremises,
				CollectionProviderTypeCodeDescriptionList.Codes.AlternateGLAccount,
			};
			CombineAssertions(() =>
			{
				var allLookupTypes = CollectionAndModuleIDBuilder.Lookup_Exposed.Except(unSupportedLookUpType).Except(countrySpecificLookupType.SelectMany(o => o.Value)).Except(exceptLookupTypes);
				foreach (var lookupType in allLookupTypes)
				{
					var searchParames = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "1", Top = 5 };
					var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupType);
					var collection = LookupFilterDataHelper.LoadModuleDataForStaff(collectionProvider.ModuleID, collectionProvider.CollectionForFindbox, searchParames);

					var codeDescriptionQuery = new ZQuery();
					LookupFilterDataHelper.BuildCodeDescriptionQuery(codeDescriptionQuery, collection.TypeOfElements, searchParames.SearchTerms);
					AssertEquals($"lookup type {lookupType}: code description query should not be empty.", expected: false, codeDescriptionQuery.IsEmpty);

					ZQuery collectionQuery = null;
					if (collection is BusinessObjectCollection legacyCollection)
					{
						var lastLoadedAdditionalFilterProperty = collection.GetType().GetProperty("LastLoadedAdditionalFilter", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
						collectionQuery = (ZQuery)lastLoadedAdditionalFilterProperty.GetValue(collection, null);
					}

					if (collection is IActiveBusinessObjectCollection flyweightCollection)
					{
						collectionQuery = flyweightCollection.AdditionalFilter;
					}

					AssertEquals($"lookup type {lookupType}: collection query should not be empty.", expected: false, collectionQuery.IsEmpty);
					AssertContains($"lookup type {lookupType}: collection query should contain code description query.", codeDescriptionQuery.LiteralTextADO, collectionQuery.LiteralTextADO);
				}
				AssertEquals("There should be no silent error.", 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestGetCountrySpecificLookupData()
		{
			AssertEquals("Pre condition: current user is not a web user.", false, Env.CurrentUser.IsWebUser);
			var reportFilterController = new ReportFilterController();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			CombineAssertions(() =>
			{
				foreach (var kv in countrySpecificLookupType)
				{
					var countryCode = kv.Key;
					var company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_Code = "C" + countryCode;
					company.GC_RN_NKCountryCode = countryCode;

					var branch = company.Branches.AddNew();
					branch.GB_Code = "B" + countryCode;

					staff.GS_GB_HomeBranch = branch.PK;

					var department = Factory.NewWithValidTestData<GlbDepartment>();
					Factory.Save();

					using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
					{
						foreach (var lookupType in kv.Value)
						{
							var searchParames = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "1", Top = 5 };
							AssertNoExceptionThrown($"Error happened, country code: {countryCode}, lookup type: {lookupType}", () =>
							{
								reportFilterController.SearchLookupData(searchParames);
							});
						}
					}
				}
				AssertEquals("There should be no silent error.", 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestGetLookupDataForContact()
		{
			AssertEquals("Pre condition: current user is not a web user.", false, Env.CurrentUser.IsWebUser);
			var reportFilterController = new ReportFilterController();

			CombineAssertions(() =>
			{
				var allLookupTypes = CollectionAndModuleIDBuilder.Lookup_Exposed.Except(unSupportedLookUpType).Except(countrySpecificLookupType.SelectMany(o => o.Value)).Where(l => IsLookupTypeSupportableInWeb(l));
				foreach (var lookupType in allLookupTypes)
				{
					var searchParames = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "1", Top = 5 };
					AssertNoExceptionThrown($"Error happened, lookup type {lookupType}", () =>
					{
						reportFilterController.SearchLookupData(searchParames);
					});
				}

				AssertEquals("There should be no silent error.", 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestBuildCodeDescriptionQueryForContact()
		{
			string[] exceptLookupTypes = { CollectionProviderTypeCodeDescriptionList.Codes.Location };
			CombineAssertions(() =>
			{
				var allLookupTypes = CollectionAndModuleIDBuilder.Lookup_Exposed.Except(unSupportedLookUpType).Except(countrySpecificLookupType.SelectMany(o => o.Value)).Except(exceptLookupTypes).Where(l => IsLookupTypeSupportableInWeb(l));
				foreach (var lookupType in allLookupTypes)
				{
					var searchParames = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "1", Top = 5 };
					var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupType);
					var collection = LookupFilterDataHelper.LoadModuleDataForContact(collectionProvider.ModuleID, searchParames, ZGuid.Empty);

					var codeDescriptionQuery = new ZQuery();
					LookupFilterDataHelper.BuildCodeDescriptionQuery(codeDescriptionQuery, collection.TypeOfElements, searchParames.SearchTerms);
					AssertEquals($"lookup type {lookupType}: code description query should not be empty.", expected: false, codeDescriptionQuery.IsEmpty);

					ZQuery collectionQuery = null;
					if (collection is BusinessObjectCollection legacyCollection)
					{
						var lastLoadedAdditionalFilterProperty = collection.GetType().GetProperty("LastLoadedAdditionalFilter", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
						collectionQuery = (ZQuery)lastLoadedAdditionalFilterProperty.GetValue(collection, null);
					}

					if (collection is IActiveBusinessObjectCollection flyweightCollection)
					{
						collectionQuery = flyweightCollection.AdditionalFilter;
					}

					AssertEquals($"lookup type {lookupType}: collection query should not be empty.", expected: false, collectionQuery.IsEmpty);

					AssertContains($"lookup type {lookupType}: collection query should contain code description query.", codeDescriptionQuery.LiteralTextADO, collectionQuery.LiteralTextADO);
				}
				AssertEquals("There should be no silent error.", 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestBuildCodeDescriptionQueryForContactWithTranslatableField()
		{
			CombineAssertions(() =>
			{
				var bizO = Factory.New<RefCommodityCode>();
				bizO.RH_Code = "ABCD";
				bizO.RH_Description = "Jerry Test";
				Factory.Save();

				using (Res.TemporarilySwitchLanguage("ZH-CN"))
				using (var mockRes = Res.UseMockData())
				{
					var resKey = bizO.RH_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Jerry Test").ResourceKey;
					mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));

					var lookupType = CollectionProviderTypeCodeDescriptionList.Codes.CommodityCode;
					var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupType);

					// Code search
					var searchArgs = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "ABCD", Top = 5 };
					var collection = LookupFilterDataHelper.LoadModuleDataForContact(collectionProvider.ModuleID, searchArgs, ZGuid.Empty);
					var loadedBizO = collection[0] as RefCommodityCode;
					AssertEquals("ABCD", loadedBizO.RH_Code);
					AssertEquals("Jerry Test", loadedBizO.RH_Description);
					AssertEquals("测试", loadedBizO.RH_DescriptionMultilingual);

					// Description search
					searchArgs = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "Jerry Test", Top = 5 };
					collection = LookupFilterDataHelper.LoadModuleDataForContact(collectionProvider.ModuleID, searchArgs, ZGuid.Empty);
					loadedBizO = collection[0] as RefCommodityCode;
					AssertEquals("ABCD", loadedBizO.RH_Code);
					AssertEquals("Jerry Test", loadedBizO.RH_Description);
					AssertEquals("测试", loadedBizO.RH_DescriptionMultilingual);

					// Description Multilingual search
					searchArgs = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "测试", Top = 5 };
					collection = LookupFilterDataHelper.LoadModuleDataForContact(collectionProvider.ModuleID, searchArgs, ZGuid.Empty);
					loadedBizO = collection[0] as RefCommodityCode;
					AssertEquals("ABCD", loadedBizO.RH_Code);
					AssertEquals("Jerry Test", loadedBizO.RH_Description);
					AssertEquals("测试", loadedBizO.RH_DescriptionMultilingual);

					// Description Multilingual search with no result
					searchArgs = new LookupFilterSearchArgs() { LookupType = lookupType, SearchTerms = "测试其他", Top = 5 };
					collection = LookupFilterDataHelper.LoadModuleDataForContact(collectionProvider.ModuleID, searchArgs, ZGuid.Empty);
					AssertEquals("Should have no result", 0, collection.Count);
				}
			});
		}

		bool IsLookupTypeSupportableInWeb(string lookupType)
		{
			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupType);
			return collectionProvider != null && WebModuleIDs.GetWebModuleIDFromModuleID(collectionProvider.ModuleID) != WebModuleIDs.NotAssigned;
		}

		readonly string[] unSupportedLookUpType =
		{
			#region Calculated Code/Description

			CollectionProviderTypeCodeDescriptionList.Codes.CACountry,

			#endregion

			#region ForTestOnly

			CollectionProviderTypeCodeDescriptionList.Codes.Dependence,

			#endregion
		};

		readonly Dictionary<string, string[]> countrySpecificLookupType = new Dictionary<string, string[]>
		{
			{
				Core.Constants.CountryCodes.UnitedStates, new string[]
				{
					CollectionProviderTypeCodeDescriptionList.Codes.DailyStatement,
					CollectionProviderTypeCodeDescriptionList.Codes.MonthlyStatement,
					CollectionProviderTypeCodeDescriptionList.Codes.USCountry,
				}
			},
		};
	}
}
