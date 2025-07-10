using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class USJobComInvoiceHeaderDeepCloneStrategy : JobComInvoiceHeaderDeepCopyStrategy
	{
		public USJobComInvoiceHeaderDeepCloneStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType)
			: base(invoice, cloneType)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public USJobComInvoiceHeaderDeepCloneStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, clonedDeclaration, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceHeader)base.CloneInternal(args);
			result.EnsureUSOrganisationAreLoadedIfNeeded();
			return result;
		}
	}
}
