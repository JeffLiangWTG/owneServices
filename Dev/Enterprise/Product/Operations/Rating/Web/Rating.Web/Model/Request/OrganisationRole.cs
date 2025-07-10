using System.Collections.Generic;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	public class OrganisationRole
	{
		/// <summary>
		/// Match against the role that the Organisation plays on the corresponding operational job to determine its priority of loading charges when same Charge code is found in multiple matching rates.
		/// Value Reference: 'CNE' (Consignee), 'CNR' (Consignor), 'SAG' (Sending Agent), 'RAG' (Receiving Agent), 'CCUS' (Controlling Customer), 'IB' (Import Broker), 'EB' (Export Broker), 'DA' (Delivery Agent), 'PA' (Pickup Agent), 'DTC' (Delivery Transport Company), 'PTC' (Pickup Transport Company), 'ICFS' (Import CFS), 'ECFS' (Export CFS), 'CA' (Controlling Agent), 'CCR' (Creditor), 'COR' (Creditor On Route), 'CAR' (Carrier), 'DCTO' (Departure CTO), 'DCTR' (Departure CTO On Route), 'DCFS' (Departure CFS), 'DCFT' (Departure CFS Transport), 'ACTO' (Arrival CTO), 'ACTR' (Arrival CTO On Route), 'ACFS' (Arrival CFS), 'ACFT' (Arrival CFS Transport)
		/// </summary>
		public string Role { get; set; }

		/// <summary>
		/// Match against the Organisation Code of CargoWise Organisation.
		/// Values Reference: CargoWise > Maintain > Master Data > Organisation > Code.
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Applicable Role Values.
		/// </summary>
		public static class Roles
		{
			/// <summary>
			/// Local Client
			/// </summary>
			public const string LC = nameof(RatingDebtorOrgTypes.LC);

			/// <summary>
			/// Agent
			/// </summary>
			public const string AG = nameof(RatingDebtorOrgTypes.AG);

			/// <summary>
			/// Consignee
			/// </summary>
			public const string CNE = nameof(RatingDebtorOrgTypes.CNE);

			/// <summary>
			/// Consignor
			/// </summary>
			public const string CNR = nameof(RatingDebtorOrgTypes.CNR);

			/// <summary>
			/// Sending Agent
			/// </summary>
			public const string SAG = nameof(RatingDebtorOrgTypes.SAG);

			/// <summary>
			/// Receiving Agent
			/// </summary>
			public const string RAG = nameof(RatingDebtorOrgTypes.RAG);

			/// <summary>
			/// Controlling Customer
			/// </summary>
			public const string CCUS = nameof(RatingDebtorOrgTypes.CCUS);

			/// <summary>
			/// Import Broker
			/// </summary>
			public const string IB = nameof(IB);

			/// <summary>
			/// Export Broker
			/// </summary>
			public const string EB = nameof(EB);

			/// <summary>
			/// Delivery Agent
			/// </summary>
			public const string DA = nameof(DA);

			/// <summary>
			/// Pickup Agent
			/// </summary>
			public const string PA = nameof(PA);

			/// <summary>
			/// Delivery Transport Company 
			/// </summary>
			public const string DTC = nameof(DTC);

			/// <summary>
			/// Pickup Transport Company
			/// </summary>
			public const string PTC = nameof(PTC);

			/// <summary>
			/// Import CFS (Container Freight Station)
			/// </summary>
			public const string ICFS = nameof(ICFS);

			/// <summary>
			/// Export CFS (Container Freight Station)
			/// </summary>
			public const string ECFS = nameof(ECFS);

			/// <summary>
			/// Controlling Agent
			/// </summary>
			public const string CA = nameof(CA);

			/// <summary>
			/// Creditor
			/// </summary>
			public const string CCR = nameof(CCR);

			/// <summary>
			/// Creditor On Route
			/// </summary>
			public const string COR = nameof(COR);

			/// <summary>
			/// Carrier
			/// </summary>
			public const string CAR = nameof(CAR);

			/// <summary>
			/// Departure CTO (Container Terminal Operator)
			/// </summary>
			public const string DCTO = nameof(DCTO);

			/// <summary>
			/// Departure CTO (Container Terminal Operator) On Route 
			/// </summary>
			public const string DCTR = nameof(DCTR);

			/// <summary>
			/// Departure CFS (Container Freight Station)
			/// </summary>
			public const string DCFS = nameof(DCFS);

			/// <summary>
			/// Departure CFS (Container Freight Station) Transport
			/// </summary>
			public const string DCFT = nameof(DCFT);

			/// <summary>
			/// Arrival CTO (Container Terminal Operator)
			/// </summary>
			public const string ACTO = nameof(ACTO);

			/// <summary>
			/// Arrival CTO (Container Terminal Operator) On Route
			/// </summary>
			public const string ACTR = nameof(ACTR);

			/// <summary>
			/// Arrival CFS (Container Freight Station)
			/// </summary>
			public const string ACFS = nameof(ACFS);

			/// <summary>
			/// Arrival CFS (Container Freight Station) Transport
			/// </summary>
			public const string ACFT = nameof(ACFT);

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { LC, AG, CNE, CNR, SAG, RAG, CCUS, IB, EB, DA, PA, DTC, PTC, ICFS, ECFS, CA, CCR, COR, CAR, DCTO, DCTR, DCFS, DCFT, ACTO, ACTR, ACFS, ACFT };
		}
	}
}
