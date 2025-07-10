using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageExRateDependentCollection : DependentBusinessObjectCollection<VoyageExRate, JobVoyage>
	{
		public VoyageExRateDependentCollection(JobVoyage voyage)
			: base(voyage)
		{
			CompanyPK = GlbCompany.CurrentCompany.PK;
		}

		public VoyageExRate GetRateForCurrency(ZString currencyCode)
		{
			return GetRateForCurrency(currencyCode, ZString.Empty);
		}

		public VoyageExRate GetRateForCurrency(ZString currencyCode, ZString specifiedPort)
		{
			VoyageExRate exRate = null;

			if (!currencyCode.IsEmpty)
			{
				var exRates = this.Cast<VoyageExRate>();
				if (!specifiedPort.IsEmpty)
				{
					exRate = exRates.FirstOrDefault(e => e.E8_RX_NKExCurrency == currencyCode && e.E8_RL_NKPort == specifiedPort);
				}

				if (exRate == null)
				{
					exRate = exRates.FirstOrDefault(e => e.E8_RX_NKExCurrency == currencyCode && e.E8_RL_NKPort.IsEmpty);
				}
			}

			return exRate;
		}

		public ZGuid CompanyPK { get; }

		#region Overrides

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			VoyageExRate rate = (VoyageExRate)dependent;
			base.SetCollectionRelationships(rate);
			rate.E8_GC = CompanyPK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobVoyageExRateSchema.E8_GC, CompanyPK);
			return result;
		}

		#endregion
	}
}
