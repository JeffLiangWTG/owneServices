using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCarrierNamedAccountFilterStripCommonControl : ZFilterStripCommonControl
	{
		public OrgCarrierNamedAccountFilterStripCommonControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBO)
			: base(filterBO)
		{
			if (gridCollection is not OrgCarrierNamedAccountCollection orgCarrierNamedAccountCollection)
			{
				throw new ArgumentException("The provided GridCollection must be of type OrgCarrierNamedAccountCollection.", nameof(gridCollection));
			}

			if (filterBO is not UrsNamedAccountFilterBusinessObject ursNamedAccountFilterBusinessObject)
			{
				throw new ArgumentException("The provided FilterBusinessObject must be of type OrgCarrierNamedAccountFilterBusinessObject.", nameof(filterBO));
			}

			Collection = orgCarrierNamedAccountCollection;
			InitializeComponent();
			Hook();
		}

		void Hook()
		{
			PerformSearch += OrgCarrierNamedAccountFilterStripControl_PerformSearch;
		}

		void Unhook()
		{
			PerformSearch -= OrgCarrierNamedAccountFilterStripControl_PerformSearch;
		}

		void OrgCarrierNamedAccountFilterStripControl_PerformSearch(object sender, EventArgs e)
		{
			if (Collection.HasChanges)
			{
				Globals.Message.ShowWarning(Res.GetString("24f025fc-7467-4a21-b961-57e27f2508b1", "You have unsaved changes. Please save your changes and try again."));
				return;
			}

			var filter = FilterBusinessObject.Filter;
			var registryMaxNumberOfRecords = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids;
			var maxNumberOfRecords = registryMaxNumberOfRecords.Value;
			filter.MaximumRows = maxNumberOfRecords + 1;
			Collection.Load(filter);

			if (!hasMessageBeenShownMaxNumberOfRecords && Collection.Count > maxNumberOfRecords)
			{
				Collection.Remove(Collection[Collection.Count - 1]);
				hasMessageBeenShownMaxNumberOfRecords = true;
				Globals.Message.ShowInformation(
					Res.GetString(
						"4a569cdb-071b-4460-a313-455f20f3a46e",
						"Too many records to display. Only the first {0:G} records have been loaded. Configurable in registry {1}",
						maxNumberOfRecords,
						registryMaxNumberOfRecords.Location()));
			}
		}

		bool hasMessageBeenShownMaxNumberOfRecords;

		public OrgCarrierNamedAccountCollection Collection { get; }

		#region Overrides

		public override IBusinessObjectCollection GridCollection => Collection;

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return new ZFilterGrid();
		}

		protected override void InitialiseGridCore()
		{
			BindingSource.SetBindingMember(Grid, "CarrierNamedAccounts");

			Grid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCodeFindBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1f84e2e1-f55a-4ff1-818a-43492112eb52", "Foreign Name"),
					ColumnName = "ONA_ForeignName",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
					IsMandatory = true,
					ModuleID = ModuleIDs.UrsNamedAccount,
				},
				new ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("59fd7fe2-5a76-4318-8f3e-ddc794c0fe98", "Organization"),
					ColumnName = "ONA_OH_Organization",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("361a6c67-c885-4982-af3c-844bc2438b95", "Organization Name"),
					ColumnName = "Organization+OH_FullName",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(300),
					IsReadOnly = true,
				},
			});

			Grid.ReadOnly = false;
			Grid.GridId = "21f215f5-0338-4a43-9a01-d9d9ddbbb7d2";
		}

		protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(92);

		protected override bool CanSaveColumnLayouts => false;

		protected override bool ShouldAddEmptyFilterStripOnReset => true;

		#endregion
	}
}
