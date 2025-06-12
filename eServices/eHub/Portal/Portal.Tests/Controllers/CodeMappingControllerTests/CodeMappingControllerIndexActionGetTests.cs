using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerIndexActionGetTests : CodeMappingController_TestBase
    {
        [TestMethod()]
        public void ShouldReturnIndexPageWithNoSelections()
        {
            // Arrange

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            ViewDataDictionary viewData = result.ViewData;
        }



    }
}
