using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ProfitShareConsolWrapperModuleButtonGrid))]
	public class ProfitShareConsolWrapperModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestIsModuleButtonGrid()
		{
			Assert(typeof(ZModuleButtonGrid).IsAssignableFrom(typeof(ProfitShareConsolWrapperModuleButtonGrid)));
		}

		public void TestNewButtonIsNotShown()
		{
			using (var moduleButtonGrid = new ProfitShareConsolWrapperModuleButtonGrid())
			{
				Assert(!moduleButtonGrid.ShowNewButton);
			}
		}

		public void TestEditButtonIsNotShown()
		{
			using (var moduleButtonGrid = new ProfitShareConsolWrapperModuleButtonGrid())
			{
				Assert(!moduleButtonGrid.ShowEditButton);
			}
		}

		[CodeAlive("Used in later workflows WI00500713")]
		class MockConsolProfitShareRedistributionForm : ZForm
		{
			public MockConsolProfitShareRedistributionForm(ForwardingProfitShareRedistribution businessEntity)
				: base(businessEntity)
			{
			}

			public ProfitShareConsolWrapperModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ProfitShareConsolWrapperModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "O4_JobType";

				Grid.BindToGridList = "ProfitShareRules";
				Grid.BindToFindBoxList = "OrgAgentRelationship_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}
	}

	public class ProfitShareConsolWrapperModuleButtonGridAttacherTest : TestCaseWithFactory
	{
		public void TestAttachCore_UseDestinationFactory()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			factory.Save();

			var anotherfactory = new BusinessObjectFactory();
			var profitShareRedistribution = anotherfactory.New<ForwardingProfitShareRedistribution>();

			var attacher = new ProfitShareConsolWrapperModuleButtonGridAttacherForTest(profitShareRedistribution.Consols, profitShareRedistribution.Consols_List, ModuleIDs.JobConsol);
			var list = new List<BusinessObject>();
			bool result = attacher.AttachCoreForTesting(consol, list);

			Assert(result);
			AssertEquals(1, list.Count);
			AssertEquals(profitShareRedistribution.Factory, list[0].Factory);
		}
	}

	class ProfitShareConsolWrapperModuleButtonGridForTest : ProfitShareConsolWrapperModuleButtonGrid
	{
		public ProfitShareConsolWrapperModuleButtonGridAttacherForTest Attacher { get; set; }

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return Attacher ?? (Attacher = new ProfitShareConsolWrapperModuleButtonGridAttacherForTest(destinationCollection, findBoxList, moduleID));
		}
	}

	class ProfitShareConsolWrapperModuleButtonGridAttacherForTest : ProfitShareConsolWrapperModuleButtonGrid.GatewayConsolProfitShareGridAttacher
	{
		public ProfitShareConsolWrapperModuleButtonGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
		}

		public bool AttachCoreForTesting(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
