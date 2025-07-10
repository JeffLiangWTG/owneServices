using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	class DummyParent
	{
		[Key]
		public Guid PK { get; set; }
		public int ID { get; set; }
		public string Name { get; set; }
		public ICollection<DummyCode> DummyCodes { get; set; } = new List<DummyCode>();
	}

	class DummyCode
	{
		[Key]
		public Guid PK { get; set; }
		public string Code { get; set; }
	}
}
