using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing;

sealed class ValidationToolTabPageUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var userControl = new ValidationToolTabPageUserControl();
		AssertEquals(typeof(IWorkflowProvider), userControl.BindingSource.DataSourceType);
	}

	[RequiresSTA]
	public void TestValidationToolUserControl() => CombineAssertions(() =>
	{
		var mockBusiness = new Mock<IBusiness>();
		mockBusiness.SetupGet(x => x.Factory).Returns(Factory);
		var mockWorkflowProvider = mockBusiness.As<IWorkflowProvider>();
		using var form = new ZForm(mockWorkflowProvider.Object);
		var userControl = new ValidationToolTabPageUserControl();
		form.Controls.Add(userControl);
		form.Show();

		AssertEquals("Dock", DockStyle.Fill, userControl.ValidationToolUserControl.Dock);
		AssertType<NonPersistentValidationToolParent>("DataSource", userControl.ValidationToolUserControl.BindingSource.DataSource);
		AssertEquals("SetReadOnlyIncludingChildren", true, userControl.ValidationToolUserControl.ValidationRulesGrid.GetReadOnly());
	});
}
