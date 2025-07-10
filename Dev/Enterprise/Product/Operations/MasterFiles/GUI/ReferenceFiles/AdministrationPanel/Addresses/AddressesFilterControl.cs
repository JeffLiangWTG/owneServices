using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressesFilterControl : ZFilterStripControl
	{
		public AddressesFilterControl()
		{
			InitializeComponent();
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public AddressesFilterControl(MDMAdminPanelAddressCollection collection, AddressesFilterBusinessObject bizo)
			: base(collection, bizo)
		{
			InitializeComponent();
			ToolTipService.SetToolTip(this.BackgroundValidationStatusIcon, Res.GetString("EAEEA9E6-1199-49A2-A78D-2F3551926BA9", "The background validation is running..."));
		}

		protected override void ClearStrips()
		{
			base.ClearStrips();
			EnsureDefaultValidationStatusFilter();
		}

		protected override void ResetLayout()
		{
			base.ResetLayout();
			EnsureDefaultValidationStatusFilter();
		}

		void EnsureDefaultValidationStatusFilter()
		{
			var validationStatusFilter = FilterBusinessObject.ActiveModuleFilters.First(mf => mf.FilterColumn == MDMAdminPanelAddressViewSchema.MDM_ValidationStatus) as ModuleTextFilter;
			if (validationStatusFilter != null)
			{
				validationStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				validationStatusFilter.Property = AddressValidationStatus.Invalid;
			}
		}

		List<ZGuid> ExcludeList
		{
			get
			{
				List<ZGuid> list = new List<ZGuid>();
				ParentControl?.Manager.AdminPanelProcessedAddressCollection.ForEach(x => { list.Add(x.PK); });
				return list;
			}
		}

		internal AddressUserControl ParentControl { get; set; }

		public void LoadData()
		{
			try
			{
				Grid.SuspendLayout();
				var message = "";
				var meetingFilterMessage = "";
				bool meetingFilter = false;
				ZQuery filter;
				if (ExcludeList.Count != 0)
				{
					filter = GetFilter(false);
					Collection.Load(filter);
					meetingFilter = Collection.Count > 0;
					meetingFilterMessage = meetingFilter ? "\r\n" + Res.GetString("B810E320-0D69-4BC8-8A50-7692CEA4A4A9", "Some records meeting your filter criteria exist in \"Records Modified in This Session\" grid.") : "";
				}
				filter = GetFilter(true);
				CurrentAddressStatusForSerching = ((ModuleTextFilter)FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus]).Property;
				Collection.SwapFactoryAndRemoveAll(new BusinessObjectFactory());

				using (Collection.SuspendListChanged())
				{
					Collection.Load(filter);
					var showCount = Collection.Count;

					if (Collection.Count == MaxNumberOfRecordsToShowInDisplayGrids)
					{
						RemoveExtraItems();
						message = Res.GetString("D2CF228B-1093-412B-AA92-F1D6CE63C691",
							"Found at least {0} records.{1}\r\nOnly showing the top {2} records.",
							showCount, meetingFilterMessage, Collection.Count);
					}
					else if (Collection.Count > 0)
					{
						if (Collection.Count > MaximumNumberOfAddressesForMDMAddressGrid)
						{
							RemoveExtraItems();
							message = Res.GetString("B810E320-0D69-4BC8-8A50-7692CEA4A4D9",
								"Found {0} records.{1}\r\nOnly showing the top {2} records.",
								showCount, meetingFilterMessage, Collection.Count);
						}
						else if (meetingFilter)
						{
							message = meetingFilterMessage.Replace("\r\n", "");
						}
					}
					else
					{
						message = Res.GetString("848C041F-DA05-45E7-8AA2-4F20216A20D6", "Found no records.{0}", meetingFilterMessage);
					}

					UpdateNumberLoadedMessage(message: null, numberOfRecordsFound: showCount, shouldShowNumberLoadedMessageBox: false);

					if (!string.IsNullOrEmpty(message))
					{
						Globals.Message.ShowInformation(message);
					}
				}
			}
			finally
			{
				Grid.ResumeLayout(true);
			}
		}

		void RemoveExtraItems()
		{
			var itemNeedToRemoveCount = Collection.Count - MaximumNumberOfAddressesForMDMAddressGrid;
			if (itemNeedToRemoveCount > 0)
			{
				for (int index = 0; index < itemNeedToRemoveCount; index++)
				{
					Collection.Remove(Collection[Collection.Count - 1]);
				}
			}
		}

		ZQuery GetFilter(bool excludeProcessedRecord)
		{
			var filter = FilterBusinessObject.Filter;
			filter.AddToFilter(MDMAdminPanelAddressViewSchema.PK, excludeProcessedRecord ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, ExcludeList.ToArray());
			filter.MaximumRows = excludeProcessedRecord ? MaxNumberOfRecordsToShowInDisplayGrids : 1;

			if (Globals.IsTest)
			{
				filter.OrderBy = MDMAdminPanelAddressViewSchema.Constants.MDM_AddressCode;
			}

			return filter;
		}

		MDMAdminPanelAddressCollection Collection => GridCollection as MDMAdminPanelAddressCollection;

		public string CurrentAddressStatusForSerching { get; private set; }

		int MaxNumberOfRecordsToShowInDisplayGrids => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

		int MaximumNumberOfAddressesForMDMAddressGrid => SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.Value;

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new AddressFilterStrip();
		}
	}
}
