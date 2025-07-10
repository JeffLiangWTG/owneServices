using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject
	{
		readonly OrgHeader organisation;

		protected OrgHeaderWrapper(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
		}

		public static OrgHeaderWrapper New(OrgHeader organisation)
		{
			OrgHeaderWrapper result = null;

			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(organisation);
				});
			}
			return result;
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public CustomDocumentsCollection ExportCustomDocumentLabels
		{
			get
			{
				if (exportCustomDocumentLabels == null)
				{
					exportCustomDocumentLabels = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideExportDoc, organisation, Factory);
					exportCustomDocumentLabels.Load();
					exportCustomDocumentLabels.Cast<OrgCustomLabels>().ForEach(c => c.customDocumentsCollection = exportCustomDocumentLabels);
					RegisterEditableChildObject(exportCustomDocumentLabels);
				}

				return exportCustomDocumentLabels;
			}
		}

		CustomDocumentsCollection exportCustomDocumentLabels;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public CustomDocumentsCollection ImportCustomDocumentLabels
		{
			get
			{
				if (importCustomDocumentLabels == null)
				{
					importCustomDocumentLabels = new CustomDocumentsCollection(OrgConstants.CustomLabelType.OverrideImportDoc, organisation, Factory);
					importCustomDocumentLabels.Load();
					importCustomDocumentLabels.Cast<OrgCustomLabels>().ForEach(c => c.customDocumentsCollection = importCustomDocumentLabels);
					RegisterEditableChildObject(importCustomDocumentLabels);
				}

				return importCustomDocumentLabels;
			}
		}

		CustomDocumentsCollection importCustomDocumentLabels;

		TWOrgImpAddInfo fAddInfo;
		public TWOrgImpAddInfo AddInfo => fAddInfo ?? (fAddInfo = TWOrgImpAddInfo.Get(organisation));
	}
}
