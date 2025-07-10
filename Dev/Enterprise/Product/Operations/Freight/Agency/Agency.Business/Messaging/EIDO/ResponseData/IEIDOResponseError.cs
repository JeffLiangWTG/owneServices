namespace Enterprise.Freight.Agency.Business
{
	public interface IEIDOResponseError
	{
		/// <summary>
		/// Mandatory
		/// </summary>
		string Code { get; }

		/// <summary>
		/// Mandatory
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Optional
		/// </summary>
		string Reference { get; }

		/// <summary>
		/// Optional
		/// </summary>
		EIDOResponseErrorRefType ErrorRefType { get; }
	}
}
