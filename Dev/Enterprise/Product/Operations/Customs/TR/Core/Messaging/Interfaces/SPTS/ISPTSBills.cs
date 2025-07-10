using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface ISPTSBills
	{
		///<summary>
		/// Xml Tag: TasimaSenediNumarasi
		///</summary
		ZString BillNumber { get; }

		///<summary>
		/// Xml Tag: TasimaSenediSiraNumarasi
		///</summary
		ZInt BillOrderNo { get; }

		///<summary>
		/// Xml Tag: BeyanTuru
		///</summary
		ZString DeclarationType { get; }

		///<summary>
		/// Xml Tag: BeyanNumarasi
		///</summary
		ZString DeclarationNo { get; }

		///<summary>
		/// Xml Tag: ParcaliMi
		///</summary
		ZString IsSubType { get; }

		///<summary>
		/// Xml Tag: AktarmaSatirlari
		///</summary
		IEnumerable<ISPTSBillLines> SPTSBillLines { get; }
	}
}
