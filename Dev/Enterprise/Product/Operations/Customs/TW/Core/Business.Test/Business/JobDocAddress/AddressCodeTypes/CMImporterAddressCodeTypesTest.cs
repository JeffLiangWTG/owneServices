using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CMImporterAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<CMImporterAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID }, AddressCodeTypes.IDCodeTypes);
		}

		protected override CMImporterAddressCodeTypes GetAddressCodeTypes()
		{
			var controllingMessageHeader = Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new CMImporterAddressCodeTypes(controllingMessageHeader.ImporterDocumentaryAddress);
		}
	}
}
