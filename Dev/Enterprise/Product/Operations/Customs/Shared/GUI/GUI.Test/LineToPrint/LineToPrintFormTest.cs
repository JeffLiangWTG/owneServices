using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(LineToPrintForm))]
	sealed class LineToPrintFormBasherTest : ZFormBasherTest
	{
		public void TestCaptionTakenFromProviderAttribute()
		{
			var declaration = Factory.New<DummyDeclaration>();
			using (var form = new LineToPrintForm(declaration))
			{
				form.Show();
				var lineToPrintGrid = form.Controls.Find("LineToPrintGrid", true)[0] as ZGrid;
				Assert(lineToPrintGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(l => l.ColumnName == LineToPrintCollection.OrganisationColName && l.CaptionResourceString.Caption == "Alternative Organization Caption"));
				Assert(lineToPrintGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(l => l.ColumnName == LineToPrintCollection.IdentifierColName && l.CaptionResourceString.Caption == "Alternative Identifier Caption"));
			}
		}

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var declaration = Factory.New<DummyDeclaration>();
			return new LineToPrintForm(declaration);
		}

		sealed class DummyDeclaration : BaseJobDeclaration
		{
			public DummyDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override LineToPrintCollection GetNewLineToPrintCollection() => new DummyLineToPrintCollection(this);
		}

		sealed class DummyLineToPrintCollection : LineToPrintCollection
		{
			public DummyLineToPrintCollection(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900298. Form this is used in requires ResourceString.")]
			protected override Dictionary<string, ResourceStringData> GetColumnCaptionResourceStringDictionaryCore()
			{
				var result = new Dictionary<string, ResourceStringData>() { };
				result.Add(OrganisationColName, Res.GetData("DummyLineToPrintCollection|OrganisationColName", "Alternative Organization Caption"));
				result.Add(IdentifierColName, Res.GetData("DummyLineToPrintCollection|IdentifierColName", "Alternative Identifier Caption"));
				return result;
			}

			protected override void GetLinesToPrint(BaseJobDeclaration declaration)
			{
				Add(new LineToPrintCollectionTest.DummyLineToPrint(declaration));
			}
		}
	}
}
