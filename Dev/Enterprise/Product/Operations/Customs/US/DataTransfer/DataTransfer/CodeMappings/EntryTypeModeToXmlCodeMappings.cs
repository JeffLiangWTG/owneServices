using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class EntryTypeModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		EntryTypeModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(EntryModeList.Codes.RLF, nameof(Xsd.USDeclarationEntryTypeMode.RLF));
			yield return new Mapping(EntryModeList.Codes.Paired, nameof(Xsd.USDeclarationEntryTypeMode.PAI));
		}

		public static readonly EntryTypeModeToXmlCodeMappings Instance = new EntryTypeModeToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Entry Mode"; }
		}

		public new Xsd.USDeclarationEntryTypeMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationEntryTypeMode.RLF, errorContext, notify);
		}
	}
}
