using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CMApplicantAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<CMApplicantAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, AddressCodeTypes.IDCodeTypes);
		}

		protected override CMApplicantAddressCodeTypes GetAddressCodeTypes()
		{
			var controllingMessageHeader = Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new CMApplicantAddressCodeTypes(controllingMessageHeader.ApplicantDocumentaryAddress);
		}
	}
}
