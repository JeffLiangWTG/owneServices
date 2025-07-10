using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class TaxOrFeeCollectionCreator
	{
		public static UniversalCustoms.TaxOrFee Create(JobComInvoiceLineTax jlt, IDataWritingManager writeManager)
		{
			var supportingInfo = new UniversalCustoms.TaxOrFee()
			{
				Type = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair6Char>(jlt.JLT_Type, jlt.Lookups.TypeList),
				Amount = jlt.JLT_Amount,
				BaseValue = jlt.JLT_BaseValue,
				BaseQuantity = jlt.JLT_BaseQuantity,
				MethodOfCalculation = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair4Char>(jlt.JLT_MethodOfCalculation, jlt.Lookups.MethodOfCalculationList),
				MethodOfPayment = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(jlt.JLT_MethodOfPayment, jlt.Lookups.MOPList),
				RateReasonOverride = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(jlt.JLT_RateOverrideReasonCode, jlt.Lookups.RateOverrideList),
				BaseQuantityUQ = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(jlt.JLT_BaseQuantityUQ, jlt.Lookups.BaseQuantityUQList),
				Rate = jlt.JLT_Rate
			};
			writeManager.NotifyExported(supportingInfo, jlt);
			return supportingInfo;
		}

		public static List<UniversalCustoms.TaxOrFee> CreateCollection(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager)
		{
			var result = new List<UniversalCustoms.TaxOrFee>();
			if (bizObj != null)
			{
				ZGuid bizObjPK = bizObj.PK;
				if (bizObjPK.IsValid)
				{
					var query = new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, bizObjPK);
					var bizObjs = helper.Load<JobComInvoiceLineTax>(query);
					if (bizObjs.Length > 0)
					{
						foreach (var tax in bizObjs)
						{
							var data = Create(tax, writeManager);
							result.Add(data);
						}
					}
				}
			}
			return result.Count == 0 ? null : result;
		}
	}
}
