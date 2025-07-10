using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing;

internal class ValidationToolRequestsHandleTest : TestCaseWithFactory
{
	public void TestHandle_ByDefault()
	{
		var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
		assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

		TestHandleCore(assigneeOrg.PK, assigneeOrg.Contacts[0].PK, CreateRequestType(CreateRequestTemplate()), true);
	}

	public void TestHandle_FallbackToEmpty()
	{
		TestHandleCore(ZGuid.Empty, ZGuid.Empty, CreateRequestType(CreateRequestTemplate()), true);
	}

	public void TestHandle_RuleWithoutRequestTypeOnFailure()
	{
		var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
		assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

		TestHandleCore(assigneeOrg.PK, assigneeOrg.Contacts[0].PK, null, false);
	}

	public void TestHandle_RequestTypeOnFailureWithoutTemplate()
	{
		var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
		assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

		TestHandleCore(assigneeOrg.PK, assigneeOrg.Contacts[0].PK, CreateRequestType(null), false);
	}

	ExternalRequestInfoTemplate CreateRequestTemplate()
	{
		var requestTemplate = Factory.New<ExternalRequestInfoTemplate>();
		requestTemplate.RIT_Template = ZBlob.FromUTF8("check template");
		requestTemplate.RIT_JobType = "ORD";
		requestTemplate.RIT_Code = "A01";
		requestTemplate.RIT_Description = "Template Desc";

		return requestTemplate;
	}

	ExternalRequestType CreateRequestType(ExternalRequestInfoTemplate template)
	{
		var requestType = Factory.New<ExternalRequestType>();
		requestType.RQT_Code = "B01";
		requestType.RQT_Description = "Type DESC";
		requestType.RQT_JobType = "ORD";
		requestType.RQT_Assignee = "BUY";
		requestType.RQT_Reviewer = "SUD";
		requestType.RQT_RequiredInDays = 10;
		requestType.RQT_RIT_Template = template?.PK ?? ZGuid.Empty;

		return requestType;
	}

	void TestHandleCore(ZGuid assigneeOrgPK, ZGuid assignedContactPK, ExternalRequestType requestType, bool requestShouldBeGenerated)
	{
		var mockWorkflowMacroValueEvaluator = new Mock<IWorkflowMacroValueEvaluator>();
		mockWorkflowMacroValueEvaluator.Setup(p => p.GetMacroValue<string>(It.IsAny<BusinessObjectFactory>(), It.IsAny<object>(), It.IsAny<string>())).Returns(() => "macro finished");
		using var textMacroProcessorSubstitute = ObjectFactory.Substitute(mockWorkflowMacroValueEvaluator.Object);

		var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
		reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_Name = "VWG";
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

		var templateValidation = template.ProcessTemplateValidations.AddNew();
		templateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation.P0V_Description = "D1";
		templateValidation.P0V_Severity = "ERR";
		templateValidation.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		templateValidation.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";
		templateValidation.P0V_RQT_RequestTypeOnFailure = requestType?.PK ?? ZGuid.Empty;

		var dummyObject = Factory.New<TestDummyWithWorkflow>();
		dummyObject.Z0_Code = "XX1";
		dummyObject.RequestTypeCode = "XX2";
		dummyObject.AssignedOrganizationPK = assigneeOrgPK;
		dummyObject.AssignedContactPK = assignedContactPK;
		dummyObject.ReviewerOrganizationPK = reviewerOrg.PK;
		dummyObject.ReviewerContactPK = reviewerOrg.Contacts[0].PK;
		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		new ValidationToolRequestsHandle().Handle(newFactory, new List<ValidationToolRequestHandleParameter> { new ValidationToolRequestHandleParameter { BusinessEntity = dummyObject, RequestTypePKOnFailure = requestType?.PK ?? ZGuid.Empty, ValidationRulePK = templateValidation.PK } });
		Factory.Save();

		if (requestShouldBeGenerated)
		{
			AssertNull(Factory.Load<ExternalRequest>(new ZQuery(ExternalRequestSchema.REQ_RQT_Type, requestType.PK)).FirstOrDefault());

			newFactory.Save();

			var request = newFactory.Load<ExternalRequest>(new ZQuery(ExternalRequestSchema.REQ_RQT_Type, requestType.PK)).Single();

			CombineAssertions(() =>
			{
				AssertEquals(dummyObject.GetRequestJobID(), request.REQ_ParentJobID);
				AssertEquals(dummyObject.PK, request.REQ_ParentID);
				AssertEquals(dummyObject.Z0_Code, request.REQ_ParentJobID);
				AssertEquals(dummyObject.TablePrefix, request.REQ_ParentTableCode);
				AssertEquals(ExternalRequestStatuses.Codes.Assign, request.REQ_Status);
				AssertEquals(requestType.PK, request.REQ_RQT_Type);
				AssertEquals("XX2", request.REQ_Type);
				AssertEquals("XX1:Type DESC", request.REQ_Description);
				Assert(request.REQ_RequiredByDate.ToDateTime() >= DateTime.Now.Date);
				AssertEquals(dummyObject.AssignedOrganizationPK, request.REQ_OH_AssignedOrganization);
				AssertEquals(dummyObject.AssignedContactPK, request.REQ_OC_AssignedContact);
				AssertEquals(dummyObject.ReviewerOrganizationPK, request.REQ_OH_ReviewerOrganization);
				AssertEquals(dummyObject.ReviewerContactPK, request.REQ_OC_ReviewerContact);
				AssertEquals(false, request.REQ_RequesteeCompleted);
				AssertEquals(string.Empty, request.REQ_Notes.ToUTF8());
			});
		}
		else
		{
			newFactory.Save();

			var request = newFactory.Load<ExternalRequest>(new ZQuery(ExternalRequestSchema.REQ_ParentID, dummyObject.PK)).FirstOrDefault();

			AssertNull(request);
		}
	}

	class TestDummyWithWorkflow : DummyWithWorkflow, IExternalRequestGenerationProvider
	{
		public TestDummyWithWorkflow(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			Argument.NotNull(DummyWorkflowDescriptor.Instance, nameof(DummyWorkflowDescriptor));
		}

		public ZString GetRequestJobID() => Z0_Code;

		public ZGuid AssignedOrganizationPK { get; set; }

		public ZGuid AssignedContactPK { get; set; }

		public ZGuid ReviewerOrganizationPK { get; set; }

		public ZGuid ReviewerContactPK { get; set; }

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType)
		{
			if (addressType == "BUY")
			{
				return (AssignedOrganizationPK, AssignedContactPK);
			}

			if (addressType == "SUD")
			{
				return (ReviewerOrganizationPK, ReviewerContactPK);
			}

			return (ZGuid.Empty, ZGuid.Empty);
		}

		public string RequestTypeCode { get; set; }

		public ZString GetRequestTypeCode() => RequestTypeCode;

		public override string TablePrefix => "JD";
	}
}
