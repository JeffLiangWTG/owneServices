namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public static class RefCusConditionValueMapper
	{
		public static string GetConditionValueWithPGACodeAndProgram(string pgaCode, string program)
		{
			switch (pgaCode)
			{
				case "HC":
					return GetConditionValueWhenPGAIsHC(program);
				case "PHAC":
					return program.Equals("HUMAN & TERRESTRIAL ANIMAL PATHOGENS & BIOLOGICAL TOXINS", System.StringComparison.OrdinalIgnoreCase) ? Constants.ConditionValue.HAP : Constants.ConditionValue.ALL;
				case "TC":
					return GetConditionValueWhenPGAIsTC(program);
				case "ECCC":
					return GetConditionValueWhenPGAIsECCC(program);
				case "NRCAN":
					return GetConditionValueWhenPGAIsNRCAN(program);
				case "DFO":
					return GetConditionValueWhenPGAIsDFO(program);
				default:
					return Constants.ConditionValue.ALL;
			}
		}

		static string GetConditionValueWhenPGAIsHC(string program)
		{
			switch (program)
			{
				case "ACTIVE PHARMACEUTICAL INGREDIENTS":
					return Constants.ConditionValue.API;
				case "BLOOD AND BLOOD COMPONENTS":
					return Constants.ConditionValue.BBC;
				case "CELLS, TISSUES AND ORGANS":
					return Constants.ConditionValue.CTO;
				case "CONSUMER PRODUCTS":
				case "CONSUMER PRODUCT SAFETY":
					return Constants.ConditionValue.CPR;
				case "DONOR SEMEN":
					return Constants.ConditionValue.DSE;
				case "HUMAN DRUGS (INCLUDING RADIOPHARMACEUTICALS)":
				case "HUMAN DRUGS":
					return Constants.ConditionValue.HDR;
				case "OFFICE OF CONTROLLED SUBSTANCES":
					return Constants.ConditionValue.OCS;
				case "MEDICAL DEVICES":
					return Constants.ConditionValue.MDE;
				case "NATURAL HEALTH PRODUCTS":
					return Constants.ConditionValue.NHP;
				case "PESTICIDES (PEST MANAGEMENT REGULATORY AGENCY)":
					return Constants.ConditionValue.PES;
				case "RADIATION EMITTING DEVICES":
					return Constants.ConditionValue.RED;
				case "VETERINARY DRUGS":
					return Constants.ConditionValue.VET;
				default:
					return Constants.ConditionValue.ALL;
			}
		}

		static string GetConditionValueWhenPGAIsTC(string program)
		{
			if (program.StartsWith("TIRES", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.TPR;
			}
			else if (program.StartsWith("VEHICLES", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.VPR;
			}
			return Constants.ConditionValue.ALL;
		}

		static string GetConditionValueWhenPGAIsECCC(string program)
		{
			if (program.StartsWith("WASTE", System.StringComparison.OrdinalIgnoreCase) && program.Contains("REDUCTION") && program.Contains("MANAGEMENT"))
			{
				return Constants.ConditionValue.WRM;
			}
			else if (program.StartsWith("OZONE", System.StringComparison.OrdinalIgnoreCase) && program.Contains("DEPLETING"))
			{
				return Constants.ConditionValue.ODS;
			}
			else if (program.StartsWith("VEHICLE AND ENGINE EMISSIONS", System.StringComparison.OrdinalIgnoreCase) || program.StartsWith("TRANSPORT PROGRAM", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.VEE;
			}
			else if (program.StartsWith("WILDLIFE ENFORCEMENT", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.WEN;
			}
			return Constants.ConditionValue.ALL;
		}

		static string GetConditionValueWhenPGAIsNRCAN(string program)
		{
			if (program.Contains("OFFICE OF ENERGY EFFICIENCY"))
			{
				return Constants.ConditionValue.EEF;
			}
			else if (program.StartsWith("EXPLOSIVE", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.EXP;
			}
			else if (program.StartsWith("ROUGH DIAMONDS", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.RDA;
			}
			return Constants.ConditionValue.ALL;
		}

		static string GetConditionValueWhenPGAIsDFO(string program)
		{
			if (program.StartsWith("AQUATIC BIOTECHNOLOGY", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.ABI;
			}
			else if (program.StartsWith("AQUATIC INVASIVE SPECIES", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.AIS;
			}
			else if (program.StartsWith("TRADE TRACKING", System.StringComparison.OrdinalIgnoreCase))
			{
				return Constants.ConditionValue.TTP;
			}
			return Constants.ConditionValue.ALL;
		}
	}
}
