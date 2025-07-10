using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class UNDGAttributeZZ
{
	[ForeignKey("DAZ_ParentPK")]
	public virtual UNDGSubstanceRID UNDGSubstanceRID { get; set; }
	[ForeignKey("DAZ_ParentPK")]
	public virtual UNDGSubstanceADR UNDGSubstanceADR { get; set; }
	[ForeignKey("DAZ_ParentPK")]
	public virtual UNDGSubstanceADN UNDGSubstanceADN { get; set; }
	[ForeignKey("DAZ_ParentPK")]
	public virtual UNDGSubstanceJTT UNDGSubstanceJTT { get; set; }
	[ForeignKey("DAZ_ParentPK")]
	public virtual UNDGSubstanceCFR UNDGSubstanceCFR { get; set; }
}
