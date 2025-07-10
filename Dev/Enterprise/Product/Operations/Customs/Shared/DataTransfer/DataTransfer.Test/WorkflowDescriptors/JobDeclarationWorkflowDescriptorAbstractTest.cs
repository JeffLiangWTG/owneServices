using System;
using CargoWise.Definitions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class JobDeclarationWorkflowDescriptorAbstractTest<TDeclaration, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor>
			where TDeclaration : BaseJobDeclaration
			where TWorkflowDescriptor : JobDeclarationWorkflowDescriptor, new()
	{
		public void TestSupportedTriggerLineTypes()
		{
			var lineParent = (WorkflowDescriptor as IWorkflowParentWithLines);

			AssertContainsExactElementsInAnyOrder([TriggerLineTypes.Codes.CusEntryHeader, TriggerLineTypes.Codes.CusExitReport, TriggerLineTypes.Codes.CusNOEmmaMessageGenerator], lineParent.SupportedTriggerLineTypes);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.Brokerage.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", JobInvoicingConsumerTypes.Brokerage.Description, WorkflowDescriptor.Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
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
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2 is Job Type", "Job Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertEquals("Collection should have Import as an option", true, WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("IMP"));
			AssertSubType2ListDependsOnBranch();
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Customs, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetDeclarationWithOrganisations(true),
				GetDeclarationWithOrganisations(false),
			};
		}

		protected override OrgHeader[] GetExpectedOrganisationsForPartyType(ProcessTask task, MessageRecipientPartyType partyType)
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)task.Parent;
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

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobDeclarationSchema.JE_HouseBill,
					JobDeclarationSchema.JE_VesselName,
					JobDeclarationSchema.JE_VoyageFlightNo,
					JobDeclarationSchema.JE_DateAtOrigin,
					JobDeclarationSchema.JE_DateAtFinalDestination,
					JobDeclarationSchema.JE_ExportDate,
					JobDeclarationSchema.JE_DateOfArrival,
					JobDeclarationSchema.JE_MasterBill,
					JobDeclarationSchema.JE_EntryAuthorisationDate,
					JobDeclarationSchema.JE_EntrySubmittedDate,
					JobDeclarationSchema.JE_WarehouseReleaseDate,
					JobDeclarationSchema.JE_DateOfFirstArrival,
					JobDeclarationSchema.JE_EntryDate,
					JobDeclarationSchema.JE_RL_NKPortOfLoading,
					JobDeclarationSchema.JE_RL_NKPortOfFirstArrival,
					JobDeclarationSchema.JE_RL_NKPortOfArrival,
					JobDeclarationSchema.JE_RL_NKFinalDestination,
					JobDeclarationSchema.JE_LandedPieces,
					JobDeclarationSchema.JE_TotalNoOfPacks,
					JobDocsAndCartageSchema.JP_EstimatedDelivery,
					JobDocsAndCartageSchema.JP_EstimatedPickup,
					CusEntryHeaderSchema.CH_BondAcquittedDate,
					CusEntryHeaderSchema.CH_BondValidToDate
				};
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.PickupCartage |
					MessageRecipientPartyType.DeliveryCartage |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.Forwarder |
					MessageRecipientPartyType.ExternalBroker |
					MessageRecipientPartyType.ControllingCustomer;
			}
		}

		protected override IWorkflowProvider GetParentForGettingDateTimeOffset()
		{
			var declaration = GetDeclarationWithOrganisations(true);
			declaration.JE_RL_NKFinalDestination = "AUPER";

			return declaration;
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType) => TimeSpan.FromHours(8);

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			if (dateTimeSourceType == DeclarationEstimateDefaultedFromList.Codes.ETA)
			{
				((TDeclaration)workflowProvider).JE_DateAtFinalDestination = localTime;
			}
			else
			{
				base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
			}
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			var declaration = (BaseJobDeclaration)workflowProvider;
			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.PickupCartage)
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			}
			else
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			}
			declaration.DocsAndCartage.PickupCartageCoPK = PickupCartageOrg.PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = DeliveryCartageOrg.PK;
		}

		void AssertSubType2ListDependsOnBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Zimbabwe))
			{
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
				var caCompany = Factory.New<GlbCompany>();
				caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				var caBranch = caCompany.Branches.AddNew();
				caBranch.GB_Code = "CAX";
				caBranch.GB_RL_NKHomePort = "CAVAN";

				WorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

				CombineAssertions(() =>
				{
					AssertEquals("Message type list for a global should have 6 options. This is the default from BaseJobDeclaration's message type list without removing any.  If this fails, check the list's length.", "DRW, EXP, EXW, IMP, MSC, REF", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = blightyBranch.PK;
					AssertEquals("Message type list for GB should have 6 options. If this fails, check that the EU message type list has been enhanced", "ARN, DEP, EXP, IMP, MSC, ULR", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = aussieBranch.PK;
					AssertEquals("Message type list for AU should have 9 options. If this fails, check that the AU message type list has been enhanced", "AQS, DRW, EXP, EXW, EXX, IMP, IMX, MSC, WEA", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = yankeeBranch.PK;
					AssertEquals("Message type list for US should have 5 options. If this fails, check that the US message type list has been enhanced(because us have new separate process type for Drawback)", "EXP, FTZ, IMP, IMX, MSC", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = prBranch.PK;
					AssertEquals("Message type list for PR should have 5 options. If this fails, check that the US message type list has been enhanced", "EXP, FTZ, IMP, IMX, MSC", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = caBranch.PK;
					AssertEquals("Message type list for CA should have 9 options (B2, EXP, IMP, LVS, LVX, MSC, IM2, IMO). If this fails, check that the CA message type list has been enhanced", "B2, B3X, EXP, IM2, IMP, LVS, LVX, MSC, IMO", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.P0_GB = sinBranch.PK;
					AssertEquals("Message type list for SG should have 5 options (COO, INP, IPT, OUT, TNP). If this fails, check that the SG message type list has been enhanced", "COO, INP, IPT, OUT, TNP", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate.GlobalTemplate = true;
					AssertEquals("Message type list for a global should have 5 options, because branch is Singapore. If this fails, check the list's length.", "COO, INP, IPT, OUT, TNP", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());

					WorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					WorkflowDescriptor.LastProcessTaskTemplate.GlobalTemplate = true;
					AssertEquals("Message type list for a global should have 14 options. If this fails, check the list's length.", "DRW, EXP, EXW, IMP, MSC, REF, AQS, COO, EXX, IMX, INP, IPT, OUT, TNP, WEA", WorkflowDescriptor.SubTypeInformation[1].List.ToCodeStrings());
				});
			}
		}

		BaseJobDeclaration GetDeclarationWithOrganisations(bool isImport)
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = isImport ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;

			JobHeader.Loader jobLoader = new JobHeader.Loader(declaration);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			declaration.JE_OH_Importer = ConsigneeOrg.PK;
			declaration.JE_OH_Supplier = ConsignorOrg.PK;

			declaration.DocsAndCartage.PickupCartageCoPK = PickupCartageOrg.PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = DeliveryCartageOrg.PK;

			declaration.JE_OH_Forwarder = Forwarder.PK;

			declaration.JE_OH_ExternalBroker = ExternalBrokerOrg.PK;

			declaration.JE_OH_ControllingCustomer = ControllingCustomerOrg.PK;

			return declaration;
		}
	}
}
