using System;
using System.Linq;
using System.Web;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class PackLineUpdateWSMethod : TrackingWebServiceMethod<PackLineUpdateParameters>
	{
		class PackLineValues
		{
			public ZInt Quantity;
			public ZDecimal Length;
			public ZDecimal Width;
			public ZDecimal Height;
			public ZString PackUD;
			public ZDecimal Volume;
			public ZString VolumeUQ;
		}

		protected override void ExecuteCore(PackLineUpdateParameters parameters, WebServiceResponse response)
		{
			var line = GetPackLine(parameters);

			if (line != null)
			{
				GenerateResponse(parameters, line, response);
			}
		}

		PackLineValues GetPackLineValues(PackLine line)
		{
			var values = new PackLineValues();
			values.Quantity = line.JL_PackageCount;
			values.Length = line.JL_Length;
			values.Width = line.JL_Width;
			values.Height = line.JL_Height;
			values.PackUD = line.JL_UnitOfDimension;
			values.Volume = line.JL_ActualVolume;
			values.VolumeUQ = line.JL_ActualVolumeUQ;

			return values;
		}

		void GenerateResponse(PackLineUpdateParameters parameters, PackLine line, WebServiceResponse response)
		{
			var initialValues = GetPackLineValues(line);
			UpdateValues(initialValues, parameters, line);

			var updatedValues = GetPackLineValues(line);
			GenerateResponse(initialValues, updatedValues, parameters, line, response);
		}

		void GenerateResponse(PackLineValues initialValues, PackLineValues updatedValues, PackLineUpdateParameters parameters, PackLine line, WebServiceResponse response)
		{
			AddUpdateToken(response, parameters.ModifiedControlID, parameters.VolumeControlID, initialValues.Volume, updatedValues.Volume, true);
		}

		void UpdateValues(PackLineValues initialValues, PackLineUpdateParameters parameters, PackLine line)
		{
			if (parameters.ModifiedControlID == parameters.QuantityControlID && HasNewIntValue(parameters.NewValue, initialValues.Quantity, out var quantity))
			{
				line.JL_PackageCount = quantity;
			}

			if (parameters.ModifiedControlID == parameters.LengthControlID && HasNewDecimalValue(parameters.NewValue, initialValues.Length, out var length))
			{
				line.JL_Length = length;
			}

			if (parameters.ModifiedControlID == parameters.WidthControlID && HasNewDecimalValue(parameters.NewValue, initialValues.Width, out var width))
			{
				line.JL_Width = width;
			}

			if (parameters.ModifiedControlID == parameters.HeightControlID && HasNewDecimalValue(parameters.NewValue, initialValues.Height, out var height))
			{
				line.JL_Height = height;
			}

			if (parameters.ModifiedControlID == parameters.PackUDControlID && initialValues.PackUD != parameters.NewValue)
			{
				line.JL_UnitOfDimension = parameters.NewValue;
			}

			if (parameters.ModifiedControlID == parameters.VolumeUQControlID && initialValues.VolumeUQ != parameters.NewValue)
			{
				line.JL_ActualVolumeUQ = parameters.NewValue;
			}
		}

		PackLine GetPackLine(PackLineUpdateParameters parameters)
		{
			var session = HttpContext.Current?.Session;
			if (session != null && Guid.TryParse(parameters.LineRef, out Guid linePK))
			{
				var shipment = GetShipment(session[parameters.ShipmentRef]);
				return shipment?.OuterPackLines.Where(l => l.PK == linePK).FirstOrDefault();
			}

			return null;
		}

		ForwardingShipment GetShipment(object data)
		{
			if (data is TrackingBooking booking)
			{
				return booking.QuotedBooking?.Booking;
			}

			return data as ForwardingShipment;
		}

		protected override bool AddErrorMessageToResponse() => false;

		protected override string GetMethodName() => "PackLineUpdate";

		protected override string GetScriptFileName() => "PackLineUpdateWSMethod.js";
	}
}
