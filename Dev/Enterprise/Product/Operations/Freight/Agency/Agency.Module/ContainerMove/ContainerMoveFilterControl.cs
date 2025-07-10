namespace Enterprise.Freight.Agency.Module
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.GUI;

	public partial class ContainerMoveFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public ContainerMoveFilterControl()
		{
			InitializeComponent();
		}

		public ContainerMoveFilterControl(IBusinessObjectCollection collection, ContainerMoveFilterStrip strip)
			: base(collection, strip)
		{
			InitializeComponent();

			InitializeExtraColumns();
		}

		/// <summary>
		///     Adds an extra columns into the grid. Use it if columns couldn't be added via designer due some reasons.
		/// </summary>
		void InitializeExtraColumns()
		{
			var createdTimeLocalColumn = new ZArchitecture.ZDateEditColumnStyleInfo();
			createdTimeLocalColumn.ColumnName = "CreatedTimeLocal";
			createdTimeLocalColumn.GroupName = FilterStripAuditDetails.AuditDetailsGroupText;
			createdTimeLocalColumn.IsVisible = false;

			this.grid.ColumnStyles.Add(createdTimeLocalColumn);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CustomModuleFilterControlKludge();
		}
	}
}


