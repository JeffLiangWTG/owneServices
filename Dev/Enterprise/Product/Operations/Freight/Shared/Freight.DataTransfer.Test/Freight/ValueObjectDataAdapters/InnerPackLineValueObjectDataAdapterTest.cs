using System;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>))]
	sealed class InnerPackLineValueObjectDataAdapterTest : ValueObjectDataAdapterTest<PackLine, Xsd.PackageBase>
	{
		public void TestErrorOnExportForUnspecifiedPackType()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S666";

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;
			packline.JL_F3_NKPackType = "";

			Xsd.PackageBase package = Adapter.ExportToValueObject(packline, new ValueObjectExportContext(Notifications));

			AssertEquals(true, Notifications.AsString.Contains("Pack type is not specified for outer packline on shipment S666"));
			Assert("this should be concidered as error", Notifications.HasErrors);
		}

		public void TestNotificationOnExportForUnmappedPacksPackType()
		{
			AssertEquals("Precondition", false, PkgUnitXmlCodeMappings.Instance.ContainsEnterpriseCode("XXX"));

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "Bob";

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;
			packline.JL_F3_NKPackType = "XXX";

			Xsd.PackageBase package = Adapter.ExportToValueObject(packline, new ValueObjectExportContext(Notifications));

			AssertContains("A non-system defined package type (XXX) on shipment Bob has been exported. The organization that imports this XML file may not have that package type in their registry (they can add it in Registry -> Freight -> Shipment -> Packages -> Freight Packs. If they do not have the package type, their import will not fail, but the record will have an error on the package type field when they try to edit it.",
					Notifications.AsString);
			AssertEquals("this should not be concidered an error", false, Notifications.HasErrors);
			AssertEquals("XXX", package.PackType);

			AssertEquals("Precondition", true, PkgUnitXmlCodeMappings.Instance.Any());

			NotificationBuffer notifyWhenCorrectCode = new NotificationBuffer();
			packline.JL_F3_NKPackType = PkgUnitXmlCodeMappings.Instance[0].EnterpriseCode;
			packline.JL_PackageCount = 1;

			Xsd.PackageBase packageValueWhenCorrectCode = Adapter.ExportToValueObject(packline, new ValueObjectExportContext(notifyWhenCorrectCode));
			AssertEquals(notifyWhenCorrectCode.AsString, false, notifyWhenCorrectCode.AsString.Contains("A non-system defined package type ("));
			AssertEquals(packageValueWhenCorrectCode.PackType, PkgUnitXmlCodeMappings.Instance.GetExternalCode(packline.JL_F3_NKPackType, null, notifyWhenCorrectCode));
		}

		public void TestNotificationOnImportForUnmappedPacksPackType()
		{
			AssertEquals("Precondition", false, PkgUnitXmlCodeMappings.Instance.ContainsEnterpriseCode("XXX"));

			Xsd.Package package = new Xsd.Package();
			package.PackType = "XAXA";

			PackLine packline = Import(package, "Bob", Notifications);

			AssertContains($"Warning: Maximum length of this field has been exceeded (The Package type (XAXA) on a Pack line of Shipment (House Bill='BOB') has exceeded the maximum length allowed by the system. When you edit this record, the package type field will error. Please use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type; value=XAXA)", Notifications.AsString);
			AssertEquals("this should not be concidered an error", false, Notifications.HasErrors);
			AssertEquals("XAX", packline.JL_F3_NKPackType);

			Notifications.Clear();
			package.PackType = "XXX";
			packline = Import(package, "Bob", Notifications);

			AssertContains($"Warning: The Package type (XXX) on a Pack line of Shipment (House Bill='BOB') is not valid. When you edit this record, the package type field will error. To avoid this error you can either use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type or you can add this value to the reference files (Reference Files -> Package Types)", Notifications.AsString);
			AssertEquals("this should not be concidered an error", false, Notifications.HasErrors);
			AssertEquals("XXX", packline.JL_F3_NKPackType);

			AssertEquals("Precondition", true, PkgUnitXmlCodeMappings.Instance.Any());

			NotificationBuffer notifyWhenCorrectCode = new NotificationBuffer();
			package.PackType = PkgUnitXmlCodeMappings.Instance[0].ExternalCode;
			package.NumberOfPacks = 1;

			packline = Import(package, "Bob", notifyWhenCorrectCode);
			Adapter.ExportToValueObject(packline, package, new ValueObjectExportContext(notifyWhenCorrectCode));
			AssertEquals(notifyWhenCorrectCode.AsString, false, notifyWhenCorrectCode.AsString.Contains("An undefined package type ("));
			AssertEquals(packline.JL_F3_NKPackType, PkgUnitXmlCodeMappings.Instance.GetExternalCode(packline.JL_F3_NKPackType, null, notifyWhenCorrectCode));
		}

		PackLine Import(Xsd.PackageBase package, string houseBill, NotificationBuffer notify)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = houseBill;
			PackLine packline = shipment.OuterPackLines.AddNew();

			Adapter.ImportFromValueObject(packline, package, new ValueObjectImportContext(Factory, notify));
			return packline;
		}

		public void TestValidationMessages()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "HOUSEBILL";
			PackLine packline = shipment.OuterPackLines.AddNew();

			Xsd.PackageBase packlineValue = new Xsd.PackageBase();
			packlineValue.Length = new Xsd.DimensionValue();
			packlineValue.Width = new Xsd.DimensionValue();
			packlineValue.Height = new Xsd.DimensionValue();

			notifications = new NotificationBuffer();
			packlineValue.Length.DimensionType = "a";
			packlineValue.Width.DimensionType = "b";
			packlineValue.Height.DimensionType = "c";
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);
			Adapter.ImportFromValueObject(packline, packlineValue, context);

			Assert(Notifications.AsString.Contains("Error: Dimension Type on Length/Width/Height must all be the same (The Package type () on a Pack line of Shipment (House Bill='HOUSEBILL'))"));
			Assert(Notifications.AsString.Contains("Warning: Dimension Unit 'a'; The Package type () on a Pack line of Shipment (House Bill='HOUSEBILL')"));
			Assert(Notifications.AsString.Contains("Warning: Dimension Unit 'b'; The Package type () on a Pack line of Shipment (House Bill='HOUSEBILL')"));
			Assert(Notifications.AsString.Contains("Warning: Dimension Unit 'c'; The Package type () on a Pack line of Shipment (House Bill='HOUSEBILL')"));
		}

		public void TestUpdatedOrCreatedNotificationNotShown()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packline = shipment.OuterPackLines.AddNew();
			Xsd.Package packlineValue = new Xsd.Package();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			Adapter.ImportFromValueObject(packline, packlineValue, context);
			AssertEquals("Should not contain created or updated message", false, Notifications.ContainsNotificationType(WarningType.BusinessObjectCreatedOrUpdated));
		}

		public void TestLengthWidthHeightDimensionTypeMustBeSame()
		{
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);

			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packline = shipment.OuterPackLines.AddNew();
			Xsd.Package packlineValue = new Xsd.Package();
			packlineValue.Length = new Xsd.DimensionValue();
			packlineValue.Width = new Xsd.DimensionValue();
			packlineValue.Height = new Xsd.DimensionValue();

			Notifications.Clear();
			packlineValue.Length.DimensionType = "aa";
			packlineValue.Width.DimensionType = "aa";
			Adapter.ImportFromValueObject(packline, packlineValue, context);
			AssertEquals("Should have error", true, Notifications.ContainsNotificationType(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame));

			Notifications.Clear();
			packlineValue.Width.DimensionType = "bb";
			packlineValue.Height.DimensionType = "bb";
			Adapter.ImportFromValueObject(packline, packlineValue, context);
			AssertEquals("Should have error", true, Notifications.ContainsNotificationType(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame));

			Notifications.Clear();
			packlineValue.Length.DimensionType = "cc";
			packlineValue.Height.DimensionType = "cc";
			Adapter.ImportFromValueObject(packline, packlineValue, context);
			AssertEquals("Should have error", true, Notifications.ContainsNotificationType(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame));

			Notifications.Clear();
			packlineValue.Width.DimensionType = "cc";
			Adapter.ImportFromValueObject(packline, packlineValue, context);

			AssertEquals("Shouldn't have an error now all dimention types are the same", false, Notifications.ContainsNotificationType(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame));
		}

		public void TestImportDecimals_OutOfRange()
		{
			var outOfSqlRange = 999999999.9M;
			var expectedValue = 0M;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notifications);

			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packline = shipment.OuterPackLines.AddNew();
			Xsd.Package packlineValue = new Xsd.Package();

			packlineValue.Length = new Xsd.DimensionValue() { Value = outOfSqlRange };
			packlineValue.Width = new Xsd.DimensionValue() { Value = outOfSqlRange };
			packlineValue.Height = new Xsd.DimensionValue() { Value = outOfSqlRange };
			packlineValue.Weight.Value = outOfSqlRange;
			packlineValue.Volume.Value = outOfSqlRange;

			Notifications.Clear();
			Adapter.ImportFromValueObject(packline, packlineValue, context);

			Assert("Should have error", Notifications.AsString.Contains("Value overflow error"));

			AssertEquals("JL_Length", expectedValue, packline.JL_Length);
			AssertEquals("JL_Width", expectedValue, packline.JL_Width);
			AssertEquals("JL_Height", expectedValue, packline.JL_Height);
			AssertEquals("JL_ActualWeight", expectedValue, packline.JL_ActualWeight);
			AssertEquals("JL_ActualVolume", expectedValue, packline.JL_ActualVolume);

			var length = 1M;
			var width = 2M;
			var height = 3M;
			var weight = 5M;
			var volume = 6M;

			packlineValue.Length = new Xsd.DimensionValue() { Value = length };
			packlineValue.Width = new Xsd.DimensionValue() { Value = width };
			packlineValue.Height = new Xsd.DimensionValue() { Value = height };
			packlineValue.Weight.Value = weight;
			packlineValue.Volume.Value = volume;

			Notifications.Clear();
			Adapter.ImportFromValueObject(packline, packlineValue, context);

			Assert("Should not have error", !Notifications.AsString.Contains("Value overflow error"));

			AssertEquals("JL_Length", length, packline.JL_Length);
			AssertEquals("JL_Width", width, packline.JL_Width);
			AssertEquals("JL_Height", height, packline.JL_Height);
			AssertEquals("JL_ActualWeight", weight, packline.JL_ActualWeight);
			AssertEquals("JL_ActualVolume", volume, packline.JL_ActualVolume);
		}

		#region Overrides for base test

		protected override ValueObjectDataAdapter<PackLine, Xsd.PackageBase> GetNewBizObjXmlDataAdapter()
		{
			return new InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "InnerPackages"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "InnerPackage"; }
		}

		protected override PackLine NewBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packline = shipment.OuterPackLines.AddNew();
			return packline;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			PackLine emptyPackLine = Factory.New<PackLine>();
			emptyPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			emptyPackLine.JL_RefNumber = ZString.Empty;
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyInnerPackLine.xml", "EmptyInnerPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyPackLine, expectedOutputFilename, ValidationKind.None, "Empty pack line");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine populatedPackLine = shipment.OuterPackLines.AddNew();

			populatedPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			populatedPackLine.JL_PackageCount = 157;
			populatedPackLine.JL_ActualWeight = 182;
			populatedPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			populatedPackLine.JL_ActualVolume = 13.299m;
			populatedPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			populatedPackLine.JL_Length = 56;
			populatedPackLine.JL_Width = 211;
			populatedPackLine.JL_Height = 203;
			populatedPackLine.JL_Description = "Description";
			populatedPackLine.JL_UnitOfDimension = "MM";
			populatedPackLine.JL_RefNumber = "RefNumber";

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedInnerPackLine.xml", "PopulatedInnerPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedPackLine, expectedOutputFilename, ValidationKind.Xsd, "Populated pack line");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"Weight/Description",
					"Length/Description",
					"Volume/Description",
					"Height/Description",
					"Width/Description",
					"PackageID",
					"TransportRef",
					"Custom/Date1"
				};
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		#endregion

		#region Implementation

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase> Adapter
		{
			get { return adapter ?? (adapter = new InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase>()); }
		}
		InnerPackLineValueObjectDataAdapter<PackLine, Xsd.PackageBase> adapter;

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		#endregion
	}
}
