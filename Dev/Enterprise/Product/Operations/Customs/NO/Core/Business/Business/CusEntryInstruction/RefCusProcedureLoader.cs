using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

static class RefCusProcedureLoader
{
	public static ImmutableDictionary<string, RefCusProcedure> GetAllApplicableProcedures(BusinessObjectFactory factory, CusEntryInstruction instruction)
	{
		Argument.NotNull(factory, nameof(factory));
		Argument.NotNull(instruction, nameof(instruction));
		Argument.NotNull(instruction.JobDeclaration, nameof(CusEntryInstruction.JobDeclaration));

		var declaration = instruction.JobDeclaration;
		var dataGroupingCode = declaration.GetDefaultDataGroupingCode();
		var messageType = declaration.JE_MessageType;
		var dateOfValuation = instruction.DateOfValuation;
		var style = instruction.CEI_Style;

		var cacheKey = string.Join("-", "NO", "CusRefProcedureDictionary", dataGroupingCode, style, messageType, dateOfValuation);
		return factory.GetCachedValue(cacheKey, () =>
		{
			return new RefCusProcedureCollection(factory, dataGroupingCode, dateOfValuation, style, messageType)
				.ToImmutableDictionary(key => string.Concat(key.ZZ6_ProcedureCode, key.ZZ6_PreviousProcedureCode), value => value);
		});
	}
}
