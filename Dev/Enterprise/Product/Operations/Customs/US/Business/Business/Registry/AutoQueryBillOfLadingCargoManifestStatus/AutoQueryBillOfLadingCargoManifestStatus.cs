using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class AutoQueryBillOfLadingCargoManifestStatus : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string SendBasedOnETA = "SendBasedOnETA";
			public const string SendOnFirstSave = "SendOnFirstSave";
			public const string UpdateEntryWithResults = "UpdateEntryWithResults";
		}

		#region SendBasedOnETA

		public ZBool SendBasedOnETA
		{
			get { return sendBasedOnETA; }
			set
			{
				SetNonPersistentPropertyValue(SendBasedOnETAInfo, ref sendBasedOnETA, value);
			}
		}
		ZBool sendBasedOnETA;

		public ZPropertyInfo SendBasedOnETAInfo
		{
			get { return GetZPropertyInfo(Schema.SendBasedOnETA); }
		}

		#endregion

		#region SendOnFirstSave

		public ZBool SendOnFirstSave
		{
			get { return sendOnFirstSave; }
			set
			{
				bool hasChanges = SendOnFirstSave != value;
				SetNonPersistentPropertyValue(SendOnFirstSaveInfo, ref sendOnFirstSave, value);

				if (hasChanges && !SendOnFirstSave)
				{
					UpdateEntryWithResults = false;
				}
			}
		}
		ZBool sendOnFirstSave;

		public ZPropertyInfo SendOnFirstSaveInfo
		{
			get { return GetZPropertyInfo(Schema.SendOnFirstSave); }
		}

		#endregion

		#region UpdateEntryWithResults

		public ZBool UpdateEntryWithResults
		{
			get { return updateEntryWithResults; }
			set
			{
				SetNonPersistentPropertyValue(UpdateEntryWithResultsInfo, ref updateEntryWithResults, value);
			}
		}
		ZBool updateEntryWithResults;

		public ZPropertyInfo UpdateEntryWithResultsInfo
		{
			get { return GetZPropertyInfo(Schema.UpdateEntryWithResults); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoQueryBillOfLadingCargoManifestStatus();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SendBasedOnETA, SendBasedOnETA ? "Y" : "N");
			writer.WriteElementString(Schema.SendOnFirstSave, SendOnFirstSave ? "Y" : "N");
			writer.WriteElementString(Schema.UpdateEntryWithResults, UpdateEntryWithResults ? "Y" : "N");
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SendBasedOnETA = reader.ReadElementString(Schema.SendBasedOnETA) == "Y";
			SendOnFirstSave = reader.ReadElementString(Schema.SendOnFirstSave) == "Y";
			UpdateEntryWithResults = reader.ReadElementString(Schema.UpdateEntryWithResults) == "Y";
		}
	}
}
