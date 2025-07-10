namespace Enterprise.Freight.Agency.Business
{
	using Enterprise.Freight.Common.Business;

	public abstract class ReleaseImportOrderShipmentValidation : JobShipmentValidation
	{
		protected ReleaseImportOrderShipmentValidation(BillOfLading parent)
			: base(parent) { }

		#region JS_NKLoadPort

		public void ValidateJS_NKLoadPort()
		{
			ValidateCalculatedProperty(Parent.JS_NKLoadPortInfo);
		}

		protected virtual void CheckJS_NKLoadPort()
		{
			if (Parent.Sailing == null)
			{
				Parent.JS_NKLoadPortInfo.AddMessageError(Res.GetString("4947de7b-67c0-46a9-9564-5401e9ab60a6", "No sailing selected."));
			}
		}

		#endregion

		#region JS_NKDischargePort

		public void ValidateJS_NKDischargePort()
		{
			ValidateCalculatedProperty(Parent.JS_NKDischargePortInfo);
		}

		protected virtual void CheckJS_NKDischargePort()
		{
			if (Parent.Sailing == null)
			{
				Parent.JS_NKDischargePortInfo.AddMessageError(Res.GetString("4947de7b-67c0-46a9-9564-5401e9ab60a6", "No sailing selected."));
			}
		}

		#endregion

		protected new BillOfLading Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLading)base.Parent; }
		}
	}
}
