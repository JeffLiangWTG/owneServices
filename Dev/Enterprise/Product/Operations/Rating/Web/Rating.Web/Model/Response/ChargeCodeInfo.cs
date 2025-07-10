namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the CW Code and the corresponding Universal Charge Code(s) assigned to the Charge applicable to the charge line of the rates or the calculated job charges responded by the Rates API.
	/// </summary>
	public class ChargeCodeInfo
	{
		/// <summary>
		/// CW Code of the charge applicable to the rates. Can be Blank if charge from CargoSphere/CargoGuide NOT assigned to any CW Charge Code yet.
		/// Value Reference: CW > Maintain > Account > Global / (Local) Charge Codes > Codes.
		/// </summary>
		public string CWCode { get; set; }

		/// <summary>
		/// Universal Charge Code(s) assigned to the Charge Code.
		/// Value Reference: CW > Maintain > Account > Charge Codes > Universal Charge Code Mappings > Universal Code.
		/// </summary>
		public string[] UniversalCodes { get; set; }
	}
}
