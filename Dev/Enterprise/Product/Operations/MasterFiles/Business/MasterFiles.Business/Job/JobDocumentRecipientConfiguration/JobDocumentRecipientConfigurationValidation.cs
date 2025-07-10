using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientConfigurationValidation : ZValidation
	{
		public JobDocumentRecipientConfigurationValidation(JobDocumentRecipientConfiguration parent) : base(parent)
		{
			this.parent = parent;
		}
		readonly JobDocumentRecipientConfiguration parent;

		public override Type AutoValidationType => typeof(JobDocumentRecipientConfigurationValidation);

		public override void ValidateAll()
		{
			ValidateOrganisationPKFilter();
			ValidateDocumentGroupFilter();
			ValidateDocumentPKFilter();
		}

		#region OrganisationPKFilter

		public void ValidateOrganisationPKFilter()
		{
			ValidateCalculatedProperty(parent.OrganisationPKFilterInfo);
		}

		protected virtual void CheckOrganisationPKFilter()
		{
			ListValidation.ErrorIfInvalidPK(parent.OrganisationPKFilterInfo);
		}

		#endregion

		#region OrganisationPKFilter

		public void ValidateDocumentGroupFilter()
		{
			ValidateCalculatedProperty(parent.DocumentGroupFilterInfo);
		}

		protected virtual void CheckDocumentGroupFilter()
		{
			ListValidation.ErrorIfInvalidCode(parent.DocumentGroupFilterInfo);
			if (!parent.DocumentGroupFilter.IsEmpty && (parent.OrganisationFromFilter == null || parent.DocumentFromFilter != null))
			{
				parent.DocumentGroupFilterInfo.AddError(Res.GetString("672d0be7-0ef2-4df0-ac04-a8d23236f57a", "This filter must be used with the Organization filter."));
			}
		}

		#endregion

		#region DocumentPKFilter

		public void ValidateDocumentPKFilter()
		{
			ValidateCalculatedProperty(parent.DocumentPKFilterInfo);
		}

		protected virtual void CheckDocumentPKFilter()
		{
			ListValidation.ErrorIfInvalidPK(parent.DocumentPKFilterInfo);
		}

		#endregion
	}
}
