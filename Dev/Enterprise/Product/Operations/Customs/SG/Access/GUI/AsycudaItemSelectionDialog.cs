using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI
{
	public partial class AsycudaItemSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		public AsycudaItemSelectionDialog(MessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			InitializeComponent();
			zPanelCycleFileds.Visible = !IsBillsForDeclaration && BusinessEntity.RequiresCycleFields;
		}

		public new MessageChooser BusinessEntity => (MessageChooser)base.BusinessEntity;

		protected override void OnSent()
		{
			BusinessEntity.KeepCycleFieldsConsistentIfNeeded();
		}

		protected override bool HasMessageErrorsOnSelectedItems()
		{
			var messageChooser = BusinessEntity;

			return messageChooser.CycleDateInfo.HasMessageErrors() || messageChooser.CycleNumberInfo.HasMessageErrors() || base.HasMessageErrorsOnSelectedItems();
		}
	}
}
