using NUnit.Framework;

namespace Enterprise.WebCFS.Web.Testing
{
	public class TestGlobal : Global
	{
		public override string ApplicationRoot
		{
			get { return "/"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public override string MapPath(string path)
		{
			return TestCase.BaseSourcePath + @"Enterprise\Product\Operations\WebCFS\WebCFS.Web" + path;
		}
	}
}
