using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class FTZMessageConveyanceObjectWrapper : IFTZConveyance
	{
		public FTZMessageConveyanceObjectWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public FTZMessageConveyanceObjectWrapper(JobDeclaration declaration, ITAndSplitDetails splitShipmentDetailsSelected)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.splitShipmentDetailsSelected = splitShipmentDetailsSelected;
		}
		readonly ITAndSplitDetails splitShipmentDetailsSelected;

		public FTZMessageConveyanceObjectWrapper(JobDeclaration declaration, Bill splitBillWithoutShipmentDetailsSelected)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.splitBillWithoutShipmentDetailsSelected = splitBillWithoutShipmentDetailsSelected;
		}
		readonly JobDeclaration declaration;
		readonly Bill splitBillWithoutShipmentDetailsSelected;

		ZString IFTZConveyance.TransportMode => declaration.IsODZ_AdmissionType ? ZString.Empty : declaration.JE_Calc_USTransportMode;

		ZString IFTZConveyance.CarrierSCAC
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.IsODZ_AdmissionType)
				{
					if (splitShipmentDetailsSelected != null)
					{
						result = splitShipmentDetailsSelected.US_CarrierCode;
					}
					else if (splitBillWithoutShipmentDetailsSelected != null)
					{
						result = ZString.Empty;
					}
					else
					{
						result = declaration.US_UI_NKCarrierSCAC;
					}
				}
				return result;
			}
		}

		ZString IFTZConveyance.ConveyanceName
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.IsODZ_AdmissionType)
				{
					if (splitShipmentDetailsSelected != null)
					{
						var carrierSCAC = splitShipmentDetailsSelected.US_CarrierCode;
						if (!carrierSCAC.IsEmpty)
						{
							result = splitShipmentDetailsSelected.Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, carrierSCAC))?.UI_Name ?? ZString.Empty;
						}
					}
					else if (splitBillWithoutShipmentDetailsSelected != null)
					{
						result = ZString.Empty;
					}
					else if (declaration.IsSea)
					{
						result = declaration.JE_VesselName;
					}
					else
					{
						var carrier = declaration.ImportingCarrier;
						result = carrier?.UI_Name ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		ZString IFTZConveyance.VoyageNumber
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.IsODZ_AdmissionType)
				{
					if (splitShipmentDetailsSelected != null)
					{
						result = splitShipmentDetailsSelected.US_FlightNumber;
					}
					else if (splitBillWithoutShipmentDetailsSelected != null)
					{
						result = ZString.Empty;
					}
					else
					{
						result = declaration.JE_VoyageFlightNo;
					}

					if (!result.IsEmpty && declaration.IsAir)
					{
						result = result.PadLeft(4, '0');
					}
				}
				return result;
			}
		}

		ZDate IFTZConveyance.ExportDate => declaration.IsODZ_AdmissionType ? ZDate.Empty : declaration.US_DateOfExport.Date;

		ZDate IFTZConveyance.ImportDate
		{
			get
			{
				var result = ZDate.Empty;

				if (!declaration.IsODZ_AdmissionType)
				{
					if (splitShipmentDetailsSelected != null)
					{
						result = splitShipmentDetailsSelected.US_ArrivalDate.Date;
					}
					else if (splitBillWithoutShipmentDetailsSelected != null)
					{
						result = ZDate.Empty;
					}
					else
					{
						result = declaration.JE_DateOfArrival.Date;
					}
				}
				return result;
			}
		}

		ZString IFTZConveyance.PortOfUnlading => declaration.IsODZ_AdmissionType ? ZString.Empty : declaration.US_SchDArrival;

		ZDate IFTZConveyance.EstimatedDateOfArrival
		{
			get
			{
				var result = ZDate.Empty;

				if (!declaration.IsODZ_AdmissionType)
				{
					if (splitShipmentDetailsSelected != null)
					{
						result = splitShipmentDetailsSelected.US_ArrivalDate.Date;
					}
					else
					{
						result = declaration.JE_DateOfArrival.Date;
					}
				}
				return result;
			}
		}

		IEnumerable<IFTZBill> IFTZConveyance.Bills
		{
			get
			{
				if (fFTZConveyanceBillsList == null)
				{
					fFTZConveyanceBillsList = new List<IFTZBill>();

					if (!declaration.IsODZ_AdmissionType)
					{
						if (splitShipmentDetailsSelected != null)
						{
							var splitShipmentDetailsSelectedObject = (IFTZBill)new FTZMessageSplitBillObjectWrapper(splitShipmentDetailsSelected);
							if (splitShipmentDetailsSelectedObject.Lines.Any())
							{
								fFTZConveyanceBillsList.Add(splitShipmentDetailsSelectedObject);
							}
						}
						else if (splitBillWithoutShipmentDetailsSelected != null)
						{
							var splitBillWithoutShipmentDetailsSelectedObject = (IFTZBill)new FTZMessageSplitBillObjectWrapper(splitBillWithoutShipmentDetailsSelected);
							if (splitBillWithoutShipmentDetailsSelectedObject.Lines.Any())
							{
								fFTZConveyanceBillsList.Add(splitBillWithoutShipmentDetailsSelectedObject);
							}
						}
						else
						{
							fFTZConveyanceBillsList.AddRange(((IFTZCommonHeader)declaration).Bills.Where(b => !((IBillDetails)b).IsSplit).Cast<IFTZBill>());
						}
					}
					else
					{
						fFTZConveyanceBillsList.AddRange(((IFTZCommonHeader)declaration).Bills.Cast<IFTZBill>());
					}
				}

				return fFTZConveyanceBillsList;
			}
		}
		List<IFTZBill> fFTZConveyanceBillsList;
	}
}
