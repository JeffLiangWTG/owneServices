namespace Enterprise.MasterFiles.Integration
{
	using CargoWise.Types;
	using Enterprise.Integration.ZArchitecture;

	public interface IRefEquipment
	{
		ZGuid PK { get; }
		ZString RQ_ShortCode { get; set; }
		ZString RQ_EquipmentGroup { get; set; }
	}

	public interface IRefEquipmentModuleColumnsAndFiltersProvider
	{
		void AddFilters(IModuleFilterCollection filters);
		void AddColumns(IFilterControl filterControl);
	}
}