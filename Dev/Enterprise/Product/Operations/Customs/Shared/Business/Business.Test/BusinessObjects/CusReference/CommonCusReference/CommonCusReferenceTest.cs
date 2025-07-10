using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommonCusReference))]
	sealed class CommonCusReferenceTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			cusReference.CFR_ParentID = invoiceLine.PK;
			cusReference.CFR_ParentTableCode = invoiceLine.TablePrefix;
			AssertEquals(invoiceLine.PK, cusReference.Parent.PK);

			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			cusReference.CFR_ParentID = cusEntryInstruction.PK;
			cusReference.CFR_ParentTableCode = cusEntryInstruction.TablePrefix;
			AssertEquals(cusEntryInstruction.PK, cusReference.Parent.PK);
		}

		public void TestCFR_Code_Caption()
		{
			AssertEquals("Code", DataBoundResourceStrings.GetDataForProperty(cusReference.CFR_CodeInfo).Caption);
		}

		public void TestCFR_Reference_Caption()
		{
			AssertEquals("Reference", DataBoundResourceStrings.GetDataForProperty(cusReference.CFR_ReferenceInfo).Caption);
		}

		public void TestCFR_OA_Owner_Caption()
		{
			AssertEquals("Address", DataBoundResourceStrings.GetDataForProperty(cusReference.CFR_OA_OwnerInfo).Caption);
		}

		public void TestOwnerOrgPK_Caption()
		{
			AssertEquals("Owner", DataBoundResourceStrings.GetDataForProperty(cusReference.OwnerOrgPKInfo).Caption);
		}

		public void TestOwnerOrgPK_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CommonCusReference), nameof(CommonCusReference.OwnerOrgPK), false, x => x.ListDataSourceMember == nameof(CommonCusReference.Lookups) + "." + nameof(CommonCusReferenceLookups.OwnersList));
		}

		public void TestCFR_Reference_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly is false", false, cusReference.CFR_ReferenceInfo.ReadOnly);

				this.cusReference.CFR_OA_Owner = Factory.New<OrgHeader>().MainAddress.PK;
				AssertEquals("ReadOnly is true", true, cusReference.CFR_ReferenceInfo.ReadOnly);
			});
		}

		public void TestCFR_OA_Owner_DefaultValue()
		{
			var owner = Factory.New<OrgHeader>();
			var mainAddress = owner.Addresses.AddNew();
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			mainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

			cusReference.CFR_OA_Owner_ZAddress.OrgPK = owner.PK;
			AssertEquals(mainAddress.PK, cusReference.CFR_OA_Owner);
		}

		public void TestCFR_OA_Owner_InvalidOrgPK()
		{
			cusReference.CFR_OA_Owner = ZGuid.NewZGuid();
			cusReference.CFR_OA_Owner_ZAddress.OrgPK = ZGuid.NewZGuid();
			AssertEquals(ZGuid.Empty, cusReference.CFR_OA_Owner);
		}

		public void TestDataGroupingCodeExposed_ParentIsJobComInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			cusReference.CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			cusReference.CFR_ParentID = invoiceLine.PK;
			AssertEquals(Core.Constants.CountryCodes.Eritrea, cusReference.DataGroupingCodeExposed);
		}

		public void TestDataGroupingCodeExposed_ParentIsCusEntryInstruction()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			cusReference.CFR_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			cusReference.CFR_ParentID = instruction.PK;
			AssertEquals(Core.Constants.CountryCodes.Eritrea, cusReference.DataGroupingCodeExposed);
		}
		protected override void SetUp()
		{
			base.SetUp();
			cusReference = Factory.New<CommonCusReferenceForTest>();
		}

		CommonCusReferenceForTest cusReference;
	}
}
