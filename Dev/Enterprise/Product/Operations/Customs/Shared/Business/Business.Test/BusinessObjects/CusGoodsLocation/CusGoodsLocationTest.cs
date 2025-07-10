using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentLoadersForClassWithTypeDecider()
		{
			var enRouteTransshipment = (CusInBondEvent)Factory.New<Integration.Customs.EU.NCTS.IEnRouteTransshipment>();
			enRouteTransshipment.BN_Type = CusInBondEventTypes.Codes.Transshipment;
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = enRouteTransshipment.PK;
			cusGoodsLocation.CGL_ParentTableCode = enRouteTransshipment.TablePrefix;
			AssertSame(enRouteTransshipment, cusGoodsLocation.Parent);
		}

		public void TestParent_Getter()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			CombineAssertions(() =>
			{
				AssertNull("CGL_ParentID and CGL_ParentTableCode not set", cusGoodsLocation.Parent);

				cusGoodsLocation.CGL_ParentID = cei.PK;
				cusGoodsLocation.CGL_ParentTableCode = cei.TablePrefix;
				AssertSame(cei, cusGoodsLocation.Parent);
			});
		}

		public void TestParent_Setter()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.Parent = cei;
			CombineAssertions(() =>
			{
				AssertEquals("CGL_ParentID", cei.PK, cusGoodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, cusGoodsLocation.CGL_ParentTableCode);
			});
		}

		public void TestLoadOrCreate()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var cusGoodsLocation = CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("HasChanges", false, cusGoodsLocation.HasChanges);
				AssertEquals("CGL_ParentID", cei.PK, cusGoodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, cusGoodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Departure, cusGoodsLocation.CGL_LocationUse);

				var newCusGoodsLocation = CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Departure);
				AssertSame("Has been loaded", cusGoodsLocation, newCusGoodsLocation);
			});
		}

		public void TestLoad()
		{
			var cei = Factory.New<CusEntryInstruction>();
			CombineAssertions(() =>
			{
				AssertNull("CusGoodsLocation doesn't exist", CusGoodsLocation.Load<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Departure));

				var cusGoodsLocation = Factory.New<CusGoodsLocation>();
				cusGoodsLocation.Parent = cei;
				cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
				AssertNull("CGL_LocationUse doesn't match", CusGoodsLocation.Load<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Arrival));

				AssertEquals("CusGoodsLocation exists", cusGoodsLocation.PK, CusGoodsLocation.Load<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Departure).PK);
			});
		}

		public void TestNew()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var cusGoodsLocation = CusGoodsLocation.New<CusGoodsLocation>(cei, CusGoodsLocationUseList.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("HasChanges", false, cusGoodsLocation.HasChanges);
				AssertEquals("CGL_ParentID", cei.PK, cusGoodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, cusGoodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Departure, cusGoodsLocation.CGL_LocationUse);
			});
		}

		public void TestMakeNonPersistent() => CombineAssertions(() =>
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.HasChanges = true;
			Assert("Should be saved", cusGoodsLocation.IsSavedByFactory);
			cusGoodsLocation.MakeNonPersistent();
			Assert("Should no longer be saved", !cusGoodsLocation.IsSavedByFactory);
		});

		public void TestIsPersistent() => CombineAssertions(() =>
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			Assert("Default is persistent", cusGoodsLocation.IsPersistent);
			cusGoodsLocation.MakeNonPersistent();
			Assert("Now non-persistent", !cusGoodsLocation.IsPersistent);
		});

		public void TestCGL_Type_Caption()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(cusGoodsLocation.CGL_TypeInfo).Caption);
		}

		public void TestCGL_Qualifier_Caption()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			AssertEquals("Qualifier", DataBoundResourceStrings.GetDataForProperty(cusGoodsLocation.CGL_QualifierInfo).Caption);
		}

		public void TestCGL_AdditionalIdentifier_Caption()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			AssertEquals("Additional Identifier", DataBoundResourceStrings.GetDataForProperty(cusGoodsLocation.CGL_AdditionalIdentifierInfo).Caption);
		}

		public void TestCGL_CustomsOfficeIsEmptiedWhenQualifierIsNotV()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_Qualifier = "";

			cusGoodsLocation.CGL_CustomsOffice = "FR230023";
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals("Customs office should not be set empty as qualifier is change to V", "FR230023",cusGoodsLocation.CGL_CustomsOffice);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals("Customs office should have not been set to empty as qualifier is still V", "FR230023", cusGoodsLocation.CGL_CustomsOffice);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertEquals("Customs office should have been set empty as qualifier is changed to something else than V.", "", cusGoodsLocation.CGL_CustomsOffice);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewValidBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewValidBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		CusGoodsLocation GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var cusGoodsLocation = factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "DEP";
			cusGoodsLocation.CGL_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			return cusGoodsLocation;
		}

		CusGoodsLocation GetNewValidBusinessObject(BusinessObjectFactory factory)
		{
			var jobDeclaration = factory.NewWithValidTestData<BaseJobDeclaration>();
			factory.Save();
			var cusGoodsLocation = GetNewBusinessObject(factory);
			cusGoodsLocation.Parent = jobDeclaration;
			return cusGoodsLocation;
		}
	}
}
