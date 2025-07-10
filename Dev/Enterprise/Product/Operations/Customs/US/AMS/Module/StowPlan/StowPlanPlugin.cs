using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Module
{
	public class StowPlanPlugin : ZPlugIn
	{
		public StowPlanPlugin(JobVoyage voyage)
			: base(voyage)
		{
		}

		JobVoyage Voyage
		{
			get { return (JobVoyage)HostBusinessEntity; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|StowPlan", "Stow Plan"));
			var stowPlanMenuItem = (MenuItem)new ZMenuItem(ResString.GetMultilingualString("ZJobVoyageForm|Menu|StowPlanMessage", "Send Stow Plan Message"),
				delegate
				{ StowPlanForm.ShowDialog(Voyage); });
			result.MenuItems.Add(stowPlanMenuItem);
			return result;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return Res.GetString("D352CE8C-4FA6-4453-B14B-2143FF5825A8", "Stow Plan"); }
		}

		USVoyagePortCollection Collection
		{
			get { return collection ?? (collection = new USVoyagePortCollection(new VoyagePortCollection(Voyage))); }
		}
		USVoyagePortCollection collection;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Collection;
		}

		protected override Control GetNewUserControl()
		{
			return new StowPlanMessageDisplayControl();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				collection?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
