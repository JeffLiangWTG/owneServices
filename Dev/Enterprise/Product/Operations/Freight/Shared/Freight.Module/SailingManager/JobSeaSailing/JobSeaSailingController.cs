using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobSeaSailingController : JobSailingController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobSeaSailing; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobSeaSailing; }
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}

		protected override IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
		{
			if (CheckPointForNew.IsAllowed)
			{
				IBusiness loadedSourceEntity = GetLoadedBusinessEntityInLocalFactory(inMemorySourceEntity);
				JobVoyage copiedBusinessObject = SailingTemplateCopyDialog.GetTemplateCopy((JobVoyage)loadedSourceEntity);

				if (copiedBusinessObject != null)
				{
					ShowForm(PrepareNewlyCreateForm(copiedBusinessObject));
				}
				else
				{
					LastShownForm = null;
				}
			}
			else
			{
				CheckPointForNew.ShowError();
				LastShownForm = null;
			}

			return LastShownForm;
		}
	}
}
