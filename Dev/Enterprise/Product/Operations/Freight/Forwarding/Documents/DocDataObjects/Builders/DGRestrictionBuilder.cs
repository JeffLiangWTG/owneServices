using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class DGRestrictionBuilder
	{
		public DGRestriction Build(ZString unno, ZString variant, IContext context)
		{
			if (context?.Factory == null)
			{
				return null;
			}

			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(context.Factory, unno, variant, UNDGSubstanceStandardTypes.IMO).FirstOrDefault();

			if (undgSubstance == null)
			{
				return null;
			}

			return new DGRestriction
			{
				EmergencyScheduleFire = GetEmergencyScheduleFire(undgSubstance.DG_EMS),
				EmergencyScheduleSpillage = GetEmergencyScheduleSpillage(undgSubstance.DG_EMS),
				Standard = undgSubstance.DG_Standard,
				ExceptedQuantityCode = undgSubstance.DG_ExceptedQuantityCode,
				FlashPoint = undgSubstance.DG_FlashPoint,
				IMOClass = undgSubstance.DG_Class,
				MarinePollutantCode = undgSubstance.DG_MP,
				PackedInLimitedQuantity = undgSubstance.DG_LQMaxAmt > 0,
				PackingGroup = undgSubstance.DG_PG,
				ProperShippingName = undgSubstance.DG_PSN,
				State = undgSubstance.DG_State,
				SubLabel1 = undgSubstance.DG_SubLabel1,
				SubLabel2 = undgSubstance.DG_SubLabel2,
				Code = undgSubstance.DG_Code,
				Unno = undgSubstance.DG_UNNO,
				Variant = undgSubstance.DG_Variant
			};
		}

		ICodeDescription GetEmergencyScheduleFire(ZString emsCodes)
		{
			return new CodeDescription(new EmergencyScheduleFireCodes())
			{
				Code = emsCodes.Split(',').FirstOrDefault(x => x.StartsWith("F-") || x.StartsWith("f-") || x.Equals("*"))
			};
		}

		ICodeDescription GetEmergencyScheduleSpillage(ZString emsCodes)
		{
			return new CodeDescription(new EmergencyScheduleSpillageCodes())
			{
				Code = emsCodes.Split(',').FirstOrDefault(x => x.StartsWith("S-") || x.StartsWith("s-") || x.Equals("*"))
			};
		}
	}
}
