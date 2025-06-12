using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Moq;

namespace CargoWise.eHub.Portal.Tests.Model
{
	public class TestRequest : HttpRequestBase
	{
		Dictionary<string, string> container = new Dictionary<string, string>();

		public Dictionary<string, string> Container => container;

		public override string this[string key] => container.ContainsKey(key) ? container[key] : null;

		Mock<HttpFileCollectionBase> httpFiles = new Mock<HttpFileCollectionBase>();
		List<Mock<HttpPostedFileBase>> httpPostedFiles = new List<Mock<HttpPostedFileBase>>();

		public override HttpFileCollectionBase Files => httpFiles.Object;

		public void AddFile(string name, Stream contents)
		{
			var file = new Mock<HttpPostedFileBase>();
			httpPostedFiles.Add(file);
			file.SetupGet(f => f.InputStream).Returns(contents);
			httpFiles.Setup(f => f[name]).Returns(file.Object);
			httpFiles.SetupGet(f => f.Count).Returns(httpPostedFiles.Count);
		}

		public void AddFile(string name, string contents)
		{
			AddFile(name, new MemoryStream(Encoding.Default.GetBytes(contents)));
		}

		public void Clear()
		{
			container.Clear();
			httpPostedFiles.ForEach(f => f.Object.InputStream.Close());
			httpPostedFiles.Clear();
			httpFiles.SetupGet(f => f.Count).Returns(httpPostedFiles.Count);
		}

		public override UnvalidatedRequestValuesBase Unvalidated
		{
			get
			{
				var mockUnvalidatedRequestValue = new Mock<UnvalidatedRequestValuesBase>();
				System.Collections.Specialized.NameValueCollection queryString = new System.Collections.Specialized.NameValueCollection()
				{
					{"PR_XML","PR_XMLQueryString"}
				};

				var form = new System.Collections.Specialized.NameValueCollection()
				{
					{"PR_XML","test"},
				};

				mockUnvalidatedRequestValue.SetupGet(u => u.Form).Returns(form);
				mockUnvalidatedRequestValue.SetupGet(u => u.QueryString).Returns(queryString);

				return mockUnvalidatedRequestValue.Object;
			}
		}
	}
}
