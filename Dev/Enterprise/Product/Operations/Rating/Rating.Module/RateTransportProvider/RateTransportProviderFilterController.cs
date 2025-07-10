using System;

using CargoWise.EntityFramework;

using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class RateTransportProviderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.RateTransportProvider; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RateTransportProvider); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RateTransportProviderForm((RateTransportProvider)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RateTransportProvider; }
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
	}
}

