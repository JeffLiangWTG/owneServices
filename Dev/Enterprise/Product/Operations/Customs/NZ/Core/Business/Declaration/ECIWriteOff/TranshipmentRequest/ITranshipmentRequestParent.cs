using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.NZ.Business
{
	public interface ITranshipmentRequestParent
	{
		ZGuid PK { get; }
		string TablePrefix { get; }
		BusinessObjectFactory Factory { get; }
		bool IsInDatabase { get; }
		ZBool IsImport { get; }
		ZBool IsExport { get; }
		bool IsTSWCREWriteOff { get; }
		bool IsTSWICRWriteOff { get; }
		bool IsTranshipmentRequestRelevant { get; }
		ITransportParent TransportParent { get; }
		ZDateTime ArrivalDate { get; }
		ZDateTime DepartureDate { get; }
		event EventHandler MessageTypeChanged;
		event EventHandler MessageSubTypeChanged;
		event EventHandler TransportModeChanged;
		TranshipmentRequest TranshipmentRequest { get; }
	}
}
