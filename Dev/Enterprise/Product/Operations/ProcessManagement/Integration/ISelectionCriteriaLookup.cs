using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ProcessManagement.Integration
{
	public interface ISelectionCriteriaLookup
	{
		ZString GetCriterionLabel(int index);

		ICodeDescriptionPairList GetCriterionList(string criterion1, string criterion2, string criterion3, string criterion4);
	}
}
