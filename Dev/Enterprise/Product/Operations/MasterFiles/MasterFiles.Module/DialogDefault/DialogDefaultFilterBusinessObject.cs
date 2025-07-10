using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module.DialogDefault;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class DialogDefaultFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddFilter(new DialogIdentifierFilter());
			filters.AddFilter(new DialogDefaultOwnerModuleFilter("Owner"));

			filters.AddTextFilter("Caption", StmDialogDefaultSchema.SDD_Caption)
				.MultilingualDescription = ResString.GetMultilingualString("ECB90C5D-A24A-484E-8C59-B6868AD2A4F1", "Caption");

			filters.AddFlagsFilter("Show Dialog",
				new[] { Res.GetString("B10E3033-89DA-4E41-9277-E6DE198A5982", "Continue to show the dialog") },
				new GetFlagsQuery[] { (status) => new ZQuery(StmDialogDefaultSchema.SDD_ShowDialog, SQLComparisonOperator.Equal, status) })
				.MultilingualDescription = ResString.GetMultilingualString("6B90C937-5BE2-4CEC-B961-CAC7D81D009E", "Show dialog");

			filters.AddFlagsFilter("Applies to Similar Dialogs",
				new[] { Res.GetString("D3EF5EDD-A382-4D41-A3CA-86CC7626918A", "Default applies to more than one dialog") },
				new GetFlagsQuery[] { GetAppliesToSimilarDialogs })
				.MultilingualDescription = ResString.GetMultilingualString("8E438F62-A4DA-4481-A2D9-63C36EE48C0E", "Applies to similar dialogs");

			filters.AddFlagsFilter("Override Lower Defaults",
				new[] { Res.GetString("274B0884-978D-453A-9261-16045CE6FDB8", "Do override lower access defaults") },
				new GetFlagsQuery[] { (status) => new ZQuery(StmDialogDefaultSchema.SDD_OverrideAllChildLevels, SQLComparisonOperator.Equal, status) })
				.MultilingualDescription = ResString.GetMultilingualString("52DE9BDB-09B5-4798-BE17-F642A259B150", "Override lower defaults");

			return filters;
		}

		static ZQuery GetAppliesToSimilarDialogs(ZBool status)
		{
			var comparisonOperator = status ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(StmDialogDefaultSchema.SDD_Context, comparisonOperator, null);
		}
	}

#if DEBUG
	internal
#endif
	class DialogIdentifierFilter : ModuleTextFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Multilingual description is below")]
		public DialogIdentifierFilter()
			: base("Dialog Identifier", DialogIdentifierEquals)
		{
			MultilingualDescription = ResString.GetMultilingualString("5B1F9C80-26D3-42D7-B5E6-43D76763F33C", "Dialog Identifier");
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get { return new[] { ComparisonConstants.Exact }; }
		}

		static ZQuery DialogIdentifierEquals(SQLComparisonOperator comparison, ZString value)
		{
			ZGuid guid;
			return ZGuid.TryParse(value, out guid)
				? new ZQuery(StmDialogDefaultSchema.SDD_DialogIdentifier, guid)
				: ZQuery.NoResultQuery;
		}
	}
}
