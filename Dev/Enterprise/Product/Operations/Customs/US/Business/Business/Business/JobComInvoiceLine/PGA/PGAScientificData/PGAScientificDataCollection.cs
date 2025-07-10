using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class PGAScientificDataCollection : DependentCusAddInfoCollection<ScientificData, BusinessObject>
	{
		public PGAScientificDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USSCI)
		{
		}

		public void UpdateToCountryIfEmptyOrCreate(ZString countryCode)
		{
			bool countryCodeStored = false;

			foreach (ScientificData data in this)
			{
				if (data.US_PGACountryCode.IsEmpty)
				{
					countryCodeStored = true;

					data.US_PGACountryCode = countryCode;
				}
			}

			if (!countryCodeStored)
			{
				ScientificData scientificData = AddNew();
				scientificData.US_PGACountryCode = countryCode;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (ScientificData)child;

			var constituentElement = Master as ConstituentElement;
			if (constituentElement != null)
			{
				var invoiceLine = constituentElement.InvoiceLine;
				if (invoiceLine != null)
				{
					newElement.US_PGACountryCode = invoiceLine.US_UC_NKCountryOfOrigin;
				}

				using (newElement.GetValidationSuspender())
				using (newElement.SuspendSettingHasChanges())
				{
					if (Count > 0)
					{
						ScientificData previousScientificData = this[Count - 1];

						SetDefaultFromPreviousLine(previousScientificData, newElement);
					}
				}
			}
		}

		void SetDefaultFromPreviousLine(ScientificData previousScientificData, ScientificData newElement)
		{
			newElement.US_PGAScientificGenusName = previousScientificData.US_PGAScientificGenusName;
			newElement.US_PGAScientificSpeciesName = previousScientificData.US_PGAScientificSpeciesName;
		}
	}
}
