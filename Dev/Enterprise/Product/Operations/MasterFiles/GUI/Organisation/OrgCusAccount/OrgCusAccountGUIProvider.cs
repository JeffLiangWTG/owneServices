using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class OrgCusAccountGUIProvider
	{
		public OrgCusAccountGUIProvider(ZString countryCode) => CountryCode = countryCode;

		public ZString CountryCode { get; }

		public static OrgCusAccountGUIProvider GetByCountryCode(ZString countryCode) => NameBasedProvider.Get<OrgCusAccountGUIProvider>(countryCode, countryCode);

		public virtual ZGridColumnInfoControl CZ_Code => new ZGridColumnInfoControl(Res.GetData("0a7375cf-5bde-426f-8422-2c103c5ca370", "Code"), 47);
		public virtual ZGridColumnInfoControl CZ_Type => new ZGridColumnInfoControl(Res.GetData("6B17ABC2-1236-4989-9B50-F333BF58A6D7", "Account Type"), 89);
		public virtual ZGridColumnInfoControl CZ_Account => new ZGridColumnInfoControl(Res.GetData("69103b2f-0798-4eea-840c-f84c276c9a84", "Account"), 62);
		public virtual ZGridColumnInfoControl CZ_Issuer => new ZGridColumnInfoControl(Res.GetData("abdeec65-5dad-4995-ab4c-6b3b2999168c", "Issuer"), 53);
		public virtual ZGridColumnInfoControl DecryptedPassword => new ZGridColumnInfoControl(Res.GetData("c5ff34de-f049-4542-8926-d59b499604ac", "Decrypted Password"), 159);
		public virtual ZGridColumnInfoControl CZ_ReportingPeriod => new ZGridColumnInfoControl(Res.GetData("9A33FF45-4C7A-4BAD-8DD5-64DC0C76175E", "Reporting Period"), 104, false);
		public virtual ZGridColumnInfoControl CZ_RepresentativeID => null;

		public sealed class ZGridColumnInfoControl
		{
			readonly ResourceStringData resourceStringData;
			readonly bool isVisible;
			readonly int width;

			public ZGridColumnInfoControl(ResourceStringData resourceStringData, int width, bool isVisible = true)
			{
				this.resourceStringData = resourceStringData;
				this.isVisible = isVisible;
				this.width = width;
			}

			public void Update(ZGridColumnInfo gridColumn)
			{
				gridColumn.CaptionResourceString = resourceStringData;
				gridColumn.IsVisible = isVisible;
				gridColumn.Width = width;
			}
		}
	}
}
