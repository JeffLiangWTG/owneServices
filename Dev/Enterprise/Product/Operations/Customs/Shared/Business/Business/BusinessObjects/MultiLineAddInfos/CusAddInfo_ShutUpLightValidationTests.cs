
// For the light validation tests. GB.JobDeclaration has a property that wraps a CusAddInfo.
// This property is then used to determine the read-only status of some other properties. 
// The LightValidationTest (incorrectly?) demands that setting B7_Blah should mark 
// as needing validation on JobDeclaration.  That's totally unnecessary.
// This class shuts it up.

#if DEBUG

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public abstract partial class CusAddInfo
	{
		[LightValidationTestExempt]
		public override ZString B7_AddInfoData
		{
			get { return base.B7_AddInfoData; }
			set { base.B7_AddInfoData = value; }
		}

		[LightValidationTestExempt]
		public override ZString B7_Type
		{
			get { return base.B7_Type; }
			set { base.B7_Type = value; }
		}

		[LightValidationTestExempt]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set { base.B7_ParentTableCode = value; }
		}
	}
}
#endif
