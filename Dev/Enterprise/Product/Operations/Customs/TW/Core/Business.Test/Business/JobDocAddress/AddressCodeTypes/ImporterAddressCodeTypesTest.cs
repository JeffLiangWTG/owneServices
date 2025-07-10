using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImporterAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<ImporterAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }, AddressCodeTypes.IDCodeTypes);
		}

		public override void TestCBPCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark }, AddressCodeTypes.CBPCodeTypes);

			Declaration.CusEntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark }, AddressCodeTypes.CBPCodeTypes);
		}

		protected override ImporterAddressCodeTypes GetAddressCodeTypes()
		{
			return new ImporterAddressCodeTypes(Declaration.ImporterDocumentaryAddress);
		}
	}
}
