using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IEIDOResponseMessage
	{
		/// <summary>
		/// Mandatory
		/// </summary>
		EIDOResponseType ResponseType { get; }

		/// <summary>
		/// Mandatory
		/// </summary>
		DateTime ResponseDateTime { get; }

		/// <summary>
		/// Mandatory
		/// </summary>
		string DocumentReference { get; }

		/// <summary>
		/// Mandatory
		/// </summary>
		DateTime DocumentIssuedDate { get; }

		/// <summary>
		/// Mandatory
		/// </summary>
		string MessageSender { get; }

		/// <summary>
		/// Optional
		/// </summary>
		string MessageRecipient { get; }

		IEnumerable<IEIDOResponseError> Errors { get; }
	}
}
