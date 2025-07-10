using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	public class Dummy
	{
		public Guid XXX_PK { get; set; }

		public ICollection<string> Children { get; set; }

		public Guid XXX_YYY_Parent { get; set; }

		public string XXX_StringProperty { get; set; }
	}

	public class DummyWithId
	{
		public Guid Id { get; set; }

		public Guid YYY_Parent { get; set; }

		public string StringProperty { get; set; }
	}
}
