using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	class CountryReferencesModuleButtonGrid : ZModuleButtonGrid
	{
		public CountryReferencesModuleButtonGrid() : base()
		{
			NameOfAGridElement = Res.GetData("46c05275-a251-c699-482d-9eea8f026c99", "Country/Region Reference");
			DetachMessage = Res.GetData("923bbb73-4db2-7fb9-4fa7-075092fc76a1", "Are you sure you want to detach the selected records?");
			if (!DesignModeFinder.IsDesigning)
			{
				SetupColumns();
			}
		}

		protected override bool AllowDoubleClick => false;

		void SetupColumns()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			var zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGSubstance)null).UNDGCountryReferences);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGSubstance)(null)).Lookups.UNDGCountryReferences);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_RN_NKCountry);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_Type);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)(((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DCR_TypeDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_FlashPointLowerCentigradeReadOnly);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).DCR_FlashPointUpperCentigradeReadOnly);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).SubstancePivot.DCP_StorageInstruction);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UNDGCountryReference)((System.Collections.IList)(((UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot).SubstancePivot.DCP_TankStorageInstructionRetentionTray);

			Name = "CountryReferencesModuleButtonGrid";
			BindToFindBoxList = "Lookups.UNDGCountryReferences";
			ShowNewButton = false;
			ShowEditButton = false;
			zTextBoxColumnStyleInfo1.ColumnName = "DCR_RN_NKCountry";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "DCR_Type";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "DCR_TypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "DCR_Code";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ColumnName = "DCR_Description";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.ColumnName = "DCR_FlashPointLowerCentigradeReadOnly";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.ColumnName = "DCR_FlashPointUpperCentigradeReadOnly";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zDropEditColumnStyleInfo1.ColumnName = "SubstancePivot+DCP_StorageInstruction";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "SubstancePivot+DCP_TankStorageInstructionRetentionTray";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			ColumnStyles.Add(zDropEditColumnStyleInfo1);
			ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
		}
	}
}
