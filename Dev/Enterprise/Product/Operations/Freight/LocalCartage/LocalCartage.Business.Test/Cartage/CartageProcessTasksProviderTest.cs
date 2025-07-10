using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartage))]
	class CartageProcessTasksProviderTest : WorkflowProviderTest<CommonCartage, CartageProcessTaskCollection>
	{
		public void TestGetTemplateSelectionCriteria_ForBranch()
		{
			Cartage.Factory.Save();
			AssertGetTemplateFilterCriteria(Cartage.JJ_GBInfo, ProcessTaskTemplate.P0_GBInfo, GlbBranch.CurrentBranch.PK, Branch.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForCartageType()
		{
			Cartage.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Cartage.JJ_E3_NKJobTypeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.CartageJobType.NEW_AirExport, Core.Constants.CartageJobType.FCL, ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForClient()
		{
			Cartage.Factory.Save();
			AssertGetTemplateFilterCriteria(delegate(ZGuid value)
			{
				Cartage.LocalClientAddressPK = Factory.Load<OrgHeader>(value).Addresses[0].PK;
			}, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CommonCartage>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CommonCartage>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		protected override CommonCartage GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			return cartage;
		}

		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return JobInvoicingConsumerTypes.LocalCartage.Code;
			}
		}

		CommonCartage Cartage
		{
			get
			{
				return BusinessObject;
			}
		}
	}
}
