using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CommercialInvoiceLineWorkflowDescriptorAbstractTest<TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor> where TWorkflowDescriptor : CommercialInvoiceLineWorkflowDescriptor, new()
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Commercial Invoice Line", WorkflowDescriptor.Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub types", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Job Type", "Job Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertSubType2ListDependsOnBranch();
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetInvoiceLinenWithOrganisations(true),
				GetInvoiceLinenWithOrganisations(false)
			};
		}

		protected override OrgHeader[] GetExpectedOrganisationsForPartyType(ProcessTask task, MessageRecipientPartyType partyType)
		{
			var declaration = (BaseJobDeclaration)task.Parent;
			if (!declaration.IsExport)
			{
				partyType = partyType & ~MessageRecipientPartyType.PickupCartage;
			}
			if (!declaration.IsImport)
			{
				partyType = partyType & ~MessageRecipientPartyType.DeliveryCartage;
			}
			return base.GetExpectedOrganisationsForPartyType(task, partyType);
		}

		void AssertSubType2ListDependsOnBranch()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Zimbabwe);
			var blightyCompany = Factory.New<GlbCompany>();
			blightyCompany.GC_RN_NKCountryCode = "GB";
			blightyCompany.GC_Code = "UKX";
			var blightyBranch = blightyCompany.Branches.AddNew();
			blightyBranch.GB_Code = "UKX";
			blightyBranch.GB_RL_NKHomePort = "GBMIK";
			var aussieCompany = Factory.New<GlbCompany>();
			aussieCompany.GC_RN_NKCountryCode = "AU";
			aussieCompany.GC_Code = "AUX";
			var aussieBranch = aussieCompany.Branches.AddNew();
			aussieBranch.GB_Code = "AUX";
			aussieBranch.GB_RL_NKHomePort = "AUSYD";
			var yankeeCompany = Factory.New<GlbCompany>();
			yankeeCompany.GC_RN_NKCountryCode = "US";
			yankeeCompany.GC_Code = "USX";
			var yankeeBranch = yankeeCompany.Branches.AddNew();
			yankeeBranch.GB_Code = "USX";
			yankeeBranch.GB_RL_NKHomePort = "USNYC";
			var sinCompany = Factory.New<GlbCompany>();
			sinCompany.GC_RN_NKCountryCode = "SG";
			sinCompany.GC_Code = "SGX";
			var sinBranch = sinCompany.Branches.AddNew();
			sinBranch.GB_Code = "SGX";
			sinBranch.GB_RL_NKHomePort = "SGSIN";
			var prCompany = Factory.New<GlbCompany>();
			prCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			prCompany.GC_Code = "PRX";
			var prBranch = prCompany.Branches.AddNew();
			prBranch.GB_Code = "PRX";
			prBranch.GB_RL_NKHomePort = "PRTRD";

			WorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			CombineAssertions(() =>
			{
				AssertEquals("Message type list for a global should have 6 options. This is the default from BaseJobDeclaration's message type list without removing any.  If this fails, check the list's length.", "DRW, EXP, EXW, IMP, MSC, REF", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = blightyBranch.PK;
				AssertEquals("Message type list for GB should have 6 options. If this fails, check that the EU message type list has been enhanced", "ARN, DEP, EXP, IMP, MSC, ULR", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = aussieBranch.PK;
				AssertEquals("Message type list for AU should have 9 options. If this fails, check that the AU message type list has been enhanced", "AQS, DRW, EXP, EXW, EXX, IMP, IMX, MSC, WEA", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = yankeeBranch.PK;
				AssertEquals("Message type list for US should have 5 options. If this fails, check that the US message type list has been enhanced(because us have new separate process type for Drawback and Recon)", "EXP, FTZ, IMP, IMX, MSC", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = prBranch.PK;
				AssertEquals("Message type list for PR should have 5 options. If this fails, check that the US message type list has been enhanced", "EXP, FTZ, IMP, IMX, MSC", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = sinBranch.PK;
				AssertEquals("Message type list for SG should have 5 options. If this fails, check that the SG message type list has been enhanced", "COO, INP, IPT, OUT, TNP", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());

				WorkflowDescriptor.LastProcessTaskTemplate.GlobalTemplate = true;
				AssertEquals("Message type list for a global should have 5 options, because branch is Singapore. If this fails, check the list's length.", "COO, INP, IPT, OUT, TNP", WorkflowDescriptor.SubTypeInformation[0].List.ToCodeStrings());
			});
		}

		BaseJobComInvoiceLine GetInvoiceLinenWithOrganisations(bool isImport)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = isImport ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;

			var jobLoader = new JobHeader.Loader(declaration);
			var job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			declaration.JE_OH_Importer = ConsigneeOrg.PK;
			declaration.JE_OH_Supplier = ConsignorOrg.PK;

			declaration.DocsAndCartage.PickupCartageCoPK = PickupCartageOrg.PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = DeliveryCartageOrg.PK;

			declaration.JE_OH_Forwarder = Forwarder.PK;

			return declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}
}
