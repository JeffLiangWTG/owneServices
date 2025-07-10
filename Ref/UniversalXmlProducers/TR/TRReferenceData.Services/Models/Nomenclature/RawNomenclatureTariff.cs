using FlexCel.Core;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class RawNomenclatureTariff
	{
		public int SeqNum { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public string UOM { get; set; }
		public string DutyRate { get; set; }
		public bool IsSubchapter { get; set; }
		public NomenclatureDescription ParsedDescription { get; set; }
		public Nomenclature HSCodeInfo { get; set; }
		public TFlxFormat DescriptionFormat { get; set; }
		public bool IsHorizontalCenter => DescriptionFormat != null && DescriptionFormat.HAlignment == THFlxAlignment.center;
	}
}
