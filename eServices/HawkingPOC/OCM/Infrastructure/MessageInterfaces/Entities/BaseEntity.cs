using System;

namespace OcmPoc.Infrastructure.MessageInterfaces.Entities
{
	public class BaseEntity
    {
		public int Id { get; set; }
		public Guid Guid { get; set; } = Guid.NewGuid();
		public DateTime Created { get; set; }
		public DateTime LastUpdate { get; set; }
	}
}
