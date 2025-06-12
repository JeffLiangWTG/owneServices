using System.Collections.Generic;

namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
    public class OCMIndexModel
    {
		public List<Option> Options { get; set; }

        public string Selected { get; set; }
    }

	public class Option
	{
		public string OptionID { get; set; }
		public string Description { get; set; }
	}
}