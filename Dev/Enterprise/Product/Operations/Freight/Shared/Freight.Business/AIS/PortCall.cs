using System;
using System.Collections.Generic;

namespace Enterprise.Freight.AIS
{
	public partial class Port
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Unloco { get; set; }
	}

	public class PortCall
	{
		public Port Port { get; set; }
		public DateTimeOffset? Ata { get; set; }
		public DateTimeOffset? Atd { get; set; }
		public DateTimeOffset? Eta { get; set; }
		public DateTimeOffset? Etd { get; set; }
	}

	public class PortCallPage
	{
		public ICollection<PortCall> Items { get; set; }
	}
}
