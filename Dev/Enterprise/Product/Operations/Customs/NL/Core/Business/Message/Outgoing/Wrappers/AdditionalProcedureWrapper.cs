using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class AdditionalProcedureWrapper : IAdditionalProcedure
{
	public AdditionalProcedureWrapper(ZString jI_Procedure, int sequenceNumeric)
	{
		this.jI_Procedure = jI_Procedure;
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly ZString jI_Procedure;

	public string ProcedureCode => jI_Procedure.SubstringSafe(4, 3);
	public string CCQualifierCode => null;

	public int SequenceNumeric { get; }
}
