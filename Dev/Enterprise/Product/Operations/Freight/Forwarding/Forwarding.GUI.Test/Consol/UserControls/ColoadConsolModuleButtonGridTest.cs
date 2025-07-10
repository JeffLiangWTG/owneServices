using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ColoadConsolModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestIsModuleButtonGrid()
		{
			Assert(typeof(ZModuleButtonGrid).IsAssignableFrom(typeof(ColoadConsolModuleButtonGrid)));
		}

		public void TestNewButtonIsDisabled()
		{
			using (var moduleButtonGrid = new ColoadConsolModuleButtonGrid())
			{
				Assert(!moduleButtonGrid.ShowNewButton);
			}
		}

		public void TestGridContainsEssentialColumns()
		{
			var essentialColumnNames = new string[]
			{
				JobConsolSchema.Constants.JK_UniqueConsignRef,
				JobConsolSchema.Constants.JK_MasterBillNum,
				JobConsolSchema.Constants.JK_RL_NKLoadPort,
				JobConsolSchema.Constants.JK_RL_NKDischargePort,
				ForwardingConsol.Schema.JK_JX_JA_E_DEP,
				ForwardingConsol.Schema.JK_JX_JB_E_ARV,
				ForwardingConsol.Schema.JK_JX_JV_VoyageFlight,
				ForwardingConsol.Schema.JK_TotalShipmentWeight,
				ForwardingConsol.Schema.JK_TotalShipmentWeightUnit,
				ForwardingConsol.Schema.JK_TotalShipmentVolume,
				ForwardingConsol.Schema.JK_TotalShipmentVolumeUnit,
				ForwardingConsol.Schema.JK_AWBServiceLevel,
				ForwardingConsol.Schema.JK_IsHazardous,
				ForwardingConsol.Schema.JK_RequiredTemperatureMinimum,
				ForwardingConsol.Schema.JK_RequiredTemperatureMaximum,
				ForwardingConsol.Schema.JK_RequiredTemperatureUnit,
				"JK_Calc_ContainerTypesSummary"
			};

			using (var moduleButtonGrid = new ColoadConsolModuleButtonGrid())
			{
				var allColumnNames = moduleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(column => column.ColumnName);
				AssertContainsExactElementsInAnyOrder(essentialColumnNames, allColumnNames.Intersect(essentialColumnNames));
			}
		}

		public void TestAttachButton_Click_AWBMaster_CutOffDateNotPassed()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(1);

			AssertGreaterThan(consol.JK_ConsolCutOffDate, ZDateTime.UtcNow);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				button.PerformClick();
				Assert("No error message is shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
				button.PerformClick();
				Assert("No error message is shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestAttachButton_Click_AWBMaster_CutOffDatePassed()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(-1);

			AssertLessThan(consol.JK_ConsolCutOffDate, ZDateTime.UtcNow);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				button.PerformClick();
				Assert("No error message is shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
				button.PerformClick();
				Assert("Error message is shown", !UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#region Implementation

		class ConsolFormForTest : ZForm
		{
			public ConsolFormForTest(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ColoadConsolModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ColoadConsolModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo.ColumnName = "JK_UniqueConsignRef";

				Grid.BindToGridList = "ColoadConsols";
				Grid.BindToFindBoxList = "ColoadConsols_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
				Controls.Add(Grid);
			}
		}

		class ColoadConsolModuleButtonGridForTest : ColoadConsolModuleButtonGrid
		{
			public ColoadConsolModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new ColoadConsolModuleButtonGridAttacherForTest(ParentConsol, destinationCollection, findBoxList, moduleID));
			}

			public new IBusinessObjectCollection Collection => base.Collection;
		}

		class ColoadConsolModuleButtonGridAttacherForTest : ZRecordAttacher
		{
			public ColoadConsolModuleButtonGridAttacherForTest(ForwardingConsol consol, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID)
			{
				Consol = consol;
			}

			public ForwardingConsol Consol { get; private set; }
		}

		#endregion
	}
}
