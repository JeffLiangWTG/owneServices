namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;
	using CargoWise.Types;

	public interface ICrew
	{
		/// <summary>
		/// US: ACE ID/CDL/License number/Proximity Card ID if pre-registered in ACE.
		///		(If pre-registered, the Party ID is used to associate pre-registered Crew Member to Manifest. Otherwise individual Crew data elements to be provided)
		///		(C/10/20/20/50), Condition: If Crew/Person in charge is pre-registered in ACE.
		/// CA: CDRP/FAST Id. 
		///		(C/25), Condition: If applicable.
		/// </summary>
		ZString CrewId { get; }

		/// <summary>
		/// Crew/Passenger.
		/// US,CA: (M/3).
		/// </summary>
		ZString CrewType { get; }

		/// <summary>
		/// US,CA: (M/40)
		/// </summary>
		ZString LastName { get; }

		/// <summary>
		/// US,CA: (M/40)
		/// </summary>
		ZString FirstName { get; }

		/// <summary>
		/// US,CA: (C/40)
		/// </summary>
		ZString MiddleName { get; }

		/// <summary>
		/// US: (M/MMDDYY).
		/// CA: (M/YYYYMMDD)
		/// </summary>
		ZDate DateOfBirth { get; }

		/// <summary>
		/// US:(M/1).
		/// CA: Not required.
		/// </summary>
		ZString Gender { get; }

		/// <summary>
		/// Nationality/Citizenship. 
		/// US,CA: (M/2).
		/// </summary>
		ZString Citizenship { get; }

		/// <summary>
		/// Parties Travel Documents.
		/// Multiple Travel documents may be required as per CBP Policy.
		/// US: (M/1).
		/// CA: (C/1), Condition: Must be transmitted if CDRP/FAST ID is not provided. 
		///</summary>
		IEnumerable<ITravelDocument> TravelDocuments { get; }
	}
}
