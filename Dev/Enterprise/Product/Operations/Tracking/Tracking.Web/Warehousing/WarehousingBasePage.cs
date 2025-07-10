using System;

namespace Enterprise.Tracking.Web
{
	public abstract class WarehousingBasePage : BasePageWithAuthorisation
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (LinesGridAddOn != null)
			{
				Controls.Add(LinesGridAddOn);
			}
		}

		protected WarehouseDocketLineGridAddOn LinesGridAddOn => linesGridAddOn ?? (linesGridAddOn = GetNewLinesGridAddOn());
		WarehouseDocketLineGridAddOn linesGridAddOn;

		protected virtual WarehouseDocketLineGridAddOn GetNewLinesGridAddOn() => null;
	}
}
