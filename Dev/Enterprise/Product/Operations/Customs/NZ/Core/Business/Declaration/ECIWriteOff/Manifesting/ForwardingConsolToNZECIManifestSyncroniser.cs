using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class ForwardingConsolToNZECIManifestSyncroniser : ICanSyncroniseFromConsol
	{
		public void Syncronise(CommonConsol consol)
		{
			if (NZCustomsDataRegistry.Instance.UpdateAttachedManifestedECIsWhenConsolDetailsChange.Value)
			{
				ZQuery declarationsToUpdateQuery = GetDeclarationsToUpdateQuery(consol);
				JobDeclaration[] declarationsToUpdate = consol.Factory.Load<JobDeclaration>(declarationsToUpdateQuery);
				foreach (JobDeclaration declarationToUpdate in declarationsToUpdate)
				{
					Update(declarationToUpdate, consol);
				}
			}
		}

		static ZQuery GetDeclarationsToUpdateQuery(CommonConsol consol)
		{
			var result = new ZQuery { IsNoResultQuery = consol.Shipments.Count == 0 };
			if (!result.IsNoResultQuery)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageSubType, JobMessageSubTypeList.Codes.WriteOff);
				result.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, NumberFountains.ECIManifestReferencePrefix);
				var branches = (from branch in GlbCompany.CurrentCompany.Branches select branch.PK).ToArray();
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_GB, SQLComparisonOperator.Equal, branches);
				var shipments = (from shipment in consol.Shipments select shipment.PK).ToArray();
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_JS, SQLComparisonOperator.Equal, shipments);
			}
			return result;
		}

		void Update(JobDeclaration declaration, CommonConsol consol)
		{
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				UpdateIfChanged(declaration.JE_MasterBillInfo, consol.JK_MasterBillNumInfo);
				UpdateIfChanged(declaration.JE_OH_ShippingLineInfo, consol.JK_OA_ShippingLineAddressInfo, consol.ShippingLinePK);

				Transport interestingLeg = consol.Transports.MostInterestingTransport;
				if (interestingLeg != null)
				{
					UpdateIfChanged(declaration.JE_ExportDateInfo, interestingLeg.JW_ATDInfo, interestingLeg.JW_ETDInfo);
					UpdateIfChanged(declaration.JE_DateOfArrivalInfo, interestingLeg.JW_ATAInfo, interestingLeg.JW_ETAInfo);
					UpdateIfChanged(declaration.JE_VoyageFlightNoInfo, interestingLeg.JW_VoyageFlightInfo);
					UpdateIfChanged(declaration.JE_VesselNameInfo, interestingLeg.JW_VesselInfo);
				}

				Transport portsLeg = declaration.IsImport ? consol.Transports.ImportTransport : consol.Transports.ExportTransport;
				if (portsLeg != null)
				{
					UpdateIfChanged(declaration.JE_RL_NKPortOfLoadingInfo, portsLeg.JW_RL_NKLoadPortInfo);
					UpdateIfChanged(declaration.JE_RL_NKPortOfArrivalInfo, portsLeg.JW_RL_NKDiscPortInfo);
				}
			}
		}

		void UpdateIfChanged(ZPropertyInfo destInfo, ZPropertyInfo sourceInfo1, ZPropertyInfo sourceInfo2)
		{
			UpdateIfChanged(destInfo, sourceInfo1.Value.IsEmpty ? sourceInfo2 : sourceInfo1);
		}

		void UpdateIfChanged(ZPropertyInfo destInfo, ZPropertyInfo sourceInfo)
		{
			if (sourceInfo.HasChanges)
			{
				IZType newValue = sourceInfo.Value;
				if (newValue.IsValid && !newValue.IsEmpty)
				{
					destInfo.Value = newValue;
				}
			}
		}

		void UpdateIfChanged(ZPropertyInfo destInfo, ZPropertyInfo sourceInfo, IZType sourceValue)
		{
			if (sourceInfo.HasChanges)
			{
				if (sourceValue.IsValid && !sourceValue.IsEmpty)
				{
					destInfo.Value = sourceValue;
				}
			}
		}
	}
}
