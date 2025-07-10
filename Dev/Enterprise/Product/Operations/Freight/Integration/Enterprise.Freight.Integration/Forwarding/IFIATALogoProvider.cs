using System.Drawing;

namespace Enterprise.Freight.Integration
{
	public interface IFIATALogoProvider
	{
		Image GetFIATALogo(string countryCode);
		Image GetFIATATextLogo(bool forSeaWaybill);
	}
}
