using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class MergeStyleToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		MergeStyleToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(MergeStyleList.Codes.Default, nameof(Xsd.ISFActionMessagingLineMergeStyle.Default));
			yield return new Mapping(MergeStyleList.Codes.Merge, nameof(Xsd.ISFActionMessagingLineMergeStyle.Merge));
			yield return new Mapping(MergeStyleList.Codes.NotMerge, nameof(Xsd.ISFActionMessagingLineMergeStyle.NotMerge));
		}

		public static readonly MergeStyleToXmlCodeMappings Instance = new MergeStyleToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Merge Style Type"; }
		}

		public new Xsd.ISFActionMessagingLineMergeStyle GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFActionMessagingLineMergeStyle.Default, errorContext, notify);
		}
	}
}
