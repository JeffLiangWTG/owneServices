using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.PermitPrinting
{
	public class PrintPermit : NonPersistentBusinessObject, IObsoleteValidation, IVisualizerNoteSupporter
	{
		public PrintPermit(IPrintPermit inPrintPermit, BusinessObjectFactory factory, ZGuid? parentID = null)
			: base(factory)
		{
			this.inPrintPermit = inPrintPermit;
			this.parentID = parentID ?? ZGuid.Empty;
		}

		public IPrintPermit Permit
		{
			get { return inPrintPermit; }
		}
		readonly IPrintPermit inPrintPermit;
		readonly ZGuid parentID;

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => parentID;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;

		#endregion
	}

	public class PrintPermitConsignmentDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PrintPermitConsignmentDetails(IPrintPermitConsignment inConsignment, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inConsignment = inConsignment;
		}

		protected IPrintPermitConsignment inConsignment;

		public IPrintPermitConsignment Consignment
		{
			get { return inConsignment; }
		}

		public ZString SerialNb
		{
			get { return inConsignment.SerialNb; }
		}

		public ZString HSCode
		{
			get { return inConsignment.HSCode; }
		}

		public ZString BrandName
		{
			get { return inConsignment.BrandName; }
		}

		public ZString HSQuantity
		{
			get { return inConsignment.HSQuantity; }
		}

		public ZString Marking
		{
			get { return inConsignment.Marking; }
		}

		public ZString CityOfOrigin
		{
			get { return inConsignment.CityOfOrigin; }
		}

		public ZString Model
		{
			get { return inConsignment.Model; }
		}

		public ZString DutQuantity
		{
			get { return inConsignment.DutQuantity; }
		}

		public ZString InwardMawbObl
		{
			get { return inConsignment.InwardMawbObl; }
		}

		public ZString InwardHawbHbl
		{
			get { return inConsignment.InwardHawbHbl; }
		}

		public ZString OutwardMawbObl
		{
			get { return inConsignment.OutwardMawbObl; }
		}

		public ZString OutwardHawbHbl
		{
			get { return inConsignment.OutwardHawbHbl; }
		}

		public ZString GoodsDescription
		{
			get { return inConsignment.GoodsDescription; }
		}

		public ZDecimal UnitPrice
		{
			get { return inConsignment.UnitPrice; }
		}

		public ZString UnitPriceCurrency
		{
			get { return inConsignment.UnitPriceCurrency; }
		}

		public ZDecimal CustomsDutyPayable
		{
			get { return inConsignment.CustomsDutyPayable; }
		}

		public ZDecimal ExciseDutyPayable
		{
			get { return inConsignment.ExciseDutyPayable; }
		}

		public ZDecimal OtherTaxPayable
		{
			get { return inConsignment.OtherTaxPayable; }
		}

		public ZString CurrentLotNb
		{
			get { return inConsignment.CurrentLotNb; }
		}

		public ZString PreviousLotNb
		{
			get { return inConsignment.PreviousLotNb; }
		}

		public ZDecimal CifFobLspValue
		{
			get { return inConsignment.CifFobLspValue; }
		}

		public ZDecimal GstAmount
		{
			get { return inConsignment.GstAmount; }
		}

		public ZString CASCProductCode
		{
			get { return inConsignment.CASCProductCode; }
		}

		public ZDecimal CASCProductQty
		{
			get { return inConsignment.CASCProductQty; }
		}

		public ZString EngineNbChassisNb
		{
			get { return inConsignment.EngineNbChassisNb; }
		}

		public ICASCProductCode[] CASCProductCodes => inConsignment.CASCProductCodes ?? Array.Empty<ICASCProductCode>();

		public IEngineOrChassisNumber[] EngineOrChassisNumbers => inConsignment.EngineOrChassisNumbers ?? Array.Empty<IEngineOrChassisNumber>();
	}

	public class PrintPermitConditions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PrintPermitConditions(ICConditions inConditions, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inConditions = inConditions;
		}

		public ICConditions inConditions;
	}

	public class TN41PrintPermitConditions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TN41PrintPermitConditions(ITN41PermitConditions inConditions, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inConditions = inConditions;
		}

		public readonly ITN41PermitConditions inConditions;
	}

	public class PrintPermitContainers : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PrintPermitContainers(IPrintPermitContainers inContainers, BusinessObjectFactory factory)
			: base(factory)
		{
			this.inContainers = inContainers;
		}

		public IPrintPermitContainers inContainers;
	}
}
