using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class UNDGDataItemFormManager : MultipleItemFormManager<IUNDGDataItemProvider>
	{
		public UNDGDataItemFormManager(ZGrid grid, string bindingPrefix = "", UNDGDataItemFormManagerConfig? config = null)
			: base(grid, bindingPrefix)
		{
			if (config is null)
			{
				config = UNDGDataItemFormManagerConfig.Default();
			}

			this.config = (UNDGDataItemFormManagerConfig)config;
			columnsToUpdate = new List<ZString>();
			undgSubstanceManagerName = this.config.IsIMOOnly ? nameof(UNDGDataItemCollection.UNDGSubstanceManager) : nameof(UNDGDataItemCollection.UNDGSubstanceManagerGuid);
			undgSubstanceCollectionName = this.config.IsIMOOnly ? nameof(UNDGDataItemCollection.UNDGSubstances) : nameof(UNDGDataItemCollection.AllUNDGSubstances);
		}

		readonly UNDGDataItemFormManagerConfig config;

		readonly List<ZString> columnsToUpdate;

		readonly string undgSubstanceManagerName;
		readonly string undgSubstanceCollectionName;

		public static class ColumnNames
		{
			public const string UNDGSubstanceManagerValue = "UNDGs+UNDGSubstanceManager+Value";
			public const string UNDGClassManagerValue = "UNDGs+UNDGClassManager+Value";
		}

		protected override ZString LinkLabelText
		{
			get { return Res.GetString("62C69C7B-5649-44df-BC5D-7C73C15F9111", "Multiple Dangerous Goods Details..."); }
		}

		protected override ZString[] ColumnsToUpdate
		{
			get { return columnsToUpdate.ToArray(); }
		}

		protected override void AddCountChangedEventHandlerToItemsCollection(IUNDGDataItemProvider parent, EventHandler eventHandler)
		{
			if (parent != null)
			{
				parent.UNDGs.CountChanged -= eventHandler;
				parent.UNDGs.CountChanged += eventHandler;
			}
		}

		public Func<ZGridColumnInfo, bool> ShouldHidePredicate;

		protected override int CollectionCount(IUNDGDataItemProvider parent)
		{
			return parent.UNDGs.Count;
		}

		protected override void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus)
		{
			AddColumnCore(gridToAddTo, bindingPrefixWithPlus, undgSubstanceManagerName, Res.GetData("a06042ad-2cee-49b8-8dd2-f90459e495f0", "DG", "DG Subs", "DG Substance", "Dangerous Goods Substance"), undgSubstanceCollectionName);
			AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGClassManager", Res.GetData("a8195876-2709-4f66-a6ff-25e2c9197bac", "DG Class"), "DGClassList");

			if (!config.ShouldHideSubstanceProperties)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGFlashPointManager", Res.GetData("a06042ad-2cee-49b8-8dd2-f90459e495f1", "Flash", "Flash Point", "DG Flash Point", "Dangerous Goods Flash Point"));
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGContactManager", Res.GetData("a06042ad-2cee-49b8-8dd2-f90459e495f2", "Contact", "DG Contact", "Dangerous Goods Contact"), (NoResString)"Contacts", 180);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGMarinePollutantManager", Res.GetData("41eacf3c-5e25-45c8-bc3f-8ef8a971fbdd", "Marine Pollutant"), null, 102);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGTechnicalNameManager", Res.GetData("b405a7a1-87a8-4b9d-ba44-4e5270e37e20", "Technical Name"), null, 150);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGWeightManager", Res.GetData("afacc2f9-a5e0-4681-b9b7-8c5df907a4a4", "DG Weight"), null, 77);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGWeightUnitManager", Res.GetData("18596f55-91f2-4ff6-bbd6-1450f73c24df", "DG UW", "DG Weight Units", "Units of Weight"), "WeightUnits", 58);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGVolumeManager", Res.GetData("abfcb938-1d6a-482b-a122-a3264f58c44b", "DG Volume"), null, 77);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGVolumeUnitManager", Res.GetData("2d79d49e-6e5e-476a-95b5-067d60fde828", "DG UV", "Volume Units", "Units of Volume"), "VolumeUnits", 58);
			}

			if (!config.ShouldHideUNDGProperties)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGHazardousWasteCodeManager", Res.GetData("e020300f-a598-46c3-ae18-861117900bde", "DG WC", "Waste Code", "Hazardous Waste Code"), null, 140);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGSpecialPermitIssueDateManager", Res.GetData("5e5cafd3-6d22-46ad-8227-003f1bfdd7de", "Permit Issued", "Special Permit Issue Date"), null, 140);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGSpecialPermitNumberManager", Res.GetData("21c2066e-3499-484c-93ec-8ac3d61899c0", "Permit No.", "Special Permit Number"), null, 140);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGIsSalvagePackagingManager", Res.GetData("babe94d2-6596-4a90-a351-12f05b2174aa", "Salvage Packaging"), null, 140);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGIsResidueLastContainedManager", Res.GetData("38de5726-d3f0-4fbb-ac06-7a71a0daa243", "Residue Last Contained"), null, 140);
			}

			if (!config.ShouldHideOverpack)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGHasOverpackManager", Res.GetData("34df24ca-e571-4742-9ea2-e33973d33182", "Has Overpack"), null, 60);
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGOverpackIDManager", Res.GetData("af43c2e8-cb8f-4814-b91e-4194965f657f", "Overpack ID"), null, 160);
			}

			if (!config.ShouldHideProperShippingName)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGProperShippingNameManager", Res.GetData("ad22f97c-c290-44e2-a409-964753dacef70", "Proper Shipping name"), null, 150);
			}

			if (!config.ShouldHidePSAGroup)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGPSAGroupsManager", Res.GetData("6b28d81e-751b-6c8b-4df9-da06bd385483", "PSA Group"), null, 140);
			}

			if (!config.ShouldHideLimitedQuantity)
			{
				AddColumnCore(gridToAddTo, bindingPrefixWithPlus, "UNDGIsLimitedQuantityManager", Res.GetData("cbf0c048-dc2f-4d51-b75f-0f7551770018", "Limited Quantity"), null, 140);
			}
		}

		void AddColumnCore(ZGrid grid, string bindingPrefixWithPlus, string name, ResourceStringData caption, string list = null, int? width = null)
		{
			var columnName = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}UNDGs+{1}+Value", bindingPrefixWithPlus, name);

			if (grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnName))
			{
				return;
			}

			var gridColumnInfo = new ZMultiControlColumnStyleInfo()
			{
				ColumnName = columnName,
				FieldTypeColumnName = string.Format(CultureInfo.InvariantCulture, "{0}UNDGs+{1}+FieldColumnType", bindingPrefixWithPlus, name),
				BindToList = list != null ? string.Format(CultureInfo.InvariantCulture, "{0}UNDGs+{1}", bindingPrefixWithPlus, list) : string.Empty,
				GroupName = Res.GetData("UNDGDataItemFormManager|bdb57f17-44f2-42d4-823e-2daaeea69852", "Dangerous Goods"),
				CaptionResourceString = caption,
				IsVisible = false,
			};

			if (width != null)
			{
				ControlDpiScalingHelper.SetWidth(ref gridColumnInfo, (int)width, true);
			}

			if (ShouldHidePredicate?.Invoke(gridColumnInfo) ?? false)
			{
				return;
			}

			grid.ColumnStyles.Add(gridColumnInfo);
			columnsToUpdate.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"UNDGs+{0}+Value", name));
		}

		public void RemoveAllColumns(ZGrid gridToAddTo)
		{
			gridToAddTo.RemoveFromAvailableColumns(ColumnNames.UNDGSubstanceManagerValue,
				"UNDGs+UNDGFlashPointManager+Value",
				"UNDGs+UNDGContactManager+Value");

			columnsToUpdate.Clear();
		}

		protected override MultilingualString MenuCaption
		{
			get { return ResString.GetMultilingualString("07B9584D-CFFF-4d62-9499-F2DFA6884399", "Dangerous Goods"); }
		}

		protected override Shortcut MenuShortcut
		{
			get { return Shortcut.CtrlD; }
		}

		protected override void ShowMultipleItemFormCore(IUNDGDataItemProvider parent, Form parentForm)
		{
			if (config.IsIMOOnly)
			{
				ZFormModaliser.Show(new UNDGDataItemForm(parent), parentForm);
			}
			else
			{
				ZFormModaliser.Show(new UNDGDataItemFormGuid(parent, GetAdditionalColumnInfosOnUNDGDataItemFormGuid()), parentForm);
			}
		}

		IEnumerable<ZGridColumnInfo> GetAdditionalColumnInfosOnUNDGDataItemFormGuid()
		{
			var infos = new List<ZGridColumnInfo>();
			if (!config.ShouldHidePSAGroup)
			{
				var psaGroupColumn = new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("baf83d95-7e3d-4f4e-93bc-ddfc9a737da0", "PSA Group"),
					ColumnName = "PSAGroup",
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				};

				infos.Add(psaGroupColumn);
			}

			return infos;
		}
	}
}
