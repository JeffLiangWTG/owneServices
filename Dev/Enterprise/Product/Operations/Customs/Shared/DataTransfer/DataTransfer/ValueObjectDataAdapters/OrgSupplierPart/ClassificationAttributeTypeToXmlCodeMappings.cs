using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	[Immutable]
	public class ClassificationAttributeTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ClassificationAttributeTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(nameof(CusAttributeFilter.AttributeFilterName.AT1), nameof(Xsd.ClassificationAttributeType.AT1));
			yield return new Mapping(nameof(CusAttributeFilter.AttributeFilterName.AT2), nameof(Xsd.ClassificationAttributeType.AT2));
			yield return new Mapping(nameof(CusAttributeFilter.AttributeFilterName.AT3), nameof(Xsd.ClassificationAttributeType.AT3));
		}

		public static readonly ClassificationAttributeTypeToXmlCodeMappings Instance = new ClassificationAttributeTypeToXmlCodeMappings();

		public new Xsd.ClassificationAttributeType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ClassificationAttributeType.AT1, errorContext, notifications);
		}

		protected override string Name
		{
			get { return Res.GetString("59181c5b-328d-4667-8b2f-570c19cde74f", "Classification Attribute Type"); }
		}
	}
}
