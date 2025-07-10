using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CustomsReferenceGridDropEditTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFilteredListForDropDown_NoExceptionWhenListIsNull()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var referenceNumber = declaration.AdditionalReferenceNumbers.AddNew();

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var zCustomsReferenceGridDropEdit = new CustomsReferenceGridDropEdit();
				form.Controls.Add(zCustomsReferenceGridDropEdit);
				zCustomsReferenceGridDropEdit.SetDataBinding(referenceNumber, "CE_EntryType");
				form.Show();
				Application.DoEvents();

				var filteredListForDropDownMethod = typeof(CustomsReferenceGridDropEdit).GetMethod("GetFilteredListForDropDown", BindingFlags.NonPublic | BindingFlags.Instance);
				var filteredListForDropDown = (IList)filteredListForDropDownMethod.Invoke(zCustomsReferenceGridDropEdit, null);

				CombineAssertions(() =>
				{
					AssertNotNull("List not null", filteredListForDropDown);

					zCustomsReferenceGridDropEdit.List = null;
					zCustomsReferenceGridDropEdit.SetDataBinding(null, "CE_EntryType");
					zCustomsReferenceGridDropEdit.Refresh();
					var nullFilteredListForDropDown = filteredListForDropDownMethod.Invoke(zCustomsReferenceGridDropEdit, null);
					AssertNull("List is null", zCustomsReferenceGridDropEdit.List);
					AssertNull("No exception", nullFilteredListForDropDown);
				});
			}
		}
	}
}
