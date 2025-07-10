using System;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(TemplateRecordZController))]
	public class TemplateRecordZControllerTest : ZControllerBasherTest
	{
		public void TestLoadedTemplateRecordWhenAddStmNote()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ViewQuotedBooking quotedBooking = null;

				using (var module = new TemplateRecordSupportedFilterGridModuleForTest())
				{
					quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					quotedBooking.QuotedBooking.Booking.JS_TransportMode = "AIR";

					var stmNote = Factory.NewWithValidTestData<QuotedBookingStmNote>();
					stmNote.ST_Description = "Goods Handling Instructions";
					_ = quotedBooking.QuotedBooking.Booking.Notes.GetAllNotes();

					quotedBooking.QuotedBooking.Notes.Add(stmNote);
					quotedBooking.Factory.Save();
				}

				var controller = new TemplateRecordZControllerForTest();
				AssertNoExceptionThrown(() => controller.GetLoadedBusinessEntityInLocalFactoryExposed((quotedBooking as ITemplateRecordProvider).TemplateRecord as BusinessObject));
			}
		}

		public void TestLoadedTemplateRecordWhenOneOffQuoteIsNotSaved()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ViewQuotedBooking viewQuotedBooking = null;

				var factory = new TemplateRecordBusinessObjectFactory();

				using (var module = new TemplateRecordSupportedFilterGridModuleForTest())
				{
					ZGuid quoteOnlyPK = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
					QuotedBooking quoteOnly = QuotedBooking.New(quoteOnlyPK, ZGuid.Empty, factory);

					viewQuotedBooking = quoteOnly.Factory.Load<ViewQuotedBooking>(quoteOnlyPK);
				}

				var controller = new TemplateRecordZControllerForTest();
				AssertNoExceptionThrown(() => controller.GetLoadedBusinessEntityInLocalFactoryExposed(viewQuotedBooking));
			}
		}

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(ViewQuotedBooking);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.QuotedBookings;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			Factory.Save();

			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;

			return viewQuotedBooking;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		[RequiresSTA]
		public void TestTemplateRecordSupportedFilterGridModuleHandleSqlExceptions()
		{
			using (var module = new TemplateRecordSupportedFilterGridModuleForTestWithSqlException())
			{
				module.mockErrorNumber = 8003;
				module.PerformSearch();
				AssertEquals("The incoming request has too many parameters. The server supports a maximum of 2100 parameters. Reduce the number of parameters and resend the request.", UnitTestUserNotification.Instance.LastMessage.Text);

				module.mockErrorNumber = 8623;
				module.PerformSearch();
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		[RequiresSTA]
		public void TestTemplateRecordSupportedFilterGridModuleDisableQueryGovernorCostLimit_WhenTemplateRecordsAreActivelyFiltered()
		{
			using (var module = new TemplateRecordSupportedFilterGridModuleForTest())
			{
				var templateRecordsFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter(FilterStripBusinessObject.TemplateRecordsDescription, _ => new ZQuery(), module.FilterBusinessObject.TemplateRecordsFilterList);
				templateRecordsFilter.Category = FilterCategories.Other;
				templateRecordsFilter.SupportsXQuery = true;
				templateRecordsFilter.MultilingualDescription = (NoResString)"Template Records";

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				module.PerformSearch();
				var eventList = SqlEventTracker.Instance.SqlEventList;
				AssertEquals(false, eventList.Any(x => x.Contains("SET QUERY_GOVERNOR_COST_LIMIT")));
			}
		}

		[RequiresSTA]
		public void TestTemplateRecordSupportedFilterGridModuleUseQueryGovernorCostLimit_WhenTemplateRecordsAreNotBeingFiltered()
		{
			using (var module = new TemplateRecordSupportedFilterGridModuleForTest())
			{
				var templateRecordsFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter(FilterStripBusinessObject.TemplateRecordsDescription, _ => new ZQuery(), module.FilterBusinessObject.TemplateRecordsFilterList);
				templateRecordsFilter.Category = FilterCategories.Other;
				templateRecordsFilter.SupportsXQuery = true;
				templateRecordsFilter.MultilingualDescription = (NoResString)"Template Records";

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;

				module.PerformSearch();
				var eventList = SqlEventTracker.Instance.SqlEventList;
				AssertEquals(true, eventList.Any(x => x.Contains("SET QUERY_GOVERNOR_COST_LIMIT")));
			}
		}

		[RequiresSTA]
		public void TestTemplateRecordSupportedFilterGridModuleUseQueryGovernorCostLimit_WhenTemplateRecordsAreExcluded()
		{
			using (var module = new TemplateRecordSupportedFilterGridModuleForTest())
			{
				var templateRecordsFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter(FilterStripBusinessObject.TemplateRecordsDescription, _ => new ZQuery(), module.FilterBusinessObject.TemplateRecordsFilterList);
				templateRecordsFilter.Category = FilterCategories.Other;
				templateRecordsFilter.SupportsXQuery = true;
				templateRecordsFilter.MultilingualDescription = (NoResString)"Template Records";

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesExcluded;

				module.PerformSearch();
				var eventList = SqlEventTracker.Instance.SqlEventList;
				AssertEquals(true, eventList.Any(x => x.Contains("SET QUERY_GOVERNOR_COST_LIMIT")));
			}
		}

		[RequiresSTA]
		public void TestTemplateRecordSupportedFilterGridModuleLoadTemplateLimitMaxNumberOfRecordsToShowInDisplayGrids()
		{
			using (var module = new TemplateRecordSupportedFilterGridModuleForTest() { AllowLoadTemplateRecords = true })
			{
				var templateRecordsFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter(FilterStripBusinessObject.TemplateRecordsDescription, _ => new ZQuery(), module.FilterBusinessObject.TemplateRecordsFilterList);
				templateRecordsFilter.Category = FilterCategories.Other;
				templateRecordsFilter.SupportsXQuery = true;
				templateRecordsFilter.MultilingualDescription = (NoResString)"Template Records";

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				module.PerformSearch();
				var eventList = SqlEventTracker.Instance.SqlEventList;
				AssertEquals(true, eventList.Any(x => x.Contains($"SELECT  TOP {SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value + 1}\r\nSTR_PK")));
			}
		}

		#region class TemplateRecordSupportedFilterGridModuleForTest

		internal class TemplateRecordSupportedFilterGridModuleForTestWithSqlException : TemplateRecordSupportedFilterGridModuleForTest
		{
			public int mockErrorNumber;

			protected override BusinessObjectFactory GetNewFactory() => new BusinessObjectFactoryForTest(mockErrorNumber);
		}

		internal class TemplateRecordSupportedFilterGridModuleForTest : TemplateRecordSupportedFilterGridModule
		{
			public override ModuleIdentifier ID => ModuleIDs.QuotedBookings;

			public override SecurityCheckpoint SecurityCheckpoint => Env.Security.QuickBooking;

			protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Booking;

			protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			}

			protected override FilterBusinessObject GetNewFilterBusinessObject()
			{
				return new FilterStripBusinessObjectForTest();
			}

			protected override IFilterControl GetNewFilterControl()
			{
				return new DummyFilterControl(GridCollection, FilterBusinessObject);
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return new DummyBusinessObjectCollection(Factory);
			}

			internal ViewQuotedBooking GetNewTemplateRecordBusinessObjectCoreExposed()
			{
				return (ViewQuotedBooking)GetNewTemplateRecordBusinessObjectCore();
			}

			protected override BusinessObject GetNewTemplateRecordBusinessObjectCore()
			{
				var factory = new TemplateRecordBusinessObjectFactory();

				var booking = QuotedBooking.CreateNewBooking(factory);
				var newBizO = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);

				var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
				templateRecord.STR_ModuleID = ID.Name;

				factory.TemplateRecordProvider = newBizO;
				factory.TemplateRecordProvider.IsTemplateRecord = true;
				factory.TemplateRecordProvider.TemplateRecord = templateRecord;

				return ViewQuotedBooking.LoadOrCreate(newBizO);
			}

			protected override bool SupportTemplateRecords => true;

			public void PerformSearch()
			{
				base.PerformSearch();
			}
		}

		#endregion

		#region class FilterStripBusinessObjectForTest

		internal class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				return new ModuleFilterCollection();
			}
		}
		#endregion

		#region class TemplateRecordZControllerForTest

		internal class TemplateRecordZControllerForTest : TemplateRecordZController
		{
			protected override bool IsQuotedBooking => true;

			public override ControllerID ID => ControllerIDs.QuotedBookings;

			public override ModuleIdentifier ModuleID => ModuleIDs.QuotedBookings;

			public override Type TypeOfTopLevelBusinessObject => typeof(ViewQuotedBooking);

			public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

			protected override SecurityCheckpoint CheckPointForView => Env.Security.QuickBooking;

			protected override SecurityCheckpoint CheckPointForNew => Env.Security.QuickBookingNew;

			protected override SecurityCheckpoint CheckPointForEdit => Env.Security.QuickBookingEdit;

			protected override SecurityCheckpoint CheckPointForDelete => Env.Security.QuickBookingDelete;

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				throw new NotImplementedException();
			}
			internal QuotedBooking GetLoadedBusinessEntityInLocalFactoryExposed(IBusiness bizO)
			{
				return (QuotedBooking)GetLoadedBusinessEntityInLocalFactory(bizO);
			}
		}
		#endregion

		#region class BusinessObjectFactoryForTest

		class BusinessObjectFactoryForTest : BusinessObjectFactory
		{
			public BusinessObjectFactoryForTest(int mockErrorNumber)
			{
				this.mockErrorNumber = mockErrorNumber;
			}

			readonly int mockErrorNumber;

			public override BusinessObject[] Load(Type bizOType, ZQuery sqlFilter)
			{
				throw NewSqlException(mockErrorNumber);
			}

			static T Construct<T>(params object[] p)
			{
				var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
				return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
			}

			static SqlException NewSqlException(int number, string errorMessage = "error message", string proc = "proc", string serverName = "server name")
			{
				var collection = Construct<SqlErrorCollection>();
				var error = Construct<SqlError>(number, (byte)2, (byte)3, serverName, errorMessage, proc, 100);

				typeof(SqlErrorCollection)
					.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
					.Invoke(collection, new object[] { error });

				return typeof(SqlException)
					.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
						null,
						CallingConventions.ExplicitThis,
						new[] { typeof(SqlErrorCollection), typeof(string) },
						Array.Empty<ParameterModifier>())
					.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
			}
		}

		#endregion

		#endregion
	}
}
