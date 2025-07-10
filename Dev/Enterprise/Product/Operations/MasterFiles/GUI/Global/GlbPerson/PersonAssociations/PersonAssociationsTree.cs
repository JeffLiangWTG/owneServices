using System.Linq;
using Aga.Controls.Tree;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class PersonAssociationsTree : ZTreeViewAdv, ICaptionedComponents
	{
		public PersonAssociationsTree()
		{
			ColumnReordered += OnColumnReordered;
		}

		public override bool AllowDrop
		{
			get { return false; }
			set { }
		}

		void OnColumnReordered(object sender, TreeColumnEventArgs treeColumnEventArgs)
		{
			if (Columns[0].Header != GroupColumnHeader)
			{
				var relationColumn = Columns.FirstOrDefault(column => column.Header == GroupColumnHeader);
				if (relationColumn != null)
				{
					Columns.Remove(relationColumn);
					Columns.Insert(0, relationColumn);
				}
			}
		}

		#region Column Headers

		internal static string GroupColumnHeader => Res.GetString("6D60E230-786E-4D71-94CF-87B5DE15409F", "");

		internal static string DescriptionColumnHeader => Res.GetString("A3626305-AD36-4AE1-8F73-501834056452", "Name");

		internal static string ActiveColumnHeader => Res.GetString("A429A59D-BFF1-4C0A-AF54-6CEDDBFF0D04", "Active");

		internal static string PrimaryColumnHeader => Res.GetString("4b11d912-ebef-4b1a-b9be-b5378ff93562", "Primary Workplace");

		internal static string WorkingAddressUNLOCOColumnHeader => Res.GetString("bc892965-5386-416b-b929-7259bc1571eb", "UNLOCO");

		internal static string CityColumnHeader => Res.GetString("C80D3F98-32C9-4A19-90BA-6D9A23820EB1", "City");

		internal static string StateColumnHeader => Res.GetString("E809EF39-675C-421E-B0CA-F8CA5B4D3C51", "State");

		internal static string CountryColumnHeader => Res.GetString("19C82CC7-12A4-45AD-98DB-41610F955703", "Country/Region");

		internal static string OfficePhoneColumnHeader => Res.GetString("C399BD96-39A8-44DA-BFAA-BD470008DB2D", "Work");

		internal static string DirectPhoneColumnHeader => Res.GetString("5BC8781D-E2C2-4B1E-91D1-238484EB90DB", "Mobile");

		internal static string EmailColumnHeader => Res.GetString("5FCBED98-5CC3-46AA-BCCE-4B07A703C2B8", "Email");

		internal static string ShowInactiveCheckBoxCaption => Res.GetString("69B2E0A1-C136-41D1-BE3A-8D87F15DBAF9", "Show Inactive");

		internal static string CreatedTimeColumnHeader => Res.GetString("5E3E9CBF-3549-4430-8A51-444683BBDBF7", "Created");

		#endregion
	}
}
