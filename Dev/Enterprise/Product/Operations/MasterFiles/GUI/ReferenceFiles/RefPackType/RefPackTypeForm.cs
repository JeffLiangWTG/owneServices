using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class RefPackTypeForm : ZTemplateForm
	{
		public RefPackTypeForm(RefPackType packType)
			: base(packType)
		{
			if (packType != null)
			{
				if (!packType.IsInDatabase)
				{
					using (packType.SuspendSettingHasChanges())
					{
						packType.F3_IsUpdatable = false;
					}
				}
				else if (packType.F3_IsUpdatable)
				{
					packType.HasChangesChanged += PackType_HasChangesChanged;
					packType.F3_IsUpdatableInfo.ValueChanged += F3_IsUpdatableInfo_ValuedChanged;
				}
			}
		}

		#region PackType

		RefPackType PackType
		{
			get { return BusinessEntity as RefPackType; }
		}

		#endregion

		#region SetDataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var packType = PackType;
			if (packType != null)
			{
				MainTabPage.RunWhenBindingOrFirstShown(delegate
				{
					this.PackTypeIsReservedTypeLabel.Visible = packType.IsReservedType;
				});
			}
		}

		#endregion

		#region F3_IsUpdatable Handling

		void PackType_HasChangesChanged(object sender, EventArgs e)
		{
			if (PackType != null && PackType.HasChanges)
			{
				PackType.HasChangesChanged -= PackType_HasChangesChanged;
				PackType.F3_IsUpdatableInfo.ValueChanged -= F3_IsUpdatableInfo_ValuedChanged;

				PackType.F3_IsUpdatableInfo.AdditionalValidation += F3_IsUpdatableInfo_AdditionalValidation;
				PackType.F3_IsUpdatable = false;
				PackType.F3_IsUpdatableInfo.AdditionalValidation -= F3_IsUpdatableInfo_AdditionalValidation;
			}
		}

		void F3_IsUpdatableInfo_ValuedChanged(object sender, EventArgs e)
		{
			if (PackType != null)
			{
				PackType.HasChangesChanged -= PackType_HasChangesChanged;
				PackType.F3_IsUpdatableInfo.ValueChanged -= F3_IsUpdatableInfo_ValuedChanged;
			}
		}

		void F3_IsUpdatableInfo_AdditionalValidation()
		{
			if (PackType != null)
			{
				PackType.F3_IsUpdatableInfo.AddWarning(Res.GetString("0fe7661d-bc84-4128-b56a-b7d6742f04f2", "This option was automatically unticked to save user changes during future system data updates."));
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (PackType != null)
				{
					PackType.HasChangesChanged -= PackType_HasChangesChanged;
					PackType.F3_IsUpdatableInfo.ValueChanged -= F3_IsUpdatableInfo_ValuedChanged;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
