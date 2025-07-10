using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedPortValidation : Customs.Business.CusCodeDataValidation
	{
		public VisitedPortValidation(VisitedPort parent) : base(parent)
		{
		}
		public new VisitedPort Parent => (VisitedPort)base.Parent;
		bool SupportsCustomsPorts
		{
			get { return Parent.VisitedPortParent?.SupportsCustomsPorts ?? false; }
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var visitedPort = Parent;

			if (visitedPort != null)
			{
				ListValidation.MessageErrorIfInvalidCode(visitedPort.CY_DataInfo);

				if (visitedPort.CY_Data.IsEmpty)
				{
					if (!SupportsCustomsPorts || visitedPort.CY_Code.IsEmpty)
					{
						visitedPort.CY_DataInfo.AddMessageError(mandatoryValidationOfCY_Data);
					}
				}
				else if (visitedPort.CY_Code.IsEmpty)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(visitedPort.CY_DataInfo);
				}
			}
		}

		protected override void CheckCY_Code()
		{
			var visitedPort = Parent;
			if (visitedPort != null)
			{
				ListValidation.MessageErrorIfInvalidCode(visitedPort.CY_CodeInfo);
				if (visitedPort.CY_Code.IsEmpty)
				{
					if (SupportsCustomsPorts && visitedPort.IsTRPort(visitedPort.CY_Data))
					{
						MandatoryValidation.MessageErrorIfNotEntered(visitedPort.CY_CodeInfo);
					}
				}
				else
				{
					if (SupportsCustomsPorts)
					{
						ValidationHelper.CheckCustomsPort(visitedPort.CY_CodeInfo, visitedPort.CY_DataInfo, visitedPort.PortOfUNLOCO, visitedPort.VisitedPortParent?.ManifestHeader);
						PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(visitedPort.CY_CodeInfo);
					}
					else
					{
						visitedPort.CY_CodeInfo.AddWarning(dataWillNotBeSendToCustoms);
					}
				}
				visitedPort.Validation.ValidateCY_Data();
			}
		}

		readonly ZString dataWillNotBeSendToCustoms = ResString.GetMultilingualString("80FD0B9C-35A5-4E32-B9A2-8E7BB9F8A4F2", "The Port(TR) code will not be sent to customs if the transport mode is not sea.");
		readonly ZString mandatoryValidationOfCY_Data = ResString.GetMultilingualString("7D0D5A9D-E6B6-4A26-A23C-45C6639B5F60", "You have not entered a Port(UNLOCO) or Port(TR).");
	}
}
