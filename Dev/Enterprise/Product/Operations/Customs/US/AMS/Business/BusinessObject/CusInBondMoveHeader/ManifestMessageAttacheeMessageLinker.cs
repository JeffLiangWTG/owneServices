using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class ManifestMessageAttacheeMessageLinker : IManifestMessageAttacheeMessageLinker
	{
		public static IManifestMessageAttachee Link(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			return OriginalMessageLinker.Link(message) as IManifestMessageAttachee ?? MatchManifestAndLink(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
		}

		public static IManifestMessageAttachee MatchManifestAndLink(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			var bizObj = MatchManifest(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
			if (!carrierCode.IsEmpty)
			{
				if (bizObj == null)
				{
					message.EM_LinkedObject = null;
					message.EM_LinkTable = "";
				}
				else
				{
					var obj = (CargoWise.EntityFramework.BusinessObject)bizObj;
					message.EM_LinkedObject = obj;
					message.EM_LinkTable = obj.TableName;
				}
			}
			return bizObj;
		}

		static IManifestMessageAttachee MatchManifest(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			IManifestMessageAttachee bizObj = null;
			if (!carrierCode.IsEmpty)
			{
				var loader = new CusInBondMoveHeader.Loader(message.Factory);
				if (!vesselName.IsEmpty && !carrierCode.IsEmpty && !voyageNumber.IsEmpty)
				{
					bizObj = loader.FindByManifestDataAndBillOfLading(carrierCode,
																	vesselName,
																	voyageNumber,
																	districtPortOfUnladingCode,
																	estimatedDate,
																	billOfLadingIssuerCode,
																	billOfLadingNumber,
																	refNumQualifier,
																	refNum,
																	inBondNumber,
																	movementMatch,
																	matchComparer);
				}
				if (bizObj == null || bizObj.CarrierCode != carrierCode)
				{
					bizObj = loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany(carrierCode, manifestSequenceNumber) ?? bizObj;
				}
			}
			return bizObj;
		}

		#region IManifestMessageAttacheeMessageLinker Members

		IManifestMessageAttachee IManifestMessageAttacheeMessageLinker.Link(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			return Link(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
		}

		IManifestMessageAttachee IManifestMessageAttacheeMessageLinker.MatchManifestAndLink(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			return MatchManifestAndLink(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
		}

		IManifestMessageAttachee IManifestMessageAttacheeMessageLinker.MatchManifest(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer)
		{
			return MatchManifest(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, matchComparer);
		}

		#endregion
	}
}
