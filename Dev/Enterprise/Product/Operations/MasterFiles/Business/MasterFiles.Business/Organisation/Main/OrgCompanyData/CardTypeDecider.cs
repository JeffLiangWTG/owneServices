using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class CardTypeDecider
	{
		static class CardTypePrefixes
		{
			public static class AmericanExpress
			{
				public const string Prefix1 = "34";
				public const string Prefix2 = "37";
			}

			public static class Bankcard
			{
				public const string Prefix = "5610";
				public const int RangeStart = 560221;
				public const int RangeEnd = 560225;
			}

			public static class ChinaUnionPay
			{
				public const int RangeStart = 622126;
				public const int RangeEnd = 622925;
			}

			public static class DinersClubCarteBlanche
			{
				public const int RangeStart = 300;
				public const int RangeEnd = 305;
			}

			public static class DinersClubenRoute
			{
				public const string Prefix1 = "2014";
				public const string Prefix2 = "2149";
			}

			public static class DinersClubInternational
			{
				public const string Prefix = "36";
			}

			public static class DinersClubUSandCanada
			{
				public const string Prefix1 = "54";
				public const string Prefix2 = "55";
			}

			public static class DiscoverCard
			{
				public const int Range1Start = 60112;
				public const int Range1End = 60114;
				public const string Prefix1 = "601174";
				public const int Range2Start = 601177;
				public const int Range2End = 601179;
				public const int Range3Start = 601186;
				public const int Range3End = 601199;
				public const int Range4Start = 644;
				public const int Range4End = 649;
				public const string Prefix2 = "65";
			}

			public static class JCB
			{
				public const int RangeStart = 3528;
				public const int RangeEnd = 3589;
			}

			public static class JCBobsolete
			{
				public const string Prefix1 = "1800";
				public const string Prefix2 = "2131";
			}

			public static class Mastercard
			{
				public const int RangeStart = 51;
				public const int RangeEnd = 55;
			}

			public static class Visa
			{
				public const string Prefix = "4";
			}

			public static class VisaElectron
			{
				public const string Prefix1 = "417500";
				public const string Prefix2 = "4917";
				public const string Prefix3 = "4913";
				public const string Prefix4 = "4508";
				public const string Prefix5 = "4844";
			}
		}

		public static ZString IdentifyCardType(ZString cardNum)
		{
			try
			{
				if (cardNum.StartsWith(CardTypePrefixes.AmericanExpress.Prefix1) ||
					cardNum.StartsWith(CardTypePrefixes.AmericanExpress.Prefix2))
				{
					return CreditCardTypeList.Codes.AmExpress;
				}

				if (cardNum.StartsWith(CardTypePrefixes.Bankcard.Prefix)
					||
					(Convert.ToInt32(cardNum.Left(6)) >= CardTypePrefixes.Bankcard.RangeStart &&
					 Convert.ToInt32(cardNum.Left(6)) <= CardTypePrefixes.Bankcard.RangeEnd))
				{
					return CreditCardTypeList.Codes.Bankcard;
				}

				if (Convert.ToInt32(cardNum.Left(6)) >= CardTypePrefixes.ChinaUnionPay.RangeStart &&
					Convert.ToInt32(cardNum.Left(6)) <= CardTypePrefixes.ChinaUnionPay.RangeEnd)
				{
					return CreditCardTypeList.Codes.ChinaUnionPay;
				}

				if (Convert.ToInt32(cardNum.Left(3)) >= CardTypePrefixes.DinersClubCarteBlanche.RangeStart &&
					Convert.ToInt32(cardNum.Left(3)) <= CardTypePrefixes.DinersClubCarteBlanche.RangeEnd)
				{
					return CreditCardTypeList.Codes.DinClub;
				}

				if (cardNum.StartsWith(CardTypePrefixes.DinersClubenRoute.Prefix1) ||
					cardNum.StartsWith(CardTypePrefixes.DinersClubenRoute.Prefix2))
				{
					return CreditCardTypeList.Codes.DinClubenRoute;
				}

				if (cardNum.StartsWith(CardTypePrefixes.DinersClubInternational.Prefix))
				{
					return CreditCardTypeList.Codes.DinClubInt;
				}

				if (cardNum.StartsWith(CardTypePrefixes.DinersClubUSandCanada.Prefix1) ||
					cardNum.StartsWith(CardTypePrefixes.DinersClubUSandCanada.Prefix2))
				{
					return CreditCardTypeList.Codes.DinClubUSCanada;
				}

				if ((Convert.ToInt32(cardNum.Left(5)) >= CardTypePrefixes.DiscoverCard.Range1Start &&
					 Convert.ToInt32(cardNum.Left(5)) <= CardTypePrefixes.DiscoverCard.Range1End) ||
					cardNum.StartsWith(CardTypePrefixes.DiscoverCard.Prefix1) ||
					(Convert.ToInt32(cardNum.Left(6)) >= CardTypePrefixes.DiscoverCard.Range2Start &&
					 Convert.ToInt32(cardNum.Left(6)) <= CardTypePrefixes.DiscoverCard.Range2End) ||
					(Convert.ToInt32(cardNum.Left(6)) >= CardTypePrefixes.DiscoverCard.Range3Start &&
					 Convert.ToInt32(cardNum.Left(6)) <= CardTypePrefixes.DiscoverCard.Range3End) ||
					(Convert.ToInt32(cardNum.Left(3)) >= CardTypePrefixes.DiscoverCard.Range4Start &&
					 Convert.ToInt32(cardNum.Left(3)) <= CardTypePrefixes.DiscoverCard.Range4End) ||
					cardNum.StartsWith(CardTypePrefixes.DiscoverCard.Prefix2))
				{
					return CreditCardTypeList.Codes.Discover;
				}

				if (Convert.ToInt32(cardNum.Left(4)) >= CardTypePrefixes.JCB.RangeStart &&
					Convert.ToInt32(cardNum.Left(4)) <= CardTypePrefixes.JCB.RangeEnd)
				{
					return CreditCardTypeList.Codes.JCB;
				}

				if (cardNum.StartsWith(CardTypePrefixes.JCBobsolete.Prefix1) ||
					cardNum.StartsWith(CardTypePrefixes.JCBobsolete.Prefix2))
				{
					return CreditCardTypeList.Codes.JCBobsolete;
				}

				if (Convert.ToInt32(cardNum.Left(2)) >= CardTypePrefixes.Mastercard.RangeStart &&
					Convert.ToInt32(cardNum.Left(2)) <= CardTypePrefixes.Mastercard.RangeEnd)
				{
					return CreditCardTypeList.Codes.Mastercard;
				}

				if (cardNum.StartsWith(CardTypePrefixes.VisaElectron.Prefix1) ||
					cardNum.StartsWith(CardTypePrefixes.VisaElectron.Prefix2) ||
					cardNum.StartsWith(CardTypePrefixes.VisaElectron.Prefix3) ||
					cardNum.StartsWith(CardTypePrefixes.VisaElectron.Prefix4) ||
					cardNum.StartsWith(CardTypePrefixes.VisaElectron.Prefix5))
				{
					return CreditCardTypeList.Codes.VisaEl;
				}

				if (cardNum.StartsWith(CardTypePrefixes.Visa.Prefix))
				{
					return CreditCardTypeList.Codes.Visa;
				}
			}
			catch (FormatException)
			{
			}

			return ZString.Empty;
		}
	}
}
