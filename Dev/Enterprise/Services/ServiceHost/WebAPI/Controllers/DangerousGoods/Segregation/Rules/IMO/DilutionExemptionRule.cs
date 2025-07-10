using System.Collections.Generic;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class DilutionExemptionRule : ISegregationRule
	{
		public string ApplicableStandard => UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => true;

		readonly Dictionary<(ZString, ZString, ZString, ZString), HashSet<ZString>> compatibleSubstances = new Dictionary<(ZString, ZString, ZString, ZString), HashSet<ZString>>
		{
			{ ( "2014", (NoResString)"HYDROGEN PEROXIDE, AQUEOUS SOLUTION", "II", string.Empty ), new HashSet<ZString> { "7.2.6.3.1" } },
			{ ( "2984", (NoResString)"HYDROGEN PEROXIDE, AQUEOUS SOLUTION", "III", string.Empty ), new HashSet<ZString> { "7.2.6.3.1" } },
			{ ( "3105", (NoResString)"ORGANIC PEROXIDE TYPE D, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, peroxyacetic acid, type D, stabilized." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3105", (NoResString)"ORGANIC PEROXIDE TYPE D, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, other formulations." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3107", (NoResString)"ORGANIC PEROXIDE TYPE E, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, peroxyacetic acid, type E, stabilized." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3107", (NoResString)"ORGANIC PEROXIDE TYPE E, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, other formulations." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3109", (NoResString)"ORGANIC PEROXIDE TYPE F, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, peroxyacetic acid, type F, stabilized." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3109", (NoResString)"ORGANIC PEROXIDE TYPE F, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label, other formulations." ), new HashSet<ZString> { "7.2.6.3.1", "7.2.6.3.4" } },
			{ ( "3149", (NoResString)"HYDROGEN PEROXIDE AND PEROXYACETIC ACID MIXTURE, STABILIZED", "II", string.Empty ), new HashSet<ZString> { "7.2.6.3.1" } },
			{ ( "1295", (NoResString)"TRICHLOROSILANE", "I", string.Empty ), new HashSet<ZString> { "7.2.6.3.2" } },
			{ ( "1818", (NoResString)"SILICON TETRACHLORIDE", "II", string.Empty ), new HashSet<ZString> { "7.2.6.3.2" } },
			{ ( "2189", (NoResString)"DICHLOROSILANE", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.2" } },
			{ ( "3391", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, PYROPHORIC", "I", string.Empty ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3392", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, PYROPHORIC", "I", string.Empty ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3393", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, PYROPHORIC, WATER-REACTIVE", "I", string.Empty ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3394", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, PYROPHORIC, WATER-REACTIVE", "I", string.Empty ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3395", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE", "I", (NoResString)"Packing group I.") , new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3395", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE", "II", (NoResString)"Packing group II.") , new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3395", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE", "III", (NoResString)"Packing group III.") , new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3396", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, FLAMMABLE", "I", (NoResString)"Packing group I." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3396", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, FLAMMABLE", "II", (NoResString)"Packing group II." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3396", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, FLAMMABLE", "III", (NoResString)"Packing group III." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3397", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, SELF-HEATING", "I", (NoResString)"Packing group I." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3397", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, SELF-HEATING", "II", (NoResString)"Packing group II." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3397", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, WATER-REACTIVE, SELF-HEATING", "III", (NoResString)"Packing group III." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3398", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE", "I", (NoResString)"Packing group I." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3398", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE", "II", (NoResString)"Packing group II." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3398", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE", "III", (NoResString)"Packing group III." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3399", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE, FLAMMABLE", "I", (NoResString)"Packing group I." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3399", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE, FLAMMABLE", "II", (NoResString)"Packing group II." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3399", (NoResString)"ORGANOMETALLIC SUBSTANCE, LIQUID, WATER-REACTIVE, FLAMMABLE", "III", (NoResString)"Packing group III." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3400", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, SELF-HEATING", "II", (NoResString)"Packing group II." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3400", (NoResString)"ORGANOMETALLIC SUBSTANCE, SOLID, SELF-HEATING", "III", (NoResString)"Packing group III." ), new HashSet<ZString> { "7.2.6.3.3" } },
			{ ( "3101", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID", string.Empty, (NoResString)"Bearing both corrosive and explosive labels." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3101", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID", string.Empty, (NoResString)"Bearing a corrosive but not an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3101", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID", string.Empty, (NoResString)"Bearing an explosive but not a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3101", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID", string.Empty, (NoResString)"Bearing neither corrosive nor explosive labels." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3102", (NoResString)"ORGANIC PEROXIDE TYPE B, SOLID", string.Empty, (NoResString)"Bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3102", (NoResString)"ORGANIC PEROXIDE TYPE B, SOLID", string.Empty, (NoResString)"Not bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3103", (NoResString)"ORGANIC PEROXIDE TYPE C, LIQUID", string.Empty, (NoResString)"Bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3103", (NoResString)"ORGANIC PEROXIDE TYPE C, LIQUID", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3104", (NoResString)"ORGANIC PEROXIDE TYPE C, SOLID", string.Empty, (NoResString)"Bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3104", (NoResString)"ORGANIC PEROXIDE TYPE C, SOLID", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3105", (NoResString)"ORGANIC PEROXIDE TYPE D, LIQUID", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3106", (NoResString)"ORGANIC PEROXIDE TYPE D, SOLID", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3107", (NoResString)"ORGANIC PEROXIDE TYPE E, LIQUID", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3108", (NoResString)"ORGANIC PEROXIDE TYPE E, SOLID", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3109", (NoResString)"ORGANIC PEROXIDE TYPE F, LIQUID", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3110", (NoResString)"ORGANIC PEROXIDE TYPE F, SOLID", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3111", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3111", (NoResString)"ORGANIC PEROXIDE TYPE B, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Not bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3112", (NoResString)"ORGANIC PEROXIDE TYPE B, SOLID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3112", (NoResString)"ORGANIC PEROXIDE TYPE B, SOLID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Not bearing an explosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3113", (NoResString)"ORGANIC PEROXIDE TYPE C, LIQUID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3114", (NoResString)"ORGANIC PEROXIDE TYPE C, SOLID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3115", (NoResString)"ORGANIC PEROXIDE TYPE D, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3115", (NoResString)"ORGANIC PEROXIDE TYPE D, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3116", (NoResString)"ORGANIC PEROXIDE TYPE D, SOLID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3117", (NoResString)"ORGANIC PEROXIDE TYPE E, LIQUID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3118", (NoResString)"ORGANIC PEROXIDE TYPE E, SOLID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3119", (NoResString)"ORGANIC PEROXIDE TYPE F, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Bearing a corrosive label, peroxyacetic acid, type F, stabilized." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3119", (NoResString)"ORGANIC PEROXIDE TYPE F, LIQUID, TEMPERATURE CONTROLLED", string.Empty, (NoResString)"Not bearing a corrosive label." ), new HashSet<ZString> { "7.2.6.3.4" } },
			{ ( "3120", (NoResString)"ORGANIC PEROXIDE TYPE F, SOLID, TEMPERATURE CONTROLLED", string.Empty, string.Empty ), new HashSet<ZString> { "7.2.6.3.4" } }
		};

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var errorMessage = Res.GetString("c681aa45-259f-4fe7-9e09-4175be5d1279", "These dangerous goods substances require segregation");
			var substanceIdentifier1 = (substance1.DG_UNNO, substance1.DG_PSN, substance1.DG_PG, substance1.DG_Variation);
			var substanceIdentifier2 = (substance2.DG_UNNO, substance2.DG_PSN, substance2.DG_PG, substance2.DG_Variation);

			if (compatibleSubstances.ContainsKey(substanceIdentifier1) && compatibleSubstances.ContainsKey(substanceIdentifier2) && compatibleSubstances[substanceIdentifier1].Overlaps(compatibleSubstances[substanceIdentifier2]))
			{
				return new List<Message>();
			}

			return new List<Message> { new Message(MessageType.Error, errorMessage) };
		}
	}
}
