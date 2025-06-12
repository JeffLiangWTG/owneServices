using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.Core.Providers.Test.EnumTests
{
	[TestFixture]
	public class EnumExtensionsTests
	{
		[Test]
		public void TestConvertToString_Provider()
		{
			var type1 = ProviderType.CTCGB;
			var type2 = ProviderType.GVMS;
			var type255 = ProviderType.Unknown;

			var result1 = type1.ConvertToString();
			var result2 = type2.ConvertToString();
			var result255 = type255.ConvertToString();
			var resultNull = EnumExtensions.ConvertToString(null);

			Assert.AreEqual("CTCGB", result1);
			Assert.AreEqual("GVMS", result2); 
			Assert.AreEqual( "Unknown", result255);
			Assert.AreEqual( "Unknown", resultNull);
		}

		[Test]
		public void TestConvertToEnum_Provider()
		{
			var type1 = "CTCGB";
			var type2 = "GVMS";
			var type255 = "Unknown";
			var typeNotExist = "DoesNotExist";

			var result1 = type1.ConvertToEnum<ProviderType>();
			var result2 = type2.ConvertToEnum<ProviderType>();
			var result255 = type255.ConvertToEnum<ProviderType>();
			var resultNotExist = typeNotExist.ConvertToEnum<ProviderType>();

			Assert.AreEqual(ProviderType.CTCGB, result1);
			Assert.AreEqual(ProviderType.GVMS, result2);
			Assert.AreEqual(ProviderType.Unknown, result255);
			Assert.AreEqual(ProviderType.Unknown, resultNotExist);
		}

		[Test]
		public void TestConvertToString_CorrelationType()
		{
			var type0 = CorrelationType.Subscribed;
			var type1 = CorrelationType.Additional;
			var type255 = CorrelationType.Unknown;

			var result0 = type0.ConvertToString();
			var result1 = type1.ConvertToString();
			var result255 = type255.ConvertToString();
			var resultNull = EnumExtensions.ConvertToString(null);

			Assert.AreEqual("Subscribed", result0);
			Assert.AreEqual("Additional", result1);
			Assert.AreEqual("Unknown", result255);
			Assert.AreEqual("Unknown", resultNull);
		}

		[Test]
		public void TestConvertToEnum_CorrelationType()
		{
			var type0 = "Subscribed";
			var type1 = "Additional";
			var type255 = "Unknown";
			var typeNotExist = "DoesNotExist";

			var result0 = type0.ConvertToEnum<CorrelationType>();
			var result1 = type1.ConvertToEnum<CorrelationType>();
			var result255 = type255.ConvertToEnum<CorrelationType>();
			var resultNotExist = typeNotExist.ConvertToEnum<CorrelationType>();

			Assert.AreEqual(CorrelationType.Subscribed, result0);
			Assert.AreEqual(CorrelationType.Additional, result1);
			Assert.AreEqual(CorrelationType.Unknown, result255);
			Assert.AreEqual(CorrelationType.Unknown, resultNotExist);
		}

		[Test]
		public void TestConvertToString_ExtractionType()
		{
			var type0 = ExtractionType.Header;
			var type1 = ExtractionType.XMLBody;
			var type2 = ExtractionType.JSONBody;
			var type3 = ExtractionType.Parameter;
			var type255 = ExtractionType.Unknown;

			var result0 = type0.ConvertToString();
			var result1 = type1.ConvertToString();
			var result2 = type2.ConvertToString();
			var result3 = type3.ConvertToString();
			var result255 = type255.ConvertToString();
			var resultNull = EnumExtensions.ConvertToString(null);

			Assert.AreEqual("Header", result0);
			Assert.AreEqual("XMLBody", result1);
			Assert.AreEqual("JSONBody", result2);
			Assert.AreEqual("Parameter", result3);
			Assert.AreEqual("Unknown", result255);
			Assert.AreEqual("Unknown", resultNull);
		}

		[Test]
		public void TestConvertToEnum_ExtractionType()
		{
			var type0 = "Header";
			var type1 = "XMLBody";
			var type2 = "JSONBody";
			var type3 = "Parameter";
			var type255 = "Unknown";
			var typeNotExist = "DoesNotExist";

			var result0 = type0.ConvertToEnum<ExtractionType>();
			var result1 = type1.ConvertToEnum<ExtractionType>();
			var result2 = type2.ConvertToEnum<ExtractionType>();
			var result3 = type3.ConvertToEnum<ExtractionType>();
			var result255 = type255.ConvertToEnum<ExtractionType>();
			var resultNotExist = typeNotExist.ConvertToEnum<ExtractionType>();

			Assert.AreEqual(ExtractionType.Header, result0);
			Assert.AreEqual(ExtractionType.XMLBody, result1);
			Assert.AreEqual(ExtractionType.JSONBody, result2);
			Assert.AreEqual(ExtractionType.Parameter, result3);
			Assert.AreEqual(ExtractionType.Unknown, result255);
			Assert.AreEqual(ExtractionType.Unknown, resultNotExist);
		}

		[Test]
		public void TestConvertToString_GBCustomsSource()
		{
			var type0 = GBCustomsSource.Synchronous;
			var type1 = GBCustomsSource.Notification;
			var type255 = GBCustomsSource.Unknown;

			var result0 = type0.ConvertToString();
			var result1 = type1.ConvertToString();
			var result255 = type255.ConvertToString();
			var resultNull = EnumExtensions.ConvertToString(null);

			Assert.AreEqual("Synchronous", result0);
			Assert.AreEqual("Notification", result1);
			Assert.AreEqual("Unknown", result255);
			Assert.AreEqual("Unknown", resultNull);
		}

		[Test]
		public void TestConvertToEnum_GBCustomsSource()
		{
			var type0 = "Synchronous";
			var type1 = "Notification";
			var type255 = "Unknown";
			var typeNotExist = "DoesNotExist";

			var result0 = type0.ConvertToEnum<GBCustomsSource>();
			var result1 = type1.ConvertToEnum<GBCustomsSource>();
			var result255 = type255.ConvertToEnum<GBCustomsSource>();
			var resultNotExist = typeNotExist.ConvertToEnum<GBCustomsSource>();

			Assert.AreEqual(GBCustomsSource.Synchronous, result0);
			Assert.AreEqual(GBCustomsSource.Notification, result1);
			Assert.AreEqual(GBCustomsSource.Unknown, result255);
			Assert.AreEqual(GBCustomsSource.Unknown, resultNotExist);
		}

	}
}
