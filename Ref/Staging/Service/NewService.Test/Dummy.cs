using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	public class Dummy
	{
		public Guid XXX_PK { get; set; }
		public ICollection<string> Children { get; set; }
		public Guid XXX_YYY_Parent { get; set; }
		public string XXX_StringProperty { get; set; }
	}
}
