using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class AccOrgTaxConfigurationTemplateController : ZController
	{
		public override ControllerID ID => ControllerIDs.AccOrgTaxConfigurationTemplate;

		public override ModuleIdentifier ModuleID => ModuleIDs.AccOrgTaxConfigurationTemplate;

		public override Type TypeOfTopLevelBusinessObject => typeof(AccOrgTaxConfigurationTemplate);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TaxConfigurationTemplate;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.TaxConfigurationTemplateNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.TaxConfigurationTemplateModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.TaxConfigurationTemplateDelete;

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject bizo)
		{
			return Env.Security.TaxConfigurationTemplateCopy;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccOrgTaxConfigurationTemplateForm(businessEntity as AccOrgTaxConfigurationTemplate);
		}
	}
}
