using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCAffirmationOfComplianceSchema.Constants.UL_Code), DescriptionProperty(USCAffirmationOfComplianceSchema.Constants.UL_Description)]
	public class USCAffirmationOfCompliance : AutoUSCAffirmationOfCompliance
	{
		#region Constructors

		public USCAffirmationOfCompliance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public override bool SupportsNotes
		{
			get { return false; }
		}
	}
}
