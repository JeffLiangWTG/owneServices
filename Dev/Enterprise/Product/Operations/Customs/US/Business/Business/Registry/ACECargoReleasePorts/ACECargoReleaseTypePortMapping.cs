using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class ACECargoReleaseTypePortMapping : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string CertificationOption = "CertificationOption";
			public const int CertificationOptionMaxLength = 3;

			public const string PortsAndModesCount = "PortsAndModesCount";
			public const string CertificationMethodFormat = "CertificationMethod_{0}";
			public const string PortFormat = "Port_{0}";
			public const string TransportModeFormat = "TransportMode_{0}";
		}

		#region Properties

		[List(nameof(CertificationOptionsList))]
		[MaxLength(Schema.CertificationOptionMaxLength)]
		public ZString CertificationOption
		{
			get { return сertificationOption; }
			set
			{
				SetNonPersistentPropertyValue(CertificationOptionInfo, ref сertificationOption, value);

				if (!CertificationOption.IsEmpty)
				{
					PortsAndModes.RemoveAndDeleteAll();
				}
				if (!IsValidationSuspended)
				{
					ValidateCertificationOption();
				}
			}
		}
		ZString сertificationOption;

		public ZPropertyInfo CertificationOptionInfo
		{
			get { return GetZPropertyInfo(Schema.CertificationOption); }
		}

		public void ValidateCertificationOption()
		{
			CertificationOptionInfo.ClearAllNotifications();
			CheckCertificationOption();
		}

		protected void CheckCertificationOption()
		{
			ListValidation.ErrorIfInvalidCode(CertificationOptionInfo, CertificationOptionsList);
		}

		public CertificationOptionsList CertificationOptionsList
		{
			get { return CurrentFactory.GetCachedValue<CertificationOptionsList>(); }
		}

		public PortsAndModesCollection PortsAndModes
		{
			get
			{
				if (portsAndModes == null)
				{
					portsAndModes = new PortsAndModesCollection(CurrentFactory);
					RegisterEditableChildObject(portsAndModes);
				}
				return portsAndModes;
			}
		}
		PortsAndModesCollection portsAndModes;

		public void ValidatePortsAndModes()
		{
			PortsAndModes.RunPreSaveValidation();
		}

		internal ZString GetCertificationMethod(ZString entryPort, ZString transportMode)
		{
			var result = CertificationOption;
			if (result.IsEmpty)
			{
				var details = PortsAndModes.OfType<PortsAndModes>().FirstOrDefault(x => x.Port == entryPort && (x.TransportMode == transportMode || x.TransportMode.IsEmpty));
				if (details != null)
				{
					result = details.CertificationMethod;
				}
			}
			return result;
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var data = new ACECargoReleaseTypePortMapping();
			using (data.GetValidationSuspender())
			{
				data.CertificationOption = this.CertificationOption;

				foreach (PortsAndModes ports in this.PortsAndModes)
				{
					if (!ports.IsDeleted)
					{
						var clonedPorts = data.PortsAndModes.AddNew();
						clonedPorts.CertificationMethod = ports.CertificationMethod;
						clonedPorts.Port = ports.Port;
						clonedPorts.TransportMode = ports.TransportMode;
					}
				}
			}

			return data;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCertificationOption();
			ValidatePortsAndModes();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CertificationOption, CertificationOption.ToString());
			writer.WriteElementString(Schema.PortsAndModesCount, PortsAndModes.Count.ToString());
			for (int i = 0; i < PortsAndModes.Count; i++)
			{
				writer.WriteElementString(string.Format(Schema.CertificationMethodFormat, i), PortsAndModes[i].CertificationMethod);
				writer.WriteElementString(string.Format(Schema.PortFormat, i), PortsAndModes[i].Port);
				writer.WriteElementString(string.Format(Schema.TransportModeFormat, i), PortsAndModes[i].TransportMode);
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CertificationOption = reader.ReadElementString(Schema.CertificationOption);
			int portsAndModesCount = reader.ReadElementStringAsZInt(Schema.PortsAndModesCount);
			for (int i = 0; i < portsAndModesCount; i++)
			{
				var portsAndModes = new PortsAndModes(PortsAndModes);
				portsAndModes.CertificationMethod = reader.ReadElementString(string.Format(Schema.CertificationMethodFormat, i));
				portsAndModes.Port = reader.ReadElementString(string.Format(Schema.PortFormat, i));
				portsAndModes.TransportMode = reader.ReadElementString(string.Format(Schema.TransportModeFormat, i));
				PortsAndModes.Add(portsAndModes);
			}
		}

		#endregion
	}
}
