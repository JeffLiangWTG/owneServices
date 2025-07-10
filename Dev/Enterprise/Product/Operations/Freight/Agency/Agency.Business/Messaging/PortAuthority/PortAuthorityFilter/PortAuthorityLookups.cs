using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityLookups : PortMessageLookups
	{
		public PortAuthorityLookups(PortAuthority parent)
			: base(parent) { }

		public override PortMessageTargetPortList Port_List
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
				{
					return Parent.DeliverTo3rdParty ? PortListWithoutSettings : PortListWithSettings;
				}
				else
				{
					return EmptyPortList;
				}
			}
		}

		public PortAuthoritySettingCollection Settings_List
		{
			get { return settings_List ?? (settings_List = AgencyRegistry.Instance.PortAuthoritySettings.Value.Settings); }
		}
		PortAuthoritySettingCollection settings_List;

		#region Implementation

		public new PortAuthority Parent
		{
			get { return (PortAuthority)base.Parent; }
		}

		PortMessageTargetPortList PortListWithSettings
		{
			get
			{
				if (portListWithSettings == null)
				{
					portListWithSettings = new PortAuthorityMessageTargetPortList(Parent.Voyage, Settings_List);
					portListWithSettings.Load();
				}

				return portListWithSettings;
			}
		}

		PortMessageTargetPortList PortListWithoutSettings
		{
			get
			{
				if (portListWithoutSettings == null)
				{
					portListWithoutSettings = new PortAuthorityMessageTargetPortList(Parent.Voyage, null);
					portListWithoutSettings.Load();
				}

				return portListWithoutSettings;
			}
		}

		PortMessageTargetPortList EmptyPortList
		{
			get
			{
				if (emptyPortList == null)
				{
					emptyPortList = new PortAuthorityMessageTargetPortList(Parent.Voyage, null);
				}

				return emptyPortList;
			}
		}

		PortAuthorityMessageTargetPortList portListWithSettings;
		PortAuthorityMessageTargetPortList portListWithoutSettings;
		PortAuthorityMessageTargetPortList emptyPortList;

		#endregion
	}
}
