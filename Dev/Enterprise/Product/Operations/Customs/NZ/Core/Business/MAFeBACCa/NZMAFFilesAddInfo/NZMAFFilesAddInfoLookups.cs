//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZMAFFilesAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZMAFFilesAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class NZMAFFilesAddInfoLookups : AutoNZMAFFilesAddInfoLookups
	{
		public NZMAFFilesAddInfoLookups(AutoNZMAFFilesAddInfo parent)
			: base(parent)
		{
		}

		public AvailableEDocList AvailableEDocs
		{
			get
			{
				if (mafMessagingCached == null)
				{
					var fileAddInfo = Parent as MAFFile;
					if (fileAddInfo != null && fileAddInfo.MAFMessaging != null)
					{
						mafMessagingCached = fileAddInfo.MAFMessaging;
					}
					else
					{
						return new AvailableEDocList();
					}
				}
				return mafMessagingCached.AvailableEDocs;
			}
		}

		MAFMessagingBO mafMessagingCached;

		public DocumentTypeList DocumentTypes
		{
			get { return Factory.GetCachedValue<DocumentTypeList>(); }
		}
	}
}
