using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.TransportConsignment.Business.Common.GlowHelper;

namespace Enterprise.TransportConsignment.Business.Testing.Common
{
	public class GlowHelperTest : TestCaseWithFactory
	{
		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenEntityPKEmpty_ShouldReturnNullAndErrorMessage()
		{
			var urlResult = GetGlowUrlForExistingEntity(["LTP"], EntityName.Consignment, ZGuid.Empty, publish: true, isRestrict: false);

			AssertNull("When entity PK is not supported, Should return null", urlResult.Uri);
			AssertEquals("When entity PK is not supported, Should return error message", "Invalid entity PK.", urlResult.ErrorMessage.ToString());
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenGlowServiceUriIsEmpty_ShouldReturnNullAndErrorMessage()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: false, isRestrict: false);

				AssertNull("When glow service uri is not configured, Should return null", urlResult.Uri);
				AssertEquals("When glow service URL is not configured, Should return error message", "Glow service URL has not been configured for this client. Registry: GLOW/Services/GLOW Service URL.", urlResult.ErrorMessage.ToString());
			}
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenGlowPortalUriIsEmpty_ShouldReturnNullAndErrorMessage()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: false, isRestrict: false);

				AssertNull("When glow portal uri is not configured, Should return null", urlResult.Uri);
				AssertEquals("When glow portal URL is not configured, Should return error message", "Glow portal URL has not been configured for this client. Registry: GLOW/Services/GLOW Portals Root URL.", urlResult.ErrorMessage.ToString());
			}
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenLTPInGlowRestrictedModulesOverride_ShouldReturnCorrectGlow2Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["LTP"], EntityName.Consignment, consignmentPK, publish: true, isRestrict: false);

			AssertContains("When LTP Module is in GlowRestrictedModulesOverride, LTP module should be enabled and glow URL should contain ConsignmentGlow2 and consignment PK",
				$"https://localhost/Goto/ConsignmentGlow2?sso_otp=sometoken&entityPK={ConsignmentGuid}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenLTPAndCSTNotInGlowRestrictedModulesOverride_ShouldReturnGlow2Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["BMP"], EntityName.RunSheet, runSheetPK, publish: true, isRestrict: false);

			AssertContains("When LTP and CST Modules are not in GlowRestrictedModulesOverride, LTP module should be enabled and glow URL should contain RunSheetGlow2",
				$"https://localhost/Goto/RunSheetGlow2?sso_otp=sometoken&entityPK={runSheetPK}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenLTPPublishedAndUnrestricted_ShouldReturnGlow2Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: true, isRestrict: false);

			AssertContains("When LTP is published and unrestricted, LTP module should be enabled and glow URL should contain ConsignmentGlow2 and consignment PK",
				$"https://localhost/Goto/ConsignmentGlow2?sso_otp=sometoken&entityPK={ConsignmentGuid}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_WhenLTPNotPublishedAndUnrestricted_ShouldReturnGlow1Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: false, isRestrict: false);

			AssertContains("When LTP is not published and unrestricted, CST module should be enabled and glow URL should contain ConsignmentGlow1",
				$"https://localhost/Goto/ConsignmentGlow1?sso_otp=sometoken&entityPK={ConsignmentGuid}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGetLTPModuleEnabled_WhenLTPPublishedAndRestricted_ShouldReturnGlow1Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: true, isRestrict: true);

			AssertContains("When LTP is not published and restricted, CST module should be enabled and glow URL should contain ConsignmentGlow1",
				$"https://localhost/Goto/ConsignmentGlow1?sso_otp=sometoken&entityPK={ConsignmentGuid}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGetLTPModuleEnabled_WhenLTPNotPublishedAndRestricted_ShouldReturnGlow1Url()
		{
			var urlResult = GetGlowUrlForExistingEntity(["CST"], EntityName.Consignment, consignmentPK, publish: false, isRestrict: true);

			AssertContains("When LTP is not published and restricted, CST module should be enabled and glow URL should contain ConsignmentGlow1",
				$"https://localhost/Goto/ConsignmentGlow1?sso_otp=sometoken&entityPK={ConsignmentGuid}",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGenerateGotoGlowUrlForNewEntityAsync_ShouldReturnValidUriWithoutEntityPKParameter()
		{
			var urlResult = GetGlowUrlForNewEntity(["LTP"], EntityName.Consignment, publish: true, isRestrict: false);

			AssertContains("Should return correct URL to create new Consignment", "https://localhost/Goto/NewConsignmentGlow2?sso_otp=sometoken",
				urlResult.Uri.ToString());
			AssertNull("Error messages should be empty", urlResult.ErrorMessage);
		}

		public void TestGenerateGotoGlowUrlForExistingEntityAsync_ShouldUseDbCorrectly()
		{
			var lastError = string.Empty;
			var thread = new Thread(() =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StringContent($"{{\"Publish\":true,\"IsRestricted\":false}}");
				clientMock.Setup(c => c.GetAsync(Endpoint)).ReturnsAsync(response);
				ErrorReporter.Clear();

				GenerateGotoGlowUrlForExistingEntityAsync(GlowHelper.EntityName.Consignment, consignmentPK).GetAwaiter().GetResult();

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});

			thread.Start();
			thread.Join(1000);

			AssertEquals("Should not contain \"Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()\"", string.Empty, lastError);
		}

		#region Implementation

		GenerateGotoGlowUrlResult GetGlowUrlForExistingEntity(string[] restrictedModules, EntityName entityName, ZGuid entityPK, bool publish, bool isRestrict)
		{
			SetUpGlowContext(restrictedModules, publish, isRestrict);

			return GenerateGotoGlowUrlForExistingEntityAsync(entityName, entityPK).Result;
		}

		GenerateGotoGlowUrlResult GetGlowUrlForNewEntity(string[] restrictedModules, EntityName entityName, bool publish, bool isRestrict)
		{
			SetUpGlowContext(restrictedModules, publish, isRestrict);

			return GenerateGotoGlowUrlForNewEntityAsync(entityName).Result;
		}

		void SetUpGlowContext(string[] restrictedModules, bool publish, bool isRestrict)
		{
			SetGlowRestrictedModulesOverride(restrictedModules);

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			response.Content = new StringContent($"{{\"Publish\":{(publish ? "true" : "false")},\"IsRestricted\":{(isRestrict ? "true" : "false")}}}");
			disposables.Add(response);

			clientMock.Setup(c => c.GetAsync(Endpoint)).ReturnsAsync(response);
		}

		static void SetGlowRestrictedModulesOverride(string[] modules)
		{
			const string glowRestrictedModulesOverrideName = "GlowRestrictedModulesOverride";
			var businessObjectFactory = new BusinessObjectFactory();
			var stmData = businessObjectFactory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, glowRestrictedModulesOverrideName));

			if (stmData == null)
			{
				stmData = businessObjectFactory.New<StmData>();
				stmData.SD_Name = glowRestrictedModulesOverrideName;
			}

			using (var stream = new MemoryStream())
			{
				var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(string[]));
				serializer.WriteObject(stream, modules);
				stream.Position = 0;

				using (var streamSource = new StreamSource(stream))
				{
					stmData.SetSD_BinaryValueSource(streamSource);
					businessObjectFactory.Save();
				}
			}
		}

		protected override void SetUp()
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();

			singleSignOnHelperMock.Setup(h => h.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>())).Returns("sometoken");
			clientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(clientMock.Object);

			disposables.Add(ObjectFactory.Substitute(singleSignOnHelperMock.Object));
			disposables.Add(ObjectFactory.Substitute(clientFactoryMock.Object));
			disposables.Add(GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GlowPortalUrl));
			disposables.Add(GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GlowServiceUrl));
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposables.ForEach(d => d.Dispose());
		}

		const string ConsignmentGuid = "9684c3a6-fad6-4c73-865b-c3197d8f4c9e";
		const string RunSheetGuid = "def193c6-070a-48f5-bf2a-b57a4255fd44";
		const string GlowServiceUrl = "https://localhost/Glow";
		const string GlowPortalUrl = "https://localhost/";
		const string Endpoint = "api/bpm/module/GetModuleInfo?code=LTP";

		readonly ZGuid consignmentPK = new ZGuid(ConsignmentGuid);
		readonly ZGuid runSheetPK = new ZGuid(RunSheetGuid);
		readonly List<IDisposable> disposables = new List<IDisposable>();
		readonly Mock<IGlowServiceClient> clientMock = new Mock<IGlowServiceClient>();

		#endregion Implementation
	}
}
