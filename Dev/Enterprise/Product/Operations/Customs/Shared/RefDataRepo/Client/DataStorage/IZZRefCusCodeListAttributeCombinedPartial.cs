namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	[ReferenceXMLMapping("RefCusCodeListAttribute")]
	[ReferenceXMLMappingType(typeof(IZZRefCusCodeListAttributeCombinedMetadata))]
	public partial interface IZZRefCusCodeListAttributeCombined
	{
	}

	interface IZZRefCusCodeListAttributeCombinedMetadata
	{
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsAir { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsSea { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsFix { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsRai { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsRoa { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsMai { get; set; }
		[ReferenceXMLMapping("RefCusCodeOrAttributeTransportModes.ZZU_TransportMode")]
		bool? ZZE_IsInw { get; set; }
	}
}
