using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ProfitShareRuleModuleButtonGrid))]
	public class ProfitShareRuleModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestIsModuleButtonGrid()
		{
			Assert(typeof(ZModuleButtonGrid).IsAssignableFrom(typeof(ProfitShareRuleModuleButtonGrid)));
		}

		public void TestNewButtonIsShown()
		{
			using (var moduleButtonGrid = new ProfitShareRuleModuleButtonGrid())
			{
				Assert(moduleButtonGrid.ShowNewButton);
			}
		}

		public void TestEditButtonIsShown()
		{
			using (var moduleButtonGrid = new ProfitShareRuleModuleButtonGrid())
			{
				Assert(moduleButtonGrid.ShowEditButton);
			}
		}

		[CodeAlive("Used in later workflows WI00500713")]
		class MockProfitShareRulesForm : ZForm
		{
			public MockProfitShareRulesForm(ForwardingProfitShareRedistribution businessEntity)
				: base(businessEntity)
			{
			}

			public ProfitShareRuleModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ProfitShareRuleModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JK_UniqueConsignRef";

				Grid.BindToGridList = "Consols";
				Grid.BindToFindBoxList = "Consols_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}
	}

	class ProfitShareRuleModuleButtonGridForTest : ProfitShareRuleModuleButtonGrid
	{
		public ProfitShareRuleModuleButtonGridAttacherForTest Attacher { get; set; }

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return Attacher ?? (Attacher = new ProfitShareRuleModuleButtonGridAttacherForTest(destinationCollection, findBoxList, moduleID));
		}
	}

	class ProfitShareRuleModuleButtonGridAttacherForTest : ProfitShareRuleModuleButtonGrid.ProfitShareRulesGridAttacher
	{
		public ProfitShareRuleModuleButtonGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
		}
	}
}
