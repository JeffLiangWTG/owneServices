using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class RateTransportZoneController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.RateTransportZone; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RateTransportZone); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RateTransportProviderForm(((RateTransportZone)businessEntity).TransportProvider);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RateTransportZone; }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RateTransportProviderView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RateTransportProviderNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RateTransportProviderEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RateTransportProviderDelete; }
		}

		#endregion

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(
				Res.GetString("5339db21-9678-4a42-addf-5451e95a945e",
					"To add a new Transport Zone, please either create a new Transport Zone Set from the Maintain --> Locations --> Transport Zone Sets menu, or locate the existing Transport Zone Set and edit it."),
				Res.GetString("0feaadea-a5c0-4542-948b-42e27d2c1c3c", "New"),
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

			return null;
		}
	}
}

