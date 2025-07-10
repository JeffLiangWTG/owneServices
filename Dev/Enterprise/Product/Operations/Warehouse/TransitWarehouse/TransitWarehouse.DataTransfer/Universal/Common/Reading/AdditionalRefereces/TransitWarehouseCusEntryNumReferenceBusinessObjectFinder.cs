using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<T> : MatchingBusinessObjectFinder<TransitAdditionalReferenceInfo, T>
		where T : BusinessObject
	{
		public TransitWarehouseCusEntryNumReferenceBusinessObjectFinder(TransitAdditionalReferenceInfo dataObject)
			: base(dataObject)
		{
		}

		public T Find(IHaveCusEntryNumReferences cusEntryNumReferenceParent)
		{
			Argument.NotNull(cusEntryNumReferenceParent, "cusEntryNumReferenceParent");
			var cusEntryNumReferences = cusEntryNumReferenceParent.CusEntryNumReferences.Cast<ICusEntryNumber>();
			T res;
			if (dataObject.Category.GetValueOrDefault() == TransitWarehouseReferenceCategories.Codes.CustomsReference)
			{
				res = (T)cusEntryNumReferences.Where(c => c.CE_Category == dataObject.Category.GetValueOrDefault() &&
															c.CE_EntryType == dataObject.Type.GetValueOrDefault() &&
															c.CE_EntryNum == dataObject.Value.GetValueOrDefault())
					.OrderBy(c => c.CE_EntryNum)
					.FirstOrDefault();
			}
			else
			{
				res = (T)cusEntryNumReferences.Where(c => c.CE_Category == dataObject.Category.GetValueOrDefault() &&
															c.CE_EntryType == dataObject.Type.GetValueOrDefault() &&
															c.CE_EntryType != WarehouseAdditionalReferenceTypes.Codes.Other ||
															(c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.Other && c.CE_EntryNum == dataObject.Value.GetValueOrDefault()))
					.OrderBy(c => c.CE_EntryNum)
					.FirstOrDefault();
			}

			return res;
		}

		protected override T FindCore(IEnumerable<T> businessObjects)
		{
			throw new NotImplementedException();
		}
	}
}
