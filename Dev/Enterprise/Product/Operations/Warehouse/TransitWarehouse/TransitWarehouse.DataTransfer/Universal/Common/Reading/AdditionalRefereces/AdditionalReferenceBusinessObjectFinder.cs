using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class AdditionalReferenceBusinessObjectFinder<T> : MatchingBusinessObjectFinder<AdditionalReference, T>
		where T : BusinessObject
	{
		public AdditionalReferenceBusinessObjectFinder(AdditionalReference dataObject)
			: base(dataObject)
		{
		}

		public T Find(IHaveCusEntryNumReferences cusEntryNumReferenceParent)
		{
			Argument.NotNull(cusEntryNumReferenceParent, "cusEntryNumReferenceParent");

			var cusEntryNumverReferences = cusEntryNumReferenceParent.CusEntryNumReferences.Cast<ICusEntryNumber>();
			var validPortReferences = cusEntryNumverReferences.Where(c => c.CE_Category == TransitWarehouseReferenceCategories.Codes.AdditionalReference
				&& c.CE_EntryType == dataObject.Type.GetCodeAsUpperCase()
				&& c.CE_EntryNum == dataObject.ReferenceNumber.GetValueOrDefault());

			return (T)validPortReferences.FirstOrDefault();
		}

		protected override T FindCore(IEnumerable<T> businessObjects)
		{
			throw new NotImplementedException();
		}
	}
}
