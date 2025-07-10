using System;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WorkflowExceptionsControllerTest : TestCaseWithFactory
	{
		public void TestCreate_EmptyExceptionType_ShouldError()
		{
			var args = new CreateWorkflowExceptionArgs { ExceptionTypePK = Guid.Empty };
			var result = controller.Create(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Please enter an Exception Type", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestCreate_NoParentFound_ShouldError()
		{
			var dummy = Factory.New<DummyBaseBusinessObject>();
			var args = new CreateWorkflowExceptionArgs { ParentPK = Guid.Empty, ParentTableCode = dummy.TablePrefix, ExceptionTypePK = Guid.NewGuid() };
			var result = controller.Create(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Unable to find valid parent or it does not support workflow.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestCreate_ParentFoundButNoWorkflow_ShouldError()
		{
			var dummy = Factory.New<DummyBaseBusinessObject>();
			var args = new CreateWorkflowExceptionArgs { ParentPK = dummy.PK.ToGuid(), ParentTableCode = dummy.TablePrefix, ExceptionTypePK = Guid.NewGuid() };
			var result = controller.Create(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Unable to find valid parent or it does not support workflow.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestCreate_ExceptionDescriptionLengthExceed_ShouldError()
		{
			var dummy = Factory.New<DummyBaseBusinessObject>();
			var args = new CreateWorkflowExceptionArgs { ExceptionTypePK = dummy.PK.ToGuid(), ExceptionDescription = "Is Total Volume Below Customer Fill Minimum Capacity." };
			var result = controller.Create(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Exception description should not exceed max length 50", (result as BadRequestErrorMessageResult).Message);
		}

		[TestDate(2023, 01, 01, 10, 20, 30)]
		public void TestCreate_MinimumRequiredHappyPath_ShouldCreateAndSaveException()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			var exceptionType = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			exceptionType.WET_Description = "My Custom Type Description";
			Factory.Save();

			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = container.PK.ToGuid(),
				ParentTableCode = container.TablePrefix,
				ExceptionTypePK = exceptionType.PK.ToGuid()
			};

			var result = controller.Create(args);
			AssertType<OkResult>(result);
			container.Reload();

			AssertEquals(1, container.WorkflowItems.Exceptions.Count);
			var exception = container.WorkflowItems.Exceptions[0];

			CombineAssertions(() =>
			{
				AssertEquals("Exception Type", exceptionType.WET_Code, exception.ExceptionTypeCode);

				Assert("Published", !exception.P9_IsPublished);
				AssertEquals("Cause", Guid.Empty, exception.ExceptionCausePK);
				AssertEquals("Actual Date UTC", ZDateTime.UtcNow, exception.P9_ActualDateUtc);
				AssertEquals("Description", "My Custom Type Description", exception.P9_Description);
				AssertNullOrEmpty("Staff", exception.P9_GS_NKAssignedStaffMember);
				AssertNullOrEmpty("Staff Group", exception.P9_GG_AssignedGroupCode);
				AssertNullOrEmpty("Note", exception.P9_NotesAsString);
			});
		}

		public void TestCreate_WithOtherFieldsIncluded_ShouldSetVariousFieldsIfNotEmpty()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var exceptionType = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffGroup = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = order.PK.ToGuid(),
				ParentTableCode = order.TablePrefix,
				ExceptionTypePK = exceptionType.PK.ToGuid(),
				ExceptionStaffPK = staff.PK.ToGuid(),
				ExceptionStaffGroupPK = staffGroup.PK.ToGuid(),
				ExceptionPublished = true,
				ExceptionDescription = "Some description",
				ExceptionNotes = "Some notes",
			};

			var result = controller.Create(args);
			AssertType<OkResult>(result);
			order.Reload();

			AssertEquals(1, order.WorkflowItems.Exceptions.Count);
			var exception = order.WorkflowItems.Exceptions[0];

			CombineAssertions(() =>
			{
				Assert("Published", exception.P9_IsPublished);
				AssertEquals("Staff", staff.GS_Code, exception.P9_GS_NKAssignedStaffMember);
				AssertEquals("Staff Group", staffGroup.GG_Code, exception.P9_GG_AssignedGroupCode);
				AssertEquals("Exception Description", "Some description", exception.P9_Description);
				AssertEquals("Exception Notes", "Some notes", exception.P9_NotesAsString);
			});
		}

		public void TestCreate_WithOtherFieldsIncluded_ShouldSetExceptionPublished()
		{
			var order = Factory.NewWithValidTestData<Order>();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Code = "Test Address";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;
			order.JD_OA_BuyerAddress = orgAddress.PK;

			var exceptionType = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			Factory.Save();

			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = order.PK.ToGuid(),
				ParentTableCode = order.TablePrefix,
				ExceptionTypePK = exceptionType.PK.ToGuid(),
				ExceptionPublished = false,
			};

			var testCases = new[] { true, false };
			testCases.ForEach(isContact =>
			{
				if (isContact)
				{
					GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
				}
				else
				{
					GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
				}
				var result = controller.Create(args);
				AssertType<OkResult>(result);

				AssertEquals(1, order.WorkflowItems.Exceptions.Count);
				var exception = order.WorkflowItems.Exceptions[0];
				AssertEquals("Published", isContact || args.ExceptionPublished, exception.P9_IsPublished);
				order.WorkflowItems.Exceptions.DeleteAll();
			});
		}

		[TestDate(2023, 01, 01, 10, 20, 30)]
		public void TestCreate_WithExceptionTimeIncluded_ShouldTryParseISOString()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var exceptionType = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			Factory.Save();

			var date = ZDateTime.UtcNow.AddDays(-5);
			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = order.PK.ToGuid(),
				ParentTableCode = order.TablePrefix,
				ExceptionTypePK = exceptionType.PK.ToGuid(),
				ExceptionTimeUTC = date.ToISO8601String()
			};

			var result = controller.Create(args);
			AssertType<OkResult>(result);
			order.Reload();

			AssertEquals(1, order.WorkflowItems.Exceptions.Count);

			var exception = order.WorkflowItems.Exceptions[0];
			AssertEquals(date, exception.P9_ActualDateUtc);
		}

		public void TestCreate_WithInvalidExceptionTime_ShouldError()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var exceptionType = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			Factory.Save();

			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = order.PK.ToGuid(),
				ParentTableCode = order.TablePrefix,
				ExceptionTypePK = exceptionType.PK.ToGuid(),
				ExceptionTimeUTC = "Some Invalid Date",
			};

			var result = controller.Create(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Exception Time is not valid.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestCreate_IfContactUser_ShouldErrorIfNoTVFVisibility()
		{
			CreateOrderManagerData();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			Factory.Save();

			AssertSecurityError(order);
			AssertSecurityError(supplierBookingCY);
			AssertSecurityError(supplierBookingCFS);
			AssertSecurityError(containerLoadListCY);
			AssertSecurityError(containerLoadListCFS);

			void AssertSecurityError(BusinessObject entity)
			{
				var args = new CreateWorkflowExceptionArgs
				{
					ParentPK = entity.PK.ToGuid(),
					ParentTableCode = entity.TablePrefix,
					ExceptionTypePK = customException.PK.ToGuid()
				};

				var result = controller.Create(args);

				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("You do not have permission to perform this action. Please contact your system administrator.", (result as BadRequestErrorMessageResult).Message);
			}
		}

		public void TestCreate_IfContactUser_ShouldNotErrorIfHasTVFVisibility()
		{
			CreateOrderManagerData();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Code = "Test Address";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			order.JD_OA_BuyerAddress = orgAddress.PK;
			supplierBookingCY.JSB_OH_BookingParty = orgHeader.PK;
			supplierBookingCFS.JSB_OH_BookingParty = orgHeader.PK;
			containerLoadListCY.CLH_OH_LoadListParty = orgHeader.PK;

			Factory.Save();

			AssertNoSecurityError(order);
			AssertNoSecurityError(supplierBookingCY);
			AssertNoSecurityError(supplierBookingCFS);
			AssertNoSecurityError(containerLoadListCY);

			void AssertNoSecurityError(BusinessObject entity)
			{
				var args = new CreateWorkflowExceptionArgs
				{
					ParentPK = entity.PK.ToGuid(),
					ParentTableCode = entity.TablePrefix,
					ExceptionTypePK = customException.PK.ToGuid()
				};

				var result = controller.Create(args);
				AssertType<OkResult>(result);
			}
		}

		public void TestCreate_IfContactUser_ShouldNotErrorIfHasTVFVisibility_RegardlessOfEditability()
		{
			CreateOrderManagerData();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Code = "Test Address";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			supplierBookingCY.JSB_OH_BookingParty = orgHeader.PK;
			supplierBookingCY.JSB_Status = SupplierBookingStatusList.Codes.CAN;
			Factory.Save();

			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = supplierBookingCY.PK.ToGuid(),
				ParentTableCode = supplierBookingCY.TablePrefix,
				ExceptionTypePK = customException.PK.ToGuid()
			};

			var result = controller.Create(args);
			AssertType<OkResult>(result);
		}

		[TestDate(2023, 01, 01, 10, 20, 30)]
		public void TestCreate_IfEntityCreatedHasValidationError_ShouldError()
		{
			CreateOrderManagerData();

			Factory.Save();

			var fiftyYearsAgo = ZDateTime.UtcNow.AddYears(-50);
			var args = new CreateWorkflowExceptionArgs
			{
				ParentPK = order.PK.ToGuid(),
				ParentTableCode = order.TablePrefix,
				ExceptionTypePK = customException.PK.ToGuid(),
				ExceptionTimeUTC = fiftyYearsAgo.ToISO8601String(),
			};

			var result = controller.Create(args);
			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals($"The date '{fiftyYearsAgo:dd-MMM-yyyy}' is more than 10 years old and thus is not valid.", (result as BadRequestErrorMessageResult).Message);
		}

		#region Description Route

		public void TestDescription_NoParentFound_ShouldError()
		{
			var dummy = Factory.New<DummyBaseBusinessObject>();
			var result = controller.Description(Guid.Empty, dummy.TablePrefix);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Unable to find valid parent or it does not support workflow.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestDescription_ForOMEntities_ShouldSetDescriptionWithCC()
		{
			CreateOrderManagerData();

			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.OH_Code = "CONTRSYD";

			AssertShouldPopulateDescriptionWithCC(order);
			AssertShouldPopulateDescriptionWithCC(supplierBookingCY);
			AssertShouldPopulateDescriptionWithCC(supplierBookingCFS);
			AssertShouldPopulateDescriptionWithCC(containerLoadListCY);
			AssertShouldPopulateDescriptionWithCC(containerLoadListCFS);

			void AssertShouldPopulateDescriptionWithCC(IWorkflowProvider entity)
			{
				var controllingCustomer = (entity as IDocAddresses).DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer);
				controllingCustomer.OrganisationPK = controllingCustomerOrg.PK;

				Factory.Save();

				var result = AssertDescriptionRequestOKAndGetDescription(entity);
				AssertEquals("CC:CONTRSYD", result);
			}
		}

		public void TestDescription_ForOMEntities_ShouldFallbackToBuyerIfNoCC()
		{
			CreateOrderManagerData();

			var buyerOrg = Factory.NewWithValidTestData<OrgHeader>();
			buyerOrg.OH_Code = "BUYSYD";

			var buyerAddress = Factory.NewWithValidTestData<OrgAddress>();
			buyerAddress.OA_OH = buyerOrg.PK;
			buyerAddress.OA_Code = "Test Address";

			order.JD_OA_BuyerAddress = buyerAddress.PK;

			Factory.Save();

			AssertShouldPopulateDescriptionWithBuyer(order);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCY);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCFS, expectBuyerInDescription: false);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCY);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCFS, expectBuyerInDescription: false);

			void AssertShouldPopulateDescriptionWithBuyer(IWorkflowProvider entity, bool expectBuyerInDescription = true)
			{
				var result = AssertDescriptionRequestOKAndGetDescription(entity);

				if (expectBuyerInDescription)
				{
					AssertContains((entity as BusinessObject).TableName, "BUY:BUYSYD", result);
					AssertNotContains((entity as BusinessObject).TableName, "CC:BUYSYD", result);
				}
				else
				{
					AssertNotContains((entity as BusinessObject).TableName, "BUY:BUYSYD", result);
				}
			}
		}

		public void TestDescription_ForOMEntities_ShouldSetSupplierInDescriptionIfRelevant()
		{
			CreateOrderManagerData();

			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.OH_Code = "SUPPLMEL";

			var supplierAddress = Factory.NewWithValidTestData<OrgAddress>();
			supplierAddress.OA_OH = supplierOrg.PK;
			supplierAddress.OA_Code = "Test Address";

			var supplierDocAddressCY = (supplierBookingCY as IDocAddresses).DocAddresses.CreateWithAddressType(DocAddressType.SupplierDocumentaryAddress);
			supplierDocAddressCY.OrganisationPK = supplierOrg.PK;

			var supplierDocAddressCFS = (supplierBookingCFS as IDocAddresses).DocAddresses.CreateWithAddressType(DocAddressType.SupplierDocumentaryAddress);
			supplierDocAddressCFS.OrganisationPK = supplierOrg.PK;

			order.JD_OA_SupplierAddress = supplierAddress.PK;

			Factory.Save();

			AssertShouldPopulateDescriptionWithBuyer(order);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCY);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCFS);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCY);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCFS, expectSupplierInDescription: false);

			void AssertShouldPopulateDescriptionWithBuyer(IWorkflowProvider entity, bool expectSupplierInDescription = true)
			{
				var result = AssertDescriptionRequestOKAndGetDescription(entity);

				if (expectSupplierInDescription)
				{
					AssertContains((entity as BusinessObject).TableName, "SUP:SUPPLMEL", result);
				}
				else
				{
					AssertNotContains((entity as BusinessObject).TableName, "SUP:SUPPLMEL", result);
				}
			}
		}

		public void TestDescription_ForOMEntities_ShouldSetCFSAddressInDescriptionIfRelevant()
		{
			CreateOrderManagerData();

			var cfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			cfsOrg.OH_Code = "SUPPLMEL";

			var cfsAddress = Factory.NewWithValidTestData<OrgAddress>();
			cfsAddress.OA_OH = cfsOrg.PK;
			cfsAddress.OA_Code = "Test Address";

			supplierBookingCY.JSB_OA_CFSAddress = cfsAddress.PK;
			supplierBookingCFS.JSB_OA_CFSAddress = cfsAddress.PK;
			containerLoadListCY.CLH_OA_CFSAddress = cfsAddress.PK;
			containerLoadListCFS.CLH_OA_CFSAddress = cfsAddress.PK;

			Factory.Save();

			AssertShouldPopulateDescriptionWithBuyer(order, expectCFSAddressInDescription: false);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCY, expectCFSAddressInDescription: false);
			AssertShouldPopulateDescriptionWithBuyer(supplierBookingCFS);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCY, expectCFSAddressInDescription: false);
			AssertShouldPopulateDescriptionWithBuyer(containerLoadListCFS);

			void AssertShouldPopulateDescriptionWithBuyer(IWorkflowProvider entity, bool expectCFSAddressInDescription = true)
			{
				var result = AssertDescriptionRequestOKAndGetDescription(entity);

				if (expectCFSAddressInDescription)
				{
					AssertContains((entity as BusinessObject).TableName, "CFS:SUPPLMEL", result);
				}
				else
				{
					AssertNotContains((entity as BusinessObject).TableName, "CFS:SUPPLMEL", result);
				}
			}
		}

		public void TestDescription_ForOMEntities_DescriptionContainingMultipleOrgsCommaSeparatedIfRelevant()
		{
			CreateOrderManagerData();

			var buyerOrg = Factory.NewWithValidTestData<OrgHeader>();
			buyerOrg.OH_Code = "BUYSYD";

			var buyerAddress = Factory.NewWithValidTestData<OrgAddress>();
			buyerAddress.OA_OH = buyerOrg.PK;
			buyerAddress.OA_Code = "Test Address";

			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.OH_Code = "SUPPLMEL";

			var supplierAddress = Factory.NewWithValidTestData<OrgAddress>();
			supplierAddress.OA_OH = supplierOrg.PK;
			supplierAddress.OA_Code = "Test Address";

			order.JD_OA_BuyerAddress = buyerAddress.PK;
			order.JD_OA_SupplierAddress = supplierAddress.PK;

			Factory.Save();

			AssertContains("BUY:BUYSYD, SUP:SUPPLMEL", AssertDescriptionRequestOKAndGetDescription(order));
		}

		public void TestMarkActioned_NoExceptionFound_ShouldError()
		{
			var result = controller.MarkActioned(Guid.Empty);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Unable to find exception.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestMarkActioned_EmptyCauseAndResolution_ShouldError()
		{
			CreateOrderManagerData();
			customException.WET_Code = "ABC";
			customException.WET_IsCauseRequired = true;
			customException.WET_IsResolutionRequired = true;

			var exception = Factory.New<ProcessTask>();
			exception.P9_Type = "EXC";
			exception.P9_Description = "ABC";
			exception.ExceptionTypeCode = customException.WET_Code;

			Factory.Save();

			var result = controller.MarkActioned(exception.PK);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Error - ExceptionCausePK: Please enter a cause.\nError - ExceptionResolutionPK: Please enter a resolution.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestMarkActioned_Success()
		{
			var exception = Factory.New<ProcessTask>();
			exception.P9_Type = "EXC";
			exception.P9_Description = "ABC";
			Factory.Save();

			AssertEquals("OPN", exception.P9_Status);
			var result = controller.MarkActioned(exception.PK);

			AssertType<OkResult>(result);
			AssertEquals("RSL", exception.P9_Status);
		}

		#endregion

		#region Implementation

		void CreateOrderManagerData()
		{
			order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();

			supplierBookingCY = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBookingCY.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CY;
			supplierBookingCY.JSB_Status = SupplierBookingStatusList.Codes.INC;
			supplierBookingLineCY = supplierBookingCY.SupplierBookingLines.AddNew();
			supplierBookingLineCY.JSL_JO_OrderLine = orderLine.PK;

			supplierBookingCFS = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBookingCFS.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CFS;
			supplierBookingCFS.JSB_Status = SupplierBookingStatusList.Codes.INC;
			supplierBookingLineCFS = supplierBookingCFS.SupplierBookingLines.AddNew();
			supplierBookingLineCFS.JSL_JO_OrderLine = orderLine.PK;

			containerLoadListCY = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadListCY.CLH_JSB_Booking = supplierBookingCY.PK;
			containerLoadListLineCY = containerLoadListCY.LoadListLines.AddNew();
			containerLoadListLineCY.CLL_JSL_BookingLine = supplierBookingLineCY.PK;

			containerLoadListCFS = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadListLineCFS = containerLoadListCFS.LoadListLines.AddNew();
			containerLoadListLineCFS.CLL_JSL_BookingLine = supplierBookingLineCFS.PK;

			customException = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
		}

		string AssertDescriptionRequestOKAndGetDescription(IWorkflowProvider entity)
		{
			var response = controller.Description(entity.PK.ToGuid(), (entity as BusinessObject).TablePrefix) as OkNegotiatedContentResult<string>;
			return response.Content;
		}

		protected override void SetUp()
		{
			controller = new WorkflowExceptionsController();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
		}

		Order order;
		JobSupplierBooking supplierBookingCY;
		JobSupplierBooking supplierBookingCFS;
		JobSupplierBookingLine supplierBookingLineCY;
		JobSupplierBookingLine supplierBookingLineCFS;
		CYContainerLoadList containerLoadListCY;
		CFSContainerLoadList containerLoadListCFS;
		ContainerLoadListLine containerLoadListLineCY;
		ContainerLoadListLine containerLoadListLineCFS;
		ProcessWorkflowExceptionType customException;
		WorkflowExceptionsController controller;

		#endregion
	}
}
