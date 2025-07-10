using System.Collections.Immutable;
using Enterprise.eTail.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eTail.DataTransfer.Universal
{
	[Immutable]
	public sealed class DefaultHVLVConsignmentDataExportStrategy : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new DefaultHVLVConsignmentDataExportStrategy();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => true;
	}

	[Immutable]
	public sealed class HVLVConsignmentDataExportStrategyWithoutNotes : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new HVLVConsignmentDataExportStrategyWithoutNotes();

		public static readonly ImmutableHashSet<string> DefaultDisallowedFields = new[]
		{
			nameof(Shipment.NoteCollection),
		}.ToImmutableHashSet();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => !DefaultDisallowedFields.Contains(fieldName);
	}

	[Immutable]
	public sealed class HVLVConsignmentToCargoReportDataExportStrategy : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new HVLVConsignmentToCargoReportDataExportStrategy();

		public static readonly ImmutableHashSet<string> DefaultDisallowedFields = new[]
		{
			nameof(Shipment.NoteCollection),
			nameof(Shipment.AdditionalReferenceCollection),
		}.ToImmutableHashSet();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => !DefaultDisallowedFields.Contains(fieldName);
	}

	[Immutable]
	public sealed class HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new HVLVConsignmentToCargoReportWithAdditionalReferencesDataExportStrategy();

		public static readonly ImmutableHashSet<string> DefaultDisallowedFields = new[]
		{
		   nameof(Shipment.NoteCollection),
	   }.ToImmutableHashSet();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => !DefaultDisallowedFields.Contains(fieldName);
	}

	[Immutable]
	public sealed class HVLVConsignmentToRTUSDataExportStrategy : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new HVLVConsignmentToRTUSDataExportStrategy();

		public static readonly ImmutableHashSet<string> DefaultDisallowedFields = new[]
		{
			nameof(Shipment.NoteCollection),
			nameof(Shipment.AdditionalReferenceCollection),
			nameof(Shipment.PackingLineCollection),
		}.ToImmutableHashSet();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => !DefaultDisallowedFields.Contains(fieldName);
	}

	[Immutable]
	public sealed class HVLVConsignmentToDeclarationDataExportStrategy : IHVLVConsignmentDataExportStrategy
	{
		public static readonly IHVLVConsignmentDataExportStrategy Instance = new HVLVConsignmentToDeclarationDataExportStrategy();

		public bool IsAllowSetForConsignmentDataExportContext(string fieldName) => true;
	}
}
