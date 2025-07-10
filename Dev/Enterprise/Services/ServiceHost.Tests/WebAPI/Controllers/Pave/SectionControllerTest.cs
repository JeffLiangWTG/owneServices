using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Services.ServiceHost.Tests
{
	class SectionControllerTest : PaveControllerTestCase<ISectionService, SectionController>
	{
		protected Mock<ISectionService> ServiceMock;
		protected SectionController Controller;
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		protected override void SetUp()
		{
			base.SetUp();
			serializerSettings.Converters.Add(new StringEnumConverter());
			ServiceMock = new Mock<ISectionService>();
			var mockHttpRequestMessage = new Mock<HttpRequestMessage> { CallBase = true };
			var mocHttpConfiguration = new Mock<HttpConfiguration> { CallBase = true };
			var identity = GlowTicketTestHelper.CreateStaffIdentity((GlbStaff)Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			Controller = (SectionController)Activator.CreateInstance(typeof(SectionController));
			Controller.SetServiceForTest(ServiceMock.Object);
			Controller.ControllerContext = new HttpControllerContext(mocHttpConfiguration.Object, new HttpRouteData(new HttpRoute()), mockHttpRequestMessage.Object)
			{
				Controller = Controller
			};
			Controller.Request.RequestUri = new Uri("http://URI");
			Controller.User = new GenericPrincipal(identity, null);
		}

		public override void TestOnCreateController_ShouldUseDbConnectionCorrectly()
		{
			var lasErrorReported = string.Empty;
			SectionController controller = null;

			Task.Run(() =>
			{
				ErrorReporter.Clear();
				lasErrorReported = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				controller = new SectionController();

				controller.Dispose();
			})
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult();

			AssertNullOrEmpty(lasErrorReported);
		}

		public void TestGet_ReturnJson()
		{
			var sectionPK = Guid.NewGuid();
			var dummyDTO = CreateDummySectionDTO();

			ServiceMock.Setup(m => m.Get(sectionPK)).Returns(dummyDTO);
			var jsonResult = GetResult(Controller.Get(sectionPK));

			ServiceMock.Verify();
			AssertEquals(SerializeToPaveResponseJson(dummyDTO), jsonResult);
		}

		public void TestGetFilterStrips()
		{
			var sectionPK = Guid.NewGuid();
			var dummyDTO = CreateDummyFilterStripsDTO();

			ServiceMock.Setup(m => m.GetFilterStrips(sectionPK)).Returns(dummyDTO);
			var jsonResult = GetResult(Controller.GetFilterStrips(sectionPK));

			ServiceMock.Verify();
			AssertEquals(SerializeToPaveResponseJson(dummyDTO), jsonResult);
		}

		protected new string SerializeToPaveResponseJson<T>(T obj = null, PaveError error = null)
			where T : class
		{
			var paveResponse = new PaveResponse<T>(obj, error);
			return JsonConvert.SerializeObject(paveResponse, serializerSettings);
		}

		#region DummyDTOs

		static ISection CreateDummySectionDTO()
		{
			return new ComponentSectionDTO()
			{
				PK = Guid.NewGuid(),
				Jobs = new[]
				{
					new JobDTO()
					{
						PK = Guid.NewGuid(),
						Properties = new Dictionary<string, object> { { "code", "WI001" }, { "description", "Wave configuration" } }
					},
					new JobDTO()
					{
						PK = Guid.NewGuid(),
						Properties = new Dictionary<string, object> { { "code", "WI002" }, { "description", "Wave Data" } }
					}
				},
				Workflows = new[]
				{
					new WorkflowDTO()
					{
						PK = Guid.NewGuid(),
						ComponentPK = Guid.NewGuid(),
						JobPK = Guid.NewGuid(),
						Index = 33.33m,
						Properties = new Dictionary<string, object> { { "description", "workflow 1 " }, { "status", "OPN" } }
					},
					new WorkflowDTO()
					{
						PK = Guid.NewGuid(),
						ComponentPK = Guid.NewGuid(),
						JobPK = Guid.NewGuid(),
						Index = 33.33m,
						Properties = new Dictionary<string, object> { { "description", "workflow 2 " }, { "status", "BLK" } }
					}
				},
				Capabilities = new[]
				{
					new CapabilityDTO()
					{
						PK = Guid.NewGuid(),
						Code = "Cap1",
						Name = "Capability 1"
					},
					new CapabilityDTO()
					{
						PK = Guid.NewGuid(),
						Code = "Cap2",
						Name = "Capability 2"
					}
				},
				Tags = new[]
				{
					new TagDTO()
					{
						PK = Guid.NewGuid(),
						Code = "Tg1",
						Description = "Tag 1",
						Color = "#FF00FF",
						ColorApplicationStyle = TagColorApplicationStyle.None,
					},
					new TagDTO()
					{
						PK = Guid.NewGuid(),
						Code = "Tg2",
						Description = "Tag 2",
						Color = "#00FF00",
						ColorApplicationStyle = TagColorApplicationStyle.ApplyToBackground,
					},
					new TagDTO()
					{
						PK = Guid.NewGuid(),
						Code = "Tg3",
						Description = "Tag 3",
						Color = "#0000FF",
						ColorApplicationStyle = TagColorApplicationStyle.ApplyToBorder,
						BorderSize = TagBorderSize.Small,
						BorderStyle = TagBorderStyle.Dotted
					}
				},
				Tasks = new[]
				{
					new TaskDTO()
					{
						PK = Guid.NewGuid(),
						WorkflowPK = Guid.NewGuid(),
						CapabilityPK = Guid.NewGuid(),
						TagPKs = new[] { Guid.NewGuid(), Guid.NewGuid() },
						DisplayOrder = 0,
						Properties = new Dictionary<string, object>
						{
							{ "description", "Task 1" },
							{ "note", "some note 1" },
							{ "status", "OPN" },
							{ "type", "UDF" },
							{ "resourcePK", Guid.NewGuid() },
							{ "lowEstimatedMinutes", 30 },
							{ "standardEstimatedMinutes", 60 },
							{ "highEstimatedMinutes", 120 },
							{ "estimateVariationFactor", 2 },
							{ "estimatedTimeToCompleteMinutes", 45 },
							{ "isStartable", true }
						}
					},
					new TaskDTO()
					{
						PK = Guid.NewGuid(),
						WorkflowPK = Guid.NewGuid(),
						CapabilityPK = Guid.NewGuid(),
						TagPKs = new[] { Guid.NewGuid(), Guid.NewGuid() },
						DisplayOrder = 1,
						Properties = new Dictionary<string, object>
						{
							{ "description", "Task 2" },
							{ "note", "some note 2" },
							{ "status", "ASN" },
							{ "type", "CDF" },
							{ "resourcePK", null },
							{ "lowEstimatedMinutes", 60 },
							{ "standardEstimatedMinutes", 120 },
							{ "highEstimatedMinutes", 240 },
							{ "estimateVariationFactor", 4 },
							{ "estimatedTimeToCompleteMinutes", 90 },
							{ "isStartable", false }
						}
					}
				},
				Channels = new[]
				{
					new ChannelContentDTO()
					{
						EntityPK = Guid.NewGuid(),
						Tasks = new[] { Guid.NewGuid(), Guid.NewGuid() }
					},
					new ChannelContentDTO()
					{
						EntityPK = Guid.NewGuid(),
						Tasks = new[] { Guid.NewGuid(), Guid.NewGuid() }
					}
				}
			};
		}

		public static IEnumerable<FilterStripDTO> CreateDummyFilterStripsDTO()
		{
			yield return new FilterStripDTO
			{
				Filter = "P9_Status = @status",
				Type = FilterType.Tasks,
				Parameters = new[] { new CargoWise.PAVE.Common.DTO.SqlParameter { Name = "@status", Type = "varchar(3)", Value = "WRK" } }
			};
		}

		#endregion
	}
}
