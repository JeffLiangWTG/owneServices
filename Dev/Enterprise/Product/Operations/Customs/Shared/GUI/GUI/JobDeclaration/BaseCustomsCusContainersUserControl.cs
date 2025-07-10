using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsCusContainersUserControl : BaseCustomsEntryUserControl
	{
		#region Controls

		protected internal ZPanel BaseAllPanel;
		protected internal ZPanel BaseContainerPanel;
		protected internal ZGroupBox ContainersGroupBox;
		public ZModuleButtonGrid CusContainersBoundGrid;

		#endregion

		public BaseCustomsCusContainersUserControl()
		{
			InitializeComponent();
			CusContainersBoundGrid.InnerGrid.OnRemovingBizOFromList += new ZGrid.RemoveBizOFromListHandler(OnContainerRemovingFromList);
		}

		#region OnContainerRemovingFromList

		ZGrid.ContinueWithRemove OnContainerRemovingFromList(BusinessObject bizOToBeDeleted)
		{
			BaseCusContainer container = (BaseCusContainer)bizOToBeDeleted;

			bool result = true;

			if (container.Declaration != null &&
				container.PackingGroups.HasElementsToBeDeletedWhenContainerDeleted)
			{
				result = Globals.Message.Show(Res.GetString("593981cd-6dd4-46a0-9b66-aaef0472d455", "This container, {0} is about to be deleted and all the packages linked to this container will be deleted as well. Alternatively, you can clear the container number from the packages you want to retain and then, delete the container. Do you wish to delete the container now?", container.CO_ContainerNumber), Res.GetString("63ad7198-7387-4ec8-952c-e006e912f5bf", "Deleting container"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
			}

			return result ? ZGrid.ContinueWithRemove.Remove : ZGrid.ContinueWithRemove.CancelRemoval;
		}

		#endregion

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (fCusContainersGridLayoutPersister != null)
			{
				fCusContainersGridLayoutPersister.Dispose();
				fCusContainersGridLayoutPersister = null;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				fCusContainersGridLayoutPersister = new CustomLabelsGridLayoutPersister(
					CusContainersBoundGrid.InnerGrid, new BaseCusContainer.CustomLabelsProvider(JobDeclaration), "", Core.Constants.CustomLabels.CusContainer.Prefix);
			}
		}
		protected CustomLabelsGridLayoutPersister fCusContainersGridLayoutPersister;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fCusContainersGridLayoutPersister != null)
				{
					fCusContainersGridLayoutPersister.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
