using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class MultiPersonMergerResult
	{
		public BusinessObjectFactory NewFactory { get; set; }
		public BusinessObjectFactory OriginalFactory { get; set; }
		public GlbPerson DissolvedPerson { get; set; }
		public bool IsSuccessful { get; set; }
	}
}
