using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingPackLineValueObjectDataAdapter<ForwardingPackLine, Xsd.Package>))]
	sealed class ForwardingPackLineValueObjectDataAdapterTest : ValueObjectDataAdapterTest<ForwardingPackLine, Xsd.Package>
	{
		#region Overrides for base test

		protected override ValueObjectDataAdapter<ForwardingPackLine, Xsd.Package> GetNewBizObjXmlDataAdapter()
		{
			return new ForwardingPackLineValueObjectDataAdapter<ForwardingPackLine, Xsd.Package>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Packages"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Package"; }
		}

		protected override ForwardingPackLine NewBusinessObject()
		{
			ForwardingPackLine packline = Factory.New<ForwardingPackLine>();
			packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			return packline;
		}

		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);
			ForwardingPackLine packline = bizObjToImportTo as ForwardingPackLine;
			packline.JL_CustomDate1 = ((ForwardingPackLine)bizObjOriginallyExportedFrom).JL_CustomDate1;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			ForwardingPackLine emptyPackLine = Factory.New<ForwardingPackLine>();
			emptyPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			var temporaryOutputFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.EmptyForwardingPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyPackLine, temporaryOutputFileName, ValidationKind.None, "Empty pack line");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingPackLine populatedPackLine = shipment.OuterPackLines.AddNew();

			UNDGSubstance undgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").FirstOrDefault();
			undgSubstance.DG_PG = "III";
			undgSubstance.DG_MP = "S";

			populatedPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			populatedPackLine.JL_PackageCount = 157;
			populatedPackLine.JL_ActualWeight = 182;
			populatedPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			populatedPackLine.JL_ActualVolume = 13.299m;
			populatedPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			populatedPackLine.JL_LoadingMeters = 10.24m;
			populatedPackLine.JL_Length = 56;
			populatedPackLine.JL_Width = 211;
			populatedPackLine.JL_Height = 203;
			populatedPackLine.JL_Description = "Description";
			populatedPackLine.JL_RN_NKOrigin = "AU";
			populatedPackLine.JL_UnitOfDimension = "MM";
			populatedPackLine.UNDGs.AddNew().LinkDefault(undgSubstance);
			populatedPackLine.JL_MarksAndNumbers = "MARKSANDNUMBERS";
			populatedPackLine.JL_RH_NKCommodityCode = "GEN";
			populatedPackLine.UNDGs.UNDGFlashPointManager.Value = ((ZDecimal)3.45m).ToString();
			populatedPackLine.JL_RefNumber = "REFNUM";
			populatedPackLine.JL_HarmonisedCode = "HRM";
			populatedPackLine.JL_CustomDate1 = new ZDateTime(2009, 1, 1, 12, 12, 30);
			populatedPackLine.JL_CustomDate2 = new ZDateTime(2009, 6, 1, 12, 12, 30);
			populatedPackLine.JL_CustomAttrib1 = "Custom text 1";
			populatedPackLine.JL_CustomAttrib2 = "Custom text 2";
			populatedPackLine.JL_CustomAttrib3 = "Custom text 3";
			populatedPackLine.JL_CustomAttrib4 = "Custom text 4";
			populatedPackLine.JL_CustomDecimal1 = 1234.56m;
			populatedPackLine.JL_CustomDecimal2 = 5678.91m;
			populatedPackLine.JL_CustomFlag1 = true;
			populatedPackLine.JL_CustomFlag2 = false;

			PackProduct packProduct = populatedPackLine.Products.AddNew();
			packProduct.D2_ProductCode = "Product Code";
			packProduct.D2_ProductQuantity = 10m;
			packProduct.D2_ProductUnitOfQty = Core.Constants.Weight.Kilograms;

			CommonContainer container = populatedPackLine.Containers.AddNew();
			container.JC_JK = consol.PK;
			populatedPackLine.CurrentConsol = consol;

			var temporaryOutputFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.PopulatedForwardingPackLine.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedPackLine, temporaryOutputFileName, ValidationKind.Xsd, "Populated pack line");
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
					// handled by other data adapters
					"DGContact/Organisation",
					"DangerousGoods",
					"HazardousGoods",

					// ??
					"ContainerNumber",
					"Weight/Description",
					"Length/Description",
					"Volume/Description",
					"Height/Description",
					"Width/Description",
					"PackageID",
					"TransportRef",
					"PackageProducts/ProductQuantity/Description"
				};
			}
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
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
	}
}
