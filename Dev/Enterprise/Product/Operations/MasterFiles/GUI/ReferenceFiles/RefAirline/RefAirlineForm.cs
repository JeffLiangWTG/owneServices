using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class RefAirlineForm : ZTemplateForm
	{
		public RefAirlineForm(RefAirline airline) : base(airline)
		{
			InitializeComponent();
			PlugIns.Add(ControllerIDs.Audit);

			if (airline != null)
			{
				if (!airline.IsInDatabase)
				{
					using (airline.SuspendSettingHasChanges())
					{
						airline.RM_IsUpdatable = false;
					}
				}
				else if (airline.RM_IsUpdatable)
				{
					airline.HasChangesChanged += RefAirline_HasChangesChanged;
					airline.RM_IsUpdatableInfo.ValueChanged += RM_IsUpdatableInfo_ValuedChanged;
				}
			}
			DefaultCommodityTabPage.TabVisible = true;
			// The other details tab page shows useless info so for now, we are hiding it - ZA Sept 2009.
			OtherDetailsTabPage.TabVisible = false;
		}

		#region Implementation

		RefAirline AirLine
		{
			get { return BusinessEntity as RefAirline; }
		}

		#endregion

		#region RM_IsUpdatable Handling

		void RefAirline_HasChangesChanged(object sender, EventArgs e)
		{
			if (AirLine != null && AirLine.HasChanges)
			{
				AirLine.HasChangesChanged -= RefAirline_HasChangesChanged;
				AirLine.RM_IsUpdatableInfo.ValueChanged -= RM_IsUpdatableInfo_ValuedChanged;
			}
		}

		void RM_IsUpdatableInfo_ValuedChanged(object sender, EventArgs e)
		{
			if (AirLine != null)
			{
				AirLine.HasChangesChanged -= RefAirline_HasChangesChanged;
				AirLine.RM_IsUpdatableInfo.ValueChanged -= RM_IsUpdatableInfo_ValuedChanged;
			}
		}

		#endregion
	}
}
