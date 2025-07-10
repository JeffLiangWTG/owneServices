using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	[ReferenceXMLMapping("RefCusCodeList")]
	[ReferenceXMLMappingType(typeof(IZZRefCusCodeListCombinedMetadata))]
	[CodeAlive("Partial interface")]
	public partial interface IZZRefCusCodeListCombined
	{
		IEnumerable<IZZRefCusCodeListAttributeCombined> ZZRefCusCodeListAttributeCombined { get; }
	}

	interface IZZRefCusCodeListCombinedMetadata
	{
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsAir { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsSea { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsFix { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsRai { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsRoa { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsMai { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZD_IsInw { get; set; }
		[ReferenceXMLMapping("ZZD_ZZZ_NKDataGrouping")]
		string ZZD_CountryOrGrouping { get; set; }
		[ReferenceXMLMapping("ZZD_ZZK_NKCodeType")]
		string ZZD_CodeType { get; set; }
	}
}
