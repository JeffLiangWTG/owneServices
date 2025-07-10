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

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public ISFCustomisationRegistryDataType()
			: base(new ISFNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("B8A470A2-41EA-4E97-864E-4CE7003D63C1", "Importer Security Filing Number");
			MaxLength = Math.Min(CusISFHeaderSchema.BF_JobReference.MaxLength, JobHeaderSchema.JH_JobNum.MaxLength);
		}

		protected override Type DataTypeCore
		{
			get { return typeof(ISFNumberCustomisation); }
		}
	}

	[XmlSerializerAssembly("Enterprise.Customs.US.ISF.Business.XmlSerializers")]
	public class ISFNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public ISFNumberCustomisation()
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
			return new ISFNumberCustomisation();
		}
	}
}
