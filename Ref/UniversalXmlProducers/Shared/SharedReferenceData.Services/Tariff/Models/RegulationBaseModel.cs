namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public abstract class RegulationBaseModel : LoaderModel, IRegulationData
	{
		public string RegulationId { get; set; }
		public string RegulationRoleTypeId { get; set; }
	}
}
