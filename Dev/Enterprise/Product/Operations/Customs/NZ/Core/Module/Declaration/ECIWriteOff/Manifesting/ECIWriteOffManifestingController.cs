using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration
{
	public class ECIWriteOffManifestingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is CusEntryHeader)
			{
				return new EditManifestForm((CusEntryHeader)businessEntity);
			}
			else if (businessEntity is NewManifestCreator)
			{
				return new NewManifestSelectionForm((NewManifestCreator)businessEntity);
			}
			throw new NotSupportedException("Cannot call GetForm unless the Business Entity passed is either a CusEntryHeader or NewManifestCreator");
		}

		public override ControllerID ID => ControllerIDs.Customs.NZ.ECIWriteOffManifesting;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusEntryHeader);

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);

		protected override ODisplayMode GetDisplayModeForNew() => ODisplayMode.Undefined;

		public override ModuleIdentifier ModuleID => null;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.NZCustomsECIManifesting;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.NZCustomsECIManifestingEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.NZCustomsECIManifestingNew;
	}
}
