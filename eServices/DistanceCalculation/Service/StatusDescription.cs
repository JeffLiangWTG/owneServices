using System.ComponentModel;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	public enum StatusDescription
	{
		[Description("Distance calculated successfully.")]
		OK,
		[Description("The origin and/or destination address could not be resolved by the server. Please check addresses are valid.")]
		NOT_FOUND,
		[Description("The route between the origin and destination could not be resolved by the server.")]
		ZERO_RESULTS,
		[Description("Invalid request. Please check addresses are valid.")]
		INVALID_REQUEST,
		[Description("Too many addresses passed to the server. Please limit to 1 Origin and 1 Destination.")]
		MAX_ELEMENTS_EXCEEDED,
		[Description("The server has received too many requests in a short amount of time. Please try again later.")]
		OVER_QUERY_LIMIT,
		[Description("The server has denied your request. Please try again later.")]
		REQUEST_DENIED,
		[Description("The server has responded with an unknown error. Please try again later.")]
		UNKNOWN_ERROR,
	}
}