using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgAddressEmbeddedModulePopup : EmbeddedModulePopup, IEmbeddedModulePopupOKButtonStrategy
	{
		public OrgAddressEmbeddedModulePopup()
		{
			InitializeComponent();
		}

		public OrgAddressEmbeddedModulePopup(ZFilterModule module, ZGuid parentOrgPK, ZArchitecture.Business.AddressType? defaultAddressType) : base(module)
		{
			InitializeComponent();
			EmbeddedModulePopupOKButtonStrategy = this;
			RequireAtLeastOneItemToBeSelected = true;
			BindConfigFilter(parentOrgPK, defaultAddressType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description for dev")]
		void BindConfigFilter(ZGuid parentOrgPK, ZArchitecture.Business.AddressType? defaultAddressType)
		{
			const string typeFilterDescription = "Type";
			var filterStripControl = Module.EmbeddedControl as ZFilterStripControl;
			var filterBizO = Module.FilterBusinessObject;
			ModuleTextFilter addressTypeFilter = null;
			ModuleGuidFilter orgFilter = null;

			Load += delegate
			{
				try
				{
					addressTypeFilter = Module.FilterBusinessObject.ModuleFilters[typeFilterDescription] as ModuleTextFilter;
					addressTypeFilter.Visibility = FilterVisibility.AlwaysVisible;

					filterStripControl.ResetFilterStrips();

					orgFilter = Module.FilterBusinessObject.ModuleFilters["Organisation"] as ModuleGuidFilter;
					orgFilter.Property = parentOrgPK;
					orgFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

					addressTypeFilter.Property = defaultAddressType.ToString();

					if (defaultAddressType == ZArchitecture.Business.AddressType.DLV || defaultAddressType == ZArchitecture.Business.AddressType.PIC)
					{
						var filterStrip = Module.FilterBusinessObject.FilterStrips.LastOrDefault() as FilterStrip;
						if (filterStrip != null)
						{
							filterStrip.FilterDescription = typeFilterDescription;
							var filter = filterStrip.CurrentModuleFilter as ModuleTextFilter;
							if (filter != null)
							{
								filter.Property = nameof(ZArchitecture.Business.AddressType.PAD);
								Module.FilterBusinessObject.FilterStrips.OfType<FilterStrip>()
									.Where(f => f.FilterDescription == typeFilterDescription)
									.ForEach(f => f.OrCategory = FilterOrCategory.Red);
							}
						}
					}

					filterStripControl.FirePerformSearch();
				}
				catch (NullReferenceException)
				{
					var errorMessage = $@"NullReferenceException occured on OrgAddressEmbeddedModulePopup.
filterStripControl: {filterStripControl}
addressTypeFilter: {addressTypeFilter}
orgFilter: {orgFilter}
filterBizO: {filterBizO}
type of filterBizO.ModuleFilters[typeFilterDescription]: {filterBizO?.ModuleFilters[typeFilterDescription]?.GetType()}
type of filterBizO.ModuleFilters[""Organisation""]: {filterBizO?.ModuleFilters["Organisation"]?.GetType()}
";
					ErrorReporter.ReportOnce("NullReferenceException_OrgAddressEmbeddedModulePopup", errorMessage);
					throw;
				}
			};
		}

		#region IEmbeddedModulePopupOKButtonStrategy
		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			if (selectedBusinessObject != null && selectedBusinessObject.Length > 0)
			{
				HandleSelection(selectedBusinessObject);
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}
		#endregion
	}
}
