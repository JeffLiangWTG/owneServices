using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New;

public partial class UNDGSubstanceJTT
{
	public virtual ICollection<UNDGAttributeZZ> UNDGAttributeZZs { get; set; } = new List<UNDGAttributeZZ>();
}
