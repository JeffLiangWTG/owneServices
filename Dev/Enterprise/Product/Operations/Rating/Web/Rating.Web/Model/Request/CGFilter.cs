
namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides attributes specific to CargoGuide serving as filters for searching of CargoGuide rates.
	/// </summary>
	public class CGFilter
	{
		/// <summary>
		/// Match against the Rate Type for searching of CargoGuide rates.
		/// </summary>
		public int[] RateClasses { get; set; }

		/// <summary>
		/// Match against the Reference for searching of CargoGuide rates.
		/// </summary>
		public string[] References { get; set; }

		/// <summary>
		/// Match against the Product for searching of CargoGuide rates.
		/// </summary>
		public string[] Products { get; set; }

		/// <summary>
		/// Match against the Via for searching of CargoGuide rates.
		/// </summary>
		public string[] Vias { get; set; }
	}
}
