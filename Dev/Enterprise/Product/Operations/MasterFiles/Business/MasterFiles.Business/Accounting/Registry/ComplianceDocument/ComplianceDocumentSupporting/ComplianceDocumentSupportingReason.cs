using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentSupportingReason : RegistryBusinessObject
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentSupportingReason();
		}

		protected override int MaxDescriptionLength
		{
			get { return 80; }
		}

		protected override void ValidateDescriptionCore()
		{
			base.ValidateDescriptionCore();
			MandatoryValidation.CheckEntered(DescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("5486E165-95E1-4BF0-82B7-25CDD0AF40C2", "Description"));
		}

		internal int MaxDescriptionLengthInternal => MaxDescriptionLength;
	}
}
