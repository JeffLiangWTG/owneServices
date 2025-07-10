using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class QuarantineWrapper : IQuarantine
	{
		public QuarantineWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			objectFeature = invoiceLine.JI_QuarantineFeatures;
			treatment = invoiceLine.JI_QuarantineTreatment;
			animal = new AnimalWrapper(invoiceLine);
		}

		readonly JobComInvoiceLine invoiceLine;

		ZString IQuarantine.ObjectFeature => objectFeature;

		ZString IQuarantine.Treatment => treatment;

		IAnimal IQuarantine.Animal => animal;

		readonly ZString objectFeature;

		readonly ZString treatment;

		readonly IAnimal animal;

		IEnumerable<IAdditionalDocument> IQuarantine.AdditionalDocument => invoiceLine.SlaughterDateCollection.Any() ? invoiceLine.SlaughterDateCollection.Select(x => new AdditionalDocumentWrapper(x.CY_Date)) : null;

		IEnumerable<IAdditionalInformation> IQuarantine.AdditionalInformation => invoiceLine.PackingHouseCollection.Any() ? invoiceLine.PackingHouseCollection.Select(x => new AdditionalInformationWrapper(x.CY_Code)) : null;

		IEnumerable<IPackaging> IQuarantine.Packing => invoiceLine.PackingDateCollection.Any() ? invoiceLine.PackingDateCollection.Select(x => new PackingWarpper(x.CY_Date)) : null;
	}
}
