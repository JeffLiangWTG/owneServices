using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccOrgTaxConfigurationTemplateModuleForTest : AccOrgTaxConfigurationTemplateModule
	{
		public new ZController GetNewController(BusinessObject selectedBusinessObject) => base.GetNewController(selectedBusinessObject);

		public IFilterControl NewFilterControl => GetNewFilterControl();

		public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

		public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();

		public override BusinessObject[] GetSelectedBusinessObjects() => BusinessObjectsToSelect ?? Array.Empty<BusinessObject>();

		protected override BusinessObject[] SelectedBusinessObjects => BusinessObjectsToSelect;

		public BusinessObject[] BusinessObjectsToSelect { get; set; }
	}
}
