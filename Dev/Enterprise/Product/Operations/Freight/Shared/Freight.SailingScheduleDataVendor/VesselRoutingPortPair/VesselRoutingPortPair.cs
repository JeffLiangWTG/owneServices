using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	public class VesselRoutingPortPair : AutoViewVesselRoutingPortPairs
	{
		public VesselRoutingPortPair(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			bool result;

			switch (property.Name)
			{
				case Schema.E9_IsSelected:
					result = E9_RL_NKLoadPort.IsEmpty || E9_RL_NKDischargePort.IsEmpty;
					break;

				case Schema.E9_Publish:
					result = E9_IsRegistered;
					break;

				default:
					result = true;
					break;
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#region Schema

		public new abstract class Schema : AutoViewVesselRoutingPortPairs.Schema
		{
			public const string E9_IsSelected = "E9_IsSelected";
			public const string E9_Publish = "E9_Publish";
		}

		#endregion

		#region Business Object Overrides

		public new VesselRoutingPortPair Clone()
		{
			return (VesselRoutingPortPair)base.Clone();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			VesselRoutingPortPair result = (VesselRoutingPortPair)base.CloneInternal(args);
			CopyValuesFrom(result);
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		#endregion

		#region Related Business Objects

		public VesselRoutingVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					ZQuery query = new ZQuery(ViewVesselRoutingVoyagesSchema.E8_LloydsNumber, E9_LloydsNumber);
					query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_Voyage, E9_Voyage);
					query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_LineOperator, E9_LineOperator);

					voyage = Factory.LoadTop1<VesselRoutingVoyage>(query);
				}

				return voyage;
			}
		}
		VesselRoutingVoyage voyage;

#if DEBUG
		internal
#endif
 JobSailing JobSailing
		{
			get
			{
				if (jobSailing == null && Voyage != null && Voyage.Vessel != null)
				{
					var lineOperatorPK = SailingScheduleHelper.GetLineOperatorPKFromExternalCode(E9_LineOperator, E9_DataProvider, Factory);

					jobSailing = new SailingLocator(Factory).FindSailingFromSailingManager(
						Core.Constants.TransportModes.Sea,
						E9_RL_NKLoadPort,
						E9_RL_NKDischargePort,
						Voyage.Vessel.RV_Name,
						E9_Voyage,
						lineOperatorPK,
						E9_ETD,
						E9_ETA)
						.Sailing;
				}

				return jobSailing;
			}
		}
		JobSailing jobSailing;

		#endregion

		#region New Properties

		#region PortPairType

		public PortPairTypes PortPairType
		{
			get
			{
				PortPairTypes result = PortPairTypes.None;
				string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				if (!E9_RL_NKLoadPort.StartsWith(countryCode) &&
					E9_RL_NKDischargePort.StartsWith(countryCode))
				{
					result = PortPairTypes.Import;
				}
				else if (
					E9_RL_NKLoadPort.StartsWith(countryCode) &&
					E9_RL_NKDischargePort.StartsWith(countryCode))
				{
					result = PortPairTypes.Domestic;
				}
				else if (
					E9_RL_NKLoadPort.StartsWith(countryCode) &&
					!E9_RL_NKDischargePort.StartsWith(countryCode))
				{
					result = PortPairTypes.Export;
				}

				return result;
			}
		}

		#endregion

		#region E9_IsSelected

		public ZBool E9_IsSelected
		{
			get { return fE9_IsSelected && (!E9_RL_NKLoadPort.IsEmpty && !E9_RL_NKDischargePort.IsEmpty); }
			set
			{
				if (E9_IsSelected != value)
				{
					fE9_IsSelected = value;
					E9_IsSelectedInfo.RefreshBinding();

					if (Voyage != null)
					{
						Voyage.E8_IsSelectedInfo.RefreshBinding();
						Voyage.Validation.ValidateE8_LloydsNumber();
					}
				}
			}
		}
		ZBool fE9_IsSelected;

		public ZPropertyInfo E9_IsSelectedInfo
		{
			get { return GetZPropertyInfo(Schema.E9_IsSelected); }
		}

		#endregion

		#region E9_Publish

		public ZBool E9_Publish
		{
			get { return JobSailing != null ? JobSailing.JX_IsPublished : GetUserEnteredValue<ZBool>(E9_PublishInfo.Name, true); }
			set
			{
				SetUserEnteredValue(E9_PublishInfo.Name, value);
				E9_PublishInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E9_PublishInfo
		{
			get { return GetZPropertyInfo(Schema.E9_Publish); }
		}

		#endregion

		#region E9_IsRegistered

		public ZBool E9_IsRegistered
		{
			get { return JobSailing != null; }
		}

		public ZPropertyInfo E9_IsRegisteredInfo
		{
			get { return GetZPropertyInfo(nameof(E9_IsRegistered)); }
		}

		#endregion

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude] // severs the link to VesselRoutingVoyage
		public override ZString E9_LloydsNumber
		{
			get { return base.E9_LloydsNumber; }
			set { base.E9_LloydsNumber = value; }
		}

		[BusinessObjectTestExclude] // severs the link to VesselRoutingVoyage
		public override ZString E9_Voyage
		{
			get { return base.E9_Voyage; }
			set { base.E9_Voyage = value; }
		}

		[BusinessObjectTestExclude] // severs the link to VesselRoutingVoyage
		public override ZString E9_LineOperator
		{
			get { return base.E9_LineOperator; }
			set { base.E9_LineOperator = value; }
		}

		#endregion

		#region User Entered Data Storage

		T GetUserEnteredValue<T>(string propertyName, T defaultValue)
		{
			return (Voyage == null) ? defaultValue : Voyage.GetUserEnteredValue(new UserEnteredValueKey(propertyName, this), defaultValue);
		}

		void SetUserEnteredValue<T>(string propertyName, T value)
		{
			if (Voyage == null)
			{
				ErrorReporter.ReportOnce("VesselRoutingPortPair.SetUserEnteredValue." + propertyName, "No link to VesselRoutingVoyage via Lloyds / Voyage, User Entered Data cannot be set at this time");
			}

			Voyage.SetUserEnteredValue(new UserEnteredValueKey(propertyName, this), value);
		}

		class UserEnteredValueKey
		{
			public UserEnteredValueKey(string propertyName, VesselRoutingPortPair portPair)
			{
				this.propertyName = propertyName;
				this.loadPort = portPair.E9_RL_NKLoadPort;
				this.dischargePort = portPair.E9_RL_NKDischargePort;
			}

			public override bool Equals(object obj)
			{
				UserEnteredValueKey rhs = obj as UserEnteredValueKey;
				return
					rhs != null &&
					propertyName == rhs.propertyName &&
					loadPort == rhs.loadPort &&
					dischargePort == rhs.dischargePort;
			}

			public override int GetHashCode()
			{
				return propertyName.GetHashCode() ^ loadPort.GetHashCode() ^ dischargePort.GetHashCode();
			}

			readonly string propertyName;
			readonly string loadPort;
			readonly string dischargePort;
		}

		#endregion
	}
}
