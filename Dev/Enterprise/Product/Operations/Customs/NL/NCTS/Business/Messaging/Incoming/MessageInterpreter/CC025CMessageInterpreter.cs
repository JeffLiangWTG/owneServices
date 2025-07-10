using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC025CMessageInterpreter : IMessageInterpreter<ICC025CDataProvider>
{
	public string Interpret(ICC025CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		switch (dataProvider.ReleaseIndicator)
		{
			case NLNctsConstants.ReleaseIndicator.FullRelease:
				note.Append((NoResString)"All Goods are released for transit upon arrival. The movement is closed.");
				break;
			case NLNctsConstants.ReleaseIndicator.PartialRelease:
				note.Append((NoResString)"Goods are partially released.");
				break;
			case NLNctsConstants.ReleaseIndicator.PartialReleaseClosed:
				note.Append((NoResString)"Goods are partially released. The movement is closed.");
				break;
			case NLNctsConstants.ReleaseIndicator.NoRelease:
				note.Append((NoResString)"No release of Goods.");
				break;
		}

		if (dataProvider.ReleaseIndicator != NLNctsConstants.ReleaseIndicator.NoRelease)
		{
			foreach (var houseConsignment in dataProvider.HouseConsignments)
			{
				note.Append($"{houseConsignment.SequenceNumeric}) House Bill: {(houseConsignment.ReleaseType == NLNctsConstants.ReleaseType.PartialRelease ? (NoResString)"partial release" : (NoResString)"full release")}");
				foreach (var consignmentItem in houseConsignment.ConsignmentItems)
				{
					note.Append($"{consignmentItem.DeclarationGoodsItemNumber}) Item: {(consignmentItem.ReleaseType == NLNctsConstants.ReleaseType.PartialRelease ? (NoResString)"partial release" : (NoResString)"full release")}");
					foreach (var package in consignmentItem.Packagings)
					{
						note.Append($"-- {package.NumberOfPackages} {package.TypeOfPackages} with the marks and numbers '{package.ShippingMarks}' are released");
					}
				}
			}
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
