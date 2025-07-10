#if NETFRAMEWORK
using System.Web.Http.Results;
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using Microsoft.AspNetCore.Mvc;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.Charges;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Services.ServiceHost.WebAPI.Controllers.Charges.JobHeaderAndChargesController;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.Accounting
{
	class JobHeaderAndChargesControllerTest : TestCaseWithFactory
	{
		const string MaxErrorError = "Error";
		const string MaxErrorWarning = "Warning";
		const string MaxErrorNone = "None";

		#region Tests

		public void TestUpdateCharges_NullJobHeader()
		{
			var controller = GetChargesControllerForTest();

			var updateChargesResult = controller.UpdateJobHeaderAndCharges(null).GetResult();
			Assert(updateChargesResult.GetStatusCode() == (int)HttpStatusCode.BadRequest);
		}

		public void TestUpdateCharges_NullJobCharges()
		{
			var controller = GetChargesControllerForTest();

			var jobHeader = GetDummyJobHeaderForTest();
			jobHeader.JobCharges = null;

			var updateChargesResult = controller.UpdateJobHeaderAndCharges(jobHeader).GetResult();
			Assert(updateChargesResult.GetStatusCode() == (int)HttpStatusCode.BadRequest);
		}

		public void TestUpdateCharges_NoJobCharges()
		{
			var controller = GetChargesControllerForTest();

			var jobHeader = GetDummyJobHeaderForTest();
			jobHeader.JobCharges = new List<JobChargeModel>();

			var updateChargesResult = controller.UpdateJobHeaderAndCharges(jobHeader).GetResult();
			Assert(updateChargesResult.GetStatusCode() == (int)HttpStatusCode.BadRequest);
		}

		public void TestUpdateCharges_WithUseableData()
		{
			var mockedAdapter = new Mock<IJobCostingAdapter>();

			using (ObjectFactory.Substitute(mockedAdapter.Object))
			{
				var controller = GetChargesControllerForTest();
				var jobHeader = GetDummyJobHeaderWithJobChargesForTest();

				mockedAdapter.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()));
				var response = controller.UpdateJobHeaderAndCharges(jobHeader);

				AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorNone, "");

				mockedAdapter.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()))
					.Callback((BusinessObjectFactory factory, IXmlImportLogger logger, IJobCostingData shipment, ZGuid jobHeaderParentPK, ZString jobHeaderParentTablePrefix) =>
					{
						logger.Log(Integration.LogType.Warning, "Warning - This is a warning \"Something might go wrong\"");
					});

				response = controller.UpdateJobHeaderAndCharges(jobHeader);

				AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorWarning, "Warning - This is a warning \"Something might go wrong\"\r\n");

				mockedAdapter.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()))
					.Callback((BusinessObjectFactory factory, IXmlImportLogger logger, IJobCostingData shipment, ZGuid jobHeaderParentPK, ZString jobHeaderParentTablePrefix) =>
					{
						logger.Log(Integration.LogType.Error, "Error - This is an error \"Something went wrong\"");
						logger.Log(Integration.LogType.Warning, "Warning - This is a warning \"Something might go wrong\"");
						throw new DataObjectReadFailureException();
					});

				response = controller.UpdateJobHeaderAndCharges(jobHeader);

				AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorError, "Error - This is an error \"Something went wrong\"\r\nWarning - This is a warning \"Something might go wrong\"\r\n");
			}
		}

		public void TestUpdateCharges_WithUsableData_CallsImportWithPKCriteria()
		{
			var mockedAdapter = new Mock<IJobCostingAdapter>();

			using (ObjectFactory.Substitute(mockedAdapter.Object))
			{
				var controller = GetChargesControllerForTest();
				var jobHeader = GetDummyJobHeaderWithJobChargesForTest();

				IJobCostingData shipment = null;
				mockedAdapter
					.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()))
					.Callback((BusinessObjectFactory f, IXmlImportLogger l, IJobCostingData s, ZGuid x, ZString y) => { shipment = s; });
				var response = controller.UpdateJobHeaderAndCharges(jobHeader);
				mockedAdapter.Verify(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()), Times.AtLeastOnce);
				var shipmentChargeLineArray = shipment.JobCosting.ChargeLineCollection.ToArray();
				var chargeLineArray = jobHeader.JobCharges.ToArray();
				for (var i = 0; i < shipmentChargeLineArray.Length; i++)
				{
					var chargeFromShipment = shipmentChargeLineArray[i];
					var chargeFromInput = chargeLineArray[i];
					if (chargeFromInput.EntityState == "Modified" || chargeFromInput.EntityState == "Deleted")
					{
						var action = chargeFromInput.EntityState == "Modified" ? "updating" : "deleting";
						AssertGreaterThan("When " + action + ", Charge Line should have at least one matching criteria", chargeFromShipment.ImportMetaData.MatchingCriteriaCollection.Count, 0);
						AssertEquals("When " + action + ", Charge Line should have matching criteria should be on PK", "PrimaryKey", chargeFromShipment.ImportMetaData.MatchingCriteriaCollection[0].FieldName);
						AssertEquals("When " + action + ", Charge Line first matching criteria value match charge PK", chargeFromInput.JR_PK.ToString(), chargeFromShipment.ImportMetaData.MatchingCriteriaCollection[0].Value);
					}
					else
					{
						AssertEquals("When not updating or deleting, Charge Line should not have matching criteria", chargeFromShipment.ImportMetaData.MatchingCriteriaCollection.Count, 0);
					}
				}
			}
		}

		public void TestDeserialization()
		{
			var jsonString = "{\"JH_PK\":\"f3d981b0-1cb8-47a5-a6ee-a1fec71cc347\",\"JH_A_JOP\":\"/Date(2019-01-10T21:58:00.000Z)/\",\"JH_AgentChargesCFX\":0,\"JH_ARInvoiceReference\":\"\",\"JH_Description\":\"\",\"JH_ExcludeFromPeriodicRating\":false,\"JH_GB\":\"91ccb950-4ab4-4ca5-a448-0351a715943c\",\"JH_GC\":\"f4e758fe-55a3-4bb3-94a8-cb992d3bbe7f\",\"JH_GE\":\"db20a0c6-1fe5-42c6-8226-0982043f9e06\",\"JH_GS_NKRepOps\":\"KRB\",\"JH_GS_NKRepSales\":\"\",\"JH_HeaderType\":\"JOB\",\"JH_HoldReason\":\"\",\"JH_IsProfitSharePosted\":false,\"JH_IsValid\":false,\"JH_JobBufferPercentOverride\":0,\"JH_JobLocalReference\":\"00005405\",\"JH_JobNum\":\"CN0036862019GLD\",\"JH_LocalChargesCFX\":0,\"JH_LocalClientInvoicingStyle\":\"\",\"JH_Name\":\"\",\"JH_OA_LocalChargesAddr\":\"78b00b83-79ab-4caf-8b19-2ff7552b743f\",\"JH_ParentID\":\"c5daee52-567a-4277-8ed4-d8ad44d1ac45\",\"JH_PaymentCollectionStatus\":\"\",\"JH_ProfitLossReasonCode\":\"\",\"JH_RatingHasBeenRun\":true,\"JH_SingleAgentsInvoicePerConsol\":true,\"JH_Status\":\"WRK\",\"JH_SystemCreateTimeUtc\":\"/Date(2019-01-10T21:59:00.000Z)/\",\"JH_SystemCreateUser\":\"KRB\",\"JH_SystemLastEditTimeUtc\":\"/Date(2019-01-10T21:59:00.000Z)/\",\"JH_SystemLastEditUser\":\"KRB\",\"JH_TH_NKQuoteNumber\":\"\",\"JH_UniqueJobInvoiceNumber\":0,\"entityAspect\":{\"entityState\":\"Unchanged\"},\"JobCharges\":[{\"JR_PK\":\"263eb9ac-1e47-4af0-8acb-4ef9e79b4a3a\",\"JR_AC\":\"b89c1c01-8ca4-4d77-bd2a-7344b2ef8fcc\",\"JR_CostRatingBehaviour\":\"STP\",\"JR_SellRatingBehaviour\":\"REA\",\"JR_AgentDeclaredCostAmt\":0,\"JR_AgentDeclaredSellAmt\":361.15,\"JR_AL_ARLine\":\"4a22a897-6e7b-44ea-84a4-59c883cc0006\",\"JR_APInvoiceDate\":\"/Date(2019-10-17T04:38:00.000Z)/\",\"JR_APInvoiceNum\":\"\",\"JR_APLinePostingStatus\":\"\",\"JR_APNumberOfSupportingDocuments\":0,\"JR_ARLinePostingStatus\":\"\",\"JR_ARNumberOfSupportingDocuments\":0,\"JR_AT_SellGSTRate\":\"ef810ab9-7a97-44ff-b277-826a2fdddb86\",\"JR_ChargeType\":\"\",\"JR_ChequeNo\":\"\",\"JR_CostGovtChargeCode\":\"\",\"JR_CostRated\":false,\"JR_CostRatingOverride\":false,\"JR_CostRatingOverrideComment\":\"\",\"JR_CostReference\":\"CN0036862019GLD\",\"JR_DeclaredOSCostAmt\":0,\"JR_Desc\":\"LCL Per M3 Delivery { CN0036862019GLD }\",\"JR_DisplaySequence\":2,\"JR_EstimatedCost\":0,\"JR_EstimatedRevenue\":361.15,\"JR_GB\":\"91ccb950-4ab4-4ca5-a448-0351a715943c\",\"JR_GC\":\"f4e758fe-55a3-4bb3-94a8-cb992d3bbe7f\",\"JR_GE\":\"db20a0c6-1fe5-42c6-8226-0982043f9e06\",\"JR_InvoiceType\":\"FIN\",\"JR_IsCostTaxAmountOverridden\":false,\"JR_IsIncludedInProfitShare\":false,\"JR_IsValid\":false,\"JR_JH\":\"f3d981b0-1cb8-47a5-a6ee-a1fec71cc347\",\"JR_LineCFX\":0,\"JR_LineType\":\"BTH\",\"JR_LocalCostAmt\":0,\"JR_LocalSellAmt\":361.15,\"JR_MarginPercentage\":0,\"JR_OH_SellAccount\":\"11e177b3-ed1d-42e0-b548-f924ed774c77\",\"JR_OrderReference\":\"CN0036862019GLD\",\"JR_OSCostAmt\":0,\"JR_OSCostExRate\":1,\"JR_OSCostGSTAmt\":0,\"JR_OSCostWHTAmt\":0,\"JR_OSSellAmt\":361.15,\"JR_OSSellExRate\":1,\"JR_OSSellWHTAmt\":0,\"JR_PaymentType\":\"   \",\"JR_PreventInvoicePrintGrouping\":false,\"JR_ProductQuantity\":0,\"JR_ProFormaCost\":false,\"JR_ProFormaRevenue\":false,\"JR_RX_NKCostCurrency\":\"\",\"JR_RX_NKSellCurrency\":\"AUD\",\"JR_RX_NKSellInvoiceCurrency\":\"\",\"JR_SellGovtChargeCode\":\"\",\"JR_SellRated\":true,\"JR_SellRatingOverride\":false,\"JR_SellRatingOverrideComment\":\"\",\"JR_SellReference\":\"\",\"entityAspect\":{\"EntityState\":\"Added\"}}]}";
			var jobHeader = ReadJsonToObject<JobHeaderModel>(jsonString);
			AssertEquals(false, jobHeader == null);
			AssertEquals(1, jobHeader.JobCharges.Count);
			var charge = jobHeader.JobCharges.ToList()[0];
			AssertEquals("Deserialisation should properly deserialise the charges PK", Guid.Parse("263eb9ac-1e47-4af0-8acb-4ef9e79b4a3a"), charge.JR_PK);
			AssertEquals("LCL Per M3 Delivery { CN0036862019GLD }", charge.JR_Desc);
		}

		public void TestReturnsErrorIfJobHeaderPKIsEmpty()
		{
			var controller = GetChargesControllerForTest();

			var jobHeaderModel = GetDummyJobHeaderForTest();
			jobHeaderModel.JH_PK = Guid.Empty;

			var updateChargesResult = controller.UpdateJobHeaderAndCharges(jobHeaderModel).GetResult();
			Assert(updateChargesResult.GetStatusCode() == (int)HttpStatusCode.BadRequest);
		}

		public void TestReturnsErrorIfJobHeaderIsUnmodifiedAndHasNoModifiedCharges()
		{
			var controller = GetChargesControllerForTest();

			var jobHeader = GetDummyJobHeaderForTest();
			jobHeader.EntityState = "Unchanged";

			var updateChargesResult = controller.UpdateJobHeaderAndCharges(jobHeader).GetResult();
			Assert(updateChargesResult.GetStatusCode() == (int)HttpStatusCode.BadRequest);
		}

		public void TestReturnsErrorIfSaveFailsAfterMakingChangesToJobHeader()
		{
			var controller = GetChargesControllerForTest();

			var jobHeader = GetJobBOForTest();
			jobHeader.JH_Description = "description";
			Factory.Save();

			var jobHeaderModel = GetDummyJobHeaderForTest();
			jobHeaderModel.JH_PK = jobHeader.PK.ToGuid();
			jobHeaderModel.EntityState = "Modified";
			jobHeaderModel.JH_ParentID = jobHeader.JH_ParentID.ToGuid();
			jobHeaderModel.JH_Status = "ZZZ";

			var response = controller.UpdateJobHeaderAndCharges(jobHeaderModel);
			AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorError, "Error Saving Record", containsResponseMessage: true);
			AssertEquals("description", jobHeader.JH_Description);
		}

		public void TestAddJobHeader()
		{
			var controller = GetChargesControllerForTest();

			var parent = Factory.NewWithValidTestData<DtbConsignment>();
			var localChargesAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var jobHeaderModel = GetDummyJobHeaderForTest();
			jobHeaderModel.EntityState = "Added";
			jobHeaderModel.JH_ParentID = parent.PK.ToGuid();
			jobHeaderModel.JH_OA_LocalChargesAddr = localChargesAddress.PK.ToGuid();

			var response = controller.UpdateJobHeaderAndCharges(jobHeaderModel);
			AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorNone, string.Empty);

			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, parent.PK);
			Job job = Factory.Load<Job>(jobQuery)[0];
			AssertNotNull(job);
		}

		public void TestReturnsErrorIfJobHeaderBusinessObjectIsNotFound()
		{
			var controller = GetChargesControllerForTest();

			var jobHeaderModel = GetDummyJobHeaderForTest();
			jobHeaderModel.EntityState = "Modified";

			var response = controller.UpdateJobHeaderAndCharges(jobHeaderModel);

			AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorError, "Failed to load Job.\r\n");
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobHeaderIsModified()
		{
			var controller = GetChargesControllerForTest();

			var jobHeader = GetJobBOForTest();
			jobHeader.JH_Description = "description";

			var jobHeaderModel = GetDummyJobHeaderForTest();
			jobHeaderModel.JH_PK = jobHeader.PK.ToGuid();

			var newLocalChargesAddress = Factory.NewWithValidTestData<OrgAddress>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			jobHeaderModel.JH_OA_LocalChargesAddr = newLocalChargesAddress.PK.ToGuid();
			jobHeaderModel.JH_A_JOP = new DateTime(2019, 7, 3);
			jobHeaderModel.JH_AgentChargesCFX = 1;
			jobHeaderModel.JH_ARInvoiceReference = "new ref";
			jobHeaderModel.JH_Description = "new description";
			jobHeaderModel.JH_ExcludeFromPeriodicRating = true;
			jobHeaderModel.JH_GB = newBranch.PK.ToGuid();
			jobHeaderModel.JH_GC = newCompany.PK.ToGuid();
			jobHeaderModel.JH_GE = newDepartment.PK.ToGuid();
			jobHeaderModel.JH_GS_NKRepOps = "AA";
			jobHeaderModel.JH_GS_NKRepSales = "AAA";
			jobHeaderModel.JH_HeaderType = "AAA";
			jobHeaderModel.JH_HoldReason = "new hold reason";
			jobHeaderModel.JH_IsProfitSharePosted = true;
			jobHeaderModel.JH_IsValid = true;
			jobHeaderModel.JH_JobBufferPercentOverride = 1;
			jobHeaderModel.JH_JobLocalReference = "22222222";
			jobHeaderModel.JH_LocalChargesCFX = 1;
			jobHeaderModel.JH_LocalClientInvoicingStyle = "AAA";
			jobHeaderModel.JH_Name = "new jh name";
			jobHeaderModel.JH_PaymentCollectionStatus = "AAA";
			jobHeaderModel.JH_ProfitLossReasonCode = "AAA";
			jobHeaderModel.JH_RatingHasBeenRun = false;
			jobHeaderModel.JH_SingleAgentsInvoicePerConsol = false;
			jobHeaderModel.JH_Status = "JFC";
			jobHeaderModel.JH_SystemCreateTimeUtc = new DateTime(2019, 7, 4);
			jobHeaderModel.JH_SystemCreateUser = "AAA";
			jobHeaderModel.JH_TH_NKQuoteNumber = "new quote num";
			jobHeaderModel.JH_UniqueJobInvoiceNumber = 1;

			jobHeaderModel.EntityState = "Modified";

			var response = controller.UpdateJobHeaderAndCharges(jobHeaderModel);

			CombineAssertions(() =>
			{
				AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorNone, string.Empty);
				AssertEquals("new description", jobHeader.JH_Description);
				AssertEquals("JH_OA_LocalChargesAddr should have changed.", newLocalChargesAddress.PK.ToGuid(), jobHeader.JH_OA_LocalChargesAddr);
				AssertEquals("JH_A_JOP should have changed.", new DateTime(2019, 7, 3), jobHeader.JH_A_JOP);
				AssertEquals("JH_AgentChargesCFX should have changed.", 1m, jobHeader.JH_AgentChargesCFX);
				AssertEquals("JH_ARInvoiceReference should have changed.", "new ref", jobHeader.JH_ARInvoiceReference);
				AssertEquals("JH_ExcludeFromPeriodicRating should have changed.", true, jobHeader.JH_ExcludeFromPeriodicRating);
				AssertEquals("JH_GB should have changed.", newBranch.PK.ToGuid(), jobHeader.JH_GB);
				AssertEquals("JH_GC should have changed.", newCompany.PK.ToGuid(), jobHeader.JH_GC);
				AssertEquals("JH_GE should have changed.", newDepartment.PK.ToGuid(), jobHeader.JH_GE);
				AssertEquals("JH_GS_NKRepOps should have changed.", "AA", jobHeader.JH_GS_NKRepOps);
				AssertEquals("JH_GS_NKRepSales should have changed.", "AAA", jobHeader.JH_GS_NKRepSales);
				AssertEquals("JH_HeaderType should have changed.", "AAA", jobHeader.JH_HeaderType);
				AssertEquals("JH_HoldReason should have changed.", "new hold reason", jobHeader.JH_HoldReason);
				AssertEquals("JH_IsProfitSharePosted should have changed.", true, jobHeader.JH_IsProfitSharePosted);
				AssertEquals("JH_JobBufferPercentOverride should have changed.", (byte)1, jobHeader.JH_JobBufferPercentOverride);
				AssertEquals("JH_JobLocalReference should have changed.", "22222222", jobHeader.JH_JobLocalReference);
				AssertEquals("JH_LocalChargesCFX should have changed.", 1m, jobHeader.JH_LocalChargesCFX);
				AssertEquals("JH_LocalClientInvoicingStyle should have changed.", "AAA", jobHeader.JH_LocalClientInvoicingStyle);
				AssertEquals("JH_Name should have changed.", "new jh name", jobHeader.JH_Name);
				AssertEquals("JH_PaymentCollectionStatus should have changed.", "AAA", jobHeader.JH_PaymentCollectionStatus);
				AssertEquals("JH_ProfitLossReasonCode should have changed.", "AAA", jobHeader.JH_ProfitLossReasonCode);
				AssertEquals("JH_RatingHasBeenRun should have changed.", false, jobHeader.JH_RatingHasBeenRun);
				AssertEquals("JH_SingleAgentsInvoicePerConsol should have changed.", false, jobHeader.JH_SingleAgentsInvoicePerConsol);
				AssertEquals("JH_Status should have changed.", "JFC", jobHeader.JH_Status);
				AssertEquals("JH_SystemCreateTimeUtc should have changed.", new DateTime(2019, 7, 4), jobHeader.JH_SystemCreateTimeUtc);
				AssertEquals("JH_SystemCreateUser should have changed.", "AAA", jobHeader.JH_SystemCreateUser);
				AssertEquals("JH_TH_NKQuoteNumber should have changed.", "new quote num", jobHeader.JH_TH_NKQuoteNumber);
				AssertEquals("JH_UniqueJobInvoiceNumber should have changed.", (byte)1, jobHeader.JH_UniqueJobInvoiceNumber);
			});
		}

		public void TestJobHeaderAndChargesAreUpdated()
		{
			var controller = GetChargesControllerForTest();
			var mockedAdapter = new Mock<IJobCostingAdapter>();

			var jobHeader = GetJobBOForTest();
			jobHeader.JH_Description = "description";
			Factory.Save();

			var jobHeaderModel = GetDummyJobHeaderWithJobChargesForTest();
			jobHeaderModel.JH_PK = jobHeader.PK.ToGuid();
			jobHeaderModel.JH_OA_LocalChargesAddr = jobHeader.JH_OA_LocalChargesAddr.ToGuid();
			jobHeaderModel.EntityState = "Modified";

			mockedAdapter.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()));
			var response = controller.UpdateJobHeaderAndCharges(jobHeaderModel);

			AssertNegotiatedContentResult(response, (int)HttpStatusCode.OK, MaxErrorNone, string.Empty);
			AssertEquals("New description", jobHeader.JH_Description);
		}

		#endregion

		#region Implementation

		public static T ReadJsonToObject<T>(string json)
		{
			using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var serialiser = new DataContractJsonSerializer(typeof(T));
			var deserializedObject = (T)serialiser.ReadObject(memoryStream);
			return deserializedObject;
		}

		JobHeaderAndChargesController GetChargesControllerForTest() =>
#if NETFRAMEWORK
			ControllerHelper.GetController<JobHeaderAndChargesController>();
#elif NET
			ControllerHelper.GetController<JobHeaderAndChargesController, AccountingBillingService>();
#endif

		JobHeaderModel GetDummyJobHeaderWithJobChargesForTest()
		{
			var jobHeader = GetDummyJobHeaderForTest();
			var jobCharges = GetDummyJobChargesForTest();
			jobHeader.JobCharges = jobCharges;
			return jobHeader;
		}

		List<JobChargeModel> GetDummyJobChargesForTest()
		{
			var charge1 = new JobChargeModel()
			{
				JR_AC = Guid.NewGuid(),
				JR_AT_CostGSTRate = Guid.NewGuid(),
				JR_AT_SellGSTRate = Guid.NewGuid(),
				JR_CostGovtChargeCode = "CostGCode1",
				JR_SellGovtChargeCode = "SellGCode1",
				JR_CostReference = "CN0036862019GLD",
				JR_Desc = "Description2",
				JR_DisplaySequence = 2,
				JR_GB = Guid.NewGuid(),
				JR_GE = Guid.NewGuid(),
				JR_InvoiceType = "FID",
				JR_LocalSellAmt = 361.15m,
				JR_OH_CostAccount = Guid.NewGuid(),
				JR_OH_SellAccount = Guid.NewGuid(),
				JR_OSCostExRate = 1,
				JR_OSCostGSTAmt = 1,
				JR_OSSellAmt = 361.15m,
				JR_OSSellExRate = 1,
				JR_RX_NKCostCurrency = "AUD",
				JR_RX_NKSellCurrency = "AUD",
				EntityState = "Added"
			};

			var charge2 = new JobChargeModel()
			{
				JR_PK = Guid.NewGuid(),
				JR_AC = Guid.NewGuid(),
				JR_APInvoiceDate = DateTime.Now,
				JR_APInvoiceNum = "INVNUM00001",
				JR_AT_CostGSTRate = Guid.NewGuid(),
				JR_AT_SellGSTRate = Guid.NewGuid(),
				JR_CostGovtChargeCode = "COSTGOV001",
				JR_SellGovtChargeCode = "SELLGOV001",
				JR_CostReference = "COSTREF001",
				JR_Desc = "Description",
				JR_DisplaySequence = 1,
				JR_GB = Guid.NewGuid(),
				JR_GE = Guid.NewGuid(),
				JR_InvoiceType = "FID",
				JR_LocalCostAmt = 10m,
				JR_LocalSellAmt = 20m,
				JR_OH_CostAccount = Guid.NewGuid(),
				JR_OH_SellAccount = Guid.NewGuid(),
				JR_OSCostAmt = 10m,
				JR_OSCostExRate = 1,
				JR_OSCostGSTAmt = 1,
				JR_OSSellAmt = 20m,
				JR_OSSellExRate = 1,
				JR_PaymentDate = DateTime.Now,
				JR_RX_NKCostCurrency = "AUD",
				JR_RX_NKSellCurrency = "AUD",
				EntityState = "Modified"
			};

			var charges = new List<JobChargeModel> { charge1, charge2 };
			return charges;
		}

		JobHeaderModel GetDummyJobHeaderForTest()
		{
			var model = new JobHeaderModel()
			{
				JH_PK = Guid.NewGuid(),
				JH_A_JOP = new DateTime(2017, 5, 1),
				JH_AgentChargesCFX = 0,
				JH_ARInvoiceReference = "",
				JH_Description = "New description",
				JH_ExcludeFromPeriodicRating = false,
				JH_GB = GlbBranch.CurrentBranch.PK.ToGuid(),
				JH_GC = GlbCompany.CurrentCompany.PK.ToGuid(),
				JH_GE = GlbDepartment.CurrentDepartment.PK.ToGuid(),
				JH_GS_NKRepOps = "ZZ",
				JH_GS_NKRepSales = "",
				JH_HeaderType = "JOB",
				JH_HoldReason = "",
				JH_IsProfitSharePosted = false,
				JH_IsValid = false,
				JH_JobBufferPercentOverride = 0,
				JH_JobLocalReference = "00005511",
				JH_JobNum = "CN0041242019STD",
				JH_LocalChargesCFX = 0,
				JH_LocalClientInvoicingStyle = "",
				JH_Name = "",
				JH_OA_LocalChargesAddr = Guid.NewGuid(),
				JH_ParentID = Guid.NewGuid(),
				JH_PaymentCollectionStatus = "",
				JH_ProfitLossReasonCode = "",
				JH_RatingHasBeenRun = true,
				JH_SingleAgentsInvoicePerConsol = true,
				JH_Status = "WRK",
				JH_SystemCreateTimeUtc = new DateTime(2017, 5, 2),
				JH_SystemCreateUser = "ZZ",
				JH_SystemLastEditTimeUtc = new DateTime(2017, 5, 1),
				JH_SystemLastEditUser = "ZZ",
				JH_TH_NKQuoteNumber = "",
				JH_UniqueJobInvoiceNumber = 0,
				EntityState = "Unchanged",
				ParentTableCode = DtbConsignmentSchema.Constants.Prefix
			};
			return model;
		}

		Job GetJobBOForTest()
		{
			var parent = Factory.NewWithValidTestData<DtbConsignment>();
			Job job = new Job.Loader(parent).TryCreate();
			var localChargesAddress = Factory.NewWithValidTestData<OrgAddress>();

			job.JH_A_JOP = new DateTime(2017, 5, 1);
			job.JH_AgentChargesCFX = 0;
			job.JH_ARInvoiceReference = "";
			job.JH_Description = "";
			job.JH_ExcludeFromPeriodicRating = false;
			job.JH_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			job.JH_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			job.JH_GS_NKRepOps = "ZZ";
			job.JH_GS_NKRepSales = "";
			job.JH_HeaderType = "JOB";
			job.JH_HoldReason = "";
			job.JH_IsProfitSharePosted = false;
			job.JH_JobBufferPercentOverride = 0;
			job.JH_JobLocalReference = "00005511";
			job.JH_JobNum = "CN0041242019STD";
			job.JH_LocalChargesCFX = 0;
			job.JH_LocalClientInvoicingStyle = "";
			job.JH_Name = "";
			job.JH_ParentID = parent.PK;
			job.JH_PaymentCollectionStatus = "";
			job.JH_ProfitLossReasonCode = "";
			job.JH_RatingHasBeenRun = true;
			job.JH_SingleAgentsInvoicePerConsol = true;
			job.JH_Status = "WRK";
			job.JH_SystemCreateTimeUtc = new DateTime(2017, 5, 2);
			job.JH_SystemCreateUser = "ZZ";
			job.JH_SystemLastEditTimeUtc = new DateTime(2017, 5, 1);
			job.JH_SystemLastEditUser = "ZZ";
			job.JH_TH_NKQuoteNumber = "";
			job.JH_UniqueJobInvoiceNumber = 0;
			job.JH_OA_LocalChargesAddr = localChargesAddress.PK;
			job.JH_ParentTableCode = parent.TablePrefix;

			return job;
		}

#endregion

		#region Assertions

#if NETFRAMEWORK
		public static void AssertNegotiatedContentResult(IActionResult response, int httpStatusCode, string maxErrorMsg, string responseMsg, bool containsResponseMessage = false)
		{
			var responseJsonObject = ReadJsonToObject<UpdateChargesJsonResponse>(((NegotiatedContentResult<string>)response).Content);

			AssertEquals("Response should have correct type", typeof(NegotiatedContentResult<string>), response.GetType());
			AssertEquals("Status code of response should be correct", httpStatusCode, (int)((NegotiatedContentResult<string>)response).StatusCode);
			AssertEquals("Response MaxError should be 'None'", maxErrorMsg, responseJsonObject.MaxError);

			if (containsResponseMessage)
			{
				AssertContains("Response Message should be correct", responseMsg, responseJsonObject.Message);
			}
			else
			{
				AssertEquals("Response Message should be correct", responseMsg, responseJsonObject.Message);
			}
		}
#elif NET
		public static void AssertNegotiatedContentResult(IActionResult response, int httpStatusCode, string maxErrorMsg, string responseMsg, bool containsResponseMessage = false)
		{
			var result = string.Empty;
			var statusCode = 0;

			switch (response)
			{
				case ObjectResult objectResult:
					result = objectResult.Value.ToString();
					statusCode = (int)objectResult.StatusCode;
					AssertEquals("Response should have correct type", typeof(OkObjectResult), response.GetType());
					AssertEquals("Status code of response should be correct", httpStatusCode, statusCode);
					break;
				case JsonResult jsonResult:
					result = jsonResult.Value.ToString();
					AssertEquals("Response should have correct type", typeof(JsonResult), response.GetType());
					break;
			}
			var responseJsonObject = ReadJsonToObject<UpdateChargesJsonResponse>(result);

			AssertEquals("Response MaxError should be 'None'", maxErrorMsg, responseJsonObject.MaxError);

			if (containsResponseMessage)
			{
				AssertContains("Response Message should be correct", responseMsg, responseJsonObject.Message);
			}
			else
			{
				AssertEquals("Response Message should be correct", responseMsg, responseJsonObject.Message);
			}
		}
#endif

		#endregion
	}
}
