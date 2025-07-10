namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// Response structure
	/// </summary>
	public class ApiResponse
	{
		/// <summary>
		/// The result of the call
		/// </summary>
		public Rate[] Rates { get; set; }

		/// <summary>
		/// Warnings and information
		/// </summary>
		public string[] Warnings { get; set; }
	}
}
