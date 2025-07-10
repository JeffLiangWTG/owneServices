//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderReferenceValidation
//
//    This class should be used for overriding validation in AutoAccTransactionHeaderReferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderReferenceValidation : AutoAccTransactionHeaderReferenceValidation
	{
		public AccTransactionHeaderReferenceValidation(AutoAccTransactionHeaderReference parent) : base(parent)
		{
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccTransactionHeaderReferenceSchema.AH1_AH.Name)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}
