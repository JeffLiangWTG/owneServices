using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class LineToPrintForm : ZChildForm
	{
		public LineToPrintForm(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			GetColumnCaptions();
		}

		void GetColumnCaptions()
		{
			var declaration = (BaseJobDeclaration)BusinessEntity;
			var linesToPrint = declaration.LinesToPrint;
			var dic = linesToPrint.ColumnCaptionResourceStringDictionary;
			if (dic.Any())
			{
				foreach (ZGridColumnInfo column in this.LineToPrintGrid.ColumnStyles)
				{
					ResourceStringData captionString = null;
					switch (column.ColumnName)
					{
						case LineToPrintCollection.OrganisationColName:
							captionString = dic.TryGetValue(LineToPrintCollection.OrganisationColName, out captionString) ? captionString : null;
							break;
						case LineToPrintCollection.IdentifierColName:
							captionString = dic.TryGetValue(LineToPrintCollection.IdentifierColName, out captionString) ? captionString : null;
							break;
					}
					if (captionString != null)
					{
						column.CaptionResourceString = captionString;
					}
				}
			}
		}
	}
}
