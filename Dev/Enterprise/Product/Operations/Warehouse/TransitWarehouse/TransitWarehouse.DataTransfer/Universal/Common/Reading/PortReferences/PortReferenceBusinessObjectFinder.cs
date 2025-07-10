using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class PortReferenceBusinessObjectFinder<T> : MatchingBusinessObjectFinder<PortReference, T>
		where T : BusinessObject
	{
		public PortReferenceBusinessObjectFinder(PortReference dataObject)
			: base(dataObject)
		{
		}

		public T Find(IHavePortReferences portReferenceParent)
		{
			Argument.NotNull(portReferenceParent, "portReferenceParent");

			var portReferences = portReferenceParent.PortReferences.Cast<ICusEntryNumber>();
			var validPortReferences = portReferences.Where(c => c.CE_Category == TransitWarehouseReferenceCategories.Codes.PortReference
				&& c.CE_EntryType == dataObject.Type.GetCodeAsUpperCase()
				&& c.CE_RN_NKCountryCode == dataObject.Country.GetCodeAsUpperCase());

			return (T)validPortReferences.FirstOrDefault();
		}

		protected override T FindCore(IEnumerable<T> businessObjects)
		{
			throw new NotImplementedException();
		}
	}
}
