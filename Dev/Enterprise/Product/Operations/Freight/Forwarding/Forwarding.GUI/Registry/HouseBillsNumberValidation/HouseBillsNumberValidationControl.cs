using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class HouseBillsNumberValidationControl : RegistryZUserControl
	{
		public HouseBillsNumberValidationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			HouseBillsNumberValidationGrid.ReadOnly = readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!firstBound)
			{
				AddConditionColumn();
				firstBound = true;
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		bool firstBound;

		void AddConditionColumn()
		{
			var conditionExpressionColumn = new ZMacrosFindBoxColumnStyleInfo();
			conditionExpressionColumn.Caption = Res.GetString("f3d0158e-b8dc-4cdb-b012-f3ebc94cdf85", "User Defined Condition");
			conditionExpressionColumn.ColumnName = HouseBillsNumberValidation.Schema.UserDefinedCondition;
			ControlDpiScalingHelper.SetWidth(ref conditionExpressionColumn, 300, true);
			conditionExpressionColumn.AllowMultipleMacroses = true;
			conditionExpressionColumn.UsePredefinedRoots = true;
			conditionExpressionColumn.IsUsedForExpressions = true;

			conditionExpressionColumn.RootTypes = new[] { GenericWrapperLoader.GetFromDataContext(Constants.DataContext.GenericFreightJobFromShipment).GetWrapperType() };

			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(conditionExpressionColumn);
		}
	}
}
