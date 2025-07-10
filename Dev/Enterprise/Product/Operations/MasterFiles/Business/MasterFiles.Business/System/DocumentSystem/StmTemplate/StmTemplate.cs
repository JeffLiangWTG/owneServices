using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ExcelTemplates.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoStmTemplate.Schema.SO_Name), DescriptionProperty(AutoStmTemplate.Schema.SO_Name)]
	public class StmTemplate : AutoStmTemplate, IStmTemplate, IDataVersionLoggingSupported, IAuditParent
	{
		public StmTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SO_TemplateType = StmTemplateTypes.Codes.Document;
		}

		public ZBool IsDocBuilderStyle
		{
			get { return IsSystemDocBuilderStyle || IsClientSpecificDocBuilderStyle; }
		}

#if DEBUG
		public ZBool IsDocBuilderStyleForTest
		{
			get;
			set;
		}
#endif

		public ZBool IsSystemDocBuilderStyle => RepositoryRegexFor(Constants.SectionRepositoryTemplateNames.System).IsMatch(SO_Name);

		public ZBool IsClientSpecificDocBuilderStyle => RepositoryRegexFor(Constants.SectionRepositoryTemplateNames.User).IsMatch(SO_NameInfo.OriginalValue.ToString());

		public ZBool IsDefaultLanguageClientSpecificDocBuilderStyle => SO_NameInfo.OriginalValue.ToString() == Constants.SectionRepositoryTemplateNames.User;

		static Regex RepositoryRegexFor(string baseName)
		{
			return new Regex(string.Format(@"^{0}( \[[A-Z]{{2,3}}(-[A-Z]{{2,3}})?])?$", baseName));
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => new StmTemplateDataVersionLogValueFormatter();

		#endregion

		#region IAuditParent

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
