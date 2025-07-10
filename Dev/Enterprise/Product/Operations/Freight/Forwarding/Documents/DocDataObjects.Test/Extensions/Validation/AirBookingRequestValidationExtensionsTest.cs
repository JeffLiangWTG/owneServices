using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirBookingRequestValidation;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class AirBookingRequestValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestAddIATAValidation()
		{
			var context = new CommonContext(Factory);
			var unloco = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			unloco.AddIATAValidation("dummy");
			AssertUNLOCOIATAValidation(unloco, "dummy");
		}

		public void TestAddMaximumLengthValidation()
		{
			var dummyObject = new DummyDocDataObject();
			dummyObject.TextInfo.AddMaximumLengthValidation("Text", 7);
			dummyObject.AmountInfo.AddMaximumLengthValidation("Amount", 5);
			dummyObject.WeightInfo.AddMaximumLengthValidation("Weight", 9);

			AssertMaximumLengthValidation(dummyObject.TextInfo, 7, "Text must not exceed 7 characters.");
			AssertMaximumLengthValidation(dummyObject.AmountInfo, 5, "Amount must not exceed the maximum 99999.");
			AssertMaximumLengthValidation(dummyObject.WeightInfo, 9, "Weight must not exceed the maximum length 9.");
		}

		sealed class DummyDocDataObject : DocDataObject
		{
			#region Text

			public ZString Text
			{
				get => text;
				set
				{
					if (SetNonPersistentPropertyValue(TextInfo, ref text, value))
					{
						Validate(TextInfo);
					}
				}
			}

			ZString text;

			public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));

			#endregion

			#region Amout

			public ZInt Amout
			{
				get => amount;
				set
				{
					if (SetNonPersistentPropertyValue(AmountInfo, ref amount, value))
					{
						Validate(AmountInfo);
					}
				}
			}

			ZInt amount;

			public ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amout));

			#endregion

			#region Weight

			public ZDecimal Weight
			{
				get => weight;
				set
				{
					if (SetNonPersistentPropertyValue(WeightInfo, ref weight, value))
					{
						Validate(WeightInfo);
					}
				}
			}
			ZDecimal weight;

			public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

			#endregion
		}
	}
}
