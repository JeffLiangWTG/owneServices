using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business
{
	#region Rounding Types

	public static class RatingRoundingTypes
	{
		public const string DefaultFromRegistry = "DEF";
		public const string NoRounding = Utilities.RoundingTypes.NoRounding;
		public const string Bankers = Utilities.RoundingTypes.Bankers;
		public const string UpToHalf = Utilities.RoundingTypes.UpToHalf;
		public const string UpTo1 = Utilities.RoundingTypes.UpTo1;
		public const string UpTo1IfLessThanOne = Utilities.RoundingTypes.UpTo1IfLessThanOne;
		public const string Chargeable = "CHG";
		public const string Custom = Utilities.RoundingTypes.Custom;
	}

	public class RatingRoundingTypeList : CodeDescriptionPairList
	{
		public RatingRoundingTypeList()
		{
			AddPair(RatingRoundingTypes.NoRounding, ResString.GetMultilingualString("49ebf271-0211-498c-8966-31c977a15781", "No Rounding"));
			AddPair(RatingRoundingTypes.Bankers, ResString.GetMultilingualString("932c128f-a6fc-46c2-a955-55ace6172f32", "Banker's Rounding"));
			AddPair(RatingRoundingTypes.UpTo1, ResString.GetMultilingualString("fcedd135-8003-4b1e-8bb9-ae3150f41e6a", "Round Up to Nearest unit"));
			AddPair(RatingRoundingTypes.UpTo1IfLessThanOne, ResString.GetMultilingualString("14418af5-9f4d-4f04-a237-622c4d683df3", "Round Up to Nearest unit if less than 1 unit"));
			AddPair(RatingRoundingTypes.UpToHalf, ResString.GetMultilingualString("e08cda7e-05d5-4d1d-8485-b6b5b5cf2125", "Round Up to Nearest 0.5 unit"));
			AddPair(RatingRoundingTypes.Custom, ResString.GetMultilingualString("3fe2bd43-0228-4655-91f0-1af5a98aaf56", "Round Up using custom Rounding Factor"));
			AddPair(RatingRoundingTypes.Chargeable, ResString.GetMultilingualString("8373f9dd-eee6-4301-814f-65114d0efff9", "Use Chargeable from Job"));
		}
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class DefaultRoundings : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string RoundingType = "RoundingType";
			public const string RoundingFactor = "RoundingFactor";

			public const int CodeMaxLength = 3;
			public const int DescriptionMaxLength = 100;
			public const int RoundingTypeMaxLength = 3;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultRoundings();
		}

		#endregion

		#region Properties

		#region Code

		[CargoWise.ComponentModel.MaxLength(DefaultRoundings.Schema.CodeMaxLength)]
		public ZString Code
		{
			get => fCode;
			set
			{
				CheckMaximumLength(CodeInfo, value);
				fCode = value;
				CodeInfo.RefreshBinding();
				ValidateCode();
				RoundingFactorInfo.RefreshBinding();
			}
		}

		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(DefaultRoundings.Schema.Code); }
		}

		public void ValidateCode()
		{
			if (!IsValidationSuspended)
			{
				CodeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CodeInfo);
				ListValidation.ErrorIfInvalidCode(CodeInfo, RateCategoriesAndGroupsList);
			}
		}

		#endregion

		#region Description

		[CargoWise.ComponentModel.MaxLength(DefaultRoundings.Schema.DescriptionMaxLength)]
		public ZString Description => Res.GetString("4d1e9f4f-9e28-4358-9054-8e77927c0cff", "Default Rounding For") + " " + RateCategoriesAndGroupsList.GetDescriptionFromCode(Code);

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(DefaultRoundings.Schema.Description);

		#endregion

		#region RoundingType

		[CargoWise.ComponentModel.MaxLength(DefaultRoundings.Schema.RoundingTypeMaxLength)]
		public ZString RoundingType
		{
			get => fRoundingType;
			set
			{
				CheckMaximumLength(RoundingTypeInfo, value);
				fRoundingType = value;
				RoundingTypeInfo.RefreshBinding();
				ValidateRoundingType();

				RoundingFactorInfo.RefreshBinding();
				ValidateRoundingFactor();
			}
		}

		ZString fRoundingType;

		public ZPropertyInfo RoundingTypeInfo
		{
			get { return GetZPropertyInfo(DefaultRoundings.Schema.RoundingType); }
		}

		public void ValidateRoundingType()
		{
			if (!IsValidationSuspended)
			{
				RoundingTypeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(RoundingTypeInfo);
				ListValidation.ErrorIfInvalidCode(RoundingTypeInfo, RoundingTypeList);
				ValidateChargeableRoundingTypes();
			}
		}

		void ValidateChargeableRoundingTypes()
		{
			var rateTypesSupportingCHG = new List<string>
			{
				Category.AIR,
				Category.LCL,
				Category.FCL,
				Category.ORG,
				Category.DST,
				Category.TBC,
				Category.CAI,
				Category.CLC,
				Category.CFC,
				Category.COR,
				Category.CDS,
			};

			if (!rateTypesSupportingCHG.Contains(Code) && RoundingType == RatingRoundingTypes.Chargeable)
			{
				var message = Res.GetString("0a90f530-8be3-42ee-a5ea-529ba84b9e78", "This rating mode cannot have chargeable rounding type.");
				RoundingTypeInfo.AddError(message);
			}
		}

		#endregion

		#region RoundingFactor

		static Dictionary<ZString, ZDecimal> DisplayingRoundingFactors => new Dictionary<ZString, ZDecimal>
		{
			{ RatingRoundingTypes.NoRounding, 0m },
			{ RatingRoundingTypes.Bankers, 1m },
			{ RatingRoundingTypes.UpTo1, 1m },
			{ RatingRoundingTypes.UpToHalf, 0.5m },
			{ RatingRoundingTypes.UpTo1IfLessThanOne, 1m },
			{ RatingRoundingTypes.Chargeable, 0m },
		};

		public ZDecimal RoundingFactor
		{
			get => DisplayingRoundingFactors.TryGetValue(RoundingType, out var roundingValue) ? roundingValue : roundingFactor;
			set
			{
				roundingFactor = value;
				RoundingFactorInfo.RefreshBinding();
				ValidateRoundingFactor();
			}
		}

		ZDecimal roundingFactor;

		public ZPropertyInfo RoundingFactorInfo => GetZPropertyInfo(Schema.RoundingFactor);

		protected bool RoundingFactor_ReadOnly => RoundingType != RatingRoundingTypes.Custom;

		public void ValidateRoundingFactor()
		{
			RoundingFactorInfo.ClearAllNotifications();
			if (RoundingType == RatingRoundingTypes.Custom)
			{
				MandatoryValidation.CheckEntered(RoundingFactorInfo);
				TypeValidation.CheckValidDecimal(RoundingFactorInfo, 8, 3);
			}
		}

		#endregion

		#endregion

		#region Default Values

		public void SetDefaults(string code)
		{
			Code = code;
			switch (code.ToUpper(CultureInfo.InvariantCulture))
			{
				case RatingConstants.RateCategory.ALL:
					RoundingType = RatingRoundingTypes.NoRounding;
					break;

				case RatingConstants.RateCategory.WHS:
					RoundingType = RatingRoundingTypes.UpTo1;
					break;
			}
		}

		#endregion

		#region Lookups

		public RatingRoundingTypeList RoundingTypeList
		{
			get { return new RatingRoundingTypeList(); }
		}

		public CodeDescriptionPairList RateCategoriesAndGroupsList
		{
			get { return RateCategoriesList.GetRateCategoriesAndGroupsList(); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateRoundingType();
			ValidateRoundingFactor();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.RoundingType, RoundingType);
			writer.WriteElementString(Schema.RoundingFactor, string.Format(CultureInfo.InvariantCulture, "{0}", RoundingFactor));
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			Code = wrapper.ReadElementString(Schema.Code);
			SetDefaults(Code);

			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.RoundingType:
						RoundingType = reader.ReadElementString();
						break;

					case Schema.RoundingFactor:
						RoundingFactor = wrapper.ReadElementStringAsZDecimal(Schema.RoundingFactor);
						break;

					default:
						reader.ReadElementString();
						break;
				}
			}
		}

		#endregion
	}
}

