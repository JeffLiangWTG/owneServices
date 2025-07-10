using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal
{
	public class LandedCostDataReader : DataObjectReader, UniversalData.ILandedCostDataReader
	{
		public LandedCostDataReader(IXmlImportLogger logger, UniversalObjectFactory factory, ILandedCostHeader hostEntity)
			: base(logger)
		{
			this.factory = Argument.NotNull(factory, "factory");
			this.hostEntity = Argument.NotNull(hostEntity, "hostEntity");
		}

		public Integration.LandedCosting.ILandedCostHeader ReadIntoBusinessObject()
		{
			Integration.LandedCosting.ILandedCostHeader result = null;
			if (costInputsCached != null)
			{
				result = Header;
				var company = Header.Company;
				if (company != null)
				{
					foreach (var data in costInputsCached)
					{
						var bizObj = data.Key;
						foreach (var transportLogisticsCost in data.Value)
						{
							var charge = GetCharge(company.PK, transportLogisticsCost);
							var chargeDescription = transportLogisticsCost.ChargeDescription;
							var chargePK = ZGuid.Empty;
							if (charge != null)
							{
								chargePK = charge.PK;
								if (!chargeDescription.HasValue)
								{
									chargeDescription = new ZString(charge.AC_DescMultilingual.GetUnresolvedString()).Left(LandCostInput.Schema.LI_ChargeDescriptionMaxLength);
								}
							}
							if (chargePK.IsEmpty && transportLogisticsCost.ChargeCode != null)
							{
								var chargeCode = transportLogisticsCost.ChargeCode.Code.GetValueOrDefault();
								logger.Log(Integration.LogType.Error, string.Format("Transport Logistics Cost ChargeCode '{0}' does not exists for Company '{1}'", chargeCode, company.GC_Name));
							}
							var costCurrency = transportLogisticsCost.CostCurrency.GetCodeAsUpperCase();
							var costInput = GetCostInput(chargePK, costCurrency, bizObj.PK, bizObj.TablePrefix, chargeDescription.GetValueOrDefault());
							new LandCostInputDataObjectReader(transportLogisticsCost, logger, factory, costInput).ReadIntoBusinessObject();
						}
					}
				}
			}
			return result;
		}

		public void CollectTransportLogisticsCost(BusinessObject bizObj, UniversalData.ITransportLogisticsCostCollectionParent transportLogisticsCostCollectionParent)
		{
			if (bizObj != null && transportLogisticsCostCollectionParent.TransportLogisticsCostCollection != null)
			{
				CostInputs.Add(bizObj, transportLogisticsCostCollectionParent.TransportLogisticsCostCollection);
			}
		}

		#region Implementation

		Dictionary<BusinessObject, List<UniversalData.TransportLogisticsCost>> CostInputs
		{
			get { return costInputsCached ?? (costInputsCached = new Dictionary<BusinessObject, List<UniversalData.TransportLogisticsCost>>()); }
		}
		Dictionary<BusinessObject, List<UniversalData.TransportLogisticsCost>> costInputsCached;

		LandCostInput GetCostInput(ZGuid chargePK, ZString costCurrency, ZGuid parentID, ZString parentTableCode, ZString chargeDescription)
		{
			var result = Header.CostInputs.OfType<LandCostInput>().FirstOrDefault(x => x.LI_AC_ChargeCode == chargePK
				&& x.LI_RX_NKCostCurrency == costCurrency
				&& x.LI_ParentID == parentID
				&& x.LI_ParentTableCode == parentTableCode
				&& x.LI_ChargeDescription == chargeDescription);
			if (result == null)
			{
				result = Header.CostInputs.AddNew();
				result.LI_ParentID = parentID;
				result.LI_ParentTableCode = parentTableCode;
				result.LI_AC_ChargeCode = chargePK;
			}
			return result;
		}

		AccChargeCode GetCharge(ZGuid companyPK, UniversalData.TransportLogisticsCost dataObject)
		{
			AccChargeCode result = null;
			if (dataObject != null && dataObject.ChargeCode != null)
			{
				var chargeCode = (ZString)dataObject.ChargeCode.Code.GetValueOrDefault();
				if (!chargeCode.IsEmpty)
				{
					var query = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
					query.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);
					var charge = factory.LoadTop1<AccChargeCode>(query);
					if (charge != null)
					{
						result = charge;
					}
				}
			}
			return result;
		}

		LandedCostHeader GetExistingHeader()
		{
			var filter = new LandedCostHeaderFilter(hostEntity);
			filter.AddToFilter(JoinCondition.And, LandedCostHeaderSchema.LT_ParentTableCode, SQLComparisonOperator.Equal, hostEntity.TableCode);
			filter.IgnoreActiveFilter = true;
			filter.FetchOnlyFromLocalCache = !hostEntity.IsInDatabase;
			return factory.LoadTop1<LandedCostHeader>(filter);
		}

		LandedCostHeader Header
		{
			get
			{
				if (header == null)
				{
					header = GetExistingHeader();
					if (header == null)
					{
						var mutex = hostEntity.GetLandedCostMutex();

						if (!mutex.Lock())
						{
							throw new InvalidOperationException(string.Format("Could not create a new landed costing; someone else is already in the process of creating a landed costing for job ({0}).", hostEntity.JobNumber));
						}
						else
						{
							factory.CleanupAfterSaving += (a, b) => { if (mutex.HasLock) { mutex.Unlock(); } };
						}

						header = factory.New<LandedCostHeader>();

						using (header.GetValidationSuspender())
						using (header.SuspendSettingHasChanges())
						{
							header.LT_ParentID = hostEntity.PK;
							header.LT_ParentTableCode = hostEntity.TableCode;
							header.LT_GC = hostEntity.CompanyPK;
							header.LT_DateOfEntry = hostEntity.DateOfEntry;
							header.LT_LandedCostType = hostEntity.LandedCostType;
						}
					}
				}
				return header;
			}
		}
		LandedCostHeader header;

		readonly UniversalObjectFactory factory;
		readonly ILandedCostHeader hostEntity;

		#endregion
	}
}
