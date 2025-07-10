using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class UNDGSubstanceADN
{
	public virtual ICollection<UNDGAttributeZZ> UNDGAttributeZZs { get; set; } = new List<UNDGAttributeZZ>();
}
