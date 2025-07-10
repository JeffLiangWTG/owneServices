//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessQueueLookups
//
//    This class should be used for overriding collections in AutoProcessQueueLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessQueueLookups : AutoProcessQueueLookups
	{
		public ProcessQueueLookups(AutoProcessQueue parent) : base(parent)
		{
		}

		#region Customs

		#region CustomsQueueList

		public CodeDescriptionPairList CustomsQueueList
		{
			get
			{
				if (fCustomsQueueList == null || !CustomsQueueListShouldBeCached)
				{
					fCustomsQueueList = GetCustomsQueueList();
				}
				return fCustomsQueueList;
			}
		}

		protected virtual CodeDescriptionPairList GetCustomsQueueList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CustomsQueueListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCustomsQueueList;

		#endregion

		#region CustomsStatusList

		public CodeDescriptionPairList CustomsStatusList
		{
			get
			{
				if (fCustomsStatusList == null || !CustomsStatusListShouldBeCached)
				{
					fCustomsStatusList = GetCustomsStatusList();
				}
				return fCustomsStatusList;
			}
		}

		protected virtual CodeDescriptionPairList GetCustomsStatusList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CustomsStatusListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCustomsStatusList;

		#endregion

		#region CustomsSubStatusList

		public CodeDescriptionPairList CustomsSubStatusList
		{
			get
			{
				if (fCustomsSubStatusList == null || !CustomsSubStatusListShouldBeCached)
				{
					fCustomsSubStatusList = GetCustomsSubStatusList();
				}
				return fCustomsSubStatusList;
			}
		}

		protected virtual CodeDescriptionPairList GetCustomsSubStatusList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CustomsSubStatusListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCustomsSubStatusList;

		#endregion

		#endregion

		#region Commercial

		#region CommercialQueueList

		public CodeDescriptionPairList CommercialQueueList
		{
			get
			{
				if (fCommercialQueueList == null || !CommercialQueueListShouldBeCached)
				{
					fCommercialQueueList = GetCommercialQueueList();
				}
				return fCommercialQueueList;
			}
		}

		protected virtual CodeDescriptionPairList GetCommercialQueueList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CommercialQueueListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCommercialQueueList;

		#endregion

		#region CommercialStatusList

		public CodeDescriptionPairList CommercialStatusList
		{
			get
			{
				if (fCommercialStatusList == null || !CommercialStatusListShouldBeCached)
				{
					fCommercialStatusList = GetCommercialStatusList();
				}
				return fCommercialStatusList;
			}
		}

		protected virtual CodeDescriptionPairList GetCommercialStatusList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CommercialStatusListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCommercialStatusList;

		#endregion

		#region CommercialSubStatusList

		public CodeDescriptionPairList CommercialSubStatusList
		{
			get
			{
				if (fCommercialSubStatusList == null || !CommercialSubStatusListShouldBeCached)
				{
					fCommercialSubStatusList = GetCommercialSubStatusList();
				}
				return fCommercialSubStatusList;
			}
		}

		protected virtual CodeDescriptionPairList GetCommercialSubStatusList()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual bool CommercialSubStatusListShouldBeCached
		{
			get { return true; }
		}

		CodeDescriptionPairList fCommercialSubStatusList;

		#endregion

		#endregion
	}
}
