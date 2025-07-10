using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCAESResponseCodeSchema.Constants.UY_Code), DescriptionProperty(USCAESResponseCodeSchema.Constants.UY_NarrativeText)]
	public class USCAESResponseCode : AutoUSCAESResponseCode
	{
		#region Constructors

		public USCAESResponseCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public enum SeverityType
		{
			NotApplicable = 0,
			Notification = 1,
			Informational = 2,
			Compliance = 3,
			Verify = 4,
			Warning = 5,
			Fatal = 6
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public SeverityType Severity
		{
			get { return GetSeverityType(UY_Severity); }
		}

		protected SeverityType GetSeverityType(ZString severityCode)
		{
			SeverityType result = SeverityType.NotApplicable;
			ZString codeToCheck = severityCode.Trim().ToUpper();
			if (!codeToCheck.IsEmpty && SeverityTypeList.ContainsKey(codeToCheck))
			{
				result = SeverityTypeList[codeToCheck];
			}
			return result;
		}

		#region SeverityTypeList
		Dictionary<string, SeverityType> SeverityTypeList
		{
			get
			{
				if (fSeverityTypeList == null)
				{
					fSeverityTypeList = new Dictionary<string, SeverityType>();
					fSeverityTypeList.Add("NOTIFICATION", SeverityType.Notification);
					fSeverityTypeList.Add("INFORMATIONAL", SeverityType.Informational);
					fSeverityTypeList.Add("COMPLIANCE", SeverityType.Compliance);
					fSeverityTypeList.Add("VERIFY", SeverityType.Verify);
					fSeverityTypeList.Add("WARNING", SeverityType.Warning);
					fSeverityTypeList.Add("FATAL", SeverityType.Fatal);
				}
				return fSeverityTypeList;
			}
		}
		Dictionary<string, SeverityType> fSeverityTypeList;
		#endregion
	}
}
