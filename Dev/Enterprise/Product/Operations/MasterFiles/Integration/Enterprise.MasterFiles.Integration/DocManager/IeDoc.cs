using System;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IeDoc : IeDocBase
	{
		CodeDescriptionPairList DocType_List { get; }

		[MacroIgnore]
		BusinessObject ParentMain { get; }
		ZString VisibleCompanyCode { get; }
		ZString VisibleBranchCode { get; }
		ZString VisibleDepartmentCode { get; }
		ZBool IsCustomisableDocTypes { get; }
		ZDecimal FileSizeInMB { get; }

		void SetValuesForTest(ZDateTime dateTime, ZString dataType);
		IDisposable OpenForEdit();
		string CreateReference();
	}

	public interface IEDocsUnattendedConfigProvider
	{
		bool ApplyDocumentConfig(IeDoc eDoc);
	}
}
