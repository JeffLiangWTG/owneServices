using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Tests.Fakes;
using Moq;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    public class CodeMappingController_TestBase
    {
        public CodeMappingController_TestBase()
        {
			var _context = TestContextWithData.Create();

            controller = new CodeMappingController();
			controller.Context = _context;

            httpRequest = new Mock<HttpRequestBase>();
            httpResponse = new Mock<HttpResponseBase>();
            httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(ctx => ctx.Request).Returns(httpRequest.Object);
            httpContext.SetupGet(ctx => ctx.Response).Returns(httpResponse.Object);
            controller.ControllerContext = new ControllerContext(httpContext.Object, new RouteData(), controller);

            formData = new NameValueCollection();
            httpRequest.SetupGet(req => req.Form).Returns(formData);
            httpRequest.SetupGet(req => req.QueryString).Returns(formData);

            responseText = new StringBuilder();
            httpResponse.SetupProperty(rsp => rsp.StatusCode);
            httpResponse.Setup(rsp => rsp.Write(It.IsAny<string>())).Callback((string s) => responseText.Append(s));

            postedFilesNames = new List<string>();
            postedFilesStore = new List<Mock<HttpPostedFileBase>>();
            postedFiles = new Mock<HttpFileCollectionBase>();
            postedFiles.Setup(key => key.GetEnumerator()).Returns(postedFilesNames.GetEnumerator());
            httpRequest.Setup(req => req.Files).Returns(postedFiles.Object);
        }

        public void AddPostedFile(string name, string contents)
        {
            var postedFile = new Mock<HttpPostedFileBase>();
            postedFile.SetupGet(f => f.InputStream).Returns(new MemoryStream(Encoding.Default.GetBytes(contents)));
            postedFilesStore.Add(postedFile);
            postedFilesNames.Add(name);
            postedFiles.Setup(keys => keys[name]).Returns(postedFile.Object);
        }

        protected readonly CodeMappingController controller;
		protected readonly TestContext context;
        protected readonly Mock<HttpRequestBase> httpRequest;
        protected readonly Mock<HttpResponseBase> httpResponse;
        protected readonly Mock<HttpContextBase> httpContext;
        protected readonly NameValueCollection formData;
        protected readonly Mock<HttpFileCollectionBase> postedFiles;
        protected readonly List<string> postedFilesNames;
        protected readonly List<Mock<HttpPostedFileBase>> postedFilesStore;
        protected readonly StringBuilder responseText;
    }
}
