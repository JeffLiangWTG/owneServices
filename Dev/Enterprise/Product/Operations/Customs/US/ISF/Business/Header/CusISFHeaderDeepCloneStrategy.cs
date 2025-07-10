using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderDeepCloneStrategy : BusinessObjectCloneStrategy
	{
		public CusISFHeaderDeepCloneStrategy(CusISFHeader headerToClone)
			: base(headerToClone)
		{
		}

		public CusISFHeader Clone()
		{
			return (CusISFHeader)Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
		}

		CusISFHeader headerToClone
		{
			get { return (CusISFHeader)base.bizObjToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(
				new string[] {
					CusISFHeader.Schema.BF_CustomsReference,
					CusISFHeader.Schema.BF_CustomsStatus,
					CusISFHeader.Schema.BF_JobReference,
					CusISFHeader.Schema.BF_FirstAcceptedDate,
					CusISFHeader.Schema.BF_LastAcceptedDate,
					CusISFHeader.Schema.BF_OwnerReference,
					CusISFHeader.Schema.BF_IsCancelled,
					CusISFHeader.Schema.BF_BondReferenceNumber,
					CusISFHeader.Schema.BF_JS_Shipment
				});
			CusISFHeader clonedResult = (CusISFHeader)base.CloneInternal(args);

			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				clonedResult.BF_GB = GlbBranch.CurrentBranch.PK;

				Dictionary<ZGuid, ZGuid> addressPKPairs = CopyJobDocAddresses(args, clonedResult);
				//don't copy containers
				CopyReferences(args, clonedResult);
				clonedResult.BF_ShipmentSubType = headerToClone.BF_ShipmentSubType;
				clonedResult.BF_ConsigneeFullName = headerToClone.BF_ConsigneeFullName;
				clonedResult.BF_EstimatedQuantity = headerToClone.BF_EstimatedQuantity;
				clonedResult.BF_EstimatedValue = headerToClone.BF_EstimatedValue;
				clonedResult.BF_EstimatedWeight = headerToClone.BF_EstimatedWeight;
				clonedResult.BF_EstimatedWeightUQ = headerToClone.BF_EstimatedWeightUQ;
				clonedResult.CustomAttribute1 = headerToClone.CustomAttribute1;
				clonedResult.CustomAttribute2 = headerToClone.CustomAttribute2;
				CopyLines(args, clonedResult, addressPKPairs);
			}

			return clonedResult;
		}

		void CopyLines(BusinessObjectCloneArgs args, CusISFHeader clonedResult, Dictionary<ZGuid, ZGuid> addressPKPairs)
		{
			args.AddExcludedColumns(new string[] { CusISFLine.Schema.BL_BF, CusISFLine.Schema.BL_ManufacturerDocAddressPK });
			foreach (CusISFLine lineToClone in headerToClone.Lines.ToArray<CusISFLine>())
			{
				CusISFLine clonedLine = (CusISFLine)lineToClone.Clone(args);
				using (clonedLine.GetValidationSuspender())
				using (clonedLine.SuspendSettingHasChanges())
				{
					clonedLine.BL_BF = clonedResult.PK;
					clonedLine.CustomAttribute1 = lineToClone.CustomAttribute1;
					clonedLine.CustomAttribute2 = lineToClone.CustomAttribute2;
					if (!lineToClone.BL_ManufacturerDocAddressPK.IsEmpty)
					{
						ZGuid addressPK = ZGuid.Empty;
						addressPKPairs.TryGetValue(lineToClone.BL_ManufacturerDocAddressPK, out addressPK);
						clonedLine.BL_ManufacturerDocAddressPK = addressPK;
					}
				}
			}
		}

		Dictionary<ZGuid, ZGuid> CopyJobDocAddresses(BusinessObjectCloneArgs args, CusISFHeader clonedResult)
		{
			args.AddExcludedColumns(new string[] { ISFDocAddress.Schema.E2_ParentID });
			Dictionary<ZGuid, ZGuid> result = new Dictionary<ZGuid, ZGuid>();
			clonedResult.DocAddresses.RemoveAndDeleteAll();
			foreach (ISFDocAddress addressToClone in headerToClone.DocAddresses.ToArray<ISFDocAddress>())
			{
				ISFDocAddress clonedAddress = (ISFDocAddress)addressToClone.Clone(args);
				using (clonedAddress.GetValidationSuspender())
				using (clonedAddress.SuspendSettingHasChanges())
				{
					clonedAddress.E2_ParentID = clonedResult.PK;
					clonedAddress.E2_ParentTableCode = addressToClone.E2_ParentTableCode;
					result.Add(addressToClone.PK, clonedAddress.PK);
					clonedResult.DocAddresses.Add(clonedAddress);
				}
				clonedAddress.HasChanges = true;
			}
			return result;
		}

		void CopyReferences(BusinessObjectCloneArgs args, CusISFHeader clonedResult)
		{
			args.AddExcludedColumns(new string[] { CusISFBill.Schema.BB_BF, CusISFBill.Schema.BB_CustomsStatus });
			foreach (CusISFBill bill in headerToClone.ReferenceDatas.ToArray<CusISFBill>())
			{
				//don't copy shipment type information...
				if (bill.BB_BillType == BillTypeList.Codes.SuretyCode || bill.BB_BillType == BillTypeList.Codes.FullNameOfISFImporter)
				{
					CusISFBill clonedBill = (CusISFBill)bill.Clone(args);

					using (clonedBill.GetValidationSuspender())
					using (clonedBill.SuspendSettingHasChanges())
					{
						clonedBill.BB_BF = clonedResult.PK;
						clonedBill.BB_CustomsStatus = ZString.Empty;
						clonedResult.ReferenceDatas.Add(clonedBill);
					}
				}
			}
		}
	}
}
