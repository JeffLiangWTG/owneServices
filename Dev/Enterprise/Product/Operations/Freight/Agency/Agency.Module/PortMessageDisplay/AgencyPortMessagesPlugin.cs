using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class AgencyPortMessagesPlugin : ZPlugIn
	{
		public AgencyPortMessagesPlugin(JobVoyage voyage)
			: base(voyage)
		{
			this.voyage = voyage;
			countryCollection = new AgencyCountryCollection(voyage);
		}

		#region Overrides

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (IsAllowedInCurrentCountry)
			{
				var itemName = Env.Security.SailingSchedulePortMessaging.IsAllowed
					? ResString.GetMultilingualString("ZJobVoyageForm|Menu|SendPortAuthorityMessage", "Send Port Authority Message")
					: ResString.GetMultilingualString("PlugInMenu.AccessDenied", "Access denied, click this menu for details.");

				return new ZMenuItem(itemName, delegate
				{ PortAuthorityFilterDialog.ShowDialog(this.countryCollection.Voyage); });
			}
			else
			{
				var itemName = ResString.GetMultilingualString("741069b0-b01b-11e4-ad1c-902b34dc814a", "Not available, click this menu for details.");
				var message = Res.GetString("7c597c5e-8a8a-4e1e-88f1-7f91a82ee885", "Ports in your country/region are not configured for Port Messages. Contact WiseTech Global for a possibility of having this connection established.");
				var title = Res.GetString("dbb5aa30-aae0-4b25-b004-186a37fea04a", "Message cannot be sent.");

				return new ZMenuItem(itemName, (o, o1) => Globals.Message.ShowError(message, title));
			}
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new PortMessageDisplayControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Collection;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ShippingManager; }
		}

		public override string Name
		{
			get { return Res.GetString("e1193616-7cc8-4bc0-8038-46a4fc2cbe3b", "Port Messages"); }
		}

		#endregion

		#region Collection

		PortMessageHostCollection Collection
		{
			get { return collection ?? (collection = new PortMessageHostCollection(voyage)); }
		}
		PortMessageHostCollection collection;

		#endregion

		#region Implementation

		bool IsAllowedInCurrentCountry
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia;
			}
		}

		readonly JobVoyage voyage;

		readonly AgencyCountryCollection countryCollection;

		#endregion
	}
}


