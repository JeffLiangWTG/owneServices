using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	class CrewWrapper : ICrew
	{
		public CrewWrapper(ZString messageType, CrewMember crew)
		{
			this.crew = crew;
			splitName = crew.CP_FullName.Split(' ');

			crewId = crew.Certificates.FirstOrDefault(doc => doc.XZ_Type == CrewACEIdTypes.Codes.Id)
							 ?? crew.Certificates.FirstOrDefault(doc => doc.XZ_Type == CrewACEIdTypes.Codes.ProximityCardId);

			travelDocuments = from doc in crew.Certificates
							  where new TravelDocumentTypes().ContainsCode(doc.XZ_Type)
							  select (ITravelDocument)new TravelDocumentWrapper(doc);
			this.messageType = messageType;
		}

		#region Implementation of ICrew

		public ZString CrewId
		{
			get { return crewId == null ? ZString.Empty : crewId.XZ_RefNumber; }
		}

		public ZString IdType
		{
			get { return crewId == null ? ZString.Empty : crewId.XZ_Type; }
		}

		public ZString CrewType
		{
			get { return crew.CP_Type; }
		}

		public ZString LastName
		{
			get { return splitName.LastOrDefault(); }
		}

		public ZString FirstName
		{
			get { return splitName.FirstOrDefault(); }
		}

		public ZString MiddleName
		{
			get
			{
				var list = splitName.Skip(1).Take(splitName.Length - 2).Where(x => !x.IsEmpty);
				list.ForEach(x => x.Trim());
				return list.ToStringDelimited(" ");
			}
		}

		public ZDate DateOfBirth
		{
			get { return crew.CP_DateOfBirth.Date; }
		}

		public ZString Gender
		{
			get { return crew.CP_Gender; }
		}

		public ZString Citizenship
		{
			get { return crew.CP_RN_NKNationality; }
		}

		public IEnumerable<ITravelDocument> TravelDocuments
		{
			get { return travelDocuments.Where(doc => doc.TravelDocumentType != TravelDocumentTypes.Codes.HazmatEndorsement); }
		}

		public IAddress USAddress
		{
			get { return new AddressWrapper(crew.USAddress); }
		}

		public ZString HazmatEndorsement
		{
			get
			{
				var result = crew.HazmatEndorsement;
				if (messageType == MessageTypes.Codes.CrewOrEquipmentRegistration && result != CrewMember.Yes && result != CrewMember.No)
				{
					result = result.IsEmpty ? CrewMember.No : CrewMember.Yes;
				}
				return result;
			}
		}

		#endregion

		readonly CrewMember crew;
		readonly ZString[] splitName = System.Array.Empty<ZString>();
		readonly IEnumerable<ITravelDocument> travelDocuments = System.Array.Empty<ITravelDocument>();
		readonly GenRegCertAccredMaintList crewId;
		readonly string messageType;
	}
}
