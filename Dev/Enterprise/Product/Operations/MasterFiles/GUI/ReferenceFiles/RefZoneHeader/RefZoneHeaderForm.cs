using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefZoneHeaderForm : ZForm, ICanAttachWithoutSecurity
	{
		public RefZoneHeaderForm(RefZoneHeader zone)
			: base(zone)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			var warningMessage = string.Empty;

			if (result == ContinueWithSave.Yes && !string.IsNullOrEmpty(lazyGetWarningMessage()))
			{
				Globals.Message.ShowWarning(warningMessage);
			}
			return result;

			string lazyGetWarningMessage() => warningMessage = GetOverlappingZonesMessage();
		}

		string GetOverlappingZonesMessage()
		{
			var refZoneHeaderBizO = DataSource as RefZoneHeader;
			if (refZoneHeaderBizO == null)
			{
				return string.Empty;
			}

			// Only warnings we have on F2_ParentID now is about duplicate locations in different zones.
			var hasLocationWarnings = refZoneHeaderBizO
				.GetWarnings()
				.OfType<PropertyNotification>()
				.Any(x => x.PropertyName == RefZonePivotSchema.Constants.F2_ParentID);

			return hasLocationWarnings
				? refZoneHeaderBizO.GetOverlappingZonesMessage()
				: string.Empty;
		}

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				CarrierGuidFindBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				CarrierGuidFindBox.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "IsRatingAvailableZone"));
				var zone = (RefZoneHeader)DataSource;
				zone.FZ_ZoneTypeInfo.ValueChanged += FZ_ZoneTypeInfo_ValueChanged;
				FZ_ZoneTypeInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region FZ_ZoneType Event Handler

		void FZ_ZoneTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var zone = (RefZoneHeader)DataSource;

			if (zone.FZ_ZoneType.Equals(RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway) || zone.FZ_ZoneType.Equals(RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway) || zone.FZ_ZoneType.Equals(RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway))
			{
				this.CarrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Enterprise.MasterFiles.GUI.Res.GetData("7b1e79fb-4dbd-0c8c-4962-e6a4d875b67e", "Gateway").Caption;
				zone.FZ_OH_RelatedPartyInfo.HumanReadableName = Enterprise.MasterFiles.GUI.Res.GetData("f7bc62e8-3786-93b7-4d26-a75f156f256b", "Gateway").Caption;
			}
			else
			{
				this.CarrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Enterprise.MasterFiles.GUI.Res.GetData("fd12e079-674d-4c81-8c23-c2399be58e03", "Carrier/Customer").Caption;
				zone.FZ_OH_RelatedPartyInfo.HumanReadableName = Enterprise.MasterFiles.GUI.Res.GetData("86b3884d-96b4-0eab-496c-6c4b971c470e", "Carrier/Customer").Caption;
			}
		}

		#endregion
	}
}
