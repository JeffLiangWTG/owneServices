using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class CIMEDIMessage : EDIMessage, Integration.Forwarding.ICIMEDIMessage
	{
		#region Schema

		public new abstract class Schema : EDIMessage.Schema
		{
			public const string EM_StatusDateTime = "EM_StatusDateTime";
		}

		#endregion

		public CIMEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCode;
		}

		protected virtual ZString ApplicationCode
		{
			get { return ApplicationCodes.CIM; }
		}

		#region Saving and Message Number Fountain

		protected override string GetMessageReferenceNumber()
		{
			return NumberFountain.GetNextFormatted(Factory);
		}

		protected virtual INumberFountainProxy NumberFountain
		{
			get { return Env.NumberFountains.CIMNumber; }
		}

		protected override bool ClearMessageNumberOnFailureToSaveCore => true;

		#endregion

		#region Properties

		#region Staff

		GlbStaff fStaff;
		public GlbStaff Staff
		{
			get
			{
				if (fStaff == null)
				{
					StmALog log = StmALogEntryLocator.Instance.GetLastPostEventOfType(this, AutoEvents.AddedARecordToTheSystem);
					if (log != null)
					{
						fStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, log.SL_GS_NKUser);
					}
				}

				return fStaff;
			}
		}

		#endregion

		#region EM_User

		public new ZString EM_User
		{
			get
			{
				ZString result = "";
				if (IsTransmitMessage && Staff != null)
				{
					result = Staff.GS_FullName;
				}
				else
				{
					result = (NoResString)"System"; // Hard-coded constant
				}
				return result;
			}
		}

		#endregion

		#region EM_StatusDateTime

		public ZDateTime EM_StatusDateTime
		{
			get
			{
				var result = ZDateTime.Empty;

				var log = GetLastAddedOrEditedLog();
				if (log != null)
				{
					result = log.SL_EventTime;
				}

				return result;
			}
		}

		StmALog GetLastAddedOrEditedLog()
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, PK)
			{
				FetchOnlyFromLocalCache = !IsInDatabase
			};

			var eventCodes = new[]
			{
				AutoEvents.AddedARecordToTheSystemCode, AutoEvents.EditedARecordCode
			};

			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCodes);
			filter.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;

			var logs = Factory.Load<StmALog>(filter);
			return logs.FirstOrDefault();
		}

		public ZPropertyInfo EM_StatusDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.EM_StatusDateTime); }
		}

		#endregion

		public new CIMEDIInterchange Interchange
		{
			get { return Factory.Load<CIMEDIInterchange>(EM_EI); }
		}

		[List("Lookups.MessageTypeList")]
		public override ZString EM_MessageType
		{
			get { return base.EM_MessageType; }
			set { base.EM_MessageType = value; }
		}

		public ZString MessageTypeDescription
		{
			get
			{
				ZString result = Lookups.MessageTypeList.GetDescriptionFromCode(EM_MessageType);
				return result.IsEmpty ? UnknownMessageTypeDescription : result;
			}
		}

		internal static ZString UnknownMessageTypeDescription
		{
			get { return Res.GetString("f6e6d612-8e6a-45a8-aeab-42b4fe967a0b", "unknown message type"); }
		}

		#region EM_FormattedMessageText

		public new ZString EM_FormattedMessageText
		{
			get { return EM_MessageText; }
		}

		#endregion

		#endregion

		#region Constants

		public static class MessageTypes
		{
			public static class Sent
			{
				public const string FWB = CargoIMPMessageTypeList.Codes.FWB;
				public const string FHL = CargoIMPMessageTypeList.Codes.FHL;
				public const string FSR = CargoIMPMessageTypeList.Codes.FSR;
			}

			public static class Received
			{
				public const string FNA = CargoIMPMessageTypeList.Codes.FNA;
				public const string FMA = CargoIMPMessageTypeList.Codes.FMA;
				public const string FSA = CargoIMPMessageTypeList.Codes.FSA;
				public const string FSU = CargoIMPMessageTypeList.Codes.FSU;
			}
		}

		public static class MessageSubTypes
		{
			public const string Forwarding = "XXX";
			public const string Standalone = "STL";
		}

		#endregion

		#region Lookups

		public new CIMEDIMessageLookups Lookups
		{
			get { return (CIMEDIMessageLookups)base.Lookups; }
		}

		protected override EDIMessageLookups GetNewLookups()
		{
			return new CIMEDIMessageLookups(this);
		}

		#endregion
	}
}
