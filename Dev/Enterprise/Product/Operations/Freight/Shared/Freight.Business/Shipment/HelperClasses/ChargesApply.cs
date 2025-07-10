using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using AsAgreedCodes = Enterprise.Core.Constants.AWB.AsAgreedTypes.Codes;

namespace Enterprise.Freight.Business
{
	public static class ChargesApplyHelper
	{
		public static class ChargesApplyConstants
		{
			public const string NON = "NON";
			public const string ALL = "ALL";
			public const string NAL = "NAL";
			public const string NPP = "NPP";
			public const string ANO = "ANO";
			public const string APP = "APP";
			public const string CNO = "CNO";
			public const string CAL = "CAL";
			public const string CPD = "CPD";
		}

		public static CodeDescriptionPairList ChargesApplyPairList
		{
			get
			{
				var chargesApplyPairList = new CodeDescriptionPairList();

				chargesApplyPairList.AddPair(ChargesApplyConstants.NON, Res.GetString("393d390a-6b7c-c182-4ccd-01e34eeb46a9", "Do not replace rate amounts with ‘As Agreed’ on both sets."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.ALL, Res.GetString("1e62eda4-3659-d888-45cd-820e9fa46cfb", "Replace all charges with 'As Agreed' on both sets."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.NAL, Res.GetString("637a3f85-6467-bdbc-4be2-c2b53319411d", "'As Agreed': do not replace charges on first set, replace all charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.NPP, Res.GetString("20e02281-ea84-2b98-4f0a-33065b0afcd0", "'As Agreed': do not replace charges on first set, replace Prepaid charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.ANO, Res.GetString("e66059e8-910e-1c98-43f8-0ae2a5d5d3ad", "'As Agreed': replace all charges on first set, do not replace charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.APP, Res.GetString("32f31ce0-bb4e-10b4-4c17-305cf6a5226b", "'As Agreed': replace all charges on first set, replace Prepaid charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.CNO, Res.GetString("117cddbe-1f1a-49bb-41cf-dd20ae68f008", "'As Agreed': replace Collect charges on first set, do not replace charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.CAL, Res.GetString("95f9e067-3038-92bc-4185-657c860efba7", "'As Agreed': replace Collect charges on first set, replace all charges on second set."));
				chargesApplyPairList.AddPair(ChargesApplyConstants.CPD, Res.GetString("2a44181e-3cb5-748d-4927-644148e7cccb", "'As Agreed': replace collect charges on first set, replace Prepaid charges on second set."));

				return chargesApplyPairList;
			}
		}

		public static ZString GetDefaultChargesApply(ZString firstSet, ZString secondSet)
		{
			switch (firstSet)
			{
				case AsAgreedCodes.None:
					switch (secondSet)
					{
						case AsAgreedCodes.None:
							return ChargesApplyConstants.NON;

						case AsAgreedCodes.All:
							return ChargesApplyConstants.NAL;

						case AsAgreedCodes.Prepaid:
							return ChargesApplyConstants.NPP;
					}
					break;

				case AsAgreedCodes.All:
					switch (secondSet)
					{
						case AsAgreedCodes.None:
							return ChargesApplyConstants.ANO;

						case AsAgreedCodes.All:
							return ChargesApplyConstants.ALL;

						case AsAgreedCodes.Prepaid:
							return ChargesApplyConstants.APP;
					}
					break;

				case AsAgreedCodes.Collect:
					switch (secondSet)
					{
						case AsAgreedCodes.None:
							return ChargesApplyConstants.CNO;

						case AsAgreedCodes.All:
							return ChargesApplyConstants.CAL;

						case AsAgreedCodes.Prepaid:
							return ChargesApplyConstants.CPD;
					}
					break;
			}

			return ChargesApplyConstants.NON;
		}

		public static (ZString asAgreedFirstSet, ZString asAgreedSecondSet) GetAsAgreedCodesFromChargesApply(ZString chargesApply)
		{
			switch (chargesApply)
			{
				case ChargesApplyConstants.NON:
					return (AsAgreedCodes.None, AsAgreedCodes.None);

				case ChargesApplyConstants.NAL:
					return (AsAgreedCodes.None, AsAgreedCodes.All);

				case ChargesApplyConstants.NPP:
					return (AsAgreedCodes.None, AsAgreedCodes.Prepaid);

				case ChargesApplyConstants.ANO:
					return (AsAgreedCodes.All, AsAgreedCodes.None);

				case ChargesApplyConstants.ALL:
					return (AsAgreedCodes.All, AsAgreedCodes.All);

				case ChargesApplyConstants.APP:
					return (AsAgreedCodes.All, AsAgreedCodes.Prepaid);

				case ChargesApplyConstants.CNO:
					return (AsAgreedCodes.Collect, AsAgreedCodes.None);

				case ChargesApplyConstants.CAL:
					return (AsAgreedCodes.Collect, AsAgreedCodes.All);

				case ChargesApplyConstants.CPD:
					return (AsAgreedCodes.Collect, AsAgreedCodes.Prepaid);

				default:
					return (AsAgreedCodes.None, AsAgreedCodes.None);
			}
		}
	}
}
