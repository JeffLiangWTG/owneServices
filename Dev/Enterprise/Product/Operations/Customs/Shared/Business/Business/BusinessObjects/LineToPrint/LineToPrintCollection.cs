using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class LineToPrintCollection : NonPersistentBusinessObjectCollection<LineToPrint>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Column Name Constant")]
		public const string OrganisationColName = "Organisation";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Column Name Constant")]
		public const string IdentifierColName = "Identifier";

		public LineToPrintCollection(BaseJobDeclaration declaration)
			: base(declaration.Factory)
		{
			GetLinesToPrint(declaration);
			if (this.Count == 1)
			{
				this[0].ShouldBePrinted = true;
			}
		}

		protected virtual void GetLinesToPrint(BaseJobDeclaration declaration)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		public Dictionary<string, ResourceStringData> ColumnCaptionResourceStringDictionary => GetColumnCaptionResourceStringDictionaryCore();

		protected virtual Dictionary<string, ResourceStringData> GetColumnCaptionResourceStringDictionaryCore()
		{
			return new Dictionary<string, ResourceStringData>();
		}
	}
}
