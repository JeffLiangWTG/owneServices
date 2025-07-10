using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V3.Business;
using Enterprise.Customs.SG.V3.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.V3.Module
{
	public class V3JobDeclarationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.SGV3JobDeclaration; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(V3JobDeclaration); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new V3JobDeclarationForm(new V3Brokerage(((V3JobDeclaration)businessEntity).PK, businessEntity.Factory));
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDeclarationEnquiryEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsDeclarationEnquiryNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsDeclarationEnquiryDelete; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
