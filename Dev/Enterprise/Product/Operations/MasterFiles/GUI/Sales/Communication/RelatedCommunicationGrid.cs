using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RelatedCommunicationGrid : CommunicationGrid
	{
		public RelatedCommunicationGrid()
		{
			InitializeComponent();
		}

		protected override void OnFormShowingForNewOrgSalesCall(FormShowingForOrgSalesCallArgs e)
		{
			base.OnFormShowingForNewOrgSalesCall(e);

			if (!e.Cancelled)
			{
				var relatableOrgSalesCallCollection = BindingSource.Current as RelatedOrgSalesCallCollection;
				if (relatableOrgSalesCallCollection != null)
				{
					var master = relatableOrgSalesCallCollection.Relationship.Master as IRelatableActivity;
					if (master != null && !e.OrgSalesCall.RelatedParentActivityPivotCollection.HasParent(master))
					{
						var setParentResult = e.OrgSalesCall.RelatedParentActivityPivotCollection.AddActivity(master);
						if (!setParentResult.Success)
						{
							Globals.Message.ShowError(ResString.GetMultilingualString("3d5720c7-bb55-48ac-a63e-a6fb9243d835", "Can not create related communication - {0}", setParentResult.Reason), CanNotCreateRelatedOrgSalesCallCaption);
							e.Cancelled = true;
						}
					}
				}
			}
		}

		ResourceString CanNotCreateRelatedOrgSalesCallCaption
		{
			get { return ResString.GetMultilingualString("c7a96b0b-1336-4162-9ad5-f8bb4293cb8e", "Can not create related communication"); }
		}
	}
}
