using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public abstract class XsdMovement : Xsd.Movement
	{
		public static Xsd.Movement FromPortEstimatedActualDates(BusinessObjectFactory factory, ZString port, ZDateTime estimatedDate, ZDateTime actualDate)
		{
			Xsd.Movement result = null;
			if (!port.IsEmpty)
			{
				result = new Xsd.Movement();
				result.Port = Xsd.UNLOCO.FromPortCode(factory, port);
				if (estimatedDate.IsValid)
				{
					result.EstimatedDateTime = estimatedDate.ToSmallDateTime().ToDateTime();
				}
				if (actualDate.IsValid)
				{
					result.ActualDateTime = actualDate.ToSmallDateTime().ToDateTime();
				}
			}
			return result;
		}

		public static void ToPortEstimatedActualDates(
			Xsd.Movement movement, ZPropertyInfo portInfo, ZPropertyInfo estimatedDateInfo, ZPropertyInfo actualDateInfo,
			string errorContext, IValueObjectImportContext context)
		{
			if (movement != null && movement.IsSpecified)
			{
				BusinessObjectFactory factory = context.Factory;

				context.SetPropertyInfoValue(portInfo, movement.Port.Value, ForeignKeyType.PortNK);
				if (movement.EstimatedDateTime.IsValid)
				{
					if (estimatedDateInfo == null)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("ad712e97-e7d4-460f-8a40-340c2f2e4e63", "{0} shouldn't have specified estimated date", errorContext)));
					}
					else if (!movement.EstimatedDateTime.IsValidSmallDateTime)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("5b41ff44-73ec-4307-b2db-09441a716393", "{0} specified estimated date time is out out of range.", errorContext)));
					}
					else
					{
						estimatedDateInfo.Value = movement.EstimatedDateTime.ToSmallDateTime();
					}
				}
				if (movement.ActualDateTime.IsValid)
				{
					if (actualDateInfo == null)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("09b9fb7d-ecfa-4f3e-81d5-193c8aa48858", "{0} shouldn't have specified actual date", errorContext)));
					}
					else if (!movement.ActualDateTime.IsValidSmallDateTime)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("60c71ddc-ebf4-4e68-9cb0-9dc6c454650a", "{0} specified actual date time is out out of range.", errorContext)));
					}
					else
					{
						actualDateInfo.Value = movement.ActualDateTime.ToSmallDateTime();
					}
				}
			}
		}
	}
}
