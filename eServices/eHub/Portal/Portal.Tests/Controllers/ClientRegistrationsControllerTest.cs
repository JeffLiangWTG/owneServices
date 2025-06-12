using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.DataModel.Business.Validation;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Tests.Controllers.Sharing;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using CargoWise.eServices.Encryption.Client.Encryptor;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ClientRegistrationsControllerTest : BaseControllerTest<ClientRegistrationsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestRegistrationTypesGet()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubRegistrationTypes.AddObject(new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" });

			var result = controller.RegistrationTypes(true);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> {
				{ new { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" } } ,
			}, result, "eHubRegistrationTypes", true);
		}

		[TestMethod]
		public void TestRegistrationTypesSort()
		{
			var expected = new List<(string, string)>() { ("GBCustoms-Direct", "CUS-Authorisation Headers"), ("CARGOWISE", "OCM-CargoSmart Client ID") };
			var actual = new List<(string, string)>() { ("CARGOWISE", "OCM-CargoSmart Client ID"), ("GBCustoms-Direct", "CUS-Authorisation Headers") };
			actual = actual.OrderBy(s => controller.SortRegistrationTypes(s.Item1, s.Item2)).ToList();

			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TestRegistrationTypeInfoGet()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubRegistrationTypes.AddObject(new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" });

			var result = controller.RegistrationTypeInfo(new Guid("{00000000-EEEE-1111-1111-000000000000}"));

			Assert.IsNotNull(result);
			var resultData = result.Data.GetType().GetProperty("eHubRegistrationType").GetValue(result.Data, null);
			AssertEx.PropertyValuesAreEquals(new { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" }, resultData, false);
		}

		[TestMethod]
		public void TestRegistrationTypeInfoEdit()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["RT_ID"] = "REG002";
			request.Container["RT_Description"] = "Registration 2";
			request.Container["oper"] = "add";

			RegistrationTypeInfoEditTest(logger);

			AssertEx.PropertyValuesAreEquals(new { RT_ID = "REG002", RT_Description = "Registration 2" }, context.eHubRegistrationTypes.Select(r => new { r.RT_ID, r.RT_Description }).FirstOrDefault(), false);
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubRegistrationType:"));
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=Client, RT_ID=REG002, RT_Description=Registration 2"));
			var rtPK = context.eHubRegistrationTypes.First().RT_PK;

			request.Clear();
			request.Container["RT_PK"] = rtPK.ToString();
			request.Container["RT_ID"] = "REG002a";
			request.Container["RT_Description"] = "Registration 2a";
			request.Container["oper"] = "edit";

			RegistrationTypeInfoEditTest(logger);

			AssertEx.PropertyValuesAreEquals(new { RT_PK = rtPK, RT_ID = "REG002a", RT_Description = "Registration 2a" }, context.eHubRegistrationTypes.First(r => r.RT_PK == rtPK), false);
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubRegistrationType:"));
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=Client, RT_ID=REG002a, RT_Description=Registration 2a"));

			var registrationType = context.eHubRegistrationTypes.First();
			var clientSystem = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var clientSystemRegistration = new eHubClientSystemRegistration { CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClientSystem = clientSystem, eHubRegistrationType = registrationType, CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = 1 };
			var client = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var clientRegistration = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = client, eHubRegistrationType = registrationType, CX_Code = "REG001" };
			context.eHubClientSystems.AddObject(clientSystem);
			context.eHubClientSystemRegistrations.AddObject(clientSystemRegistration);
			context.eHubClients.AddObject(client);
			context.eHubClientRegistrations.AddObject(clientRegistration);
			context.SaveChanges();

			Assert.IsNotNull(context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == rtPK), "Precondition");
			Assert.IsNotNull(context.eHubClientSystemRegistrations.FirstOrDefault(r => r.CD_PK == clientSystemRegistration.CD_PK), "Precondition");
			Assert.IsNotNull(context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == clientRegistration.CX_PK), "Precondition");

			request.Clear();
			request.Container["RT_PK"] = rtPK.ToString();
			request.Container["oper"] = "del";

			RegistrationTypeInfoEditTest(logger);

			Assert.IsNull(context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == rtPK));
			Assert.IsNull(context.eHubClientSystemRegistrations.FirstOrDefault(r => r.CD_PK == clientSystemRegistration.CD_PK));
			Assert.IsNull(context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == clientRegistration.CX_PK));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubRegistrationType:"));
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=Client, RT_ID=REG002a, RT_Description=Registration 2a"));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientSystemRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CD_Code=REG001, CD_Attr1=username01, CD_Attr2=password01, CD_Flag1=1"));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CX_Code=REG001, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2="));

		}

		[TestMethod]
		public void TestRegistrationsGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CX_CC_ID";
			request.Container["sord"] = "asc";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = cxPk1, eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_ConfigXml = "<test>1</test>", CX_IssuedUTC = DateTime.Parse("2001-02-20 01:22:16"), CX_ExpiryUTC = DateTime.Parse("2027-07-21 02:30:24"), eHubRegistrationType = rt };
			var cx2 = new eHubClientRegistration { CX_PK = cxPk2, eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_IssuedUTC = DateTime.Parse("2008-05-01 07:34:42"), CX_ExpiryUTC = DateTime.Parse("2024-05-02 17:34:42"), eHubRegistrationType = rt };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			var result = controller.Registrations(rt.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {CX_PK = cxPk1, CX_CC_ID = "CLIENT001", CX_Qualifier = (string)null, CX_Code = "REG001", CX_ConfigXml = cxPk1, CX_IssuedUTC = "2001-02-20 01:22:16",  CX_ExpiryUTC = "2027-07-21 02:30:24"},
															  new {CX_PK = cxPk2, CX_CC_ID = "CLIENT002", CX_Qualifier = (string)null, CX_Code = "REG002", CX_ConfigXml = cxPk2, CX_IssuedUTC = "2008-05-01 07:34:42",  CX_ExpiryUTC = "2024-05-02 17:34:42"}},
				result, "eHubClientRegistrations");

			result = controller.Registrations(null);
			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { }, result, "eHubClientRegistrations", true);
		}

		[TestMethod]
		public void TestDownloadConfigFile()
		{

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cx1 = new eHubClientRegistration
			{
				CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"),
				eHubClient = cc1,
				CX_RT = rt.RT_PK,
				CX_Code = "REG001",
				CX_ConfigXml = "<TESTConfiguration></TESTConfiguration>"
			};

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientRegistrations.AddObject(cx1);

			request.Clear();
			request.Container["CX_PK"] = "00000000-FFFF-1111-1111-000000000000";

			var result = controller.DownloadConfiguration(cx1.CX_PK);

			Assert.AreEqual("<TESTConfiguration></TESTConfiguration>", Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual("00000000-ffff-1111-1111-000000000000.xml", result.FileDownloadName);
		}


		[TestMethod]
		public void TestRegistrations_MultipleFilter_AND()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CX_CC_ID\",\"op\":\"bw\",\"data\":\"DummyID\"},{\"field\":\"CX_Qualifier\",\"op\":\"bw\",\"data\":\"QualifierValue\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");

			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID2" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID3" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			var rt2 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2", RT_RegistrantType = "Client" };

			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG001",
				CX_Qualifier = "QualifierValue1",
				CX_Attr1 = "Attr1",
				CX_Password1 = "Password1",
				CX_Flag1 = 11,
				CX_Flag2 = 12,
				CX_ConfigXml = "ConfigXml1"
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			var cx4 = new eHubClientRegistration
			{
				CX_PK = cxPk4,
				eHubClient = cc1,
				eHubRegistrationType = rt2,
				CX_Code = "REG004",
				CX_Qualifier = "QualifierValue4",
			};
			var cx5 = new eHubClientRegistration
			{
				CX_PK = cxPk5,
				eHubClient = cc2,
				eHubRegistrationType = rt2,
				CX_Code = "REG005",
				CX_Qualifier = "QualifierValue5",
			};
			var cx6 = new eHubClientRegistration
			{
				CX_PK = cxPk6,
				eHubClient = cc3,
				eHubRegistrationType = rt2,
				CX_Code = "REG006",
				CX_Qualifier = "DummyQualifierValue6"
			};

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubRegistrationTypes.AddObject(rt2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);

			var result = controller.Registrations(rt1.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {CX_PK = cxPk1, CX_CC_ID = "DummyID", CX_Code = "REG001", CX_Qualifier = "QualifierValue1",
																	CX_Attr1 = "Attr1", CX_Password1 = "Password1", CX_Flag1 = (byte)11, CX_Flag2 = (byte)12, CX_ConfigXml = cxPk1},
															  new {CX_PK = cxPk2, CX_CC_ID = "DummyID2", CX_Code = "REG002", CX_Qualifier = "QualifierValue2"}},
				result, "eHubClientRegistrations");

			result = controller.Registrations(null);
			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {RegType = "Registration 1", CX_PK = cxPk1, CX_CC_ID = "DummyID", CX_Code = "REG001", CX_Qualifier = "QualifierValue1",
																	CX_Attr1 = "Attr1", CX_Password1 = "Password1", CX_Flag1 = (byte) 11, CX_Flag2 = (byte)12, CX_ConfigXml = "ConfigXml1"},
															  new {RegType = "Registration 2", CX_PK = cxPk4, CX_CC_ID = "DummyID", CX_Code = "REG004", CX_Qualifier = "QualifierValue4"},
															  new {RegType = "Registration 1", CX_PK = cxPk2, CX_CC_ID = "DummyID2", CX_Code = "REG002", CX_Qualifier = "QualifierValue2"},
															  new {RegType = "Registration 2", CX_PK = cxPk5, CX_CC_ID = "DummyID2", CX_Code = "REG005", CX_Qualifier = "QualifierValue5" },
			},
				result, "eHubClientRegistrations");

		}

		[TestMethod]
		public void TestRegistrations_MultipleFilter_OR()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CX_CC_ID\",\"op\":\"bw\",\"data\":\"DummyID2\"},{\"field\":\"CX_Qualifier\",\"op\":\"bw\",\"data\":\"DummyQualifierValue\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");
			var cxPk7 = new Guid("{00000000-FFFF-1111-7777-000000000000}");
			var cxPk8 = new Guid("{00000000-FFFF-1111-8888-000000000000}");
			var cxPk9 = new Guid("{00000000-FFFF-1111-9999-000000000000}");

			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID2" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID3" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			var rt2 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2", RT_RegistrantType = "Client" };
			var rt3 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-3333-000000000000}"), RT_ID = "REG003", RT_Description = "Registration 3", RT_RegistrantType = "System" };

			var cx1 = new eHubClientRegistration { CX_PK = cxPk1, eHubClient = cc1, eHubRegistrationType = rt1, CX_Code = "REG001", CX_Qualifier = "QualifierValue1" };
			var cx2 = new eHubClientRegistration { CX_PK = cxPk2, eHubClient = cc2, eHubRegistrationType = rt1, CX_Code = "REG002", CX_Qualifier = "QualifierValue2" };
			var cx3 = new eHubClientRegistration { CX_PK = cxPk3, eHubClient = cc3, eHubRegistrationType = rt1, CX_Code = "REG003", CX_Qualifier = "DummyQualifierValue3" };
			var cx4 = new eHubClientRegistration { CX_PK = cxPk4, eHubClient = cc1, eHubRegistrationType = rt2, CX_Code = "REG004", CX_Qualifier = "QualifierValue4" };
			var cx5 = new eHubClientRegistration { CX_PK = cxPk5, eHubClient = cc2, eHubRegistrationType = rt2, CX_Code = "REG005", CX_Qualifier = "QualifierValue5" };
			var cx6 = new eHubClientRegistration { CX_PK = cxPk6, eHubClient = cc3, eHubRegistrationType = rt2, CX_Code = "REG006", CX_Qualifier = "DummyQualifierValue6" };
			var cx7 = new eHubClientRegistration { CX_PK = cxPk7, eHubClient = cc1, eHubRegistrationType = rt3, CX_Code = "REG007", CX_Qualifier = "QualifierValue7" };
			var cx8 = new eHubClientRegistration { CX_PK = cxPk8, eHubClient = cc2, eHubRegistrationType = rt3, CX_Code = "REG008", CX_Qualifier = "QualifierValue8" };
			var cx9 = new eHubClientRegistration { CX_PK = cxPk9, eHubClient = cc3, eHubRegistrationType = rt3, CX_Code = "REG009", CX_Qualifier = "DummyQualifierValue9" };

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubRegistrationTypes.AddObject(rt2);
			context.eHubRegistrationTypes.AddObject(rt3);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);
			context.eHubClientRegistrations.AddObject(cx7);
			context.eHubClientRegistrations.AddObject(cx8);
			context.eHubClientRegistrations.AddObject(cx9);

			var result = controller.Registrations(rt1.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {CX_PK = cxPk2, CX_CC_ID = "DummyID2", CX_Code = "REG002", CX_Qualifier = "QualifierValue2"},
															  new {CX_PK = cxPk3, CX_CC_ID = "DummyID3", CX_Code = "REG003", CX_Qualifier = "DummyQualifierValue3"}},
				result, "eHubClientRegistrations");

			result = controller.Registrations(null);
			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {RegType = "Registration 1", CX_PK = cxPk2, CX_CC_ID = "DummyID2", CX_Code = "REG002", CX_Qualifier = "QualifierValue2"},
															  new {RegType = "Registration 2", CX_PK = cxPk5, CX_CC_ID = "DummyID2", CX_Code = "REG005", CX_Qualifier = "QualifierValue5"},
															  new {RegType = "Registration 1", CX_PK = cxPk3, CX_CC_ID = "DummyID3", CX_Code = "REG003", CX_Qualifier = "DummyQualifierValue3"},
															  new {RegType = "Registration 2", CX_PK = cxPk6, CX_CC_ID = "DummyID3", CX_Code = "REG006", CX_Qualifier = "DummyQualifierValue6"},
			},
				result, "eHubClientRegistrations");

		}

		[TestMethod]
		public void TestRegistrationsEdit()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "QUAL001";
			request.Container["CX_Code"] = "REG001";
			request.Container["CX_Password1"] = "TestPassword";
			request.Container["oper"] = "add";

			var responseAdd = RegistrationEditTest(rt.RT_PK, logger);
			var resultStateAdd = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);

			Assert.IsTrue(resultStateAdd, AssertHelper.AssertMessageBuilder("resultStateAdd", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CX_Qualifier=QUAL001, CX_Code=REG001, CX_Attr1=, CX_Password1=TestPassword, CX_Flag1=, CX_Flag2=, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String>(newId, cc.CC_PK, rt.RT_PK, "QUAL001", "REG001") },
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray());

			request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "";
			request.Container["CX_Code"] = "REG002";
			request.Container["CX_Password1"] = "TestPassword";
			request.Container["CustomValue1"] = "CustomValue1";
			request.Container["CustomValue2"] = "CustomValue2";
			request.Container["oper"] = "edit";

			var responseEdit = RegistrationEditTest(rt.RT_PK, logger);
			var resultStateEdit = (bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null);

			Assert.IsTrue(resultStateEdit, AssertHelper.AssertMessageBuilder("resultStateEdit", request.Container["oper"], responseEdit.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CX_Qualifier=, CX_Code=REG002, CX_Attr1=, CX_Password1=TestPassword, CX_Flag1=, CX_Flag2=, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String>(newId, cc.CC_PK, rt.RT_PK, null, "REG002") },
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray());

			request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["CX_Password1"] = "TestPassword";
			request.Container["oper"] = "del";

			var responseDel = RegistrationEditTest(rt.RT_PK, logger);
			var resultStateDel = (bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null);

			Assert.IsTrue(resultStateDel, AssertHelper.AssertMessageBuilder("resultStateDel", request.Container["oper"], responseDel.Data.ToString()));
			Assert.AreEqual(0, context.eHubClientRegistrations.Count());
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CX_Qualifier=, CX_Code=REG002, CX_Attr1=, CX_Password1=TestPassword, CX_Flag1=, CX_Flag2=, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
		}

		[TestMethod]
		public void TestRegistrationsEditGLSHK()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();

			var oldPassword = "TestPassword0";
			var newPassword = "TestPassword1";
			var oldPasswordByRSA = EhubClientEncryptor.Encrypt(oldPassword);
			var newPasswordByRSA = EhubClientEncryptor.Encrypt(newPassword);

			controller.Context = context;
			RemoteValidationAttribute.Context = context;

			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "GLSHK", RT_Description = "Registration 2" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001", CC_Password = oldPassword };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			var ftpUri = "ftp://test.com";

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "QUAL001";
			request.Container["CX_Code"] = "REG001";
			request.Container["CX_Password1"] = oldPassword;
			request.Container["CX_Flag1"] = "0";
			request.Container["CX_Flag2"] = "0";
			request.Container["CX_Attr1"] = ftpUri;
			request.Container["oper"] = "add";

			FormCollection formValues = new FormCollection() { { "CX_Attr1", ftpUri } };
			controller.ValueProvider = formValues.ToValueProvider();

			var responseAdd = RegistrationEditTest(rt.RT_PK, logger);
			var resultStateAdd = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);

			Assert.IsTrue(resultStateAdd, AssertHelper.AssertMessageBuilder("resultStateAdd", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains($"CX_Qualifier=QUAL001, CX_Code=REG001, CX_Attr1={ftpUri},"));
			Assert.IsTrue(logger.Log.Contains($"CX_Flag1=0, CX_Flag2=0, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String>(newId, cc.CC_PK, rt.RT_PK, "QUAL001", "REG001", "TestPassword0") },
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code, EhubServerDecryptor.Decrypt(cx.CX_Password1))).ToArray());

			request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "";
			request.Container["CX_Code"] = "REG002";
			request.Container["CX_Password1"] = "TestPassword1";
			request.Container["oper"] = "edit";

			var responseEdit = RegistrationEditTest(rt.RT_PK, logger);
			var resultStateEdit = (bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null);

			Assert.IsTrue(resultStateEdit, AssertHelper.AssertMessageBuilder("resultStateEdit", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains($"CX_Qualifier=, CX_Code=REG002, CX_Attr1={ftpUri},"));
			Assert.IsTrue(logger.Log.Contains($"CX_Flag1=, CX_Flag2=, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String>(newId, cc.CC_PK, rt.RT_PK, null, "REG002", "TestPassword1") },
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code, EhubServerDecryptor.Decrypt(cx.CX_Password1))).ToArray());

			request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "";
			request.Container["CX_Code"] = "REG003";
			request.Container["CX_Password1"] = "TestPassword1";
			request.Container["oper"] = "edit";
			responseEdit = RegistrationEditTest(rt.RT_PK, logger);
			resultStateEdit = (bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null);

			Assert.IsTrue(resultStateEdit, AssertHelper.AssertMessageBuilder("resultStateEdit", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains($"CX_Qualifier=, CX_Code=REG003, CX_Attr1={ftpUri},"));
			Assert.IsTrue(logger.Log.Contains($"CX_Flag1=, CX_Flag2=, CX_IssuedUTC=, CX_ExpiryUTC=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String>(newId, cc.CC_PK, rt.RT_PK, null, "REG003", "TestPassword1") },
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code, EhubServerDecryptor.Decrypt(cx.CX_Password1))).ToArray());

			RemoteValidationAttribute.Context = null;
		}

		[TestMethod]
		public void TestRegistrationsEditGLSHKFtpUriFormat()
		{
			var logger = new TestLogger();
			var testContext = new Fakes.TestContext();

			controller.Context = testContext;
			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "GLSHK1", RT_Description = "Registration 1" };
			var rt2 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "GLSHK2", RT_Description = "Registration 2" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001", CC_Password = "TestPassword" };
			testContext.eHubRegistrationTypes.AddObject(rt1);
			testContext.eHubRegistrationTypes.AddObject(rt2);
			testContext.eHubClients.AddObject(cc1);

			var ftpUri = "ftp://test.com";

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "QUAL001";
			request.Container["CX_Code"] = "REG001";
			request.Container["CX_Password1"] = "TestPassword";
			request.Container["CX_Flag1"] = "0";
			request.Container["CX_Flag2"] = "0";
			request.Container["CX_Attr1"] = ftpUri;
			request.Container["oper"] = "add";

			FormCollection formValues = new FormCollection() { { "CX_Attr1", ftpUri } };
			controller.ValueProvider = formValues.ToValueProvider();

			var responseAdd = RegistrationEditTest(rt1.RT_PK, logger);
			var resultStateAdd = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);

			Assert.IsTrue(resultStateAdd, AssertHelper.AssertMessageBuilder("resultStateAdd", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains($"CX_Attr1={ftpUri},"));

			var ftpexUri = "ftpex://test@test.glshk.com:2229/out";

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "QUAL001";
			request.Container["CX_Code"] = "REG001";
			request.Container["CX_Password1"] = "TestPassword";
			request.Container["CX_Flag1"] = "0";
			request.Container["CX_Flag2"] = "0";
			request.Container["CX_Attr1"] = ftpexUri;
			request.Container["oper"] = "add";

			var responseEdit = RegistrationEditTest(rt2.RT_PK, logger);
			var resultStateEdit = (bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null);

			Assert.IsTrue(resultStateEdit, AssertHelper.AssertMessageBuilder("resultStateAdd", request.Container["oper"], responseAdd.Data.ToString()));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains($"CX_Attr1={ftpexUri},"));
		}

		[TestMethod]
		public void TestValuesExportCsv()
		{
			request.Clear();

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");

			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID2" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID3" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			var rt2 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2", RT_RegistrantType = "Client" };

			request.Container["registrationTypePK"] = rt1.RT_PK.ToString();
			request.Container["gridColumns"] = "CX_CC_ID,CX_Qualifier,CX_Code,CX_Attr1,CX_Password1,CX_Flag1,CX_Flag2,CX_ConfigXml,CX_IssuedUTC,CX_ExpiryUTC";
			
			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG001",
				CX_Qualifier = "QualifierValue1",
				CX_Attr1 = "Attr1",
				CX_Password1 = "Password1",
				CX_Flag1 = 11,
				CX_Flag2 = 12,
				CX_ConfigXml = "ConfigXml1"
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			var cx4 = new eHubClientRegistration
			{
				CX_PK = cxPk4,
				eHubClient = cc1,
				eHubRegistrationType = rt2,
				CX_Code = "REG004",
				CX_Qualifier = "QualifierValue4",
			};
			var cx5 = new eHubClientRegistration
			{
				CX_PK = cxPk5,
				eHubClient = cc2,
				eHubRegistrationType = rt2,
				CX_Code = "REG005",
				CX_Qualifier = "QualifierValue5",
			};
			var cx6 = new eHubClientRegistration
			{
				CX_PK = cxPk6,
				eHubClient = cc3,
				eHubRegistrationType = rt2,
				CX_Code = "REG006",
				CX_Qualifier = "DummyQualifierValue6"
			};

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubRegistrationTypes.AddObject(rt2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);

			var resultWithoutFilter = controller.RegistrationsExportCsv();
			Assert.AreEqual(
@"Client,Qualifier,Code,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC
DummyID,QualifierValue1,REG001,Attr1,Password1,11,12,00000000-ffff-1111-1111-000000000000,,
DummyID2,QualifierValue2,REG002,,,,,00000000-ffff-1111-2222-000000000000,,
DummyID3,DummyQualifierValue3,REG003,,,,,00000000-ffff-1111-3333-000000000000,,
",
								Encoding.Default.GetString(resultWithoutFilter.FileContents));
			Assert.AreEqual("REG001_Registration1.csv", resultWithoutFilter.FileDownloadName);

			request.Container["filterForExportCsv"] = "(CX_CC_ID LIKE \"DummyID%\" AND CX_Qualifier LIKE \"QualifierValue%\")";
			request.Container["sortDataExportCsv"] = "{\"sidx\":\"CX_Code\",\"sord\":\"asc\"}";

			var resultWithFilter = controller.RegistrationsExportCsv();
			Assert.AreEqual(
@"Client,Qualifier,Code,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC
DummyID,QualifierValue1,REG001,Attr1,Password1,11,12,00000000-ffff-1111-1111-000000000000,,
DummyID2,QualifierValue2,REG002,,,,,00000000-ffff-1111-2222-000000000000,,
",
				Encoding.Default.GetString(resultWithFilter.FileContents)
			);
			Assert.AreEqual("REG001_Registration1.csv", resultWithFilter.FileDownloadName);
		}

		[TestMethod]
		public void TestExportCsvDecodedPassword()
		{
			request.Clear();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID2" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID3" };
			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{85d0b5f6-2221-4f34-8dd8-a5a9d3aa8a3c}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			request.Container["registrationTypePK"] = rt1.RT_PK.ToString();
			request.Container["gridColumns"] = "CX_CC_ID,CX_Qualifier,CX_Code";
			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG001",
				CX_Qualifier = "QualifierValue1",
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			var result = controller.RegistrationsExportCsv();
			Assert.AreEqual(
		@"Client,Qualifier,Code
DummyID,QualifierValue1,REG001
DummyID2,QualifierValue2,REG002
DummyID3,DummyQualifierValue3,REG003
",
			Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual("REG001_Registration1.csv", result.FileDownloadName);
		}


		[TestMethod]
		public void TestValuesImportCsvDefault()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = "XXX111", CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = "XXX222", CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\n" +
				"CLIENT001,REG001,,ftp://username1@server:port,XXX111,0,1,,,\r\n" +
				"CLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			controller.RegistrationsImportCsv();

			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String>(cc1.CC_PK, rt.RT_PK, null, "REG001"),
					new Tuple<Guid, Guid, String, String>(cc2.CC_PK, rt.RT_PK, null, "REG002"),
					new Tuple<Guid, Guid, String, String>(cc2.CC_PK, rt.RT_PK, "QUAL001", "REG003")
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray());
		}
		
		[TestMethod]
		public void TestValuesExportCsvWithoutRegistrationType()
		{
			request.Clear();

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");


			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID2" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID3" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			var rt2 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2", RT_RegistrantType = "Client" };

			request.Container["registrationTypePK"] = "";
			request.Container["sortDataExportCsv"] = "{\"sidx\":\"CX_Code\",\"sord\":\"asc\"}";

			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG001",
				CX_Qualifier = "QualifierValue1",
				CX_Attr1 = "Attr1",
				CX_Password1 = "Password1",
				CX_Flag1 = 11,
				CX_Flag2 = 12,
				CX_ConfigXml = "ConfigXml1"
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			var cx4 = new eHubClientRegistration
			{
				CX_PK = cxPk4,
				eHubClient = cc1,
				eHubRegistrationType = rt2,
				CX_Code = "REG004",
				CX_Qualifier = "QualifierValue4",
			};
			var cx5 = new eHubClientRegistration
			{
				CX_PK = cxPk5,
				eHubClient = cc2,
				eHubRegistrationType = rt2,
				CX_Code = "REG005",
				CX_Qualifier = "QualifierValue5",
			};
			var cx6 = new eHubClientRegistration
			{
				CX_PK = cxPk6,
				eHubClient = cc3,
				eHubRegistrationType = rt2,
				CX_Code = "REG006",
				CX_Qualifier = "DummyQualifierValue6"
			};

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubRegistrationTypes.AddObject(rt2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);

			var resultWithoutFilter = controller.RegistrationsExportCsvForFilteredDataWithoutRegistrationType();
			Assert.AreEqual(
				"Registration Type,Client,Qualifier,Code,Attribute 1,Password 1,Flag 1,Flag 2\r\n" +
				"Registration 1,DummyID,QualifierValue1,REG001,Attr1,Password1,11,12" + "\r\n" +
				"Registration 1,DummyID2,QualifierValue2,REG002,,,," + "\r\n" +
				"Registration 1,DummyID3,DummyQualifierValue3,REG003,,,," + "\r\n" +
				"Registration 2,DummyID,QualifierValue4,REG004,,,," + "\r\n" +
				"Registration 2,DummyID2,QualifierValue5,REG005,,,," + "\r\n" +
				"Registration 2,DummyID3,DummyQualifierValue6,REG006,,,," + "\r\n",
				Encoding.Default.GetString(resultWithoutFilter.FileContents)
			);

			request.Container["filterForExportCsv"] = "(CX_CC_ID LIKE \"DummyID%\" AND CX_Qualifier LIKE \"QualifierValue%\")";

			var resultWithFilter = controller.RegistrationsExportCsvForFilteredDataWithoutRegistrationType();
			Assert.AreEqual(
				"Registration Type,Client,Qualifier,Code,Attribute 1,Password 1,Flag 1,Flag 2\r\n" +
				"Registration 1,DummyID,QualifierValue1,REG001,Attr1,Password1,11,12" + "\r\n" +
				"Registration 1,DummyID2,QualifierValue2,REG002,,,," + "\r\n" +
				"Registration 2,DummyID,QualifierValue4,REG004,,,," + "\r\n" +
				"Registration 2,DummyID2,QualifierValue5,REG005,,,," + "\r\n",
				Encoding.Default.GetString(resultWithFilter.FileContents)
			);
		}

		[TestMethod]
		public void TestImportCsvInvalidHeadersDisplayError()
		{
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = "XXX111", CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = "XXX222", CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "InvalidHeader1,InvalidHeader2");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);
			Assert.IsFalse(success);
			Assert.AreEqual(message, "Error processing CSV: CSV headers do not match the expected format.");
		}

		[TestMethod]
		public void TestImportCsvEmptyPasswordMerge()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-BBBB-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var existing = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = cc, CX_Password1 = "" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientRegistrations.AddObject(existing);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,,,0,1,,,");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			Assert.IsNotNull(result);

			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);
			Assert.IsTrue(success);
			Assert.AreEqual(message, "CSV import completed.");
			Assert.AreEqual(existing.CX_Password1, null);
		}

		[TestMethod]
		public void TestImportCsvFieldsUpdatedMerge()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-BBBB-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var existing = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = cc, CX_Password1 = "" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientRegistrations.AddObject(existing);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,ftp://username1@server:port,XXX111,0,1,,,\r\nCLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			Assert.IsNotNull(result);

			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);
			Assert.IsTrue(success);
			Assert.AreEqual(message, "CSV import completed.");
			Assert.AreEqual(existing.CX_Attr1, "ftp://username1@server:port");
			Assert.AreEqual(existing.CX_Password1, "XXX111");
		}

		[TestMethod]
		public void TestImportCsvFieldsWithDifferentColumnOrderUpdatedMerge()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-BBBB-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var existing = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = cc, CX_Password1 = "" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientRegistrations.AddObject(existing);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Qualifier,Code,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,,REG001,ftp://username1@server:port,XXX111,0,1,,,\r\nCLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			Assert.IsNotNull(result);

			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);
			Assert.IsTrue(success);
			Assert.AreEqual(message, "CSV import completed.");
			Assert.AreEqual(existing.CX_Attr1, "ftp://username1@server:port");
			Assert.AreEqual(existing.CX_Password1, "XXX111");
		}

		[TestMethod]
		public void TestImportCsvInvalidFieldReturnsError()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-BBBB-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var existing = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = cc, CX_Password1 = "" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientRegistrations.AddObject(existing);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,,,XYZ,123,,,\r\nCLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			Assert.IsNotNull(result);

			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

			Assert.IsFalse(success);
			Assert.AreEqual("Error processing CSV: Input string was not in a correct format.", message);
		}

		[TestMethod]
		public void TestImportCsvReplace_RemovesOldUnmatchedRegistrations()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = "XXX111", CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = "XXX222", CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,,,0,1,,,\r\n");


			var result = controller.RegistrationsImportCsv() as JsonResult;
			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

			Assert.IsTrue(success);
			Assert.AreEqual("CSV import completed.", message);

			Assert.AreEqual(1, context.eHubClientRegistrations.Count());
			Assert.AreEqual("CLIENT001", context.eHubClientRegistrations.First().eHubClient.CC_ID);
		}

		[TestMethod]
		public void TestImportCsvReplaceUpdatesExistingAndRemovesOthers()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = Guid.NewGuid(), RT_ID = "REGU" };
			context.eHubRegistrationTypes.AddObject(rt);

			var client1 = new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "CLIENT001" };
			var client2 = new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "CLIENT002" };
			context.eHubClients.AddObject(client1);
			context.eHubClients.AddObject(client2);

			var registration1 = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = rt.RT_PK,
				eHubClient = client1,
				CX_Qualifier = "Q1",
				CX_Code = "OLD"
			};
			var registration2 = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = rt.RT_PK,
				eHubClient = client2,
				CX_Qualifier = "Q2",
				CX_Code = "OLD"
			};
			context.eHubClientRegistrations.AddObject(registration1);
			context.eHubClientRegistrations.AddObject(registration2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,NEWCODE,,,,0,1,,,\r\n");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

			Assert.IsTrue(success);
			Assert.AreEqual("CSV import completed.", message);

			Assert.AreEqual(1, context.eHubClientRegistrations.Count());
			var updated = context.eHubClientRegistrations.First();
			Assert.AreEqual("CLIENT001", updated.eHubClient.CC_ID);
			Assert.AreEqual("NEWCODE", updated.CX_Code);
		}

		[TestMethod]
		public void TestImportCsvReplaceEmptyFileRemovesAllRegistrations()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-AAAA-1111-1111-000000000000}"), RT_ID = "REG001" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = "XXX111", CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = "XXX222", CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\n");


			var result = controller.RegistrationsImportCsv() as JsonResult;
			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

			Assert.IsTrue(success);
			Assert.AreEqual("CSV import completed.", message);

			Assert.AreEqual(0, context.eHubClientRegistrations.Count());
		}

		[TestMethod]
		public void TestImportCsvReplaceMultipleRecords()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = Guid.NewGuid(), RT_ID = "REGM" };
			context.eHubRegistrationTypes.AddObject(rt);

			var client1 = new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "CLIENTA" };
			var client2 = new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "CLIENTB" };
			var client3 = new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "CLIENTC" };
			context.eHubClients.AddObject(client1);
			context.eHubClients.AddObject(client2);
			context.eHubClients.AddObject(client3);

			var r1 = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = client1, CX_Qualifier = "Q1", CX_Code = "CODE1" };
			var r2 = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = client2, CX_Qualifier = "Q2", CX_Code = "CODE2" };
			var r3 = new eHubClientRegistration { CX_PK = Guid.NewGuid(), CX_RT = rt.RT_PK, eHubClient = client3, CX_Qualifier = "Q3", CX_Code = "CODE3" };
			context.eHubClientRegistrations.AddObject(r1);
			context.eHubClientRegistrations.AddObject(r2);
			context.eHubClientRegistrations.AddObject(r3);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENTA,CODEX,,,,0,1,,,\r\nCLIENTC,CODE3,,,,0,1,,,\r\n");

			var result = controller.RegistrationsImportCsv() as JsonResult;
			var success = (bool)result.Data.GetType().GetProperty("success").GetValue(result.Data, null);
			var message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

			Assert.IsTrue(success);
			Assert.AreEqual("CSV import completed.", message);

			Assert.AreEqual(2, context.eHubClientRegistrations.Count());
			Assert.IsTrue(context.eHubClientRegistrations.Any(x => x.CX_Code == "CODEX" && x.eHubClient.CC_ID == "CLIENTA"));
			Assert.IsTrue(context.eHubClientRegistrations.Any(x => x.CX_Code == "CODE3" && x.eHubClient.CC_ID == "CLIENTC"));
		}

		[TestMethod]
		public void TestValuesImportCsvMerge()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = "XXX111", CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = "XXX222", CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,ftp://username1@server:port,XXX111,0,1,,,\r\nCLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			controller.RegistrationsImportCsv();

			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String>(cc1.CC_PK, rt.RT_PK, null, "REG001"),
					new Tuple<Guid, Guid, String, String>(cc2.CC_PK, rt.RT_PK, null, "REG002"),
					new Tuple<Guid, Guid, String, String>(cc2.CC_PK, rt.RT_PK, "QUAL001", "REG003")
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray()
			);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,ftp://username1@server:port,XXX222,1,0,,,\r\nCLIENT002,REG003,QUAL001,ftp://username2@server:port,XXX222,1,0,,,\r\n");

			controller.RegistrationsImportCsv();

			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String>(cc1.CC_PK, rt.RT_PK, null, "REG001"),
					new Tuple<Guid, Guid, String, String>(cc2.CC_PK, rt.RT_PK, "QUAL001", "REG003")
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray()
			);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Client,Code,Qualifier,Attribute 1,Password 1,Flag 1,Flag 2,ConfigXml,IssuedUTC,ExpiryUTC\r\nCLIENT001,REG001,,ftp://username1@server:port,XXX111,1,0,,,\r\nCLIENT002,REG003,QUAL001,ftp://username3@server:port,XXX333,0,1,,,\r\n");

			controller.RegistrationsImportCsv();

			CollectionAssert.AreEqual(new[]
				{
					new Tuple<String, String, String, String, byte?, byte?>(null, "REG001", "ftp://username1@server:port", "XXX111", 1, 0),
					new Tuple<String, String, String, String, byte?, byte?>("QUAL001", "REG003", "ftp://username3@server:port", "XXX333", 0, 1)
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<String, String, String, String, byte?, byte?>(cx.CX_Qualifier, cx.CX_Code, cx.CX_Attr1, cx.CX_Password1, cx.CX_Flag1, cx.CX_Flag2)).ToArray()
			);
		}

		[TestMethod]
		public void TestValuesImportCsvMergeForOCMSemantics()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "CAROTRANS", RT_Description = "One of the OCM Client Types" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cx = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CX_Qualifier = "Qual1", eHubClient = cc, CX_RT = rt.RT_PK, CX_Code = "REG001"};

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientRegistrations.AddObject(cx);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "eHubID,ID,EventBranch,DecodedPassword\r\n" +
				"CLIENT001,REG001,Qual1,\r\n");

			controller.RegistrationsImportCsv();
			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String>(cc.CC_PK, rt.RT_PK, "Qual1", "REG001")
				},
				context.eHubClientRegistrations.Select(cxExpect => new Tuple<Guid, Guid, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray());
		}

		[TestMethod]
		public void TestValuesImportCsvMergeForOCMSemanticsWithCarriers()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "CARGOSMART", RT_Description = "One of the OCM Client Types with Carriers" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "CLIENT002" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CX_Qualifier = "Qual1", eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001" };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-3333-2222-000000000000}"), CX_Qualifier = "Qual2", eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002" };

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "eHubID,ID,EventBranch,Carriers,DecodedPassword\r\n" +
				"CLIENT001,REG001,Qual1,,\r\n" +
				"CLIENT002,REG002,Qual2,Carrier1,\r\n");

			controller.RegistrationsImportCsv();
			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String, String>(cc1.CC_PK, rt.RT_PK, "Qual1", "REG001", null),
					new Tuple<Guid, Guid, String, String, String>(cc2.CC_PK, rt.RT_PK, "Qual2", "REG002", "Carrier1")
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, String, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code, cx.CX_Attr1)).ToArray());
		}

		[TestMethod]
		public void TestValuesImportCsvMergeForGLSHK()
		{
			var password1 = EhubClientEncryptor.Encrypt("XXX111");
			var password2 = EhubClientEncryptor.Encrypt("XXX222");
			var changePasswor1 = EhubClientEncryptor.Encrypt("XXX111aaa");
			var addPassword3 = EhubClientEncryptor.Encrypt("XXX333");

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "GLSHK", RT_Description = "Registration 2" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "CLIENT003" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CX_Qualifier = "Qual1", eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001", CX_Attr1 = "ftp://username1@server:port", CX_Password1 = password1, CX_Flag1 = 0, CX_Flag2 = 1 };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CX_Qualifier = "Qual2", eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002", CX_Attr1 = "ftp://username2@server:port", CX_Password1 = password2, CX_Flag1 = 1, CX_Flag2 = 0 };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClients.AddObject(cc3);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);

			request.Clear();
			request.Container["registrationTypePK"] = rt.RT_PK.ToString();
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "eHubID,PIMA,Branch,FTP URI,FTP Password,Last Updated From,Status\r\n" +
				"CLIENT001,REG001,Qual1,ftp://username1@server:port,XXX111aaa,0,1\r\n" +
				"CLIENT002,REG002,Qual2,ftp://username2@server:port,XXX222,1,0\r\n" +
				"CLIENT003,REG003,Qual3,ftp://username3@server:port,XXX333,0,1\r\n");

			controller.RegistrationsImportCsv();
			CollectionAssert.AreEqual(new[]
				{
					new Tuple<Guid, Guid, String, String, String>(cc1.CC_PK, rt.RT_PK, "Qual1", "REG001", "XXX111aaa"),
					new Tuple<Guid, Guid, String, String, String>(cc2.CC_PK, rt.RT_PK, "Qual2", "REG002", "XXX222"),
					new Tuple<Guid, Guid, String, String, String>(cc3.CC_PK, rt.RT_PK, "Qual3", "REG003", "XXX333")
				},
				context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, String, String, String>(cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code, EhubServerDecryptor.Decrypt(cx.CX_Password1))).ToArray());
		}

		[TestMethod]
		public void TestClientsGetIncludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
			};

			var result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected2 = new List<object>
			{
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
			};

			var result2 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected3 = new List<object>
			{
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result3 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");
		}

		[TestMethod]
		public void TestClientsExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		[TestMethod]
		public void TestClientsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "test";

			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}


		[TestMethod]
		public void TesteHubPortalSemanticsFactory()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var expectedDict = new Dictionary<string, string>()
			{
				{"CX_CC_ID", "Provider"},
				{"CX_Qualifier", "Type"},
				{"CX_Code", "Authorisation"}
			};
			var resultDict = new Dictionary<string, string>();
			foreach (var key in expectedDict.Keys)
			{
				resultDict[key] = controller.GetCustomisedHeaderForID("GBCustoms-Direct", key);
			}

			CollectionAssert.AreEquivalent(resultDict.ToList(), expectedDict.ToList());
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_GBCustomsDirect()
		{
			var expectedColumnNames = new Dictionary<string, string>()
			{
				{"CX_CC_ID", "Provider"},
				{"CX_Qualifier", "Type"},
				{"CX_Code", "Authorisation"}
			};

			TesteHubPortalSemanticsFactory("GBCustoms-Direct", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_GBCustomsMCP()
		{
			var expectedColumnNames = new Dictionary<string, string>
			{
				{"CX_CC_ID", "Provider"},
				{"CX_Code", "Authorisation"},
				{"CX_Attr1", "URL"}
			};

			TesteHubPortalSemanticsFactory("GBCustoms-MCP", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_BENxtPort()
		{
			var expectedColumnNames = new Dictionary<string, string>
			{
				{"CX_CC_ID", "Client"},
				{"CX_Qualifier", "Branch Code"},
				{"CX_Flag1", "Account Status"},
				{"CX_Password1", "Password"},
				{"CX_Attr1", "Username"},
				{"CX_Code", "Code"}
			};

			TesteHubPortalSemanticsFactory("NXPORTAPI", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_ACAS_BR()
		{
			var expectedColumnNames = new Dictionary<string, string>
			{
				{"CX_CC_ID", "eHubID"},
				{"CX_Code", "ID"},
				{"CX_Qualifier", "EventBranch"}
			};

			TesteHubPortalSemanticsFactory("ACAS_BR", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_GEI_IN_AuthenticationClientLevel()
		{
			var expectedColumnNames = new Dictionary<string, string>
			{
				{"CX_CC_ID", "ClientID"},
				{"CX_Qualifier", "Branch"},
				{"CX_Flag1", "Status"},
				{"CX_ConfigXml", "ConfigXml"}
			};

			TesteHubPortalSemanticsFactory("GEI_IN_AuthenticationClientLevel", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_TW()
		{
			var expectedColumnNames = new Dictionary<string, string>
			   {
				{"CX_CC_ID", "Company"},
				{"CX_Qualifier", "Staff|MailBox"},
				{"CX_Flag1", "ReceiveAutomatically"},
				{"CustomValue1", "ClientPassword"},
				{"CustomValue2", "CertificatePassword"},
				{"CX_ConfigXml", "ConfigXml"}
			};

			TesteHubPortalSemanticsFactory("TWCustomsAccount", expectedColumnNames);
		}

		[TestMethod]
		public void TestGetCustomisedHeaderForIDWithDefault()
		{
			var context = new Fakes.TestContext();
			var colName = "RegType";
			controller.Context = context;
			var result = controller.GetCustomisedHeaderForID("Default", colName);
			Assert.AreEqual("Registration Type", result);

			result = controller.GetCustomisedHeaderForID("GBCustoms-Direct", colName);
			Assert.AreEqual("", result);
		}

		private void TesteHubPortalSemanticsFactory(string registrationTypeId, Dictionary<string, string> expectedColumnNames)
		{
			var context = new Fakes.TestContext();
			controller.Context = context;

			var resultDict = new Dictionary<string, string>();
			foreach (var key in expectedColumnNames.Keys)
			{
				resultDict[key] = controller.GetCustomisedHeaderForID(registrationTypeId, key);
			}

			CollectionAssert.AreEquivalent(resultDict.ToList(), expectedColumnNames.ToList());
		}

		protected void RegistrationTypeInfoEditTest(ILog logger)
		{
			controller.logger = logger;
			controller.RegistrationTypeInfoEdit();
		}

		protected JsonResult RegistrationEditTest(Guid regType, ILog logger)
		{
			controller.logger = logger;
			return controller.RegistrationEdit(regType);
		}

	}
}
