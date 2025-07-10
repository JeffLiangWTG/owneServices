using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCFIRMSSchema.Constants.US_Code), DescriptionProperty("NameAndZipCode")]
	public class USCFIRMS : AutoUSCFIRMS
	{
		#region Constants

		public static class Constants
		{
			public const string Code = "Code";
			public const string Name = "Name";
			public const string DistrictPortCode = "District Port Code";
			public const string Address = "Address";
			public const string City = "City";
			public const string State = "State";
			public const string ZipCode = "Zip Code";
			public const string Country = "Country";
			public const string FacilityType = "Facility Type";
			public const string IsActive = "Is Active";
			public const string LastUpdate = "Last Update";
		}

		#endregion

		#region Constructors

		public USCFIRMS(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#region Properties For Binding

		public ZString NameAndZipCode
		{
			get { return US_Name + " (" + US_ZipCode + ")"; }
		}

		public ZString FacilityTypeDescription
		{
			get { return new FacilityTypeList().GetDescriptionFromCode(US_FacilityType); }
		}

		public ZPropertyInfo FacilityTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FacilityTypeDescription)); }
		}

		#endregion
	}
}
