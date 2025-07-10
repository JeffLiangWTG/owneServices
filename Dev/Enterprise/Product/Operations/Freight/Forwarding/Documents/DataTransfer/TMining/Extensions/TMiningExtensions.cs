using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	static class TMiningExtensions
	{
		#region ToUXmlContainer

		public static Container ToUXmlContainer(this SecureContainerReleaseContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.ContainerImportDORelease = source.ReleaseIdentification;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			container.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>
				{
					new AddInfo
					{
						Key = AddInfoKeyAction,
						Value = source.IsTranferToForwarder ? AddInfoValueActionForwarder : AddInfoValueActionTransporter
					}
				};

				return addInfos;
			});

			return container;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string AddInfoKeyAction = "Action";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string AddInfoValueActionForwarder = "Forwarder";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const value")]
		public const string AddInfoValueActionTransporter = "Transporter";

		#endregion
	}
}
