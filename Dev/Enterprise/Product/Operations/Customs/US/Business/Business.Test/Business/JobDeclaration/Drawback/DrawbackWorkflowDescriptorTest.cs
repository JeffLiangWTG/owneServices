using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackWorkflowDescriptor))]
	public class DrawbackWorkflowDescriptorTest : WorkflowDescriptorTestCase<DrawbackWorkflowDescriptor>
	{
		public void TestFieldColumnsCount()
		{
			AssertEquals(9, WorkflowDescriptor.GetWorkflowTriggerFieldColumns().Length);
		}

		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("Claim Type", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_EntryType));
			AssertEquals("Rejected Merchandise Reason", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_DRWRejectedMerchandiseReason));
			AssertEquals("Drawback Period Covered From", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_DRWDatePeriodFrom));
			AssertEquals("Drawback Period Covered To", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_DRWDatePeriodTo));
			AssertEquals("Estimated Claim Date", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_EstimatedEntryDate));
			AssertEquals("Method of Filing", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_DRWFilingMethod));
			AssertEquals("Purpose", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_DRWPurpose));
			AssertEquals("Bond Type", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_BondType));
			AssertEquals("Surety Code", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_SuretyCode));

			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Drawback declaration job", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("DRW", WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(JobDeclaration), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			return new IWorkflowProvider[] { dec };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
			}
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					USAddInfoSchema.US_EntryType,
					USAddInfoSchema.US_DRWRejectedMerchandiseReason,
					USAddInfoSchema.US_DRWDatePeriodFrom,
					USAddInfoSchema.US_DRWDatePeriodTo,
					USAddInfoSchema.US_EstimatedEntryDate,
					USAddInfoSchema.US_DRWFilingMethod,
					USAddInfoSchema.US_DRWPurpose,
					USAddInfoSchema.US_BondType,
					USAddInfoSchema.US_SuretyCode
				};
			}
		}

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			return dec;
		}

		public override string GetPrefixFromColumnName(string fieldColumnName)
		{
			var dec = Factory.New<JobDeclaration>();
			return dec.TablePrefix;
		}
	}
}
