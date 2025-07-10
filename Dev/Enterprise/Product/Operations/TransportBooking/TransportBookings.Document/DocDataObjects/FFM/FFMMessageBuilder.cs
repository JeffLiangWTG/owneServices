using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.TransportBookings.Document
{
	public sealed class FFMMessageBuilder
	{
		public FFMMessageBuilder(IFFMDetails ffmDetails, IContext context)
		{
			this.ffmDetails = Argument.NotNull(ffmDetails, nameof(ffmDetails));
			this.context = Argument.NotNull(context, nameof(context));
		}

		readonly IFFMDetails ffmDetails;
		readonly IContext context;

		public FFMMessage Build()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				var ffmMessage = new FFMMessage();

				ffmMessage.VoyageFlightNo = ffmDetails.VoyageFlightNo;
				ffmMessage.FlightDate = ffmDetails.FlightDate;
				ffmMessage.RegulatedAgentID = ffmDetails.RegulatedAgentID;
				ffmMessage.RegulatedAgentCountry = ffmDetails.RegulatedAgentCountry;
				ffmMessage.SecurityStatusCode = ffmDetails.SecurityStatusCode;

				ffmMessage.AirportOfDestinationCode = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = ffmDetails.AirportOfDestinationCode?.RL_Code ?? ZString.Empty
				};
				ffmMessage.PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = ffmDetails.PortOfLoading?.RL_Code ?? ZString.Empty
				};
				ffmMessage.DriverDocumentID = ffmDetails.DriverDocumentID;

				PopulatePackages(ffmMessage);

				AddValidation(ffmMessage);

				ffmMessage.ValidateAllIncludingChildren();
				return ffmMessage;
			}
		}

		void PopulatePackages(FFMMessage ffmMessage)
		{
			var ffmPackages = new List<FFMMessagePackage>();

			foreach (var package in ffmDetails.Packages)
			{
				var ffmPackage = new FFMMessagePackage()
				{
					ContainerNumber = package.ContainerNumber,
					WayBillNumber = package.WayBillNumber,
					GoodsDescription = package.GoodsDescription,
					Quantity = package.Quantity,
					Weight = package.Weight,
					WeightMetric = package.WeightMetric,
					Volume = package.Volume,
					VolumeMetric = package.VolumeMetric,
				};

				ffmPackage.PortOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = package.PortOfDestination?.RL_Code ?? ZString.Empty
				};
				ffmPackage.PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = package.PortOfLoading?.RL_Code ?? ZString.Empty
				};

				ffmPackages.Add(ffmPackage);
			}

			ffmMessage.Packages = ffmPackages;
		}

		void AddValidation(FFMMessage ffmMessage)
		{
			ValidateFlightInformation(ffmMessage);
			ValidateRegulatedAgent(ffmMessage);
			ValidatePortsForFFMMessage(ffmMessage);
			ValidatePackages(ffmMessage);
		}

		void ValidateFlightInformation(FFMMessage ffmMessage)
		{
			ffmMessage.VoyageFlightNoInfo.AddError(() => ffmMessage.VoyageFlightNo == ZString.Empty, ResString.GetMultilingualString("7c32fb5f-a5b3-4a74-a673-ad93fd52bc90", "Missing the Voyage Flight Number."));
			ffmMessage.FlightDateInfo.AddError(() => ffmMessage.FlightDate == ZDateTime.Empty, ResString.GetMultilingualString("f3165b49-4a54-45b5-a603-2f0a23086c06", "Missing the Flight Date."));
		}

		void ValidateRegulatedAgent(FFMMessage ffmMessage)
		{
			ffmMessage.RegulatedAgentIDInfo.AddError(() => ffmMessage.RegulatedAgentID == ZString.Empty, ResString.GetMultilingualString("573a6bba-be81-4b05-9906-c91945657cd5", "Missing the ID of the Regulated Agent."));
			ffmMessage.RegulatedAgentCountryInfo.AddError(() => ffmMessage.RegulatedAgentID == ZString.Empty, ResString.GetMultilingualString("baf61236-c45e-44d1-a26a-da21a8c77aa9", "Missing the Country of the Regulated Agent."));
		}

		void ValidatePortsForFFMMessage(FFMMessage ffmMessage)
		{
			if (ffmMessage.PortOfLoading != null && ffmMessage.PortOfLoading is Unloco portOfLoading)
			{
				portOfLoading.IATACodeInfo.AddError(() => portOfLoading.IATACode == ZString.Empty, ResString.GetMultilingualString("3a76ce6f-d452-4ecc-8a3b-33f00e952f93", "Missing The Port of Loading for this FFM Message."));
			}
			else
			{
				ffmMessage.AddRowError(ResString.GetMultilingualString("3a76ce6f-d452-4ecc-8a3b-33f00e952f93", "Missing The Port of Loading for this FFM Message."));
			}

			if (ffmMessage.AirportOfDestinationCode != null && ffmMessage.AirportOfDestinationCode is Unloco airportOfDestinationCode)
			{
				airportOfDestinationCode.IATACodeInfo.AddError(() => airportOfDestinationCode.IATACode == ZString.Empty, ResString.GetMultilingualString("4296b660-6427-4370-abc7-1c64cc7652be", "Missing The Airport Code for this FFM Message."));
			}
			else
			{
				ffmMessage.AddRowError(ResString.GetMultilingualString("4296b660-6427-4370-abc7-1c64cc7652be", "Missing The Airport Code for this FFM Message."));
			}
		}

		void ValidatePackages(FFMMessage ffmMessage)
		{
			if (ffmMessage.Packages == null || ffmMessage.Packages.Count <= 0)
			{
				ffmMessage.AddRowError(ResString.GetMultilingualString("fbe6f30b-063d-40ba-9ea6-0622219ac50d", "FFM message has no packages."));
				return;
			}

			foreach(var package in ffmMessage.Packages)
			{
				package.WayBillNumberInfo.AddError(() => package.WayBillNumber == ZString.Empty, ResString.GetMultilingualString("438a5d51-7839-45e3-95aa-9be0426035ee", "A package is missing its Way Bill Number."));
				package.GoodsDescriptionInfo.AddError(() => package.GoodsDescription == ZString.Empty, ResString.GetMultilingualString("b4f16fa8-504f-4172-ba4e-a647cefe7315", "A package is missing the descriptions for its goods."));

				package.QuantityInfo.AddError(() => package.Quantity <= ZInt.Zero, ResString.GetMultilingualString("bbf63846-1ce0-4e6a-b14c-b7f66bd87ffc", "A package's quantity must be set above zero."));

				package.VolumeInfo.AddError(() => package.Volume <= ZDecimal.Zero, ResString.GetMultilingualString("2d32463e-745f-426f-ab22-36c4e3cd8210", "A package's volume must be set above zero."));
				package.VolumeMetricInfo.AddError(() => package.VolumeMetric == ZString.Empty, ResString.GetMultilingualString("340db285-8b55-4085-a78e-f07f92f7e3b8", "A package's volume did not specify a metric."));

				package.WeightInfo.AddError(() => package.Weight <= ZInt.Zero, ResString.GetMultilingualString("60a0fb36-761d-4243-994a-02328c068650", "A package's weight must be set above zero."));
				package.WeightMetricInfo.AddError(() => package.WeightMetric == ZString.Empty, ResString.GetMultilingualString("f9fa4358-88da-4c91-b3ef-358cbeced2a7", "A package's weight did not specify a metric."));

				ValidatePackagesPortsDetails(package);
			}
		}

		void ValidatePackagesPortsDetails(FFMMessagePackage package)
		{
			if (package.PortOfLoading != null && package.PortOfLoading is Unloco portOfLoading)
			{
				portOfLoading.IATACodeInfo.AddError(() => portOfLoading.IATACode == ZString.Empty, ResString.GetMultilingualString("ecf2ac0e-2917-4575-b34e-5f686a9ce180", "Missing a package's Port of Loading."));
			}
			else
			{
				package.AddRowError(ResString.GetMultilingualString("ecf2ac0e-2917-4575-b34e-5f686a9ce180", "Missing a package's Port of Loading."));
			}

			if (package.PortOfDestination != null && package.PortOfDestination is Unloco portOfDestination)
			{
				portOfDestination.IATACodeInfo.AddError(() => portOfDestination.IATACode == ZString.Empty, ResString.GetMultilingualString("c9cc3c27-0d95-4bc6-b246-ceac00192369", "Missing a package's Port of Destination."));
			}
			else
			{
				package.AddRowError(ResString.GetMultilingualString("c9cc3c27-0d95-4bc6-b246-ceac00192369", "Missing a package's Port of Destination."));
			}
		}
	}
}
