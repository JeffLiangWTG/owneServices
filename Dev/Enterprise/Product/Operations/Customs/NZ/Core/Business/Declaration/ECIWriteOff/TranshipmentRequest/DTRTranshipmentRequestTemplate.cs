using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.NZ.Business.Express
{
	[TestedAsNonPersistentBusinessObject]
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public sealed class DTRTranshipmentRequestTemplate : TranshipmentRequest
	{
		public static DTRTranshipmentRequestTemplate GetInstance(CusMAWB mAWB)
		{
			var rowFactory = (mAWB.Factory as IBusinessObjectFactoryInternals).RowFactory;
			var row = rowFactory.New(TranshipmentRequest.Schema.TableName);
			var result = new DTRTranshipmentRequestTemplate(mAWB.Factory, row);
			result.parent = new TranshipmentRequestParentWrappingCusMAWB(mAWB, result);
			result.isAirJob = true;
			return result;
		}

		public static DTRTranshipmentRequestTemplate GetInstance(CusSCAOceanBill oceanBill)
		{
			var rowFactory = (oceanBill.Factory as IBusinessObjectFactoryInternals).RowFactory;
			var row = rowFactory.New(TranshipmentRequest.Schema.TableName);
			var result = new DTRTranshipmentRequestTemplate(oceanBill.Factory, row);
			result.parent = new TranshipmentRequestParentWrappingCusSCAOceanBill(oceanBill, result);
			result.isAirJob = false;
			return result;
		}

		DTRTranshipmentRequestTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory => false;

		public TranshipmentRequest Generate()
		{
			var result = Clone(fCloneArgs) as TranshipmentRequest;
			result.C4_MovementReason = C4_MovementReason;
			return result;
		}

		public void Populate(TranshipmentRequest copyTarget)
		{
			copyTarget.CopyPersistentValuesFrom(this, fCloneArgs);
			copyTarget.C4_MovementReason = C4_MovementReason;
		}
		readonly BusinessObjectCloneArgs fCloneArgs = new BusinessObjectCloneArgs(new string[] { TranshipmentRequest.Schema.C4_ParentID, TranshipmentRequest.Schema.C4_ParentTableCode, TranshipmentRequest.Schema.C4_SendersMessageReference }, typeof(TranshipmentRequest));

		public override ZString C4_MovementReason { get => MovementReason.Codes.DomesticTranshipmentRequest; }

		public override bool MovementReason_ReadOnly => true;

		public override ITranshipmentRequestParent Parent => parent;
		ITranshipmentRequestParent parent;

		public override bool IsAirJob => isAirJob;
		bool isAirJob;

		public override bool IsSeaJob => !isAirJob;

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class TranshipmentRequestParentWrappingCusMAWB : NonPersistentBusinessObject, ITranshipmentRequestParent
		{
			public TranshipmentRequestParentWrappingCusMAWB(CusMAWB mAWB, DTRTranshipmentRequestTemplate template)
			{
				this.mAWB = mAWB;
				this.template = template;
			}
			readonly CusMAWB mAWB;
			readonly DTRTranshipmentRequestTemplate template;

			public ZBool IsImport => mAWB.IsImport;

			public ZBool IsExport => mAWB.IsExport;

			public bool IsTSWCREWriteOff => mAWB.IsTSWWriteOff;

			public bool IsTSWICRWriteOff => mAWB.IsICRMessage;

			public bool IsTranshipmentRequestRelevant => true;

			public ITransportParent TransportParent => null;

			public ZDateTime ArrivalDate => mAWB.CM_ArrivalDate;

			public ZDateTime DepartureDate => mAWB.CM_DepartureDate;

			public TranshipmentRequest TranshipmentRequest => template;

			public event EventHandler MessageTypeChanged { add { } remove { } }
			public event EventHandler MessageSubTypeChanged { add { } remove { } }
			public event EventHandler TransportModeChanged { add { } remove { } }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class TranshipmentRequestParentWrappingCusSCAOceanBill : NonPersistentBusinessObject, ITranshipmentRequestParent
		{
			public TranshipmentRequestParentWrappingCusSCAOceanBill(CusSCAOceanBill oceanBill, DTRTranshipmentRequestTemplate template)
			{
				this.oceanBill = oceanBill;
				this.template = template;
			}

			readonly CusSCAOceanBill oceanBill;
			readonly DTRTranshipmentRequestTemplate template;

			public ZBool IsImport => oceanBill.IsImport;

			public ZBool IsExport => oceanBill.IsExport;

			public bool IsTSWCREWriteOff => oceanBill.IsCREMessage;

			public bool IsTSWICRWriteOff => oceanBill.IsICRMessage;

			public bool IsTranshipmentRequestRelevant => true;

			public ITransportParent TransportParent => null;

			public ZDateTime ArrivalDate => oceanBill.CB_DateOfArrival;

			public ZDateTime DepartureDate => oceanBill.CB_DateOfDeparture;

			public TranshipmentRequest TranshipmentRequest => template;

			public event EventHandler MessageTypeChanged { add { } remove { } }
			public event EventHandler MessageSubTypeChanged { add { } remove { } }
			public event EventHandler TransportModeChanged { add { } remove { } }
		}
	}
}
