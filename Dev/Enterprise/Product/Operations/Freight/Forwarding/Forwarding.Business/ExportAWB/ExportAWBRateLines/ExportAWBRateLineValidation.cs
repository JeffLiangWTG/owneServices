//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBRateLineValidation
//
//    This class should be used for overriding validation in AutoExportAWBRateLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBRateLineValidation : Forwarding.AWB.Business.ExportAWBRateLineValidation
	{
		public ExportAWBRateLineValidation(ExportAWBRateLine parent)
			: base(parent)
		{
		}

		protected override void CheckER_NoOfPiecesOrRCP()
		{
			base.CheckER_NoOfPiecesOrRCP();
			if (!Parent.ER_NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty)
			{
				if (!Parent.ER_NoOfPiecesOrRCP.IsLettersOnlyOrEmpty)
				{
					Parent.ER_NoOfPiecesOrRCPInfo.AddWarning(Res.GetString("a570b862-f525-4bd0-ba46-8cbfa94ba39b", "RCP should be alphabetical characters only."));
				}

				if (Parent.ER_NoOfPiecesOrRCP.Length != 3)
				{
					Parent.ER_NoOfPiecesOrRCPInfo.AddWarning(Res.GetString("a67cb66d-a1dd-4c2d-87f7-fca5fa6c1303", "RCP should be 3 characters in length."));
				}
			}
		}
	}
}
