using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ImporterAddressCodeTypes : BaseAddressCodeTypes
	{
		public ImporterAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore()
		{
			yield return OrgCusCode.CodeTypes.VATCode;
			yield return OrgCusCode.CodeTypes.PassportID;
			yield return OrgCusCode.TaiwanCodeTypes.PID;
		}

		protected override IEnumerable<string> GetCBPCodeTypesCore()
		{
			if (Address.Parent is JobDeclaration declaration && declaration.CusEntryInstruction.CEI_Style == Constants.DeclarationTypes.Import.L1)
			{
				return new[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark };
			}
			else
			{
				return base.GetCBPCodeTypesCore();
			}
		}
	}
}
