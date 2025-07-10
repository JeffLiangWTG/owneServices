using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsPackingUserControl : BasePackingControl
	{
		public BaseCustomsPackingUserControl()
		{
			InitializeComponent();
			InitializeLayoutPackingDetailsGrid();
		}

		protected virtual void InitializeLayoutPackingDetailsGrid()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (DataSource != null)
				{
					var declaration = (BaseJobDeclaration)DataSource;
					declaration.ParentPackageColumnSupportedChanged -= Declaration_ParentPackageColumnSupportedChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			var isContainerNoOrEquipmentNoAvailable = false;
			var isParentPackageAvailable = false;

			var declaration = JobDeclaration;
			if (declaration != null)
			{
				isContainerNoOrEquipmentNoAvailable = declaration.ContainersRequired || declaration.EquipmentsRequired;
				isParentPackageAvailable = declaration.SupportsParentPackage;
			}

			PackingDetailsGrid.SetAvailability(isContainerNoOrEquipmentNoAvailable, BasePackage.Schema.CW_ContainerNoOrEquipmentNo);
			if (isContainerNoOrEquipmentNoAvailable)
			{
				PackingDetailsGrid.SetColumnCaption(BasePackage.Schema.CW_ContainerNoOrEquipmentNo, declaration.ContainerOrEquipmentCaption);
			}

			PackingDetailsGrid.SetAvailability(isParentPackageAvailable, BasePackage.Schema.CW_CW_Parent);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				var declaration = (BaseJobDeclaration)dataSource;
				declaration.ParentPackageColumnSupportedChanged += Declaration_ParentPackageColumnSupportedChanged;
			}
		}

		void Declaration_ParentPackageColumnSupportedChanged(object sender, System.EventArgs e)
		{
			ChangeGridColumnsVisibility();
		}
	}
}
