using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CustomsReferenceDropEditTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFilteredListForDropDown_NoExceptionWhenListIsNull()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var referenceNumber = declaration.AdditionalReferenceNumbers.AddNew();

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var zCustomsReferenceDropEdit = new CustomsReferenceDropEdit();
				form.Controls.Add(zCustomsReferenceDropEdit);
				zCustomsReferenceDropEdit.SetDataBinding(referenceNumber, "CE_EntryType");
				form.Show();
				Application.DoEvents();

				var filteredListForDropDownMethod = typeof(CustomsReferenceDropEdit).GetMethod("GetFilteredListForDropDown", BindingFlags.NonPublic | BindingFlags.Instance);
				var filteredListForDropDown = (IList)filteredListForDropDownMethod.Invoke(zCustomsReferenceDropEdit, null);

				CombineAssertions(() =>
				{
					AssertNotNull("List not null", filteredListForDropDown);

					zCustomsReferenceDropEdit.List = null;
					zCustomsReferenceDropEdit.SetDataBinding(null, "CE_EntryType");
					zCustomsReferenceDropEdit.Refresh();
					var nullFilteredListForDropDown = filteredListForDropDownMethod.Invoke(zCustomsReferenceDropEdit, null);
					AssertNull("List is null", zCustomsReferenceDropEdit.List);
					AssertNull("No exception", nullFilteredListForDropDown);
				});
			}
		}
	}
}
