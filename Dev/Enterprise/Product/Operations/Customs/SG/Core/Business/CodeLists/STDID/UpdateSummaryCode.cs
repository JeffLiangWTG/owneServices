using System;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class UpdateSummaryCode
	{
		/// <summary>
		/// Pass in a formatted summary information code extracted from the FTX segment in *UPT permit messages
		/// from Singapore Customs.
		/// Have returned a formatted string required to print in the amended fields section of an
		/// Amendment Customs Clearance Permit
		/// </summary>
		public UpdateSummaryCode()
		{
		}

		public ZString GetFieldDescriptionFromSummaryCode(ZString summaryCode)
		{
			SplitSummaryCode(summaryCode);

			return (GetDescription() + GetUpdateType()).ToUpperInvariant();
		}

		public ZString GetTN41FieldDescriptionFromSummaryCode(ZString summaryCode)
		{
			SplitSummaryCode(summaryCode);

			return GetTN41Description().ToUpperInvariant();
		}

		#region Implementation

		ZString GetDescription()
		{
			switch (fieldNumber)
			{
				case "001":
					return FieldDescriptions.S001;
				case "002":
					return FieldDescriptions.S002;
				case "003":
					return FieldDescriptions.S003;
				case "004":
					return FieldDescriptions.S004;
				case "005":
					return FieldDescriptions.S005;
				case "006":
					return FieldDescriptions.S006;
				case "008":
					return FieldDescriptions.S008;
				case "009":
					return FieldDescriptions.S009;
				case "010":
					return FieldDescriptions.S010;
				case "011":
					return FieldDescriptions.S011;
				case "012":
					return FieldDescriptions.S012;
				case "013":
					return FieldDescriptions.S013;
				case "014":
					return FieldDescriptions.S014;
				case "015":
					return FieldDescriptions.S015;
				case "016":
					return FieldDescriptions.S016;
				case "017":
					return FieldDescriptions.S017;
				case "018":
					return FieldDescriptions.S018;
				case "019":
					return FieldDescriptions.S019;
				case "020":
					return FormattedOutput(FieldDescriptions.S020, 2);
				case "021":
					return FormattedOutput(FieldDescriptions.S021, 2);
				case "022":
					return FormattedOutput(FieldDescriptions.S022, 2);
				case "023":
					return FormattedOutput(FieldDescriptions.S023, 2);
				case "024":
					return FormattedOutput(FieldDescriptions.S024, 2);
				case "025":
					return FieldDescriptions.S025;
				case "026":
					return FieldDescriptions.S026;
				case "027":
					return FieldDescriptions.S027;
				case "028":
					return FieldDescriptions.S028;
				case "029":
					return FieldDescriptions.S029;
				case "030":
					return FieldDescriptions.S030;
				case "031":
					return FieldDescriptions.S031;
				case "032":
					return FieldDescriptions.S032;
				case "033":
					return FieldDescriptions.S033;
				case "034":
					return FieldDescriptions.S034;
				case "035":
					return FieldDescriptions.S035;
				case "036":
					return FieldDescriptions.S036;
				case "037":
					return FieldDescriptions.S037;
				case "038":
					return FieldDescriptions.S038;
				case "039":
					return FieldDescriptions.S039;
				case "040":
					return FieldDescriptions.S040;
				case "041":
					return FieldDescriptions.S041;
				case "042":
					return FieldDescriptions.S042;
				case "043":
					return FieldDescriptions.S043;
				case "044":
					return FieldDescriptions.S044;
				case "045":
					return FieldDescriptions.S045;
				case "046":
					return FieldDescriptions.S046;
				case "047":
					return FieldDescriptions.S047;
				case "048":
					return FieldDescriptions.S048;
				case "049":
					return FieldDescriptions.S049;
				case "050":
					return FieldDescriptions.S050;
				case "051":
					return FieldDescriptions.S051;
				case "052":
					return FieldDescriptions.S052;
				case "053":
					return FieldDescriptions.S053;
				case "054":
					return FieldDescriptions.S054;
				case "055":
					return FieldDescriptions.S055;
				case "056":
					return FieldDescriptions.S056;
				case "057":
					return FieldDescriptions.S057;
				case "058":
					return FieldDescriptions.S058;
				case "059":
					return FieldDescriptions.S059;
				case "060":
					return FieldDescriptions.S060;
				case "061":
					return FieldDescriptions.S061;
				case "062":
					return FieldDescriptions.S062;
				case "063":
					return FieldDescriptions.S063;
				case "064":
					return FieldDescriptions.S064;
				case "065":
					return FieldDescriptions.S065;
				case "067":
					return FieldDescriptions.S067;
				case "068":
					return FieldDescriptions.S068;
				case "069":
					return FieldDescriptions.S069;
				case "070":
					return FieldDescriptions.S070;
				case "071":
					return FieldDescriptions.S071;
				case "072":
					return FieldDescriptions.S072;
				case "073":
					return FieldDescriptions.S073;
				case "074":
					return FieldDescriptions.S074;
				case "075":
					return FormattedOutput(FieldDescriptions.S075, 1);
				case "076":
					return FormattedOutput(FieldDescriptions.S076, 1);
				case "077":
					return FormattedOutput(FieldDescriptions.S077, 1);
				case "078":
					return FormattedOutput(FieldDescriptions.S078, 1);
				case "079":
					return FormattedOutput(FieldDescriptions.S079, 1);
				case "080":
					return FormattedOutput(FieldDescriptions.S080, 1);
				case "081":
					return FormattedOutput(FieldDescriptions.S081, 1);
				case "082":
					return FormattedOutput(FieldDescriptions.S082, 1);
				case "083":
					return FormattedOutput(FieldDescriptions.S083, 1);
				case "084":
					return FormattedOutput(FieldDescriptions.S084, 1);
				case "085":
					return FormattedOutput(FieldDescriptions.S085, 1);
				case "086":
					return FormattedOutput(FieldDescriptions.S086, 1);
				case "087":
					return FormattedOutput(FieldDescriptions.S087, 1);
				case "088":
					return FormattedOutput(FieldDescriptions.S088, 1);
				case "089":
					return FormattedOutput(FieldDescriptions.S089, 1);
				case "090":
					return FormattedOutput(FieldDescriptions.S090, 1);
				case "091":
					return FormattedOutput(FieldDescriptions.S091, 1);
				case "092":
					return FormattedOutput(FieldDescriptions.S092, 1);
				case "093":
					return FormattedOutput(FieldDescriptions.S093, 1);
				case "094":
					return FormattedOutput(FieldDescriptions.S094, 1);
				case "095":
					return FormattedOutput(FieldDescriptions.S095, 1);
				case "096":
					return FormattedOutput(FieldDescriptions.S096, 1);
				case "097":
					return FormattedOutput(FieldDescriptions.S097, 2);
				case "098":
					return FormattedOutput(FieldDescriptions.S098, 2);
				case "099":
					return FormattedOutput(FieldDescriptions.S099, 2);
				case "100":
					return FormattedOutput(FieldDescriptions.S100, 3);
				case "101":
					return FormattedOutput(FieldDescriptions.S101, 3);
				case "102":
					return FormattedOutput(FieldDescriptions.S102, 3);
				case "103":
					return FormattedOutput(FieldDescriptions.S103, 1);
				case "104":
					return FormattedOutput(FieldDescriptions.S104, 1);
				case "105":
					return FormattedOutput(FieldDescriptions.S105, 1);
				case "106":
					return FormattedOutput(FieldDescriptions.S106, 1);
				case "107":
					return FormattedOutput(FieldDescriptions.S107, 1);
				case "108":
					return FormattedOutput(FieldDescriptions.S108, 1);
				case "109":
					return FormattedOutput(FieldDescriptions.S109, 1);
				case "110":
					return FormattedOutput(FieldDescriptions.S110, 1);
				case "111":
					return FormattedOutput(FieldDescriptions.S111, 1);
				case "112":
					return FormattedOutput(FieldDescriptions.S112, 1);
				case "113":
					return FormattedOutput(FieldDescriptions.S113, 1);
				case "114":
					return FormattedOutput(FieldDescriptions.S114, 1);
				case "115":
					return FormattedOutput(FieldDescriptions.S115, 1);
				case "116":
					return FormattedOutput(FieldDescriptions.S116, 1);
				case "117":
					return FormattedOutput(FieldDescriptions.S117, 1);
				case "118":
					return FormattedOutput(FieldDescriptions.S118, 1);
				case "119":
					return FormattedOutput(FieldDescriptions.S119, 1);
				case "120":
					return FormattedOutput(FieldDescriptions.S120, 1);
				case "121":
					return FormattedOutput(FieldDescriptions.S121, 1);
				case "122":
					return FormattedOutput(FieldDescriptions.S122, 1);
				case "123":
					return FormattedOutput(FieldDescriptions.S123, 1);
				case "124":
					return FormattedOutput(FieldDescriptions.S124, 1);
				case "125":
					return FormattedOutput(FieldDescriptions.S125, 1);
				case "126":
					return FormattedOutput(FieldDescriptions.S126, 1);
				case "127":
					return FormattedOutput(FieldDescriptions.S127, 1);
				case "128":
					return FormattedOutput(FieldDescriptions.S128, 1);
				case "129":
					return FormattedOutput(FieldDescriptions.S129, 1);
				case "130":
					return FormattedOutput(FieldDescriptions.S130, 1);
				case "131":
					return FormattedOutput(FieldDescriptions.S131, 1);
				case "132":
					return FormattedOutput(FieldDescriptions.S132, 1);
				case "133":
					return FormattedOutput(FieldDescriptions.S133, 1);
				case "134":
					return FormattedOutput(FieldDescriptions.S134, 1);
				case "135":
					return FormattedOutput(FieldDescriptions.S135, 1);
				case "136":
					return FormattedOutput(FieldDescriptions.S136, 1);
				case "137":
					return FormattedOutput(FieldDescriptions.S137, 1);
				case "138":
					return FormattedOutput(FieldDescriptions.S138, 1);
				case "139":
					return FormattedOutput(FieldDescriptions.S139, 1);
				case "140":
					return FormattedOutput(FieldDescriptions.S140, 1);
				case "141":
					return FormattedOutput(FieldDescriptions.S141, 1);
				case "142":
					return FormattedOutput(FieldDescriptions.S142, 1);
				case "143":
					return FormattedOutput(FieldDescriptions.S143, 1);
				case "144":
					return FormattedOutput(FieldDescriptions.S144, 1);
				case "145":
					return FormattedOutput(FieldDescriptions.S145, 1);
				case "146":
					return FormattedOutput(FieldDescriptions.S146, 1);
				case "147":
					return FormattedOutput(FieldDescriptions.S147, 1);
				case "148":
					return FormattedOutput(FieldDescriptions.S148, 1);
				case "149":
					return FormattedOutput(FieldDescriptions.S149, 1);
				case "150":
					return FormattedOutput(FieldDescriptions.S150, 1);
				case "151":
					return FormattedOutput(FieldDescriptions.S151, 1);
				case "152":
					return FormattedOutput(FieldDescriptions.S152, 1);
				case "153":
					return FormattedOutput(FieldDescriptions.S153, 1);
				case "154":
					return FormattedOutput(FieldDescriptions.S154, 1);
				case "155":
					return FormattedOutput(FieldDescriptions.S155, 1);
				case "156":
					return FormattedOutput(FieldDescriptions.S156, 1);
				case "157":
					return FormattedOutput(FieldDescriptions.S157, 1);
				case "158":
					return FormattedOutput(FieldDescriptions.S158, 1);
				case "159":
					return FormattedOutput(FieldDescriptions.S159, 1);
				case "160":
					return FieldDescriptions.S160;
				case "161":
					return FieldDescriptions.S161;
				case "162":
					return FieldDescriptions.S162;
				case "163":
					return FieldDescriptions.S163;
				case "164":
					return FieldDescriptions.S164;
				case "166":
					return FieldDescriptions.S166;
				case "167":
					return FieldDescriptions.S167;
				case "168":
					return FieldDescriptions.S168;
				case "169":
					return FieldDescriptions.S169;
				case "170":
					return FieldDescriptions.S170;
				case "171":
					return FieldDescriptions.S171;
				case "172":
					return FormattedOutput(FieldDescriptions.S172, 1);
				case "173":
					return FormattedOutput(FieldDescriptions.S173, 1);
				case "174":
					return FormattedOutput(FieldDescriptions.S174, 1);
				case "175":
					return FormattedOutput(FieldDescriptions.S175, 1);
				case "176":
					return FormattedOutput(FieldDescriptions.S176, 1);
				case "177":
					return FormattedOutput(FieldDescriptions.S177, 1);
				case "178":
					return FormattedOutput(FieldDescriptions.S178, 1);
				case "179":
					return FormattedOutput(FieldDescriptions.S179, 1);
				case "180":
					return FormattedOutput(FieldDescriptions.S180, 1);
				case "181":
					return FormattedOutput(FieldDescriptions.S181, 1);
				case "182":
					return FieldDescriptions.S182;
				case "183":
					return FieldDescriptions.S183;
				case "184":
					return FieldDescriptions.S184;
				case "185":
					return FieldDescriptions.S185;
				case "186":
					return FieldDescriptions.S186;
				case "187":
					return FieldDescriptions.S187;
				case "189":
					return FieldDescriptions.S189;
				case "190":
					return FieldDescriptions.S190;
				case "191":
					return FieldDescriptions.S191;
				case "192":
					return FieldDescriptions.S192;
				case "193":
					return FieldDescriptions.S193;
				case "194":
					return FieldDescriptions.S194;
				case "195":
					return FieldDescriptions.S195;
				case "196":
					return FieldDescriptions.S196;
				case "197":
					return FieldDescriptions.S197;
				case "198":
					return FormattedOutput(FieldDescriptions.S198, 2);
				case "901":
					return FormattedOutput(FieldDescriptions.S901, 1);
				case "902":
					return FormattedOutput(FieldDescriptions.S902, 1);
				case "903":
					return FormattedOutput(FieldDescriptions.S903, 2);
				case "904":
					return FormattedOutput(FieldDescriptions.S904, 1);
				case "905":
					return FormattedOutput(FieldDescriptions.S905, 1);
				case "906":
					return FormattedOutput(FieldDescriptions.S906, 1);
				case "907":
					return FormattedOutput(FieldDescriptions.S907, 1);
				case "908":
					return FormattedOutput(FieldDescriptions.S908, 2);
				case "909":
					return FormattedOutput(FieldDescriptions.S909, 3);
				default:
					throw new ArgumentException("Unrecognised Summary Code '" + fieldNumber + "' in Amendment Summary Code");
			}
		}

		#region TradeNet 4.1

		ZString GetTN41Description()
		{
			switch (fieldNumber)
			{
				case "001":
					return TN41UpdateDescriptions.S001;
				case "002":
					return TN41UpdateDescriptions.S002;
				case "003":
					return TN41UpdateDescriptions.S003;
				case "004":
					return TN41UpdateDescriptions.S004;
				case "005":
					return TN41UpdateDescriptions.S005;
				case "006":
					return TN41UpdateDescriptions.S006;
				case "007":
					return TN41UpdateDescriptions.S007;
				case "008":
					return TN41UpdateDescriptions.S008;
				case "009":
					return TN41UpdateDescriptions.S009;
				case "010":
					return TN41UpdateDescriptions.S010;
				case "011":
					return TN41UpdateDescriptions.S011;
				case "012":
					return TN41UpdateDescriptions.S012;
				case "013":
					return TN41UpdateDescriptions.S013;
				case "014":
					return TN41UpdateDescriptions.S014;
				case "015":
					return TN41UpdateDescriptions.S015;
				case "016":
					return TN41UpdateDescriptions.S016;
				case "017":
					return TN41UpdateDescriptions.S017;
				case "018":
					return TN41UpdateDescriptions.S018;
				case "019":
					return TN41UpdateDescriptions.S019;
				case "020":
					return FormattedOutput(TN41UpdateDescriptions.S020, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "021":
					return FormattedOutput(TN41UpdateDescriptions.S021, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "022":
					return FormattedOutput(TN41UpdateDescriptions.S022, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "023":
					return FormattedOutput(TN41UpdateDescriptions.S023, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "024":
					return FormattedOutput(TN41UpdateDescriptions.S024, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "025":
					return TN41UpdateDescriptions.S025;
				case "026":
					return TN41UpdateDescriptions.S026;
				case "027":
					return TN41UpdateDescriptions.S027;
				case "028":
					return TN41UpdateDescriptions.S028;
				case "029":
					return TN41UpdateDescriptions.S029;
				case "030":
					return TN41UpdateDescriptions.S030;
				case "031":
					return TN41UpdateDescriptions.S031;
				case "032":
					return TN41UpdateDescriptions.S032;
				case "033":
					return TN41UpdateDescriptions.S033;
				case "034":
					return TN41UpdateDescriptions.S034;
				case "035":
					return TN41UpdateDescriptions.S035;
				case "036":
					return TN41UpdateDescriptions.S036;
				case "037":
					return TN41UpdateDescriptions.S037;
				case "038":
					return TN41UpdateDescriptions.S038;
				case "039":
					return TN41UpdateDescriptions.S039;
				case "040":
					return TN41UpdateDescriptions.S040;
				case "041":
					return TN41UpdateDescriptions.S041;
				case "042":
					return TN41UpdateDescriptions.S042;
				case "043":
					return TN41UpdateDescriptions.S043;
				case "044":
					return TN41UpdateDescriptions.S044;
				case "045":
					return TN41UpdateDescriptions.S045;
				case "046":
					return TN41UpdateDescriptions.S046;
				case "047":
					return TN41UpdateDescriptions.S047;
				case "048":
					return TN41UpdateDescriptions.S048;
				case "049":
					return TN41UpdateDescriptions.S049;
				case "050":
					return TN41UpdateDescriptions.S050;
				case "051":
					return TN41UpdateDescriptions.S051;
				case "052":
					return TN41UpdateDescriptions.S052;
				case "053":
					return TN41UpdateDescriptions.S053;
				case "054":
					return TN41UpdateDescriptions.S054;
				case "055":
					return TN41UpdateDescriptions.S055;
				case "056":
					return TN41UpdateDescriptions.S056;
				case "057":
					return TN41UpdateDescriptions.S057;
				case "058":
					return TN41UpdateDescriptions.S058;
				case "059":
					return TN41UpdateDescriptions.S059;
				case "060":
					return TN41UpdateDescriptions.S060;
				case "061":
					return TN41UpdateDescriptions.S061;
				case "062":
					return TN41UpdateDescriptions.S062;
				case "063":
					return TN41UpdateDescriptions.S063;
				case "064":
					return TN41UpdateDescriptions.S064;
				case "065":
					return TN41UpdateDescriptions.S065;
				case "066":
					return TN41UpdateDescriptions.S066;
				case "067":
					return TN41UpdateDescriptions.S067;
				case "068":
					return TN41UpdateDescriptions.S068;
				case "069":
					return TN41UpdateDescriptions.S069;
				case "070":
					return TN41UpdateDescriptions.S070;
				case "071":
					return TN41UpdateDescriptions.S071;
				case "072":
					return TN41UpdateDescriptions.S072;
				case "073":
					return TN41UpdateDescriptions.S073;
				case "074":
					return TN41UpdateDescriptions.S074;
				case "075":
					return TN41UpdateDescriptions.S075;
				case "076":
					return TN41UpdateDescriptions.S076;
				case "077":
					return TN41UpdateDescriptions.S077;
				case "078":
					return TN41UpdateDescriptions.S078;
				case "079":
					return TN41UpdateDescriptions.S079;
				case "080":
					return FormattedOutput(TN41UpdateDescriptions.S080, 1);
				case "081":
					return FormattedOutput(TN41UpdateDescriptions.S081, 1);
				case "082":
					return FormattedOutput(TN41UpdateDescriptions.S082, 1);
				case "083":
					return FormattedOutput(TN41UpdateDescriptions.S083, 1);
				case "084":
					return FormattedOutput(TN41UpdateDescriptions.S084, 1);
				case "085":
					return FormattedOutput(TN41UpdateDescriptions.S085, 1);
				case "086":
					return FormattedOutput(TN41UpdateDescriptions.S086, 1);
				case "087":
					return FormattedOutput(TN41UpdateDescriptions.S087, 1);
				case "088":
					return FormattedOutput(TN41UpdateDescriptions.S088, 1);
				case "089":
					return FormattedOutput(TN41UpdateDescriptions.S089, 1);
				case "090":
					return FormattedOutput(TN41UpdateDescriptions.S090, 1);
				case "091":
					return FormattedOutput(TN41UpdateDescriptions.S091, 1);
				case "092":
					return FormattedOutput(TN41UpdateDescriptions.S092, 1);
				case "093":
					return FormattedOutput(TN41UpdateDescriptions.S093, 1);
				case "094":
					return FormattedOutput(TN41UpdateDescriptions.S094, 1);
				case "095":
					return FormattedOutput(TN41UpdateDescriptions.S095, 1);
				case "096":
					return FormattedOutput(TN41UpdateDescriptions.S096, 1);
				case "097":
					return FormattedOutput(TN41UpdateDescriptions.S097, 1);
				case "098":
					return FormattedOutput(TN41UpdateDescriptions.S098, 1);
				case "099":
					return FormattedOutput(TN41UpdateDescriptions.S099, 1);
				case "100":
					return FormattedOutput(TN41UpdateDescriptions.S100, 1);
				case "101":
					return FormattedOutput(TN41UpdateDescriptions.S101, 1);
				case "102":
					return FormattedOutput(TN41UpdateDescriptions.S102, 2, TN41UpdateDescriptions.CASCProductOccurrence);
				case "103":
					return FormattedOutput(TN41UpdateDescriptions.S103, 2, TN41UpdateDescriptions.CASCProductOccurrence);
				case "104":
					return FormattedOutput(TN41UpdateDescriptions.S104, 2, TN41UpdateDescriptions.CASCProductOccurrence);
				case "105":
					return FormattedOutput(TN41UpdateDescriptions.S105, 3, TN41UpdateDescriptions.CASCProductOccurrence, TN41UpdateDescriptions.CASCCodeOccurrence);
				case "106":
					return FormattedOutput(TN41UpdateDescriptions.S106, 3, TN41UpdateDescriptions.CASCProductOccurrence, TN41UpdateDescriptions.CASCCodeOccurrence);
				case "107":
					return FormattedOutput(TN41UpdateDescriptions.S107, 3, TN41UpdateDescriptions.CASCProductOccurrence, TN41UpdateDescriptions.CASCCodeOccurrence);
				case "108":
					return FormattedOutput(TN41UpdateDescriptions.S108, 1);
				case "109":
					return FormattedOutput(TN41UpdateDescriptions.S109, 1);
				case "110":
					return FormattedOutput(TN41UpdateDescriptions.S110, 1);
				case "111":
					return FormattedOutput(TN41UpdateDescriptions.S111, 1);
				case "112":
					return FormattedOutput(TN41UpdateDescriptions.S112, 1);
				case "113":
					return FormattedOutput(TN41UpdateDescriptions.S113, 1);
				case "114":
					return FormattedOutput(TN41UpdateDescriptions.S114, 1);
				case "115":
					return FormattedOutput(TN41UpdateDescriptions.S115, 1);
				case "116":
					return FormattedOutput(TN41UpdateDescriptions.S116, 1);
				case "117":
					return FormattedOutput(TN41UpdateDescriptions.S117, 1);
				case "118":
					return FormattedOutput(TN41UpdateDescriptions.S118, 1);
				case "119":
					return FormattedOutput(TN41UpdateDescriptions.S119, 1);
				case "120":
					return FormattedOutput(TN41UpdateDescriptions.S120, 1);
				case "121":
					return FormattedOutput(TN41UpdateDescriptions.S121, 1);
				case "122":
					return FormattedOutput(TN41UpdateDescriptions.S122, 1);
				case "123":
					return FormattedOutput(TN41UpdateDescriptions.S123, 1);
				case "124":
					return FormattedOutput(TN41UpdateDescriptions.S124, 1);
				case "125":
					return FormattedOutput(TN41UpdateDescriptions.S125, 1);
				case "126":
					return FormattedOutput(TN41UpdateDescriptions.S126, 1);
				case "127":
					return FormattedOutput(TN41UpdateDescriptions.S127, 1);
				case "128":
					return FormattedOutput(TN41UpdateDescriptions.S128, 1);
				case "129":
					return FormattedOutput(TN41UpdateDescriptions.S129, 1);
				case "130":
					return FormattedOutput(TN41UpdateDescriptions.S130, 1);
				case "131":
					return FormattedOutput(TN41UpdateDescriptions.S131, 1);
				case "132":
					return FormattedOutput(TN41UpdateDescriptions.S132, 1);
				case "133":
					return FormattedOutput(TN41UpdateDescriptions.S133, 1);
				case "134":
					return FormattedOutput(TN41UpdateDescriptions.S134, 1);
				case "135":
					return FormattedOutput(TN41UpdateDescriptions.S135, 1);
				case "136":
					return FormattedOutput(TN41UpdateDescriptions.S136, 1);
				case "137":
					return FormattedOutput(TN41UpdateDescriptions.S137, 1);
				case "138":
					return FormattedOutput(TN41UpdateDescriptions.S138, 1);
				case "139":
					return FormattedOutput(TN41UpdateDescriptions.S139, 1);
				case "140":
					return FormattedOutput(TN41UpdateDescriptions.S140, 1);
				case "141":
					return FormattedOutput(TN41UpdateDescriptions.S141, 1);
				case "142":
					return FormattedOutput(TN41UpdateDescriptions.S142, 1);
				case "143":
					return FormattedOutput(TN41UpdateDescriptions.S143, 1);
				case "144":
					return FormattedOutput(TN41UpdateDescriptions.S144, 1);
				case "145":
					return FormattedOutput(TN41UpdateDescriptions.S145, 1);
				case "146":
					return FormattedOutput(TN41UpdateDescriptions.S146, 1);
				case "147":
					return FormattedOutput(TN41UpdateDescriptions.S147, 1);
				case "148":
					return FormattedOutput(TN41UpdateDescriptions.S148, 1);
				case "149":
					return FormattedOutput(TN41UpdateDescriptions.S149, 1);
				case "150":
					return FormattedOutput(TN41UpdateDescriptions.S150, 1);
				case "151":
					return FormattedOutput(TN41UpdateDescriptions.S151, 1);
				case "152":
					return FormattedOutput(TN41UpdateDescriptions.S152, 1);
				case "153":
					return FormattedOutput(TN41UpdateDescriptions.S153, 1);
				case "154":
					return FormattedOutput(TN41UpdateDescriptions.S154, 1);
				case "155":
					return FormattedOutput(TN41UpdateDescriptions.S155, 1);
				case "156":
					return FormattedOutput(TN41UpdateDescriptions.S156, 1);
				case "157":
					return FormattedOutput(TN41UpdateDescriptions.S157, 1);
				case "158":
					return FormattedOutput(TN41UpdateDescriptions.S158, 1);
				case "159":
					return FormattedOutput(TN41UpdateDescriptions.S159, 1);
				case "160":
					return FormattedOutput(TN41UpdateDescriptions.S160, 1);
				case "161":
					return FormattedOutput(TN41UpdateDescriptions.S161, 1);
				case "162":
					return FormattedOutput(TN41UpdateDescriptions.S162, 1);
				case "163":
					return TN41UpdateDescriptions.S163;
				case "164":
					return TN41UpdateDescriptions.S164;
				case "165":
					return TN41UpdateDescriptions.S165;
				case "166":
					return TN41UpdateDescriptions.S166;
				case "167":
					return TN41UpdateDescriptions.S167;
				case "168":
					return TN41UpdateDescriptions.S168;
				case "169":
					return TN41UpdateDescriptions.S169;
				case "170":
					return TN41UpdateDescriptions.S170;
				case "171":
					return TN41UpdateDescriptions.S171;
				case "172":
					return TN41UpdateDescriptions.S172;
				case "173":
					return TN41UpdateDescriptions.S173;
				case "174":
					return FormattedOutput(TN41UpdateDescriptions.S174, 1);
				case "175":
					return FormattedOutput(TN41UpdateDescriptions.S175, 1);
				case "176":
					return FormattedOutput(TN41UpdateDescriptions.S176, 1);
				case "177":
					return FormattedOutput(TN41UpdateDescriptions.S177, 1);
				case "178":
					return FormattedOutput(TN41UpdateDescriptions.S178, 1);
				case "179":
					return FormattedOutput(TN41UpdateDescriptions.S179, 1);
				case "180":
					return FormattedOutput(TN41UpdateDescriptions.S180, 1);
				case "181":
					return FormattedOutput(TN41UpdateDescriptions.S181, 1);
				case "182":
					return FormattedOutput(TN41UpdateDescriptions.S182, 1);
				case "183":
					return FormattedOutput(TN41UpdateDescriptions.S183, 1);
				case "184":
					return TN41UpdateDescriptions.S184;
				case "185":
					return TN41UpdateDescriptions.S185;
				case "186":
					return TN41UpdateDescriptions.S186;
				case "187":
					return TN41UpdateDescriptions.S187;
				case "188":
					return TN41UpdateDescriptions.S188;
				case "189":
					return TN41UpdateDescriptions.S189;
				case "190":
					return TN41UpdateDescriptions.S190;
				case "191":
					return TN41UpdateDescriptions.S191;
				case "192":
					return TN41UpdateDescriptions.S192;
				case "193":
					return TN41UpdateDescriptions.S193;
				case "194":
					return TN41UpdateDescriptions.S194;
				case "195":
					return TN41UpdateDescriptions.S195;
				case "196":
					return TN41UpdateDescriptions.S196;
				case "197":
					return TN41UpdateDescriptions.S197;
				case "198":
					return TN41UpdateDescriptions.S198;
				case "199":
					return TN41UpdateDescriptions.S199;
				case "200":
					return TN41UpdateDescriptions.S200;
				case "201":
					return TN41UpdateDescriptions.S201;
				case "202":
					return TN41UpdateDescriptions.S202;
				case "203":
					return TN41UpdateDescriptions.S203;
				case "204":
					return TN41UpdateDescriptions.S204;
				case "205":
					return TN41UpdateDescriptions.S205;
				case "206":
					return TN41UpdateDescriptions.S206;
				case "207":
					return FormattedOutput(TN41UpdateDescriptions.S207, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "208":
					return FormattedOutput(TN41UpdateDescriptions.S208, 1);
				case "209":
					return FormattedOutput(TN41UpdateDescriptions.S209, 2, TN41UpdateDescriptions.ProcessingCodeOccurrence);
				case "210":
					return FormattedOutput(TN41UpdateDescriptions.S210, 2, TN41UpdateDescriptions.ProcessingCodeOccurrence);
				case "211":
					return FormattedOutput(TN41UpdateDescriptions.S211, 2, TN41UpdateDescriptions.ProcessingCodeOccurrence);
				case "212":
					return TN41UpdateDescriptions.S212;
				case "213":
					return FormattedOutput(TN41UpdateDescriptions.S213, 1);
				case "214":
					return FormattedOutput(TN41UpdateDescriptions.S214, 1);
				case "215":
					return FormattedOutput(TN41UpdateDescriptions.S215, 1);
				case "216":
					return TN41UpdateDescriptions.S216;
				case "217":
					return FormattedOutput(TN41UpdateDescriptions.S217, 1);
				case "218":
					return FormattedOutput(TN41UpdateDescriptions.S218, 1);
				case "219":
					return TN41UpdateDescriptions.S219;
				case "901":
					return FormattedOutput(TN41UpdateDescriptions.S901, 1);
				case "902":
					return FormattedOutput(TN41UpdateDescriptions.S902, 1);
				case "903":
					return FormattedOutput(TN41UpdateDescriptions.S903, 2, TN41UpdateDescriptions.ContainerInfoDesc);
				case "904":
					return FormattedOutput(TN41UpdateDescriptions.S904, 1);
				case "905":
					return FormattedOutput(TN41UpdateDescriptions.S905, 1);
				case "906":
					return FormattedOutput(TN41UpdateDescriptions.S906, 1);
				case "907":
					return FormattedOutput(TN41UpdateDescriptions.S907, 1);
				case "908":
					return FormattedOutput(TN41UpdateDescriptions.S908, 2, TN41UpdateDescriptions.CASCProductOccurrence);
				case "909":
					return FormattedOutput(TN41UpdateDescriptions.S909, 3, TN41UpdateDescriptions.CASCProductOccurrence, TN41UpdateDescriptions.CASCCodeOccurrence);
				case "910":
					return FormattedOutput(TN41UpdateDescriptions.S910, 1);
				case "911":
					return FormattedOutput(TN41UpdateDescriptions.S911, 2, TN41UpdateDescriptions.ProcessingCodeOccurrence);
				default:
					throw new ArgumentException("Unrecognised Summary Code '" + fieldNumber + "' in Amendment Summary Code");
			}
		}

		#endregion

		ZString GetUpdateType()
		{
			switch (updateIndicator)
			{
				case "AL":
					return " (LINE ADDED)";
				case "AF":
					return " (ADDED)";
				case "DL":
					return " (LINE DELETED)";
				case "DF":
					return " (DELETED)";
				case "MF":
					return " (MODIFICATION)";
				default:
					return ZString.Empty;
			}
		}

		ZString FormattedOutput(string desc, int levelsToFormat)
		{
			return FormattedOutput(desc, levelsToFormat, ZString.Empty);
		}

		ZString FormattedOutput(string desc, int levelsToFormat, string secondDesc)
		{
			return FormattedOutput(desc, levelsToFormat, secondDesc, ZString.Empty);
		}

		ZString FormattedOutput(string desc, int levelsToFormat, string secondDesc, string thirdDesc)
		{
			var sb = new ZStringBuilder();

			if (levelsToFormat > 0)
			{
				sb.AppendFormat(DescFormat, desc.TrimEnd(), firstLevelSequence);
			}

			if (levelsToFormat > 1)
			{
				sb.AppendFormat(DescFormat, secondDesc.TrimEnd(), secondLevelSequence);
			}

			if (levelsToFormat > 2)
			{
				sb.AppendFormat(DescFormat, thirdDesc.TrimEnd(), thirdLevelSequence);
			}

			return sb.ToString();
		}

		const string DescFormat = "{0} {1}";

		void SplitSummaryCode(ZString summaryCode)
		{
			fieldNumber = summaryCode.SubstringSafe(0, 3);
			firstLevelSequence = summaryCode.SubstringSafe(4, 2);
			secondLevelSequence = summaryCode.SubstringSafe(7, 2);
			thirdLevelSequence = summaryCode.SubstringSafe(10, 2);
			updateIndicator = summaryCode.SubstringSafe(12, 2);
		}

		ZString fieldNumber;
		ZString firstLevelSequence;
		ZString secondLevelSequence;
		ZString thirdLevelSequence;
		ZString updateIndicator;

		#endregion

		#region Updated/Amended Field Descriptions

		protected class FieldDescriptions
		{
			public const string S001 = "Message No";
			public const string S002 = "Additional Recipient Id";
			public const string S003 = "Place of release of cargo, location code";
			public const string S004 = "Place of release of cargo, location name/address";
			public const string S005 = "Place of receipt of cargo, location code";
			public const string S006 = "Place of receipt of cargo, location name/address";
			public const string S008 = "Place of storage, location code";
			public const string S009 = "Country of Final Destination";              // Note: This is a SG Customs code list value
			public const string S010 = "Arrival date";
			public const string S011 = "Departure date";
			public const string S012 = "Start date of cargo removal period";
			public const string S013 = "End of exhibition period/End date of temporary import";
			public const string S014 = "Permit validity period";
			public const string S015 = "NRT of outward vessel, value";
			public const string S016 = "Total outer pack, quantity";
			public const string S017 = "Total outer pack, unit";
			public const string S018 = "Total gross weight, quantity";
			public const string S019 = "Total gross weight, unit";
			public const string S020 = "Container Number";
			public const string S021 = "Container Type";
			public const string S022 = "Container Size";
			public const string S023 = "Container Weight";
			public const string S024 = "Container Sequence Number";
			public const string S025 = "General/Trader's remarks";
			public const string S026 = "Supply indicator";
			public const string S027 = "Inward Ocean Bill of Lading/Ocean Unique Cargo Reference Master Air Waybill";
			public const string S028 = "Outward Ocean Bill of Lading/Ocean Unique Cargo Reference Number/MAWB";
			public const string S029 = "Previous Permit number";
			public const string S030 = "Inward mode of transport";
			public const string S031 = "Inward vessel name";
			public const string S032 = "Inward voyage number/flight number/registration number";
			public const string S033 = "Outward mode of transport";
			public const string S034 = "Outward vessel type";
			public const string S035 = "Outward vessel name";
			public const string S036 = "Outward voyage number of towing vessel";
			public const string S037 = "Name of towing vessel";
			public const string S038 = "Outward voyage number/flight number/registration number";
			public const string S039 = "Nationality of vessel";
			public const string S040 = "Port of loading, port code";
			public const string S041 = "Port of discharge, port code";
			public const string S042 = "Next port of call, port code";
			public const string S043 = "Final port of call, port code";
			public const string S044 = "Inward vessel location, location code";
			public const string S045 = "Inward vessel location, location name";
			public const string S046 = "Outward vessel location, location code";
			public const string S047 = "Outward vessel location, location name";
			public const string S048 = "Declarant name";
			public const string S049 = "Declarant code";
			public const string S050 = "Declarant telephone number";
			public const string S051 = "Claimant Entity Identifier";
			public const string S052 = "Claimant organisation name";
			public const string S053 = "Claimant employee name";
			public const string S054 = "Claimant code";
			public const string S055 = "Declaring Agent Entity Identifier";
			public const string S056 = "Declaring Agent organisation name";
			public const string S057 = "Inward Carrier Agent Entity Identifier";
			public const string S058 = "Inward Carrier Agent organisation name";
			public const string S059 = "Outward Carrier Agent Entity Identifier";
			public const string S060 = "Outward Carrier Agent organisation name";
			public const string S061 = "Exporter Entity Identifier";
			public const string S062 = "Exporter organisation name";
			public const string S063 = "Importer Entity Identifier";
			public const string S064 = "Importer organisation name";
			public const string S065 = "FF/NVOCC/Consolidator/Cargo Agent Entity Identifier";
			public const string S067 = "FF/NVOCC/Consolidator/Cargo Agent name";
			public const string S068 = "Handling Agent Entity Identifier";
			public const string S069 = "Handling Agent organisation name";
			public const string S070 = "Consignee name";
			public const string S071 = "Consignee address";
			public const string S072 = "BG indicator";
			public const string S073 = "End-user Name";
			public const string S074 = "End-user Address";
			public const string S075 = "Total invoice value";
			public const string S076 = "Total Invoice currency charge";
			public const string S077 = "Total Invoice rate of exchange";
			public const string S078 = "Unit Price term type";
			public const string S079 = "Supplier/Manufacturer name";
			public const string S080 = "Supplier/Manufacturer code";
			public const string S081 = "Invoice number";
			public const string S082 = "Invoice date";
			public const string S083 = "Other taxable charge amount";
			public const string S084 = "Other taxable charge currency";
			public const string S085 = "Other taxable charge exchange rate";
			public const string S086 = "Other taxable charge percentage";
			public const string S087 = "Freight charge";
			public const string S088 = "Freight charge currency code";
			public const string S089 = "Freight charge currency rate";
			public const string S090 = "Freight charge percentage";
			public const string S091 = "Insurance charge";
			public const string S092 = "Insurance charge currency code";
			public const string S093 = "Insurance charge currency rate";
			public const string S094 = "Insurance charge percentage";
			public const string S095 = "Serial number";
			public const string S096 = "HS code";
			public const string S097 = "CA/SC Product code";
			public const string S098 = "CA/SC Product code quantity";
			public const string S099 = "CA/SC Product code quantity unit";
			public const string S100 = "CA/SC Code 1";
			public const string S101 = "CA/SC Code 2";
			public const string S102 = "CA/SC Code 3";
			public const string S103 = "HS quantity";
			public const string S104 = "HS quantity unit";
			public const string S105 = "Dutiable quantity/weight/volume";
			public const string S106 = "Dutiable quantity/weight/volume unit";
			public const string S107 = "Total dutiable quantity/weight/volume";
			public const string S108 = "Total dutiable quantity/weight/volume unit";
			public const string S109 = "Goods description";
			public const string S110 = "Brand name";
			public const string S111 = "Model";
			public const string S112 = "GST rate";
			public const string S113 = "DG indicator";
			public const string S114 = "Country of Origin";             // Note: This is a SG Customs code list value
			public const string S115 = "Current lot number";
			public const string S116 = "Previous lot number";
			public const string S117 = "% Alcohol by volume value";
			public const string S118 = "% Alcohol by volume unit";
			public const string S119 = "Outer-pack quantity";
			public const string S120 = "Outer-pack quantity unit";
			public const string S121 = "In-pack quantity";
			public const string S122 = "In-pack quantity unit";
			public const string S123 = "Inner-pack quantity";
			public const string S124 = "Inner-pack quantity unit";
			public const string S125 = "Inmost-pack quantity";
			public const string S126 = "Inmost-pack quantity unit";
			public const string S127 = "Marking";
			public const string S128 = "Marks and numbers";
			public const string S129 = "LSP value";
			public const string S130 = "CIF/FOB value";
			public const string S131 = "Unit price";
			public const string S132 = "Unit price currency";
			public const string S133 = "Unit price rate of exchange";
			public const string S134 = "Invoice number";
			public const string S135 = "Vehicle registration number";
			public const string S136 = "Date of first registration";
			public const string S137 = "Engine capacity/power value";
			public const string S138 = "Engine capacity/power unit";
			public const string S139 = "End-use description";
			public const string S140 = "Inward Ocean Bill of Lading/Ocean Unique Cargo Reference number/Master Air Waybill";
			public const string S141 = "Inward House Bill of Lading/House Unique Cargo Reference number/House Air Waybill";
			public const string S142 = "Outward Ocean Bill of Lading/Ocean Unique Cargo Reference number/ Master Air Waybill";
			public const string S143 = "Outward House Bill of Lading/House Unique Cargo Reference Number/House Air Waybill";
			public const string S144 = "Customs duty amount";
			public const string S145 = "Customs duty rate";
			public const string S146 = "Customs duty rate unit";
			public const string S147 = "Excise duty amount";
			public const string S148 = "Excise duty rate";
			public const string S149 = "Excise duty rate unit";
			public const string S150 = "GST payable";
			public const string S151 = "Optional item charges amount";
			public const string S152 = "Optional item charges currency";
			public const string S153 = "Optional item charges exchange rate";
			public const string S154 = "Preferential Indicator";
			public const string S155 = "Sequence Number of the Certificate";
			public const string S156 = "Certificate Type";
			public const string S157 = "Additional Copies";
			public const string S158 = "Document Type";
			public const string S159 = "Filename";
			public const string S160 = "Total CIF/FOB value";
			public const string S161 = "Total number of items declared";
			public const string S162 = "Total Customs duty payable";
			public const string S163 = "Total Excise duty payable";
			public const string S164 = "Total GST payable";
			public const string S166 = "Total amount payable";
			public const string S167 = "Percentage Content";
			public const string S168 = "GSP Donor Country";             // SG Customs code list value
			public const string S169 = "Entry Year";
			public const string S170 = "Certificate Additional Info";
			public const string S171 = "Transport Details";
			public const string S172 = "Date of Manufacturing Cost Statement";
			public const string S173 = "Certificate Item Description";
			public const string S174 = "Certificate Item Quantity";
			public const string S175 = "Certificate Item Quantity Unit";
			public const string S176 = "Item Value";
			public const string S177 = "Textile Category Code";
			public const string S178 = "Textile Quota Quantity";
			public const string S179 = "Textile Quota Unit";
			public const string S180 = "Origin Criterion Text";
			public const string S181 = "Origin HS Code";
			public const string S182 = "Exporter Address";
			public const string S183 = "Application Product Type";
			public const string S184 = "Application Type";
			public const string S185 = "Manufacturer Address";
			public const string S186 = "Manufacturer Name";
			public const string S187 = "Manufacturer Entity Identifier";
			public const string S189 = "Cargo Packing Type";
			public const string S190 = "Declarant Id";
			public const string S191 = "Declaration Indicator";
			public const string S192 = "Declaration Type";
			public const string S193 = "Freight Forwarder CR Number";
			public const string S194 = "Freight Forwarder Name";
			public const string S195 = "Percentage of Commonwealth Content";
			public const string S196 = "Reference Currency";
			public const string S197 = "Sender Id";
			public const string S198 = "Shipper Seal Number";
			public const string S901 = "Item";
			public const string S902 = "Container Details";
			public const string S903 = "Container information";
			public const string S904 = "CA Licence";
			public const string S905 = "Supporting Document";
			public const string S906 = "Invoice";
			public const string S907 = "Certificate Details";
			public const string S908 = "CA/SC Product,";
			public const string S909 = "CA/SC Code";
		}

		protected class TN41UpdateDescriptions
		{
			public const string S001 = "Message No";
			public const string S002 = "Additional Recipient Id";
			public const string S003 = "Place of release of cargo, location code";
			public const string S004 = "Place of release of cargo, location name/address";
			public const string S005 = "Place of receipt of cargo, location code";
			public const string S006 = "Place of receipt of cargo, location name/address";
			public const string S007 = "Place of storage, location code";
			public const string S008 = "Country of Final Destination";                      // Note: This is a SG Customs code list value
			public const string S009 = "Arrival date";
			public const string S010 = "Departure date";
			public const string S011 = "Start date of cargo removal period";
			public const string S012 = "Start of exhibition period/Start date of temporary import";
			public const string S013 = "End of exhibition period/End date of temporary import";
			public const string S014 = "Permit validity period";
			public const string S015 = "NRT of outward vessel, value";
			public const string S016 = "Total outer pack, quantity";
			public const string S017 = "Total outer pack, unit";
			public const string S018 = "Total gross weight, quantity";
			public const string S019 = "Total gross weight, unit";
			public const string S020 = "Container Number, Container details";
			public const string ContainerInfoDesc = ", Container info occ ";
			public const string S021 = "Container Type, Container details";
			public const string S022 = "Container Size, Container details";
			public const string S023 = "Container Weight, Container details";
			public const string S024 = "Container Sequence Number, Container details";
			public const string S025 = "General/Trader's remarks";
			public const string S026 = "Supply indicator";
			public const string S027 = "Inward Ocean Bill of Lading/Ocean Unique Cargo Reference Master Air Waybill";
			public const string S028 = "Outward Ocean Bill of Lading/Ocean Unique Cargo Reference Number/MAWB";
			public const string S029 = "Previous Permit number";
			public const string S030 = "Inward mode of transport";
			public const string S031 = "Inward Transport Identifier";
			public const string S032 = "Inward Conveyance Reference number";
			public const string S033 = "Outward mode of transport";
			public const string S034 = "Outward vessel type";
			public const string S035 = "Outward Transport Identifier";
			public const string S036 = "Outward Conveyance Reference number";
			public const string S037 = "Outward voyage number of towing vessel";
			public const string S038 = "Name of towing vessel";
			public const string S039 = "Nationality of vessel";
			public const string S040 = "Port of loading, port code";
			public const string S041 = "Port of discharge, port code";
			public const string S042 = "Next port of call, port code";
			public const string S043 = "Final port of call, port code";
			public const string S044 = "Declarant name";
			public const string S045 = "Declarant code";
			public const string S046 = "Declarant telephone number";
			public const string S047 = "Claimant Entity Identifier";
			public const string S048 = "Claimant organisation name";
			public const string S049 = "Claimant name";
			public const string S050 = "Claimant code";
			public const string S051 = "Declaring Agent Entity Identifier";
			public const string S052 = "Declaring Agent organisation name";
			public const string S053 = "Inward Carrier Agent Entity Identifier";
			public const string S054 = "Inward Carrier Agent organisation name";
			public const string S055 = "Outward Carrier Agent Entity Identifier";
			public const string S056 = "Outward Carrier Agent organisation name";
			public const string S057 = "Exporter Entity Identifier";
			public const string S058 = "Exporter organisation name";
			public const string S059 = "Importer Entity Identifier";
			public const string S060 = "Importer organisation name";
			public const string S061 = "Freight Forwarder Entity Identifier (including NVOCC, Consolidator and Cargo Agent)";
			public const string S062 = "Freight Forwarder organization name (including NVOCC, Consolidator and Cargo Agent)";
			public const string S063 = "Handling Agent Entity Identifier";
			public const string S064 = "Handling Agent organisation name";
			public const string S065 = "Consignee name";
			public const string S066 = "Consignee address (Street and number/P.O. box)";
			public const string S067 = "Consignee address (City Name)";
			public const string S068 = "Consignee address (Country Code)";                      // Note: This is a SG Customs code list value
			public const string S069 = "Consignee address (Country Subdivision Code)";          // Note: This is a SG Customs code list value
			public const string S070 = "Consignee address (Country Subdivision Name)";          // Note: This is a SG Customs code list value
			public const string S071 = "Consignee address (Country Postal Code)";               // Note: This is a SG Customs code list value
			public const string S072 = "BG indicator";
			public const string S073 = "End-user Name";
			public const string S074 = "End-user Address (Street and number/P.O. box)";
			public const string S075 = "End-user address (City Name)";
			public const string S076 = "End-user address (Country Code)";                       // Note: This is a SG Customs code list value
			public const string S077 = "End-user address (Country Subdivision Code)";           // Note: This is a SG Customs code list value
			public const string S078 = "End-user address (Country Subdivision Name)";           // Note: This is a SG Customs code list value
			public const string S079 = "End-user address (Country Postal Code)";                // Note: This is a SG Customs code list value
			public const string S080 = "Total invoice value, invoice item";
			public const string S081 = "Total Invoice currency charge, invoice item";
			public const string S082 = "Total Invoice rate of exchange, invoice item";
			public const string S083 = "Unit Price term type, invoice item";
			public const string S084 = "Supplier/Manufacturer name, invoice item";
			public const string S085 = "Supplier/Manufacturer code, invoice item";
			public const string S086 = "Invoice number, invoice item";
			public const string S087 = "Invoice date, invoice item";
			public const string S088 = "Other taxable charge amount, invoice item";
			public const string S089 = "Other taxable charge currency, invoice item";
			public const string S090 = "Other taxable charge exchange rate, invoice item";
			public const string S091 = "Other taxable charge percentage, invoice item";
			public const string S092 = "Freight charge, invoice item";
			public const string S093 = "Freight charge currency code, invoice item";
			public const string S094 = "Freight charge currency rate, invoice item";
			public const string S095 = "Freight charge percentage, invoice item";
			public const string S096 = "Insurance charge, invoice item";
			public const string S097 = "Insurance charge currency code, invoice item";
			public const string S098 = "Insurance charge currency rate, invoice item";
			public const string S099 = "Insurance charge percentage, invoice item";
			public const string S100 = "Serial number, item";
			public const string S101 = "HS code, item";
			public const string S102 = "CA/SC Product code, item";
			public const string CASCProductOccurrence = ", CA/SC product occ ";
			public const string S103 = "CA/SC Product code quantity, item";
			public const string S104 = "CA/SC Product code quantity unit, item";
			public const string S105 = "CA/SC Code 1, item";
			public const string CASCCodeOccurrence = ", CA/SC code occ ";
			public const string S106 = "CA/SC Code 2, item";
			public const string S107 = "CA/SC Code 3, item";
			public const string S108 = "HS quantity, item";
			public const string S109 = "HS quantity unit, item";
			public const string S110 = "Dutiable quantity/weight/volume, item";
			public const string S111 = "Dutiable quantity/weight/volume unit, item";
			public const string S112 = "Total dutiable quantity/weight/volume, item";
			public const string S113 = "Total dutiable quantity/weight/volume unit, item";
			public const string S114 = "Goods description, item";
			public const string S115 = "Brand name, item";
			public const string S116 = "Model , item";
			public const string S117 = "GST rate, item";
			public const string S118 = "DG indicator, item";
			public const string S119 = "Country of Origin, item";           // Note: This is a SG Customs code list value
			public const string S120 = "Current lot number, item";
			public const string S121 = "Previous lot number, item";
			public const string S122 = "% Alcohol by volume value, item";
			public const string S123 = "Outer-pack quantity, item";
			public const string S124 = "Outer-pack quantity unit, item";
			public const string S125 = "In-pack quantity, item";
			public const string S126 = "In-pack quantity unit, item";
			public const string S127 = "Inner-pack quantity, item";
			public const string S128 = "Inner-pack quantity unit, item";
			public const string S129 = "Inmost-pack quantity, item";
			public const string S130 = "Inmost-pack quantity unit, item";
			public const string S131 = "Marking, item";
			public const string S132 = "Marks and numbers, item";
			public const string S133 = "LSP value, item";
			public const string S134 = "CIF/FOB value, item";
			public const string S135 = "Unit price, item";
			public const string S136 = "Unit price currency, item";
			public const string S137 = "Unit price rate of exchange, item";
			public const string S138 = "Invoice number, item";
			public const string S139 = "Date of first registration, item";
			public const string S140 = "Engine capacity/power value, item";
			public const string S141 = "Engine capacity/power unit, item";
			public const string S142 = "End-use description, item";
			public const string S143 = "Inward Ocean Bill of Lading / Ocean Unique Cargo Reference number / Master Air Waybill, item";
			public const string S144 = "Inward House Bill of Lading / House Unique Cargo Reference number / House Air Waybill, item";
			public const string S145 = "Outward Ocean Bill of Lading / Ocean Unique Cargo Reference number / Master Air Waybill, item";
			public const string S146 = "Outward Ocean Bill of Lading / Ocean Unique Cargo Reference number/ Master Air Waybill, item";
			public const string S147 = "Customs duty amount, item";
			public const string S148 = "Customs duty rate, item";
			public const string S149 = "Customs duty rate unit, item";
			public const string S150 = "Excise duty amount, item";
			public const string S151 = "Excise duty rate, item";
			public const string S152 = "Excise duty rate unit, item";
			public const string S153 = "GST payable, item";
			public const string S154 = "Optional item charges amount, item";
			public const string S155 = "Optional item charges currency, item";
			public const string S156 = "Optional item charges exchange rate, item";
			public const string S157 = "Preferential Indicator, item";
			public const string S158 = "Sequence Number of the Certificate";
			public const string S159 = "Certificate Type";
			public const string S160 = "Additional Copies";
			public const string S161 = "Document Type";
			public const string S162 = "Filename";
			public const string S163 = "Total CIF/FOB value";
			public const string S164 = "Total number of items declared";
			public const string S165 = "Total Customs duty payable";
			public const string S166 = "Total Excise duty payable";
			public const string S167 = "Total GST payable";
			public const string S168 = "Total amount payable";
			public const string S169 = "Percentage Content";
			public const string S170 = "GSP Donor Country";
			public const string S171 = "Entry Year";
			public const string S172 = "Certificate Additional Info";
			public const string S173 = "Transport Details";
			public const string S174 = "Date of Manufacturing Cost Statement";
			public const string S175 = "Certificate Item Description";
			public const string S176 = "Certificate Item Quantity";
			public const string S177 = "Certificate Item Quantity Unit";
			public const string S178 = "Item Value";
			public const string S179 = "Textile Category Code";
			public const string S180 = "Textile Quota Quantity";
			public const string S181 = "Textile Quota Unit";
			public const string S182 = "Origin Criterion Text";
			public const string S183 = "Origin HS Code";
			public const string S184 = "Exporter Address (Street and number/P.O. box)";
			public const string S185 = "Exporter Address (City Name)";
			public const string S186 = "Exporter Address (Country Code)";                   // Note: This is a SG Customs code list value
			public const string S187 = "Exporter Address (Country Subdivision Code)";       // Note: This is a SG Customs code list value
			public const string S188 = "Exporter Address (Country Subdivision Name)";       // Note: This is a SG Customs code list value
			public const string S189 = "Exporter Address (Country Postal Code)";            // Note: This is a SG Customs code list value
			public const string S190 = "Application Product Type";
			public const string S191 = "Application Type";
			public const string S192 = "Manufacturer Address (Street and number/P.O. box)";
			public const string S193 = "Manufacturer Address (City Name)";
			public const string S194 = "Manufacturer Address (Country Code)";               // Note: This is a SG Customs code list value
			public const string S195 = "Manufacturer Address (Country Subdivision Code)";   // Note: This is a SG Customs code list value
			public const string S196 = "Manufacturer Address (Country Subdivision Name)";   // Note: This is a SG Customs code list value
			public const string S197 = "Manufacturer Address (Country Postal Code)";        // Note: This is a SG Customs code list value
			public const string S198 = "Manufacturer Name";
			public const string S199 = "Manufacturer Entity Identifier";
			public const string S200 = "Cargo Packing Type";
			public const string S201 = "Declarant Id";
			public const string S202 = "Declaration Indicator";
			public const string S203 = "Declaration Type";
			public const string S204 = "Percentage of Commonwealth Content";
			public const string S205 = "Reference Currency";
			public const string S206 = "Sender Id";
			public const string S207 = "Shipper Seal Number, Container details";
			public const string S208 = "Customs Procedure Code (CPC), Customs procedure occ";
			public const string S209 = "Processing Code 1, Customs procedure occ";
			public const string ProcessingCodeOccurrence = ", processing code occ ";
			public const string S210 = "Processing Code 2, Customs procedure occ";
			public const string S211 = "Processing Code 3, Customs procedure occ";
			public const string S212 = "Start Date of Blanket";
			public const string S213 = "Other tax amount, item";
			public const string S214 = "Other tax rate, item";
			public const string S215 = "Other tax rate unit, item";
			public const string S216 = "Total Other tax payable";
			public const string S217 = "Invoice number, item";
			public const string S218 = "Invoice date, item";
			public const string S219 = "Certificate Additional Details";
			public const string S901 = "Item";
			public const string S902 = "Container Details";
			public const string S903 = "Container information, Container details";
			public const string S904 = "CA Licence";
			public const string S905 = "Supporting Document";
			public const string S906 = "Invoice";
			public const string S907 = "Certificate Details";
			public const string S908 = "CA/SC Product item,";
			public const string S909 = "CA/SC Code, item";
			public const string S910 = "Customs Procedure, Customs procedure occ";
			public const string S911 = "Processing Code, Customs procedure occ";
		}

		#endregion
	}
}
