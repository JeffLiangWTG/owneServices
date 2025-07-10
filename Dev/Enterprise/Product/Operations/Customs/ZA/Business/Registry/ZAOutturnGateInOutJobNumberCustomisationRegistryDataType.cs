using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class ZAOutturnGateInOutJobNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public ZAOutturnGateInOutJobNumberCustomisationRegistryDataType()
			: base(new ZAOutturnGateInOutJobNumberCustomisation())
		{
			GeneratedNumberName = ZA.Business.ResString.GetMultilingualString("3C5BEBD5-8BE7-4BB8-9D28-771E3A802978", "Outturn & Gate In/Out Job Number");
			MaxLength = AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;
		}

		protected override Type DataTypeCore => typeof(ZAOutturnGateInOutJobNumberCustomisation);
	}

	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class ZAOutturnGateInOutJobNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public ZAOutturnGateInOutJobNumberCustomisation()
		{
			UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "7";

			foreach (BillOfLadingNumberCustomisationElement unFilteredElement in UnFilteredElements.ToArray())
			{
				if (!unFilteredElement.Matches(NumberCustomisationElementCategories.Standard))
				{
					UnFilteredElements.Remove(unFilteredElement);
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ZAOutturnGateInOutJobNumberCustomisation();
		}
	}
}
