using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class LinkedEntryCollection : DependentCusAddInfoCollection<LinkedEntry, JobDeclaration>
	{
		public LinkedEntryCollection(JobDeclaration master)
			: base(master, CusAddInfoTypeAttribute.Codes.USLinkedEntry)
		{
		}
	}
}
