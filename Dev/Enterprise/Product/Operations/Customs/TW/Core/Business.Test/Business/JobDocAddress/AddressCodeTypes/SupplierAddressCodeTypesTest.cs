using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class SupplierAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<SupplierAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInExactOrder(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, Constants.OrgCusCodeType.CustomCode }, AddressCodeTypes.IDCodeTypes);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInExactOrder(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, AddressCodeTypes.IDCodeTypes);
		}

		protected override SupplierAddressCodeTypes GetAddressCodeTypes()
		{
			return new SupplierAddressCodeTypes(Declaration.SupplierDocumentaryAddress);
		}
	}
}
