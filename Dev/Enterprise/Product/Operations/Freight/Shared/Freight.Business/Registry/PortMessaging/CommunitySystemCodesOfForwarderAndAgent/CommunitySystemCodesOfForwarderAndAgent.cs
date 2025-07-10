using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class CommunitySystemCodesOfForwarderAndAgent : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Port = "Port";
			public const string PCS = "PCS";
			public const string ForwarderCode = "ForwarderCode";
			public const string AgentCode = "AgentCode";

			public const int PortMaxLength = 5;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CommunitySystemCodesOfForwarderAndAgent();
		}

		#endregion

		#region Properties

		#region Port

		[MaxLength(Schema.PortMaxLength)]
		[List("PortList")]
		public ZString Port
		{
			get { return port; }
			set
			{
				if (value != port)
				{
					SetNonPersistentPropertyValue(PortInfo, ref port, value);
					SetPCS();

					if (!IsValidationSuspended)
					{
						ValidatePort();
						ValidatePCS();
					}
				}
			}
		}
		ZString port;

		public ZPropertyInfo PortInfo
		{
			get { return GetZPropertyInfo(Schema.Port); }
		}

		public void ValidatePort()
		{
			PortInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(PortInfo, PortList);
			CheckPort(PortInfo);
		}

		#endregion

		#region PCS

		[ReadOnly(true)]
		public ZString PCS
		{
			get { return pcs; }
			set
			{
				if (value != pcs)
				{
					SetNonPersistentPropertyValue(PCSInfo, ref pcs, value);
					if (!IsValidationSuspended)
					{
						ValidatePCS();
					}
				}
			}
		}
		ZString pcs;

		public ZPropertyInfo PCSInfo => GetZPropertyInfo(Schema.PCS);

		public void ValidatePCS()
		{
			PCSInfo.ClearAllNotifications();
			CheckPCS(PCSInfo);
		}

		#endregion

		#region ForwarderCode

		public ZString ForwarderCode
		{
			get { return forwarderCode; }
			set
			{
				if (value != forwarderCode)
				{
					SetNonPersistentPropertyValue(ForwarderCodeInfo, ref forwarderCode, value);
					if (!IsValidationSuspended)
					{
						ValidateForwarderCode();
					}
				}
			}
		}
		ZString forwarderCode;

		public ZPropertyInfo ForwarderCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ForwarderCode); }
		}

		public void ValidateForwarderCode()
		{
			ForwarderCodeInfo.ClearAllNotifications();
			CheckForwarderCode(ForwarderCodeInfo);
		}

		#endregion

		#region AgentCode

		public ZString AgentCode
		{
			get { return agentCode; }
			set
			{
				if (value != agentCode)
				{
					SetNonPersistentPropertyValue(AgentCodeInfo, ref agentCode, value);
					if (!IsValidationSuspended)
					{
						ValidateAgentCode();
					}
				}
			}
		}
		ZString agentCode;

		public ZPropertyInfo AgentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AgentCode); }
		}

		public void ValidateAgentCode()
		{
			AgentCodeInfo.ClearAllNotifications();
			CheckAgentCode(AgentCodeInfo);
		}

		#endregion

		#endregion

		#region Lookups

		public RefUNLOCOCollection PortList
		{
			get
			{
				if (portList == null)
				{
					portList = new RefUNLOCOCollection(CurrentFactory);
				}

				return portList;
			}
		}
		RefUNLOCOCollection portList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePort();
			ValidatePCS();
			ValidateForwarderCode();
			ValidateAgentCode();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Port, Port);
			writer.WriteElementString(Schema.PCS, PCS);
			writer.WriteElementString(Schema.ForwarderCode, ForwarderCode);
			writer.WriteElementString(Schema.AgentCode, AgentCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Port = reader.ReadElementString(Schema.Port);
			PCS = reader.ReadElementString(Schema.PCS);
			ForwarderCode = reader.ReadElementString(Schema.ForwarderCode);
			AgentCode = reader.ReadElementString(Schema.AgentCode);
		}

		#endregion

		#region Implementation

		void SetPCS()
		{
			var filter = new ZQuery(RefLocoMapSchema.RY_RL_NKLocoPort, Port);
			filter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, LocoMapSystemUsageList.Codes.PCS);

			PCS = CurrentFactory.LoadTop1<RefLocoMap>(filter)?.RY_LocalPortCode ?? ZString.Empty;
		}

		void CheckPort(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(Port))
			{
				propertyInfo.AddError(Res.GetString("0DE69CEC-01A9-4EFD-BA42-CE6D30BD0F12", "Port Code is required."));
			}

			if (!Constants.CountryCodes.IsFranceOrTerritory(Port.Left(2)))
			{
				propertyInfo.AddError(Res.GetString("D9AF3F39-6783-4AD9-BA92-F1A87541867D", "Please select a port in France or Territories."));
			}

			if (ParentCollections.Count > 0 && ((CommunitySystemCodesOfForwarderAndAgentCollection)ParentCollections.First()).IsDuplicateItem(this))
			{
				propertyInfo.AddError(Res.GetString("632ECC5E-572D-4EBC-AFC1-090EE926963D", "{0} already exists.", Port));
			}
		}

		void CheckPCS(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(PCS))
			{
				propertyInfo.AddError(Res.GetString("1A07EA89-64AD-495D-9243-022022119F0E", "PCS Code is required (UNLOCO > Usage Code PCS)."));
			}
		}

		void CheckForwarderCode(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(ForwarderCode))
			{
				propertyInfo.AddError(Res.GetString("7F0C9289-8572-4C50-A787-02184FC14154", "Forwarder Code is required."));
			}
		}

		void CheckAgentCode(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(AgentCode))
			{
				propertyInfo.AddError(Res.GetString("27F78AD1-33EC-4E8D-B3DC-F41582BE690D", "Agent Code is required."));
			}
		}

		#endregion
	}
}
