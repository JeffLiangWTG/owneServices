using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(
			 typeof(JobInvoicingSupporter),
			 ExcludeClientDlls = true)]
	public abstract class JobInvoicingSupporterTest : TestCaseWithFactory
	{
		#region Implementation

		class DummyJobInvoicingSupporterWithSettableAgents : JobInvoicingSupporter
		{
			public DummyJobInvoicingSupporterWithSettableAgents(IJobHeaderParent parent) : base(parent)
			{
			}

			public OrgHeader SendingAgentSettable { get; set; }
			public OrgHeader ReceivingAgentSettable { get; set; }

			public override OrgHeader SendingAgent { get => SendingAgentSettable; }
			public override OrgHeader ReceivingAgent { get => ReceivingAgentSettable; }
		}

		public Type ExpectedObjectType
		{
			get { return GetExpectedObjectType(); }
		}

		protected Type GetExpectedObjectType() => TestedTypeHelper.GetTestedType(GetType());

		IJobInvoicingSupporter GetNewObject()
		{
			return GetNewBusinessObject().InvoicingSupporter;
		}

		protected abstract IJobInvoicingPlugIn GetNewBusinessObject();

		protected virtual bool ExcludeFromTestBecauseNoBillingTab
		{
			get { return false; }
		}

		protected virtual bool ExcludeFromFailureDueToNotAbleToReturnBusinessObjectForDocumentWrapper => false; // override and return true if IJobInvoicingPlugin implementer, but NOT a Business Object AND NOT implementing IBusinesssObjectProviderForDocumentWrapper

		protected override void SetUp()
		{
			base.SetUp();

			if (!TestingCountry.IsEmpty)
			{
				storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
		}
		protected ZString storedCountry;

		protected override void TearDown()
		{
			base.TearDown();

			if (!storedCountry.IsEmpty)
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		protected virtual ZString TestingCountry => ZString.Empty;

		#endregion

		#region Test

		public void TestEarliestSendingAgent()
		{
			var supporter = new DummyJobInvoicingSupporterWithSettableAgents(null);

			AssertNull(supporter.SendingAgent);
			AssertNull(supporter.EarliestSendingAgent);

			supporter.SendingAgentSettable = Factory.New<OrgHeader>();
			AssertNotNull(supporter.SendingAgent);
			AssertEquals(supporter.SendingAgent, supporter.EarliestSendingAgent);
		}

		public void TestLatestReceivingAgent()
		{
			var supporter = new DummyJobInvoicingSupporterWithSettableAgents(null);

			AssertNull(supporter.ReceivingAgent);
			AssertNull(supporter.LatestReceivingAgent);

			supporter.ReceivingAgentSettable = Factory.New<OrgHeader>();
			AssertNotNull(supporter.ReceivingAgent);
			AssertEquals(supporter.ReceivingAgent, supporter.LatestReceivingAgent);
		}

		public void TestCreateFreightWrapperForIJobInvoicingPlugin()
		{
			if (ExcludeFromTestBecauseNoBillingTab)
			{
				Assert(true);
				return;
			}

			var obj = GetNewBusinessObject();

			if (obj == null)
			{
				Fail(string.Format(CultureInfo.InvariantCulture, $"{GetType()} returns {obj} for GetNewBusinessObject()"));
			}

			var bizObj = obj as BusinessObject;
			if (bizObj != null)
			{
				var freightWrapper = GetFreightWrapper(bizObj);
				if (freightWrapper != null)
				{
					Assert(true);
					return;
				}
			}
			AssertForIBusinesssObjectProviderForDocumentWrapperImplementation(obj);
		}

		void AssertForIBusinesssObjectProviderForDocumentWrapperImplementation(IJobInvoicingPlugIn plugin)
		{
			Argument.NotNull(plugin, "Plugin cannot be null");
			var bizObjProvider = plugin as IBusinesssObjectProviderForDocumentWrapper;

			if (bizObjProvider == null)
			{
				if (plugin is BusinessObject || ExcludeFromFailureDueToNotAbleToReturnBusinessObjectForDocumentWrapper)
				{
					Assert(true);
				}
				else
				{
					Fail(string.Format(CultureInfo.InvariantCulture, $"This type is not a BusinessObject and also does not implement IBusinesssObjectProviderForDocumentWrapper: {plugin.GetType()}."));
				}
			}
			else
			{
				var bizObjForDocumentWrapper = bizObjProvider.BusinessObjectForDocumentWrapper;
				if (bizObjForDocumentWrapper != null)
				{
					var freightWrapper = GetFreightWrapper(bizObjForDocumentWrapper);
					if (freightWrapper != null)
					{
						Assert(true);
					}
					else
					{
						Fail(string.Format(CultureInfo.InvariantCulture, $"Freight wrapper could be created for type {bizObjForDocumentWrapper.GetType()}, check the implementation of IBusinesssObjectProviderForDocumentWrapper.BusinessObjectForDocumentWrapper in type {bizObjProvider.GetType()}"));
					}
				}
				else
				{
					Fail(string.Format(CultureInfo.InvariantCulture, $"This type implements IBusinesssObjectProviderForDocumentWrapper that does not return any BusinessObject: {plugin.GetType()}, BusinessObjectForDocumentWrapper: {bizObjForDocumentWrapper}"));
				}
			}
		}

		IDocumentWrapper GetFreightWrapper(BusinessObject businessObject)
		{
			var creator = ObjectFactory.Get<IDocFreightWrapperCreator>();
			return creator.CreateFreightWrapper(businessObject, Factory);
		}

		public void TestCreateNewObject()
		{
			if (ExcludeFromTestBecauseNoBillingTab)
			{
				Assert(true);
			}
			else
			{
				AssertNotNull(GetNewObject());
			}
		}

		public virtual void TestCustomsEntryNumberType()
		{
			var parent = GetNewBusinessObject();
			var invoicingSupporter = parent.InvoicingSupporter;

			AssertEquals("Empty by default.", ZString.Empty, invoicingSupporter.CustomsEntryNumberType);
		}

		public virtual void TestCommunityTransitStatus()
		{
			var parent = GetNewBusinessObject();
			var invoicingSupporter = parent.InvoicingSupporter;

			AssertEquals("Empty by default.", ZString.Empty, invoicingSupporter.CommunityTransitStatus);
		}

		public virtual void TestJob()
		{
			var parent = GetNewBusinessObject();
			var invoicingSupporter = parent.InvoicingSupporter;
			AssertNull("Precondition: Parent not have a Job Header.", invoicingSupporter.Job);

			var jobHeader = CreateJobHeader(parent);
			AssertEquals("Should return Parents JobHeader.", jobHeader, invoicingSupporter.Job);
		}

		public void TestJobNumber_CanBeLoaded_WithoutJobHeader()
		{
			var parent = GetNewBusinessObject();
			var invoicingSupporter = parent.InvoicingSupporter;
			AssertNull("Precondition: Parent not have a Job Header.", invoicingSupporter.Job);

			var jobNumber = parent.JobNumber;
			AssertEquals("Should return Parents JobNumber.", jobNumber, invoicingSupporter.JobNumber);
		}

		public virtual void TestOuterPackTotal()
		{
			var parent = GetNewBusinessObject();
			var invoicingSupporter = parent.InvoicingSupporter;

			AssertEquals("Empty by default", 0, invoicingSupporter.OuterPackTotal);
		}

		protected virtual JobHeader CreateJobHeader(IJobInvoicingPlugIn parent)
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = parent.PK;
			jobHeader.Parent = parent;
			jobHeader.JH_GB = GlbCompany.CurrentCompany.PK;

			return jobHeader;
		}

		public void TestJobIsDeletedWhenObjectIsCancelled()
		{
			if (typeof(ICancellable).IsAssignableFrom(ExpectedObjectType))
			{
				var bizO1 = GetNewBusinessObject();
				var job1 = CreateJobHeader(bizO1);

				var bizO2 = GetNewBusinessObject();
				var job2 = CreateJobHeader(bizO2);
				var charge = ((BusinessObjectCollection)job2["Charges"]).AddNew();
				charge.FillWithValidTestData();
				charge[JobChargeSchema.JR_OSSellAmt] = 15000m;

				Factory.Save();

				Assert(!((ICancellable)bizO1).IsCancelled);

				((ICancellable)bizO1).IsCancelled = true;
				((ICancellable)bizO2).IsCancelled = true;

				Assert(!job1.IsDeleted);

				Factory.Save();

				Assert(string.Format("{0} is expected to delete the Job on saving", ExpectedObjectType), job1.IsDeleted);
				Assert(!job2.IsDeleted);

				Assert(((ICancellable)bizO1).IsCancelled);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestParentDefaultChargeCostReferenceChanged()
		{
			var parent = GetNewBusinessObject();

			var supportsSettingCostRef = SetDefaultChargeCostReference(parent, "SupRef");
			var expectedCostRef = supportsSettingCostRef ? "SupRef" : "";

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = parent.PK;
			jobHeader.Parent = parent;
			jobHeader.JH_GB = GlbCompany.CurrentCompany.PK;

			var charge1 = Factory.New<JobCharge>();
			var charge2 = Factory.New<JobCharge>();
			charge1.JR_JH = jobHeader.PK;
			charge2.JR_JH = jobHeader.PK;
			jobHeader.LoadCharges_ForTestOnly();
			AssertEquals("Set to default.", expectedCostRef, charge1.JR_CostReference);
			AssertEquals("Set to default.", expectedCostRef, charge2.JR_CostReference);

			if (supportsSettingCostRef)
			{
				charge2.JR_CostReference = "DifRef";
				SetDefaultChargeCostReference(parent, "NewRef");
				AssertEquals("Should use IJobProviderForInvoicing to update to NewRef.", "NewRef", charge1.JR_CostReference);
				AssertEquals("Only update charges with the original Cost Reference", "DifRef", charge2.JR_CostReference);
			}
		}

		protected virtual bool SetDefaultChargeCostReference(IJobInvoicingPlugIn parent, ZString costReference)
		{
			return false;
		}

		public void TestJobInvoicingSecurity()
		{
			if (ExcludeFromTestBecauseNoBillingTab)
			{
				Assert(true);
			}
			else
			{
				IJobInvoicingSupporter jobInvoicingSupporter = GetNewObject();
				AssertNotEquals("JobInvoicingSecurity should not be Env.Security.None", Env.Security.None, jobInvoicingSupporter.JobInvoicingSecurity);
			}
		}

		[ExpectNoExceptions]
		public void TestSecurityHelperWhenReversing()
		{
			if (!ExcludeFromTestBecauseNoBillingTab)
			{
				IJobInvoicingSupporter jobInvoicingSupporter = GetNewObject();
				JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(jobInvoicingSupporter.JobInvoicingSecurity);

				if (securityHelper != null)
				{
					bool isAllowed = securityHelper.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).IsAllowed;
				}
			}
		}

		public void TestDeletingJobsWhenReactivatingPlugInData()
		{
			if (!ExcludeFromTestBecauseNoBillingTab)
			{
				IJobInvoicingPlugIn bo = GetNewBusinessObject();
				ICancellable boAsICancellable = bo as ICancellable;
				if (boAsICancellable != null)
				{
					boAsICancellable.IsCancelled = true;
					Factory.Save();

					var accounting = ObjectFactory.Get<IAccounting>();

					var jobBranchDefaultOrderRule = accounting.Registry.JobBranchDefaultOrderRule;
					var newJobBranchDefaultOrderRuleValue = Activator.CreateInstance(jobBranchDefaultOrderRule.Value.GetType());
					ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBlank", (ZShort)1);
					ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBranchRelatedToPortOrWarehouseBranch", (ZShort)0);
					ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBranchOfOrganisation", (ZShort)0);
					ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToLoginUserDefault", (ZShort)0);
					jobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, newJobBranchDefaultOrderRuleValue);

					JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
					jobHeader.Parent = bo;
					jobHeader.JH_GB = ZGuid.Empty;
					AssertEquals("Job should not be deleted", false, jobHeader.IsDeleted);

					boAsICancellable.IsCancelled = false;
					Factory.Save();
					AssertEquals("Job should be deleted", true, jobHeader.IsDeleted);
				}
				else
				{
					Assert(true);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestProperties()
		{
			IJobInvoicingSupporter jobInvoicingSupporter = GetNewObject();
			ZGuid guid = jobInvoicingSupporter.OverriddenDepartmentPK;
			OrgHeader orgHeader = jobInvoicingSupporter.Consignee;
			orgHeader = jobInvoicingSupporter.Consignor;
			orgHeader = jobInvoicingSupporter.SendingAgent;
			orgHeader = jobInvoicingSupporter.ReceivingAgent;
			orgHeader = jobInvoicingSupporter.GetDefaultDebtor(null);
			orgHeader = jobInvoicingSupporter.Broker;
			orgHeader = jobInvoicingSupporter.OverriddenDefaultLocalClient;
			bool boolean = jobInvoicingSupporter.IsDirectShipment;
			ZDecimal dec = jobInvoicingSupporter.ActualChargeable;
			ZString str = jobInvoicingSupporter.ActualChargeableUnit;
			str = jobInvoicingSupporter.ConsolType;
			RefCurrency currency = jobInvoicingSupporter.ConsolRateCurrency;
			dec = jobInvoicingSupporter.ConsolExchangeRate;
			dec = jobInvoicingSupporter.GetConsolExchangeRate(string.Empty);
			str = jobInvoicingSupporter.ConsolNumber;
			RefUNLOCO unLoco = jobInvoicingSupporter.Origin;
			unLoco = jobInvoicingSupporter.Destination;
			unLoco = jobInvoicingSupporter.GetTranshipmentPort(CostSell.Cost);
			unLoco = jobInvoicingSupporter.GetTranshipmentPort(CostSell.Revenue);
			str = jobInvoicingSupporter.TransportMode;
			str = jobInvoicingSupporter.ContainerMode;
			var pt = jobInvoicingSupporter.PaymentTerm;
			boolean = jobInvoicingSupporter.IsImport;
			boolean = jobInvoicingSupporter.IsExport;
			boolean = jobInvoicingSupporter.IsDomestic;
			boolean = jobInvoicingSupporter.IsPlugInReadOnly;
			str = jobInvoicingSupporter.ShipmentNumberOfColoadMaster;
			JobInvoicingConsumerType consumerType = jobInvoicingSupporter.ConsumerType;
			str = jobInvoicingSupporter.MasterBillNumber;
			str = jobInvoicingSupporter.HouseBillNumber;
			ZDateTime date = jobInvoicingSupporter.ATA;
			date = jobInvoicingSupporter.ATD;
			date = jobInvoicingSupporter.ETA;
			date = jobInvoicingSupporter.ETD;
			date = jobInvoicingSupporter.ArrivalAtLoadPort;
			date = jobInvoicingSupporter.EstimatedArrivalAtLoadPort;
			dec = jobInvoicingSupporter.ActualWeight;
			str = jobInvoicingSupporter.ActualWeightUnit;
			dec = jobInvoicingSupporter.ActualVolume;
			str = jobInvoicingSupporter.ActualVolumeUnit;
			dec = jobInvoicingSupporter.ActualLoadingMeters;
			boolean = jobInvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob;
			GlbBranch branch = jobInvoicingSupporter.OperationsBranch;
			SecurityCheckpoint securityCheckpoint = jobInvoicingSupporter.AuditSecurity;
			securityCheckpoint = jobInvoicingSupporter.JobInvoicingSecurity;
			date = jobInvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate);
			date = jobInvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, Constants.FreightShipmentDirection.Code.Import);
			date = jobInvoicingSupporter.GetCustomsClearanceDate();
			securityCheckpoint = jobInvoicingSupporter.EditSecurityCheckpoint;
			str = jobInvoicingSupporter.EditSecurityMessage;
			boolean = jobInvoicingSupporter.EditSecurityLock;
			jobInvoicingSupporter.PostedStateChanged();
			int i = jobInvoicingSupporter.ContainerCount;
			dec = jobInvoicingSupporter.TEUCount;
			i = jobInvoicingSupporter.OuterPackTotal;
			str = jobInvoicingSupporter.DefaultChargeGroup;
			boolean = jobInvoicingSupporter.IncludeInConsolCosting(false);
			jobInvoicingSupporter.SetDefaultsForNewCharge(Factory.New<JobCharge>());
			var job = jobInvoicingSupporter.Job;

			string s = jobInvoicingSupporter.GetReasonNotToAllowPosting();
			string s2 = jobInvoicingSupporter.GetReasonNotToAllowAutoRate();
		}

		#endregion
	}
}
