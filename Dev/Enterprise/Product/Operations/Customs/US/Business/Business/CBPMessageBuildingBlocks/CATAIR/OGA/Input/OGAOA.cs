using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAOA : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAOA, IBIRDOGAStartRecord
	{
		#region IBIRDOGAStartRecord Members

		ZString IBIRDOGAStartRecord.CommercialDesc
		{
			get { return ZString.Empty; }
		}

		void IBIRDOGAStartRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			SetDisclaimIndicator(invoiceLine, OtherAgencyDeclaration);
			SetDisclaimIndicator(invoiceLine, OtherAgencyDeclaration1);
			SetDisclaimIndicator(invoiceLine, OtherAgencyDeclaration2);
			SetDisclaimIndicator(invoiceLine, OtherAgencyDeclaration3);
			SetDisclaimIndicator(invoiceLine, OtherAgencyDeclaration4);
		}

		void SetDisclaimIndicator(JobComInvoiceLine invoiceLine, ZString disclaim)
		{
			if (!disclaim.IsEmpty)
			{
				switch (disclaim)
				{
					case "FD0":
						invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
						break;
					case "DT0":
						invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
						break;
					case "FC0":
						invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
						break;
				}
			}
		}

		#endregion
	}
}
