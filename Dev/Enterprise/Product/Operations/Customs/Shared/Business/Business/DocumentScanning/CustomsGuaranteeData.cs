using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CustomsGuaranteeData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsGuarantee)]

namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.Customs.Business.DocumentScanning;
	using Enterprise.ZArchitecture.Core;

	public class CustomsGuaranteeData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(BaseCusGuaranteeHeader);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("3A61E619-918B-4571-86AA-C7C946186EE0", "Guarantee");
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new CustomsGuaranteeEDocsViaUniversalXmlSupport();
	}
}
