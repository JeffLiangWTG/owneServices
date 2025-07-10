using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefPacksForm))]
	sealed class RefPacksFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefPacksForm(fRefPacks);
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestFormWithBaseRefPacks()
		{
			using (var form = new RefPacksForm(fRefPacks))
			{
				form.Show();
			}
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestFormWithAURefPacks()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				fRefPacks = Factory.New<CusRefPacks>();
				AssertNotEquals("Customs Pack should have a non-empty list", 0, fRefPacks.RP_CustomsPack_List.Count);

				using (var form = new RefPacksForm(fRefPacks))
				{
					form.Show();
				}
			}
		}

		#region Implementation

		BaseRefPacks fRefPacks;

		protected override void SetUp()
		{
			base.SetUp();
			fRefPacks = Factory.New<BaseRefPacks>();
		}

		#endregion
	}
}
