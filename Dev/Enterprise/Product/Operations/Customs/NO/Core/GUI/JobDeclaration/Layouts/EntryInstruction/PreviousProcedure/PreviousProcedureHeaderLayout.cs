using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

sealed class PreviousProcedureHeaderLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => previousProcedureHeaderLayout.Value;
	readonly Lazy<PanelLayout> previousProcedureHeaderLayout = new (CreatePreviousProcedureHeaderLayout);

	static PanelLayout CreatePreviousProcedureHeaderLayout()
	{
		var builder = new PreviousProcedureLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.PreviousProcedureDropEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.ImportFromTemporaryStorageRegisterButton, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.ImportFromTemporaryStorageRegisterButton, x => ShowImportFromTemporaryRegisterButton(x.CSI_Procedure), x => x.CSI_ProcedureInfo);

		return builder.Build();
	}

	static bool ShowImportFromTemporaryRegisterButton(ZString cSiProcedure) => ImportEnabledCodes.Contains(cSiProcedure);

	[ThreadSafe]
	static readonly HashSet<ZString> ImportEnabledCodes =
	[
		PreviousProcedureList.Codes.TypeASimplifiedForwardersCustomsWarehouse,
		PreviousProcedureList.Codes.TypeETemporaryStorage,
		PreviousProcedureList.Codes.TypeA8TemporaryStorage10DayRuleStoredAtConsignee
	];
}
