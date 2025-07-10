using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.Module
{
	public class CompanyCredentialsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var company = (GlbCompany)businessEntity;
			CompanyCredentialsPlugIn plugin;

			switch (company?.Country?.Code ?? string.Empty)
			{
				case Core.Constants.CountryCodes.Australia:
					plugin = (CompanyCredentialsPlugIn)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IAUCompanyCredentialsPlugIn>(), company);
					break;
				case Core.Constants.CountryCodes.UnitedKingdom:
					plugin = (CompanyCredentialsPlugIn)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IGBCompanyCredentialsPlugIn>(), company);
					break;
				default:
					plugin = new CompanyCredentialsPlugIn(company);
					break;
			}

			return plugin;
		}

		public override ControllerID ID => ControllerIDs.CompanyCredentialsPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Companies;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Companies;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Companies;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Companies;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("{4B863245-EFBC-4A20-B6B5-CC2EDEF21D2F}", "Credentials");
	}
}
