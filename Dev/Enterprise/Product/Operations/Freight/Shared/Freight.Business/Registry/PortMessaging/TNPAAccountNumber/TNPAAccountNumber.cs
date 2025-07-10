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
	public class TNPAAccountNumber : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Port = "Port";
			public const string ImportNumber = "ImportNumber";
			public const string ExportNumber = "ExportNumber";
			public const string CoastwiseNumber = "CoastwiseNumber";

			public const int PortMaxLength = 5;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TNPAAccountNumber();
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

					if (!IsValidationSuspended)
					{
						ValidatePort();
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

		#region ImportNumber

		public ZString ImportNumber
		{
			get { return importNumber; }
			set
			{
				if (value != importNumber)
				{
					SetNonPersistentPropertyValue(ImportNumberInfo, ref importNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateImportNumber();
					}
				}
			}
		}
		ZString importNumber;

		public ZPropertyInfo ImportNumberInfo => GetZPropertyInfo(Schema.ImportNumber);

		public void ValidateImportNumber()
		{
			ImportNumberInfo.ClearAllNotifications();
			CheckNumbers(ImportNumberInfo);
		}

		#endregion

		#region ExportNumber

		public ZString ExportNumber
		{
			get { return exportNumber; }
			set
			{
				if (value != exportNumber)
				{
					SetNonPersistentPropertyValue(ExportNumberInfo, ref exportNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateExportNumber();
					}
				}
			}
		}
		ZString exportNumber;

		public ZPropertyInfo ExportNumberInfo => GetZPropertyInfo(Schema.ExportNumber);

		public void ValidateExportNumber()
		{
			ExportNumberInfo.ClearAllNotifications();
			CheckNumbers(ExportNumberInfo);
		}

		#endregion

		#region CoastwiseNumber

		public ZString CoastwiseNumber
		{
			get { return coastwiseNumber; }
			set
			{
				if (value != coastwiseNumber)
				{
					SetNonPersistentPropertyValue(CoastwiseNumberInfo, ref coastwiseNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateCoastwiseNumber();
					}
				}
			}
		}
		ZString coastwiseNumber;

		public ZPropertyInfo CoastwiseNumberInfo => GetZPropertyInfo(Schema.CoastwiseNumber);

		public void ValidateCoastwiseNumber()
		{
			CoastwiseNumberInfo.ClearAllNotifications();
			CheckNumbers(CoastwiseNumberInfo);
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
					portList = new RefUNLOCOCollection(CurrentFactory, new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.SouthAfrica));
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
			ValidateImportNumber();
			ValidateExportNumber();
			ValidateCoastwiseNumber();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Port, Port);
			writer.WriteElementString(Schema.ImportNumber, ImportNumber);
			writer.WriteElementString(Schema.ExportNumber, ExportNumber);
			writer.WriteElementString(Schema.CoastwiseNumber, CoastwiseNumber);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Port = reader.ReadElementString(Schema.Port);
			ImportNumber = reader.ReadElementString(Schema.ImportNumber);
			ExportNumber = reader.ReadElementString(Schema.ExportNumber);
			CoastwiseNumber = reader.ReadElementString(Schema.CoastwiseNumber);
		}

		#endregion

		#region Implementation

		void CheckPort(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(Port))
			{
				propertyInfo.AddError(Res.GetString("1bd46eeb-da0e-41e5-834c-e4c96aaf20ea", "Port Code is required."));
			}
			else if (!Port.StartsWith(Constants.CountryCodes.SouthAfrica))
			{
				propertyInfo.AddError(Res.GetString("AEDB56CE-23B0-4C4E-B715-AC00A364E911", "Port must be in South Africa."));
			}

			if (ParentCollections.Count > 0 && ((TNPAAccountNumberCollection)ParentCollections.First()).IsDuplicateItem(this))
			{
				propertyInfo.AddError(Res.GetString("c0994962-7412-4d16-aa3c-42e7b99bd3ab", "{0} already exists.", Port));
			}
		}

		void CheckNumbers(ZPropertyInfo propertyInfo)
		{
			if (string.IsNullOrEmpty(ImportNumber) && string.IsNullOrEmpty(ExportNumber) && string.IsNullOrEmpty(CoastwiseNumber))
			{
				propertyInfo.AddError(Res.GetString("A989E149-628B-4268-8D2C-1660D66AF3C0", "Either Import Number, Export Number or Coastwise Number is required."));
			}
		}

		#endregion
	}
}
