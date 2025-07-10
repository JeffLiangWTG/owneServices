using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class MessageNumStrategyIncludingLicenceCode : IMessageNumberStrategy
	{
		public MessageNumStrategyIncludingLicenceCode(Func<GlbCompany> getCompany, Func<INumberFountainProxy> getNumberFountain, BusinessObjectFactory factory)
		{
			this.getCompany = getCompany;
			this.getNumberFountain = getNumberFountain;
			this.factory = factory;
		}

		readonly Func<GlbCompany> getCompany;
		readonly Func<INumberFountainProxy> getNumberFountain;
		readonly BusinessObjectFactory factory;

		string IMessageNumberStrategy.GetMessageReferenceNumber()
		{
			var company = getCompany();
			var numberFountain = getNumberFountain();
			return company.LicenceKeyIdentifier + "_" + numberFountain.GetNextFormatted(factory);
		}
	}
}
