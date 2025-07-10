using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class TripCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public TripCustomisationRegistryDataType()
			: base(new TripNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("13EA646B-4729-43E7-B996-7EDC7167B38B", "e-Manifest Number");
			MaxLength = CusInBondHeaderSchema.BH_JobReference.MaxLength;
		}

		protected override Type DataTypeCore
		{
			get { return typeof(TripNumberCustomisation); }
		}
	}

	[XmlSerializerAssembly("Enterprise.Customs.US.eManifest.Business.XmlSerializers")]
	public class TripNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public TripNumberCustomisation()
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
			return new TripNumberCustomisation();
		}
	}
}
