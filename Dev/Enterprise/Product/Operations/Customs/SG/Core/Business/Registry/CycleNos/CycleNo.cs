using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.Registry
{
	[CodeProperty("CycleNumStr")]
	[DescriptionProperty("SubmissionTimeStr")]
	[XmlSerializerAssembly("Enterprise.Customs.SG.V4.Business.XmlSerializers")]
	public class CycleNo : RegistryBusinessObjectTemplate
	{
		public CycleNo()
		{
		}

		public CycleNo(FallbackLevel fallbackLevel, BusinessObjectFactory factory, CycleNoCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.parentCollection = parentCollection;
		}

		readonly CycleNoCollection parentCollection;

		public static class Schema
		{
			public const string CycleNum = "CycleNum";
			public const string SubmissionTime = "SubmissionTime";
		}

		#region CycleNum

		public ZInt CycleNum
		{
			get { return fCycleNum; }
			set
			{
				SetNonPersistentPropertyValue(CycleNumInfo, ref fCycleNum, value);

				if (!IsValidationSuspended)
				{
					ValidateCycleNum();
				}
			}
		}
		ZInt fCycleNum;

		public ZPropertyInfo CycleNumInfo
		{
			get { return GetZPropertyInfo(Schema.CycleNum); }
		}

		public void ValidateCycleNum()
		{
			CycleNumInfo.ClearAllNotifications();

			if (parentCollection != null)
			{
				foreach (CycleNo cycleNum in parentCollection)
				{
					if (cycleNum != this && cycleNum.CycleNum == CycleNum)
					{
						CycleNumInfo.AddError(string.Format(System.Globalization.CultureInfo.CurrentCulture, DuplicateCycleNum, CycleNum));
						break;
					}
				}
			}
		}
		internal const string DuplicateCycleNum = "You have already entered the Cycle Number, '{0}'.";

		public ZString CycleNumStr => CycleNum.ToString();

		#endregion

		#region SubmissionTime

		[BusinessObjectTestExclude]
		public ZDateTime SubmissionTime
		{
			get { return submissionTime; }
			set
			{
				ZDateTime newSubmissionTime = value.IsEmpty ? value : (value.IsValid ? ZDateTime.MinSmallDateTimeValue.Date.Add(value.TimeOfDay) : submissionTime);

				SetNonPersistentPropertyValue(SubmissionTimeInfo, ref submissionTime, newSubmissionTime, false);

				if (!IsValidationSuspended)
				{
					ValidateSubmissionTime();
				}
			}
		}
		ZDateTime submissionTime;

		public ZPropertyInfo SubmissionTimeInfo
		{
			get { return GetZPropertyInfo(Schema.SubmissionTime); }
		}

		public void ValidateSubmissionTime()
		{
			SubmissionTimeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(SubmissionTimeInfo, "AECs Submission Time");
		}

		public ZString SubmissionTimeStr => SubmissionTime.ToShortTimeString();

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CycleNo(fallbackLevel, factory, null);
		}

		#region Xml Serialisation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Time format value")]
		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			CycleNum = reader.ReadElementStringAsZInt(Schema.CycleNum);
			SubmissionTime = reader.ReadElementStringAsZDateTime(Schema.SubmissionTime, "HH:mm");
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CycleNum, CycleNumStr);
			writer.WriteElementString(Schema.SubmissionTime, SubmissionTimeStr);
		}

		#endregion
	}
}
