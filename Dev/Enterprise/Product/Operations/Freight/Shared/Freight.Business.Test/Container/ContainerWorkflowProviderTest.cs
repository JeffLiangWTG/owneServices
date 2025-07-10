using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonContainer))]
	sealed class ContainerWorkflowProviderTest : WorkflowProviderTest<CommonContainer, ContainerProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria_ForLoadPort()
		{
			Container.Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_RL_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "NZAKL", "AUSYD", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDischargePort()
		{
			Container.Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_RL_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "NZAKL", "AUSYD", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "C01";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "B01";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B02";

			ProcessTaskTemplate.P0_GC = company.PK;
			ProcessTaskTemplate.P0_GB = branch1.PK;
			ProcessTaskTemplate.P0_ProcessType = ((IWorkflowProvider)Container).WorkflowType;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Container.Factory.Save();

				Container.WorkflowItems.Tasks.RemoveAndDeleteAll();
				Container.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(1, Container.WorkflowItems.Tasks.Count);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Container.Factory.Save();

				Container.WorkflowItems.Tasks.RemoveAndDeleteAll();
				Container.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(0, Container.WorkflowItems.Tasks.Count);
			}
		}

		public void TestGetTemplateFilterCriteria_ForTransportMode()
		{
			Container.Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(Consol.JK_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, "SEA", "ROA", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForContainerMode()
		{
			Container.Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(Container.JC_ContainerModeInfo, ProcessTaskTemplate.P0_SubType2Info, "FCL", "LCL", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForCarrier()
		{
			Container.Factory.Save();

			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			AssertGetTemplateFilterCriteria(value =>
			{
				Consol.JK_OA_ShippingLineAddress = Factory.Load<OrgHeader>(value).Addresses[0].PK;
			}, ProcessTaskTemplate.P0_OH_ClientInfo, org1.PK, org2.PK, ZGuid.Empty);
		}

		#region Implementation

		CommonContainer Container
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.ContainerWorkflowDescriptorCode; }
		}

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
					consol.JK_RL_NKLoadPort = "NZAKL";
					consol.JK_RL_NKDischargePort = "AUSYD";
				}

				return consol;
			}
		}
		CommonConsol consol;

		protected override CommonContainer GetNewBusinessObject(BusinessObjectFactory factory)
		{
			CommonContainer container = base.GetNewBusinessObject(factory);
			Consol.Containers.Add(container);
			return container;
		}

		#endregion
	}
}
