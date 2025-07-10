using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IManifestMessageAttacheeMessageLinker
	{
		IManifestMessageAttachee Link(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer);

		IManifestMessageAttachee MatchManifestAndLink(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer);

		IManifestMessageAttachee MatchManifest(AMSEDIMessage message, ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch, IComparer<IBaseBillOfLading> matchComparer);
	}
}
