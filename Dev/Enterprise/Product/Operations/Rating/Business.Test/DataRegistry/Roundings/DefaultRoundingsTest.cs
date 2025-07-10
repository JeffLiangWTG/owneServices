using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(DefaultRoundings))]
	public class DefaultRoundingsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateChargeableRoundingTypes()
		{
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.ALL, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.CST, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.DST, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.FCL, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.LCL, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.ORG, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.PAC, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.SNC, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.TBC, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.TRN, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.UNP, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.WHS, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: true);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.AIR, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);

			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.CAI, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.CFC, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.CLC, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.COR, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);
			AssertValidateChargeableRoundingTypes(RatingConstants.RateCategory.CDS, RatingRoundingTypes.Chargeable, "This rating mode cannot have chargeable rounding type.", expectedHasError: false);

			void AssertValidateChargeableRoundingTypes(string code, string roundingType, string error, bool expectedHasError)
			{
				var defaultRoundings = new DefaultRoundings();
				defaultRoundings.Code = code;
				defaultRoundings.RoundingType = roundingType;
				if (expectedHasError)
				{
					AssertHasError(defaultRoundings.RoundingTypeInfo, error);
				}
				else
				{
					AssertNoError(defaultRoundings.RoundingTypeInfo, error);
				}
			}
		}

		public void TestRoundingFactorDefaultValuesAndIsReadOnly()
		{
			var expectedValues = new Dictionary<ZString, (bool isReadOnly, ZDecimal defaultValue)>
			{
				{ RatingRoundingTypes.NoRounding, (true, 0m) },
				{ RatingRoundingTypes.Bankers, (true, 1m) },
				{ RatingRoundingTypes.UpToHalf, (true, 0.5m) },
				{ RatingRoundingTypes.UpTo1, (true, 1) },
				{ RatingRoundingTypes.UpTo1IfLessThanOne, (true, 1) },
				{ RatingRoundingTypes.Chargeable, (true, 0m) },
				{ RatingRoundingTypes.Custom, (false, 0) },
			};

			var defaultRoundings = new DefaultRoundings { Code = RatingConstants.RateCategory.ALL };

			var ratingRoundingTypeList = new RatingRoundingTypeList();
			var roundingTypeCodes = ratingRoundingTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedValues.Keys, roundingTypeCodes);

			foreach (var roundingType in roundingTypeCodes)
			{
				defaultRoundings.RoundingType = roundingType;
				var expectedRounding = expectedValues[roundingType];

				var message = $"With RoundingType={roundingType}, RoundingFactor should be read only";
				AssertEquals(message, expectedRounding.isReadOnly, defaultRoundings.RoundingFactorInfo.ReadOnly);

				message = $"With RoundingType={roundingType}, RoundingFactor should have default value";
				AssertEquals(message, expectedRounding.defaultValue, defaultRoundings.RoundingFactor);
			}
		}

		public void TestValidateRoundingFactor()
		{
			var defaultRoundings = new DefaultRoundings
			{
				Code = RatingConstants.RateCategory.ALL,
				RoundingType = RatingRoundingTypes.Bankers,
				RoundingFactor = new ZDecimal(),
			};
			AssertNoError(defaultRoundings.RoundingFactorInfo, "Please enter a value.");

			defaultRoundings.RoundingType = RatingRoundingTypes.Custom;
			AssertHasError(defaultRoundings.RoundingFactorInfo, "Please enter a value.");

			defaultRoundings.RoundingFactor = 3m;
			AssertNoError(defaultRoundings.RoundingFactorInfo, "Please enter a value.");

			defaultRoundings.RoundingFactor = 111111.999m;
			AssertHasError(defaultRoundings.RoundingFactorInfo, "The number 111,111.999 is too large, the maximum value allowed for selection is 99,999.999.");
		}

		public void TestSerializeDeserializeDefaultRoundings()
		{
			var originalObject = new DefaultRoundings
			{
				Code = RatingConstants.RateCategory.ALL,
				RoundingType = RatingRoundingTypes.Custom,
				RoundingFactor = 3m,
			};
			var serializedString = originalObject.SerializeToString();
			var expectedString = "<Code>ALL</Code><RoundingType>CUS</RoundingType><RoundingFactor>3</RoundingFactor>";
			AssertEquals(expectedString, serializedString);

			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(DefaultRoundings));

			DefaultRoundings deserializedObject;

			using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<DefaultRoundings>" + serializedString + "</DefaultRoundings>")))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				deserializedObject = serialiser.Deserialize(reader) as DefaultRoundings;
			}
			AssertEquals(RatingConstants.RateCategory.ALL, deserializedObject.Code);
			AssertEquals(RatingRoundingTypes.Custom, deserializedObject.RoundingType);
			AssertEquals(3m, deserializedObject.RoundingFactor);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DefaultRoundings();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
