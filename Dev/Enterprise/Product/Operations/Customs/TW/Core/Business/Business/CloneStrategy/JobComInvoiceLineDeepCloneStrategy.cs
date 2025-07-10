using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineDeepCloneStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceLineDeepCloneStrategy(Customs.Business.BaseJobComInvoiceLine invoiceLineToClone, Customs.Business.CloneType cloneType, Customs.Business.BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceLine)base.CloneInternal(args);
			var source = (JobComInvoiceLine)invoiceLineToClone;

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				if (IsTemplateCopy)
				{
					source.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description).Where(x => !x.IsDeleted).DeepCloneNotesTo(result.Notes);
					source.Taxes.CloneElementsTo(result.Taxes);
					source.AssignedJobComInvLineRefsCollection.CloneElementsTo(result.AssignedJobComInvLineRefsCollection);
					source.ReservedFields.CloneElementsTo(result.ReservedFields);
					source.PermitCusSupportingCollection.CloneElementsTo(result.PermitCusSupportingCollection);
					source.ExemptionOfControllingAgenciesCusSupportings.CloneElementsTo(result.ExemptionOfControllingAgenciesCusSupportings);
					source.CertificateOfOriginCusSupportingCollection.CloneElementsTo(result.CertificateOfOriginCusSupportingCollection);
					source.PreviousBondedCusSupportingCollection.CloneElementsTo(result.PreviousBondedCusSupportingCollection);
				}
			}

			return result;
		}
	}
}
