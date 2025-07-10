using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public abstract class ImportClassificationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.ImportClassification; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ImportClassification; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseCusClassification); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			BaseCusClassification result = (BaseCusClassification)Factory.New(TypeOfTopLevelBusinessObject);
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			return result;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ImportClassification; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ImportClassificationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportClassificationModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ImportClassificationDelete; }
		}
	}
}
