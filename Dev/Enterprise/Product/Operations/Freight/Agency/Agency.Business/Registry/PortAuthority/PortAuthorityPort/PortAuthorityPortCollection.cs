using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class PortAuthorityPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new PortAuthorityPort this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PortAuthorityPort)Elements[index]; }
		}

		[System.Diagnostics.DebuggerStepThrough]
		public new PortAuthorityPort AddNew()
		{
			return (PortAuthorityPort)base.AddNew();
		}

		public PortAuthorityPort FindByPort(ZString portCode)
		{
			foreach (PortAuthorityPort port in this)
			{
				if (port.Port == portCode)
				{
					return port;
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not localisable")]
		public static PortAuthorityPortCollection NewWithDefaultValues()
		{
			PortAuthorityPortCollection collection = new PortAuthorityPortCollection();
			AddValue(collection, "AUBNE", PortAuthorityVersionList.Codes.V20, "pbcmanifests@portbris.com.au", (NoResString)"PORT OF BRISBANE");
			AddValue(collection, "AUMEL", PortAuthorityVersionList.Codes.V20, "manifest@portofmelbourne.com", (NoResString)"PORT OF MELBOURNE", "tmanifes@portofmelbourne.com", "PORT OF MELBOURNE");
			AddValue(collection, "AUSYD", PortAuthorityVersionList.Codes.V20, "manifest@sydneyports.com.au", "SYDNEY PORTS", "manitest@sydneyports.com.au", "SYDNEY PORTS");
			AddValue(collection, "AUPKL", PortAuthorityVersionList.Codes.V20, "ar@portkembla.com.au", "PORT KEMBLA");
			return collection;
		}

		#region Implementation

		static void AddValue(PortAuthorityPortCollection collection,
			ZString port, ZString version,
			ZString productionEmail, ZString productionID)
		{
			AddValue(collection, port, version, productionEmail, productionID, "", "");
		}

		static void AddValue(PortAuthorityPortCollection collection,
			ZString port, ZString version,
			ZString productionEmail, ZString productionID,
			ZString testingEmail, ZString testingID)
		{
			PortAuthorityPort value = collection.AddNew();
			value.Port = port;
			value.Version = version;
			value.ProductionEmail = productionEmail;
			value.ProductionID = productionID;
			value.TestingEmail = testingEmail;
			value.TestingID = testingID;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PortAuthorityPortCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortAuthorityPort();
		}

		#endregion
	}
}


