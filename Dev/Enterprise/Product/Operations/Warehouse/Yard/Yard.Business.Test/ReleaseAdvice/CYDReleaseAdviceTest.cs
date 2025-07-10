using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdvice))]
	public class CYDReleaseAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDReleaseAdvice>();
		}

		public void TestNoteTypes()
		{
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();
			var expectedNoteTypes = new PredefinedNoteType[]
			{
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.DeliveryInstructionsNote
			};

			AssertContainsExactElementsInAnyOrder(expectedNoteTypes, releaseAdvice.NoteTypes);
		}

		public void TestDocAddressesIsEmpty()
		{
			var releaseAdvice = Factory.New<CYDReleaseAdvice>();
			var jda = releaseAdvice.DocAddresses;
			AssertEquals(0, jda.Count);
		}

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var releaseAdvice1 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress1.E2_ParentID = releaseAdvice1.PK;
			jobDocAddress1.OrganisationPK = client.PK;
			jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			var result1 = (ColumnValueRanker)releaseAdvice1.GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { client.PK, ZGuid.Empty }, result1.GetValues(ProcessTaskTemplateSchema.P0_OH_Client).Cast<ZGuid>());

			var releaseAdvice2 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			var result2 = (ColumnValueRanker)releaseAdvice2.GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { ZGuid.Empty }, result2.GetValues(ProcessTaskTemplateSchema.P0_OH_Client).Cast<ZGuid>());
		}

		#endregion

		#region TestSupportedAddressTypes

		public void TestSupportedAddressTypes()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			AssertContainsExactElementsInAnyOrder(supportedDocAddressTypes, releaseAdvice.SupportedAddressTypes);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			AssertEquals("", releaseAdvice.HumanReadableName);
		}

		#endregion

		#region TestGetCanOverrideCheckpoint

		public void TestGetCanOverrideCheckpoint()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals(Env.Security.None, releaseAdvice.GetCanOverrideCheckpoint(jobDocAddress));
		}

		#endregion

		#region TestCanDeleteAddress

		public void TestCanDeleteAddress()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals(true, releaseAdvice.CanDeleteAddress(jobDocAddress));
		}

		#endregion

		#region TestGetDocAddressRequirement

		public void TestGetDocAddressRequirement()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			foreach (var docAddrType in supportedDocAddressTypes)
			{
				AssertNull("GetDocAddressRequirement", releaseAdvice.GetDocAddressRequirement(docAddrType));
			}
		}

		#endregion

		#region TestGetOrgHeaderList

		public void TestGetOrgHeaderList()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			foreach (var docAddrType in supportedDocAddressTypes)
			{
				AssertNull("GetOrgHeaderList", releaseAdvice.GetOrgHeaderList(docAddrType));
			}
		}

		#endregion

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<CYDReleaseAdvice>();
			AssertEquals(Constants.DocManagerCodes.CYDReleaseAdvice, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestPiggyBackedDocAddressValidation

		public void TestPiggyBackedDocAddressValidation()
		{
			var releaseAdvice = (IDocAddresses)Factory.New<CYDReleaseAdvice>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertNull("PiggyBackedDocAddressValidation", releaseAdvice.PiggyBackedDocAddressValidation(jobDocAddress));
		}

		#endregion

		#region Client

		public void TestClientIsPopulatedCorrectly()
		{
			var releaseAdvice = (CYDReleaseAdvice)GetNewBusinessObject();
			AssertEquals(DocAddressType.BookingPartyDocumentaryAddress, releaseAdvice.Client.DocAddressType);
			AssertEquals(ContactType.LocalClient, releaseAdvice.Client.DefaultContactType);
			AssertEquals(releaseAdvice, releaseAdvice.Client.Parent);
		}

		#endregion

		public void TestIsActiveProperty()
		{
			var receiveAdvice = (CYDReleaseAdvice)GetNewBusinessObject();
			receiveAdvice.YRE_FromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			receiveAdvice.YRE_ToDate = ZDateTime.UtcNow.Date.AddDays(2);
			AssertEquals(true, receiveAdvice.IsActive);

			receiveAdvice.YRE_FromDate = ZDateTime.UtcNow.Date.AddDays(2);
			receiveAdvice.YRE_ToDate = ZDateTime.UtcNow.Date.AddDays(3);
			AssertEquals(false, receiveAdvice.IsActive);

			receiveAdvice.YRE_FromDate = ZDateTime.UtcNow.Date.AddDays(-3);
			receiveAdvice.YRE_ToDate = ZDateTime.UtcNow.Date.AddDays(-2);
			AssertEquals(false, receiveAdvice.IsActive);
		}

		static readonly DocAddressType[] supportedDocAddressTypes =
		{
			DocAddressType.ConsigneeAddress,
			DocAddressType.ConsignorDocumentaryAddress,
			DocAddressType.Forwarder,
			DocAddressType.ShippingLineAddress,
			DocAddressType.TransportCompanyDocumentaryAddress,
			DocAddressType.BookingPartyDocumentaryAddress,
			DocAddressType.ControllingCustomer
		};
	}

	#region WorkflowProviderTests
	[TestedType(typeof(CYDReleaseAdvice))]
	public class CYDReleaseAdviceWorkflowProviderTest : WorkflowProviderTest<CYDReleaseAdvice, CYDReleaseAdviceProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDReleaseAdviceWorkflowDescriptorCode;

		public void TestTemplateIsAppliedToCorrectClients()
		{
			var clientWithTemplate = Factory.NewWithValidTestData<OrgHeader>();
			var clientWithoutTemplate = Factory.NewWithValidTestData<OrgHeader>();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;
			template.P0_OH_Client = clientWithTemplate.PK;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var releaseAdvice1 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress1.OrganisationPK = clientWithTemplate.PK;
			jobDocAddress1.E2_ParentID = releaseAdvice1.PK;
			jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			var releaseAdvice2 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress2.OrganisationPK = clientWithoutTemplate.PK;
			jobDocAddress2.E2_ParentID = releaseAdvice2.PK;
			jobDocAddress2.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			Factory.Save();

			AssertEquals("Template tasks created when client matches", 1, releaseAdvice1.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when client doesn't match", 0, releaseAdvice2.WorkflowItems.Milestones.Count);
		}
	}

	#endregion
}
