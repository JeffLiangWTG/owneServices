using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(SADDocumentWatermark))]
	sealed class SADDocumentWatermarkTest : RegistryBusinessObjectTemplateTestCase<SADDocumentWatermark>
	{
		public void TestValidateEntryStatusCode()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("4");
			Factory.Save();
			var collection = new SADDocumentWatermarkCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var watermark = collection.AddNew();
			watermark.EntryStatusCode = ZString.Empty;
			AssertHasErrorContaining(watermark.EntryStatusCodeInfo, MandatoryValidation.MustBeEntered);
			watermark.EntryStatusCode = "~";
			AssertNoErrorContaining(watermark.EntryStatusCodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(watermark.EntryStatusCodeInfo, ListValidation.InvalidCodeError);
			watermark.EntryStatusCode = "1";
			AssertNoErrorContaining(watermark.EntryStatusCodeInfo, ListValidation.InvalidCodeError);
			watermark = collection.AddNew();
			watermark.EntryStatusCode = "1";
			AssertHasError(watermark.EntryStatusCodeInfo, SADDocumentWatermark.DuplicateEntryStatusCode);
			watermark.EntryStatusCode = "4";
			AssertNoError(watermark.EntryStatusCodeInfo, SADDocumentWatermark.DuplicateEntryStatusCode);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new SADDocumentWatermarkCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override SADDocumentWatermark GetBusinessObjectToClone()
		{
			return (SADDocumentWatermark)GetNewBusinessObject();
		}

		protected override SADDocumentWatermark GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
