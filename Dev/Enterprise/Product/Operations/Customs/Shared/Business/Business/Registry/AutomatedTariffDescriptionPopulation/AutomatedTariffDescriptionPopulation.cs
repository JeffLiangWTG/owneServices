using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class AutomatedTariffDescriptionPopulation : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string EnableCommercialInvoice = "EnableCommercialInvoice";
			public const string EnableCustomsDeclaration = "EnableCustomsDeclaration";
		}

		#endregion

		#region Constructions and cloning

		public AutomatedTariffDescriptionPopulation()
			: base()
		{
			enableCommercialInvoice = true;
			enableCustomsDeclaration = true;
		}

		public AutomatedTariffDescriptionPopulation(BusinessObjectFactory factory)
			: base(factory)
		{
			enableCommercialInvoice = true;
			enableCustomsDeclaration = true;
		}

		public AutomatedTariffDescriptionPopulation(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			enableCommercialInvoice = true;
			enableCustomsDeclaration = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutomatedTariffDescriptionPopulation(fallbackLevel, factory);
		}

		#endregion

		#region Read / Write Elements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableCommercialInvoice = reader.ReadElementStringAsZBool(Schema.EnableCommercialInvoice);
			EnableCustomsDeclaration = reader.ReadElementStringAsZBool(Schema.EnableCustomsDeclaration);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.EnableCommercialInvoice, EnableCommercialInvoice.ToString());
			writer.WriteElementString(Schema.EnableCustomsDeclaration, EnableCustomsDeclaration.ToString());
		}

		#endregion

		#region EnableCommercialInvoice

		[ResourceStringData("41B9B62F-8E61-4D37-B33A-F46BE10B364F", Caption = "Commercial Invoice")]
		public ZBool EnableCommercialInvoice
		{
			get
			{
				return enableCommercialInvoice;
			}
			set
			{
				SetNonPersistentPropertyValue(EnableCommercialInvoiceInfo, ref enableCommercialInvoice, value);
			}
		}
		ZBool enableCommercialInvoice;

		public ZPropertyInfo EnableCommercialInvoiceInfo
		{
			get { return GetZPropertyInfo(Schema.EnableCommercialInvoice); }
		}

		#endregion

		#region EnableCustomsDeclaration

		[ResourceStringData("4F3AFF72-AB4D-4001-858C-F81791FCC93E", Caption = "Customs Declaration")]
		public ZBool EnableCustomsDeclaration
		{
			get
			{
				return enableCustomsDeclaration;
			}
			set
			{
				SetNonPersistentPropertyValue(EnableCustomsDeclarationInfo, ref enableCustomsDeclaration, value);
			}
		}
		ZBool enableCustomsDeclaration;

		public ZPropertyInfo EnableCustomsDeclarationInfo
		{
			get { return GetZPropertyInfo(Schema.EnableCustomsDeclaration); }
		}

		#endregion
	}
}
