using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using USDeclaration = Enterprise.Customs.US.Business.JobDeclaration;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromReconDeclaration))]
	sealed class FreightWrapperFromReconDeclarationTest : FreightWrapperTest
	{
		public void TestReconDeclaration()
		{
			var wrapper = new FreightWrapperFromReconDeclaration(ReconciliationDeclaration, Factory);
			AssertNotNull(wrapper.ReconDeclaration);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = new FreightWrapperFromReconDeclaration(ReconciliationDeclaration, Factory);
			AssertEquals("TrackingBusinessObjectPK", ReconciliationDeclaration.PK, wrapper.TrackingBusinessObjectPK);
		}

		public override void TestWrapperNotes()
		{
			ReconciliationDeclaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "TEST Special Instructions");

			var wrapper = new FreightWrapperFromReconDeclaration(ReconciliationDeclaration, Factory);
			AssertEquals("wrapper.SpecialInstructions", "TEST Special Instructions", wrapper.Notes[PredefinedNoteTypes.Instance.SpecialInstructions.Description].Text);
		}

		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "B00001103" },
					{ "JobNumberBarcodeText", "^DEC=B00001103;;|" },
					{ "JobNumberBarcodeTextForFont", "È^DEC=BÃ¯¯+#Ä;;|GÊ" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈB00001103ZÊ" },
					{ "JobNumberHeading", "Declaration" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Consignee : TEST ORGANIZATION\nUNITED STATES
ImportBroker : EDI CUSTOMS BROKERS\n10 HUTCHESON STREET\nALBION QLD\n4010\nAUSTRALIA
ShipmentType : REC - Reconciliation";
			}
		}

		ReconDeclaration ReconciliationDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_FullName = "Test Organization";

					var declaration = Factory.New<USDeclaration>();
					declaration.JE_DeclarationReference = "B00001103";
					declaration.JE_OH_Importer = orgHeader.PK;
					reconDeclaration = new ReconDeclaration(declaration);
				}
				return reconDeclaration;
			}
		}
		ReconDeclaration reconDeclaration;

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return new ReconDeclaration(Factory.New<USDeclaration>());
		}

		ZString countryToStartWith;
		protected override void SetUp()
		{
			base.SetUp();
			countryToStartWith = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(countryToStartWith);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromReconDeclaration(ReconciliationDeclaration, Factory);
		}
	}
}
