using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class EntryProcessingPortsMapping : RegistryBusinessObjectTemplate
	{
		public EntryProcessingPortsMapping()
		{
		}

		public EntryProcessingPortsMapping(FallbackLevel fallbackLevel, BusinessObjectFactory factory, EntryProcessingPortsMappingCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.parentCollection = parentCollection;
		}

		readonly EntryProcessingPortsMappingCollection parentCollection;

		public static class Schema
		{
			public const string EntryPort = "EntryPort";
			public const string ProcessingPort = "ProcessingPort";
		}

		[List(nameof(Ports))]
		[MaxLength(4)]
		public ZString EntryPort
		{
			get { return entryPort; }
			set
			{
				SetNonPersistentPropertyValue(EntryPortInfo, ref entryPort, value);

				if (!IsValidationSuspended)
				{
					ValidateEntryPort();
				}
			}
		}
		ZString entryPort;

		public ZPropertyInfo EntryPortInfo
		{
			get { return GetZPropertyInfo(Schema.EntryPort); }
		}

		public void ValidateEntryPort()
		{
			EntryPortInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EntryPortInfo, "entry port");

			if (CurrentFactory != null)
			{
				ListValidation.ErrorIfInvalidCode(EntryPortInfo, Ports);
			}

			if (parentCollection != null)
			{
				foreach (EntryProcessingPortsMapping mapping in parentCollection)
				{
					if (mapping != this && mapping.EntryPort == EntryPort)
					{
						EntryPortInfo.AddError(DuplicateEntryPort);
						break;
					}
				}
			}
		}

		internal const string DuplicateEntryPort = "This entry port is mapped already. You cannot map the same Entry Port to more than one Processing Ports.";

		public IBusinessObjectCollection Ports
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(CurrentFactory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		[List(nameof(Ports))]
		[MaxLength(4)]
		public ZString ProcessingPort
		{
			get { return processingPort; }
			set
			{
				SetNonPersistentPropertyValue(ProcessingPortInfo, ref processingPort, value);

				if (!IsValidationSuspended)
				{
					ValidateProcessingPort();
				}
			}
		}
		ZString processingPort;

		public ZPropertyInfo ProcessingPortInfo
		{
			get { return GetZPropertyInfo(Schema.ProcessingPort); }
		}

		void ValidateProcessingPort()
		{
			ProcessingPortInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ProcessingPortInfo, "processing port");

			if (CurrentFactory != null)
			{
				ListValidation.ErrorIfInvalidCode(ProcessingPortInfo, Ports);
			}

			if (!FormalImportJobDeclarationValidation.IsInSameDistrict(EntryPort, ProcessingPort))
			{
				ProcessingPortInfo.AddError(EntryPortPrcessingPortShouldBeInTheSameDistrict);
			}
		}

		public const string EntryPortPrcessingPortShouldBeInTheSameDistrict = "Processing Port and Entry Port should be in the same district.";

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEntryPort();
			ValidateProcessingPort();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntryProcessingPortsMapping(fallbackLevel, factory, null);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EntryPort, EntryPort);
			writer.WriteElementString(Schema.ProcessingPort, ProcessingPort);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			EntryPort = reader.ReadElementString(Schema.EntryPort);
			ProcessingPort = reader.ReadElementString(Schema.ProcessingPort);
		}

		#endregion
	}
}
