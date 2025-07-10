using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	class AccAllowedBranchDepartmentComboModuleButtonGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new AccAllowedBranchDepartmentComboModuleAttacher(destinationCollection, findBoxList, moduleID);
		}

		protected override void Detach(BusinessObject selected)
		{
			selected.Delete();
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			this.Form.BusinessEntity.RefreshBindingIncludingChildren();
			base.AttachButton_Click(sender, e);
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((AccAllowedBranchDepartmentCombo)selected).Department);
		}
	}

	class AccAllowedBranchDepartmentComboModuleAttacher : ZRecordAttacher
	{
		public AccAllowedBranchDepartmentComboModuleAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
		}

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var destinationCollection = this.DestinationCollection as AccAllowedBranchDepartmentComboCollection;

			if (destinationCollection == null)
			{
				return false;
			}

			var accAllowedBranchDepartmentCombo = destinationCollection.AddNew();
			accAllowedBranchDepartmentCombo.AAB_GE_Department = pk;

			return true;
		}
	}
}
