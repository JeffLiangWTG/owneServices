
namespace Enterprise.MasterFiles.GUI
{
	using System.Windows.Forms;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI.Grid.Internal;

	public class TriggerConditionValueControl : ZMultiCombinationControl
	{
		public TriggerConditionValueControl(string eventCode)
		{
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			return typeToCreate == FieldType.TextCodeFindBox ? new TriggerConditionValueLookupControl(eventCode) : base.CreateGridControl(typeToCreate);
		}

		protected override void EditCore(CurrencyManager source, string mappingName)
		{
			if (!(CurrentEditor is TriggerConditionValueLookupControl))
			{
				base.EditCore(source, mappingName);
			}
		}
	}
}
