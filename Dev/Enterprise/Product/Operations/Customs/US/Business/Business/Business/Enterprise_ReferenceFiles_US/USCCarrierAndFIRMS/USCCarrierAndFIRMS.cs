using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCCarrierAndFIRMS.Schema.US_Code), DescriptionProperty(USCCarrierAndFIRMS.Schema.US_Name)]
	public class USCCarrierAndFIRMS : AutoUSCCarrierAndFIRMS
	{
		public USCCarrierAndFIRMS(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public ZString FacilityTypeDescription
		{
			get { return string.IsNullOrEmpty(US_FacilityType) ? string.Empty : new FacilityTypeList().GetDescriptionFromCode(US_FacilityType); }
		}

		public ZPropertyInfo FacilityTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FacilityTypeDescription)); }
		}
	}
}
