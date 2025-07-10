using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CMSupplierAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<CMSupplierAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, AddressCodeTypes.IDCodeTypes);
		}

		protected override CMSupplierAddressCodeTypes GetAddressCodeTypes()
		{
			var controllingMessageHeader = Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new CMSupplierAddressCodeTypes(controllingMessageHeader.SupplierDocumentaryAddress);
		}
	}
}
