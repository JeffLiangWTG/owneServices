using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// A helper class for determining transhipment ports. You can't do this in calculated properties for binding as it may be
	/// detrimental to performance.
	/// 
	/// Note that any data you obtain from an instance of this class doesn't refresh when data on the Shipment or it's consols changes.
	/// </summary>
	public class TranshipmentHelper
	{
		public TranshipmentHelper(CommonShipment shipment)
		{
			this.Shipment = shipment;
		}

		public readonly CommonShipment Shipment;

		#region ValidateShipment

		/// <summary>
		/// Validate that the Shipment is suitable for determining transhipments.
		/// </summary>
		public virtual string[] ValidateShipment()
		{
			ArrayList result = new ArrayList();
			result.AddRange(ValidateShipmentForAtLeast1ConsolBeImportAnd1BeExport());
			result.AddRange(ValidateShipmentForNoStrayConsols());
			result.AddRange(ValidateShipmentForConsolDatesNotOverlapping());
			return (string[])result.ToArray(typeof(string));
		}

		string[] ValidateShipmentForAtLeast1ConsolBeImportAnd1BeExport()
		{
			var result = new List<string>();

			if (ImportConsol == null && Shipment.IsCrossTrade())
			{
				result.Add(Res.GetString("7adba539-8d81-4125-af1a-87e748595cc7", "There are no import consols on shipment {0}", Shipment.JS_UniqueConsignRef));
			}

			if (ExportConsol == null && Shipment.IsCrossTrade())
			{
				result.Add(Res.GetString("13b76fdb-00c1-4055-8ac0-4760db106564", "There are no export consols on shipment {0}", Shipment.JS_UniqueConsignRef));
			}
			return result.ToArray();
		}

		string[] ValidateShipmentForNoStrayConsols()
		{
			var result = new List<string>();

			if (Shipment.IsCrossTrade())
			{
				foreach (CommonConsol consol in Shipment.Consols)
				{
					bool consolLinkFound = Shipment.Consols
						.Cast<CommonConsol>()
						.Any(c => consol.JK_RL_NKLoadPort == c.JK_RL_NKDischargePort
						|| consol.JK_RL_NKDischargePort == c.JK_RL_NKLoadPort);

					if (!consolLinkFound)
					{
						result.Add(Res.GetString("d13759ef-77dc-4011-83b9-6038b70d9225", "Consol {0} does not link to any other consol via it's port of loading or port of discharge.", consol.JK_UniqueConsignRef));
						break;
					}
				}
			}
			return result.ToArray();
		}

		string[] ValidateShipmentForConsolDatesNotOverlapping()
		{
			ArrayList result = new ArrayList();
			foreach (CommonConsol consol1 in Shipment.Consols)
			{
				foreach (CommonConsol consol2 in Shipment.Consols)
				{
					if (consol1.PK != consol2.PK &&
						consol1.JK_JX_JA_A_DEP.IsValid && consol1.JK_JX_JB_A_ARV.IsValid &&
						consol2.JK_JX_JA_A_DEP.IsValid && consol2.JK_JX_JB_A_ARV.IsValid)
					{
						if ((consol1.JK_JX_JA_A_DEP >= consol2.JK_JX_JA_A_DEP && consol1.JK_JX_JA_A_DEP <= consol2.JK_JX_JB_A_ARV) ||
							(consol1.JK_JX_JB_A_ARV >= consol2.JK_JX_JA_A_DEP && consol1.JK_JX_JB_A_ARV <= consol2.JK_JX_JB_A_ARV))
						{
							result.Add(Res.GetString("2d70ee28-a378-4047-a1ef-2033803c7586", "Consol {0} overlaps departure/arrival dates of consol {1}", consol1.JK_UniqueConsignRef, consol2.JK_UniqueConsignRef));
							break;
						}
					}
				}
				if (result.Count > 0)
				{
					break;
				}
			}
			return (string[])result.ToArray(typeof(string));
		}

		#endregion

		#region TranshipmentPorts

		public string[] TranshipmentPorts
		{
			get
			{
				if (fTranshipmentPorts == null)
				{
					fTranshipmentPorts = FindTranshipmentPorts();
				}
				return fTranshipmentPorts;
			}
		}
		string[] fTranshipmentPorts;

		string[] FindTranshipmentPorts()
		{
			ArrayList result = new ArrayList();
			for (int i = 0; i < ConsolsInShippingOrder.Length; i++)
			{
				CommonConsol consol = ConsolsInShippingOrder[i];

				Transport departureTransport = consol.Transports.DepartureTransport;
				Transport arrivalTransport = consol.Transports.ArrivalTransport;

				if (i != 0 &&
					(result.Count == 0 || result[result.Count - 1].ToString() != departureTransport.JW_RL_NKLoadPort))
				{
					result.Add((string)departureTransport.JW_RL_NKLoadPort);
				}
				if (i != ConsolsInShippingOrder.Length - 1 &&
					(result.Count == 0 || result[result.Count - 1].ToString() != arrivalTransport.JW_RL_NKDiscPort))
				{
					result.Add((string)arrivalTransport.JW_RL_NKDiscPort);
				}
			}
			return (string[])result.ToArray(typeof(string));
		}

		#endregion

		#region TranshipmentPortInCurrentCountry

		public string TranshipmentPortInCurrentCountry
		{
			get
			{
				string result = null;
				foreach (string port in TranshipmentPorts)
				{
					if (port.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					{
						result = port;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		#region ImportConsol

		public CommonConsol ImportConsol
		{
			get
			{
				if (importConsol == null)
				{
					importConsol = Shipment.Consols
						.Cast<CommonConsol>()
						.FirstOrDefault(c => c.IsImport());
				}

				return importConsol;
			}
		}
		CommonConsol importConsol;

		#endregion

		#region ExportConsol

		public CommonConsol ExportConsol
		{
			get
			{
				if (exportConsol == null)
				{
					exportConsol = Shipment.Consols
						.Cast<CommonConsol>()
						.FirstOrDefault(c => c.IsExport());
				}

				return exportConsol;
			}
		}
		CommonConsol exportConsol;

		#endregion

		#region Implementation

		#region ConsolsInShippingOrder

		protected CommonConsol[] ConsolsInShippingOrder
		{
			get
			{
				if (consolsInShippingOrder == null)
				{
					consolsInShippingOrder = (CommonConsol[])Shipment.Consols.ToArray(typeof(CommonConsol));
					MovementLegComparer.SortMovementLegsByPorts(consolsInShippingOrder);
				}
				return consolsInShippingOrder;
			}
		}
		CommonConsol[] consolsInShippingOrder;

		#endregion

		#endregion
	}
}
