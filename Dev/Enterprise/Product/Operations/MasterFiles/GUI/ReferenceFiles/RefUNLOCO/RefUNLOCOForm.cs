using System;
using System.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefUNLOCOForm : ZForm
	{
		public RefUNLOCOForm(RefUNLOCO uNLOCO) : base(uNLOCO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);

			if (uNLOCO != null)
			{
				if (!uNLOCO.IsInDatabase)
				{
					using (uNLOCO.SuspendSettingHasChanges())
					{
						uNLOCO.RL_IsUpdatable = false;
					}
				}
				else if (uNLOCO.RL_IsUpdatable)
				{
					uNLOCO.HasChangesChanged += UNLOCO_HasChangesChanged;
					uNLOCO.RL_IsUpdatableInfo.ValueChanged += RL_IsUpdatableInfo_ValuedChanged;
				}
			}

#if DEBUG
			TypeDescriptor.AddAttributes(LocalDateTimeCodeLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#region Implementation

		protected override ZTabControl TopLevelTabControl
		{
			get { return MainTabControl; }
		}

		RefUNLOCO UNLOCO
		{
			get { return BusinessEntity as RefUNLOCO; }
		}

		#endregion

		#region RL_IsUpdatable Handling

		void UNLOCO_HasChangesChanged(object sender, EventArgs e)
		{
			if (UNLOCO != null && UNLOCO.HasChanges)
			{
				UNLOCO.HasChangesChanged -= UNLOCO_HasChangesChanged;
				UNLOCO.RL_IsUpdatableInfo.ValueChanged -= RL_IsUpdatableInfo_ValuedChanged;

				UNLOCO.RL_IsUpdatableInfo.AdditionalValidation += RL_IsUpdatableInfo_AdditionalValidation;
				UNLOCO.RL_IsUpdatable = false;
				UNLOCO.RL_IsUpdatableInfo.AdditionalValidation -= RL_IsUpdatableInfo_AdditionalValidation;
			}
		}

		void RL_IsUpdatableInfo_ValuedChanged(object sender, EventArgs e)
		{
			if (UNLOCO != null)
			{
				UNLOCO.HasChangesChanged -= UNLOCO_HasChangesChanged;
				UNLOCO.RL_IsUpdatableInfo.ValueChanged -= RL_IsUpdatableInfo_ValuedChanged;
			}
		}

		void RL_IsUpdatableInfo_AdditionalValidation()
		{
			if (UNLOCO != null)
			{
				UNLOCO.RL_IsUpdatableInfo.AddWarning(Res.GetString("0fe7661d-bc84-4128-b56a-b7d6742f04f2", "This option was automatically unticked to save user changes during future system data updates."));
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (UNLOCO != null)
				{
					UNLOCO.HasChangesChanged -= UNLOCO_HasChangesChanged;
					UNLOCO.RL_IsUpdatableInfo.ValueChanged -= RL_IsUpdatableInfo_ValuedChanged;
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
