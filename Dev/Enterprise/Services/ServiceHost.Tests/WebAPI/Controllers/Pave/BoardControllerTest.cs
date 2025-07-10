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
	class BoardControllerTest : PaveControllerTestCase<IBoardService, BoardController>
	{
		protected Mock<IBoardService> ServiceMock;
		protected BoardController Controller;
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		protected override void SetUp()
		{
			base.SetUp();
			serializerSettings.Converters.Add(new StringEnumConverter());
			ServiceMock = new Mock<IBoardService>();
			var mockHttpRequestMessage = new Mock<HttpRequestMessage> { CallBase = true };
			var mocHttpConfiguration = new Mock<HttpConfiguration> { CallBase = true };
			var identity = GlowTicketTestHelper.CreateStaffIdentity((GlbStaff)Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			Controller = (BoardController)Activator.CreateInstance(typeof(BoardController));
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

		public void TestGetConfiguration_ReturnJson()
		{
			var boardPK = Guid.NewGuid();
			var dummyDTO = CreateDummyConfigurationDTO();

			ServiceMock.Setup(m => m.GetConfiguration(boardPK)).Returns(dummyDTO);

			var jsonResult = GetResult(Controller.GetConfiguration(boardPK));

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

		static BoardConfigurationDTO CreateDummyConfigurationDTO()
		{
			var sections = new ComponentSectionConfigurationDTO[]
			{
				new ComponentSectionConfigurationDTO()
				{
					Name = "Section1",
					ComponentType = ComponentType.Buffer,
					Layout = new SectionLayoutDTO()
					{
						Row = 0,
						Column = 0,
						RowSpan = 0,
						ColSpan = 1,
						RowHeightPercent = 100,
						ColWidthPercent = 100,
					},
					Components = new ComponentDTO[]
					{
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component1",
						},
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component2",
						}
					},
					SubSections = 1,
					CellsPerSubSection = 10,
					FlowDirection = FlowDirection.Up,
					LastCellPosition = null,
					MaxOverdueSlots = 0,
					FadeBackgroundAtPercentage = 80,
					IsReleaseScheduler = false,
					ShowZones = true,
					BufferZonesColors = new Dictionary<int, string>() { { 0, "#FF0000" }, { 1, "#00FF00" }, { 2, "#0000FF" }, { 3, "#FFFF00" } },
					PrimaryChannels = new SectionChannelsConfigurationDTO()
					{
						ChannelBy = ChannelType.Override,
						Channels = new ChannelDTO[]
						{
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 1",
								Type = ChannelType.Capability
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 2",
								Type = ChannelType.Group
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 3",
								Type = ChannelType.Resource
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 4",
								Type = ChannelType.Tag
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 5",
								Type = ChannelType.NotChanneled
							}
						}
					},
					SecondaryChannels = new SectionChannelsConfigurationDTO()
					{
						ChannelBy = ChannelType.Time
					}
				},
				new ComponentSectionConfigurationDTO()
				{
					Name = "Section2",
					ComponentType = ComponentType.Buffer,
					Layout = new SectionLayoutDTO()
					{
						Row = 1,
						Column = 0,
						RowSpan = 0,
						ColSpan = 0,
						RowHeightPercent = 90,
						ColWidthPercent = 80,
					},
					Components = new ComponentDTO[]
					{
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component1",
						},
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component2",
						}
					},
					SubSections = 1,
					CellsPerSubSection = 1,
					FlowDirection = FlowDirection.Down,
					LastCellPosition = LastCellPosition.Left,
					MaxOverdueSlots = 0,
					FadeBackgroundAtPercentage = 80,
					IsReleaseScheduler = true,
					PrimaryChannels = new SectionChannelsConfigurationDTO()
					{
						ChannelBy = ChannelType.ReleaseScheduler,
						Channels = new ChannelDTO[]
						{
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel ReleaseScheduler",
								Type = ChannelType.ReleaseScheduler
							}
						}
					}
				},
				new ComponentSectionConfigurationDTO()
				{
					Name = "Section3",
					ComponentType = ComponentType.Bucket,
					Layout = new SectionLayoutDTO()
					{
						Row = 1,
						Column = 1,
						RowSpan = 0,
						ColSpan = 0,
						RowHeightPercent = 70,
						ColWidthPercent = 60,
					},
					Components = new ComponentDTO[]
					{
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component1",
						},
						new ComponentDTO()
						{
							PK = Guid.NewGuid(),
							Name = "Component2",
						}
					},
					SubSections = 4,
					CellsPerSubSection = 2,
					FlowDirection = FlowDirection.Left,
					LastCellPosition = LastCellPosition.Top,
					MaxOverdueSlots = 2,
					PrimaryChannels = new SectionChannelsConfigurationDTO()
					{
						ChannelBy = ChannelType.Override,
						Channels = new ChannelDTO[]
						{
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 1",
								Type = ChannelType.Capability
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 2",
								Type = ChannelType.Group
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 3",
								Type = ChannelType.Resource
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 4",
								Type = ChannelType.Tag
							},
							new ChannelDTO()
							{
								EntityPK = Guid.NewGuid(),
								Name = "Channel 5",
								Type = ChannelType.NotChanneled
							}
						}
					}
				}
			};

			return new BoardConfigurationDTO()
			{
				RefreshIntervalMinutes = 10,
				Sections = sections
			};
		}
		#endregion
	}
}
