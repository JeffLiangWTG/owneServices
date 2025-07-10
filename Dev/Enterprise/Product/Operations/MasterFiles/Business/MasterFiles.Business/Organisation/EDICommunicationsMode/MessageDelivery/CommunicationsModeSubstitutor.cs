using System.Collections.Generic;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery.Utils;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public enum CommunicationModeSubstitutorProperty
	{
		EmailAddress,
		EmailText,
		EmailSubject,
		FileName,
	}

	/// <summary>
	/// Substitute Placeholder with actual value in Communication Mode
	/// </summary>
	public class CommunicationsModeSubstitutor
	{
		public delegate ZString ExtraDataSubstitutionDelegate(CommunicationModeSubstitutorProperty property, ZString data);

		public ExtraDataSubstitutionDelegate ExtraDataSubstitution;

		public ZString JobNumber;

		public IEDICommunicationsMode Substitute(IEDICommunicationsMode mode)
		{
			var result = new DummyEDICommunicationsMode(mode);
			var org = (OrgHeader)mode.Organisation;
			result.EK_Filename = MakeFilenameSafe.MakeSafe(Substitute(CommunicationModeSubstitutorProperty.FileName, mode.EK_Filename, org), '_');
			result.EK_ServerAddressSubject = Substitute(CommunicationModeSubstitutorProperty.EmailSubject, mode.EK_ServerAddressSubject, org);
			return result;
		}

		public ZString Substitute(CommunicationModeSubstitutorProperty property, string nameSeed, OrgHeader orgHeader)
		{
			while (TimeMarker.LastMarkedTime.IsNow())
			{
				System.Threading.Thread.Sleep(1);
			}
			TimeMarker.Mark();

			return Substitute(property, nameSeed, TimeMarker.LastMarkedTime, orgHeader);
		}

		public string Substitute(CommunicationModeSubstitutorProperty property, ZString nameSeed, ZDateTime timeToSubstitute, OrgHeader orgHeader)
		{
			var dict = new Dictionary<string, string>();
			var timeAsString = timeToSubstitute.ToString("yyyyMMddHHmmssffff");
			dict.Add(EDIMessageDelivery.ReplacementConstants.DateTime, timeAsString);
			dict.Add(EDIMessageDelivery.ReplacementConstants.JobNumber, JobNumber);

			if (orgHeader != null)
			{
				dict.Add(EDIMessageDelivery.ReplacementConstants.Organisation, orgHeader.OH_FullName);
			}

			nameSeed = nameSeed.Substitute(dict);

			if (ExtraDataSubstitution != null)
			{
				nameSeed = ExtraDataSubstitution(property, nameSeed);
			}
			return nameSeed;
		}
	}

	/// <summary>
	/// In order to change the value in EDICommunicationsMode
	/// </summary>
	class DummyEDICommunicationsMode : IEDICommunicationsMode
	{
		public DummyEDICommunicationsMode(IEDICommunicationsMode mode)
		{
			EK_ECC_CommunicationPartyConfig = mode.EK_ECC_CommunicationPartyConfig;
			EK_CommunicationsTransport = mode.EK_CommunicationsTransport;
			EK_Destination = mode.EK_Destination;
			EK_FileFormat = mode.EK_FileFormat;
			EK_Filename = mode.EK_Filename;
			EK_LastFailed = mode.EK_LastFailed;
			EK_LocalPartyVanID = mode.EK_LocalPartyVanID;
			EK_LoginName = mode.EK_LoginName;
			EK_MessagePurpose = mode.EK_MessagePurpose;
			EK_Password = mode.EK_Password;
			EK_PortNumber = mode.EK_PortNumber;
			EK_RelatedPartyVanID = mode.EK_RelatedPartyVanID;
			EK_ServerAddressSubject = mode.EK_ServerAddressSubject;
			EK_PublishInternalMilestones = mode.EK_PublishInternalMilestones;
			Organisation = mode.Organisation;
		}

		public ZGuid EK_ECC_CommunicationPartyConfig { get; set; }

		public ZString EK_CommunicationsTransport { get; set; }

		public ZString EK_Destination { get; set; }

		public ZString EK_FileFormat { get; set; }

		public ZString EK_Filename { get; set; }

		public ZDateTime EK_LastFailed { get; set; }

		public ZString EK_LocalPartyVanID { get; set; }

		public ZString EK_MessagePurpose { get; set; }

		public ZString EK_RelatedPartyVanID { get; set; }

		public ZString EK_ServerAddressSubject { get; set; }

		public IOrgHeader Organisation { get; private set; }

		public ZString EK_LoginName { get; set; }

		public ZString EK_Password { get; set; }

		public ZInt EK_PortNumber { get; set; }

		public ZBool EK_PublishInternalMilestones { get; set; }
	}
}
