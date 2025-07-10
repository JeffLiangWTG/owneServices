
namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides attributes specific to CargoSphere serving as filters for searching of CargoSphere rates.
	/// </summary>
	public class CSFilter
	{
		/// <summary>
		/// Match against the Rate Type for searching of CargoSphere rates.
		/// </summary>
		public string[] RateTypes { get; set; }

		/// <summary>
		/// Match against the Rate Type 2 for searching of CargoSphere rates.
		/// </summary>
		public string[] RateTypes2 { get; set; }

		/// <summary>
		/// Match against the Service String for searching of CargoSphere rates.
		/// </summary>
		public string[] ServiceStrings { get; set; }
	}
}
