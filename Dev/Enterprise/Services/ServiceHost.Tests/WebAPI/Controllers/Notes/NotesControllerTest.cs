using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers;
using Enterprise.ZArchitecture.Business;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.Notes
{
	public class NotesControllerTest : TestCaseWithFactory
	{
		public void TestGetCustomNoteTypes()
		{
			var result = controller.GetCustomNoteTypes("Foo").GetStringResult();

			AssertEquals(
				result,
				JsonConvert.SerializeObject(new[]
				{
					new {
						Name="Module1 Note A",
						Description="Module1 Note A",
						DefaultVisibility="PUB",
						IsTextOnly= true,
					},
					new {
						Name="Module1 Note Private",
						Description="Module1 Note Private",
						DefaultVisibility="PRV",
						IsTextOnly= false,
					},
				}));

			result = controller.GetCustomNoteTypes("Bar").GetStringResult();
			AssertEquals(
				JsonConvert.SerializeObject(new[]
				{
					new {
						Name="Bar Note",
						Description="Bar Note",
						DefaultVisibility="PUB",
						IsTextOnly= true,
					},
				}),
				result);
		}

		public void TestGetCustomNoteTypes_NoNoteTypes()
		{
			var result = controller.GetCustomNoteTypes("Baz").GetStringResult();

			AssertEquals("[]", result);
		}

		public void TestGetCustomNoteTypes_TranslateDescription()
		{
			CombineAssertions(() =>
			{
				new (string LanguageCode, string ExpectedDescription)[]
				{
					(Core.SharedConstants.Languages.English, "Bar Note"),
					(Core.SharedConstants.Languages.French, "Note in French"),
					(Core.SharedConstants.Languages.ChineseSimplified, "Note in Simplified Chinese"),
					(Core.SharedConstants.Languages.Japanese, "Bar Note"),
				}.ForEach(testcase =>
				{
					var result = controller.GetCustomNoteTypes("Bar", testcase.LanguageCode).GetJsonResult();
					AssertEquals(testcase.ExpectedDescription, result[0].Value<string>("Description"));
				});
			});
		}

		public void TestGetNoteTypesInfo()
		{
			var noteTypesToTest = new[] {
				PredefinedNoteTypes.Instance.CarrierBookingRequest.Description,
				PredefinedNoteTypes.Instance.AgentNotes.Description,
				"I'm not a note.", // non-existent note types will be ignored
			};

			var result = controller.GetNoteTypesInfo(noteTypesToTest).GetStringResult();
			AssertEquals(
				JsonConvert.SerializeObject(new[]
				{
					new {
						Name="Carrier Booking Request",
						Description="Carrier Booking Request",
						DefaultVisibility="PUB",
						IsTextOnly= true,
					},
					new {
						Name="Agent Notes",
						Description="Agent Notes",
						DefaultVisibility="AGV",
						IsTextOnly= true,
					},
				}),
				result);
		}

		public void TestGetNoteTypesInfo_TranslateDescription()
		{
			CombineAssertions(() =>
			{
				new (string LanguageCode, string ExpectedDescription)[]
				{
					(Core.SharedConstants.Languages.English, "Carrier Booking Request"),
					(Core.SharedConstants.Languages.French, "Note in French"),
					(Core.SharedConstants.Languages.ChineseSimplified, "Note in Simplified Chinese"),
					(Core.SharedConstants.Languages.Japanese, "Carrier Booking Request"),
				}.ForEach(testcase =>
				{
					var result = controller.GetNoteTypesInfo(new[] { PredefinedNoteTypes.Instance.CarrierBookingRequest.Description }, testcase.LanguageCode).GetJsonResult();
					AssertEquals(testcase.ExpectedDescription, result[0].Value<string>("Description"));
				});
			});
		}

		public void TestGetNoteTypesInfo_NoRequestBody()
		{
			var result = controller.GetNoteTypesInfo(null, "EN").ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
		}

		public void TestNoteTypesExist()
		{
			CombineAssertions(() =>
			{
				new (string NoteTypeName, bool ExpectedResult)[]
				{
					("Module1 Note A", true),
					("Bar Note", true),
					("I'm not a note.", false),
				}.ForEach(testcase =>
				{
					var result = controller.NoteTypesExist(testcase.NoteTypeName).GetStringResult();
					AssertEquals(testcase.ExpectedResult, bool.Parse(result));
				});
			});
		}

		public void TestNoteTypesExist_EmptyParameter()
		{
			new string[]
			{
				null,
				""
			}.ForEach(testcase =>
			{
				var result = controller.NoteTypesExist(testcase).ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			});
		}

		static IDisposable SetCustomNoteTypes()
		{
			var module1 = new CustomNoteModuleAndCountry { ModuleIDName = "Foo" };
			AddNoteType(module1.CustomNoteTypesList, "Module1 Note A", StmNoteVisibility.PUB, true);
			AddNoteType(module1.CustomNoteTypesList, "Module1 Note Private", StmNoteVisibility.PRV, false);

			var module2 = new CustomNoteModuleAndCountry { ModuleIDName = "Bar" };
			AddNoteType(module2.CustomNoteTypesList, "Bar Note", StmNoteVisibility.PUB, true);

			var modules = new[] { module1, module2 };

			var customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.AddRange(modules);
			return SystemDataRegistry.Instance.CustomNotes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes);
		}

		static void AddNoteType(CustomNoteTypeItemCollection collection, string name, StmNoteVisibility visibility, bool isTextOnly)
		{
			var noteType = collection.AddNew();
			noteType.NoteName = name;
			noteType.IsTextOnly = isTextOnly;
			noteType.DefaultVisibility = visibility.ToString();
		}

		protected override void SetUp()
		{
			base.SetUp();

			resourceMockFr = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData();
			resourceMockFr.SetResourceGetter(key => new ResourceStringData(key, $"Note in French"));

			resourceMockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData();
			resourceMockChs.SetResourceGetter(key => new ResourceStringData(key, $"Note in Simplified Chinese"));

			resourceMockJpn = Res.GetLanguageInstance(Core.SharedConstants.Languages.Japanese).UseMockData();

			customNoteTypesDisposable = SetCustomNoteTypes();
			controller = new NotesController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			base.TearDown();

			customNoteTypesDisposable?.Dispose();

			resourceMockFr?.Dispose();
			resourceMockChs?.Dispose();
			resourceMockJpn?.Dispose();

			controller?.Dispose();
		}

		IDisposable customNoteTypesDisposable;
		IMockResourceStringCache resourceMockFr;
		IMockResourceStringCache resourceMockChs;
		IMockResourceStringCache resourceMockJpn;
		NotesController controller;
	}
}
