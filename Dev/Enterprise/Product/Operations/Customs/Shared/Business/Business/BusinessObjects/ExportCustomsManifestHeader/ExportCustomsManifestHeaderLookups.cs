//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportCustomsManifestHeaderLookups
//
//    This class should be used for overriding collections in AutoExportCustomsManifestHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestHeaderLookups : AutoExportCustomsManifestHeaderLookups
	{
		public ExportCustomsManifestHeaderLookups(AutoExportCustomsManifestHeader parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ManifestTypeList
		{
			get
			{
				if (fManifestTypeList == null)
				{
					fManifestTypeList = new ManifestTypeList();
				}
				return fManifestTypeList;
			}
		}
		protected CodeDescriptionPairList fManifestTypeList;

		public virtual CodeDescriptionPairList ModeOfTransportList
		{
			get
			{
				if (fModeOfTransportList == null)
				{
					fModeOfTransportList = new ManifestTransportModeList();
				}
				return fModeOfTransportList;
			}
		}
		protected CodeDescriptionPairList fModeOfTransportList;

		public virtual CodeDescriptionPairList DocumentStatusList
		{
			get
			{
				if (fDocumentStatusList == null)
				{
					fDocumentStatusList = new CodeDescriptionPairList();
				}
				return fDocumentStatusList;
			}
		}
		CodeDescriptionPairList fDocumentStatusList;

		public virtual CodeDescriptionPairList DocumentStatusConditionsList
		{
			get
			{
				if (fDocumentStatusConditionsList == null)
				{
					fDocumentStatusConditionsList = new CodeDescriptionPairList();
				}
				return fDocumentStatusConditionsList;
			}
		}
		CodeDescriptionPairList fDocumentStatusConditionsList;

		public virtual OrgHeaderCollection OrgHeaders
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual RefVesselCollection Vessels
		{
			get { return new RefVesselCollection(Factory); }
		}
	}
}
