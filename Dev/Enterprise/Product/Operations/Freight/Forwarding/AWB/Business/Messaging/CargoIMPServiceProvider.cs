using System;
using System.Text;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class CargoIMPServiceProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Email address")]
		public const string CargoWiseCargoImpEmailAddress = "ccn-cargoimp-edi@edi.net.au";
		protected const char EOT = '\x0004';

		protected CargoIMPServiceProvider() { }

		#region Properties

		public ZString Code { get; internal set; }
		public ZString SystemAddress { get; internal set; }
		public ZString PriorityEnvelopeHeading { get; internal set; }
		public ZBool IsGoingViaCCN { get; internal set; }

		public static CargoIMPServiceProvider CCN
		{
			get { return fCCN ?? (fCCN = new CargoIMPServiceProvider() { Code = Core.Constants.AWB.CargoIMPServiceProviderConstants.CCN, SystemAddress = "CSGAGT85GHA", PriorityEnvelopeHeading = "QK", IsGoingViaCCN = true }); }
		}
		[ThreadStatic]
		static CargoIMPServiceProvider fCCN;

		public static CargoIMPServiceProvider Descartes
		{
			get { return fDescartes ?? (fDescartes = new CargoIMPServiceProvider() { Code = Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes, PriorityEnvelopeHeading = "QK" }); }
		}
		[ThreadStatic]
		static CargoIMPServiceProvider fDescartes;

		public static CargoIMPServiceProvider BTviaCCN
		{
			get { return fBTviaCCN ?? (fBTviaCCN = new BTViaCCNCargoIMPServiceProvider() { Code = Core.Constants.AWB.CargoIMPServiceProviderConstants.BTviaCCN, SystemAddress = "LONBCCR", PriorityEnvelopeHeading = "QP", IsGoingViaCCN = true }); }
		}
		[ThreadStatic]
		static CargoIMPServiceProvider fBTviaCCN;

		public static CargoIMPServiceProvider IATA
		{
			get { return fIATA ?? (fIATA = new IATACargoIMPServiceProvider() { Code = Core.Constants.AWB.CargoIMPServiceProviderConstants.IATA, SystemAddress = "LONBCCR", PriorityEnvelopeHeading = "QP" }); }
		}
		[ThreadStatic]
		static CargoIMPServiceProvider fIATA;

		#endregion

		public static CargoIMPServiceProvider Get(string code)
		{
			switch (code)
			{
				case Core.Constants.AWB.CargoIMPServiceProviderConstants.Descartes:
					return Descartes;

				case Core.Constants.AWB.CargoIMPServiceProviderConstants.BTviaCCN:
					return BTviaCCN;

				case Core.Constants.AWB.CargoIMPServiceProviderConstants.IATA:
					return IATA;

				default:
					return CCN;
			}
		}

		public string GetTransmissionMethodHeader(ExportAWBHeader aWB, EDIMessage message, CargoIMPTransmissionMethod method)
		{
			var builder = new StringBuilder();

			switch (method)
			{
				case CargoIMPTransmissionMethod.FTP:
					AppendFTPHeader(aWB, message, builder);
					break;
				case CargoIMPTransmissionMethod.eHub:
					AppendeHubHeader(aWB, message, builder);
					break;
				default:
					AppendStandardHeader(aWB, message, builder);
					break;
			}

			return builder.ToString();
		}

		#region Implementation

		protected virtual void AppendFTPHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			builder.Append(PriorityEnvelopeHeading);
			builder.Append(" ");
			builder.Append(SystemAddress);

			ZString airlineCode = GetAirlineCode(aWB);
			if (airlineCode.IsEmpty)
			{
				throw new CargoIMPApplicationException(Res.GetString("171fec50-10cd-4c70-81c5-4feca649649c", "Airline code is not found"));
			}

			builder.Append(airlineCode);
			builder.Append(message.EM_MessageType);
			builder.Append(aWB.EH_AWBOriginCode);

			builder.AppendLine();

			builder.Append(".");

			if (string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Value))
			{
				throw new CargoIMPApplicationException(Res.GetString("27d9d37f-6438-4bdd-9778-ce409560b7b0", "'{0}/{1}' is not set in registry",
					ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Category, ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Caption));
			}

			builder.Append(ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Value);
			builder.Append(" ");
			builder.Append(ZDateTime.UtcNow.ToString("ddHHmm", Culture.Invariant));

			builder.AppendLine();
		}

		protected virtual void AppendStandardHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			builder.Append(message.EM_MessageType);
			builder.AppendLine();

			ZString airlineCode = GetAirlineCode(aWB);
			if (airlineCode.IsEmpty)
			{
				throw new CargoIMPApplicationException(Res.GetString("171fec50-10cd-4c70-81c5-4feca649649c", "Airline code is not found"));
			}

			builder.Append(airlineCode);
			builder.Append(aWB.EH_AWBOriginCode);
			builder.AppendLine();
		}

		protected void AppendeHubHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			ZString airlineCode = GetAirlineCode(aWB);
			if (airlineCode.IsEmpty)
			{
				throw new CargoIMPApplicationException(Res.GetString("171fec50-10cd-4c70-81c5-4feca649649c", "Airline code is not found"));
			}
			var hawb = message.EM_MessageType == "FHL" ? message.EM_ApplicationReference.KeepAlphanumericCharacters() : ZString.Empty;
			var mawb = aWB.MasterBill;

			builder.Append(CargoIMPInterchangeGenerator.CreateHubHeader(message, PriorityEnvelopeHeading, airlineCode, hawb, mawb));
		}

		public static string GetAirlineCode(ExportAWBHeader aWB)
		{
			RefAirline airline = GetAirline(aWB);
			return airline != null ? airline.RM_TwoCharacterCode : ZString.Empty;
		}

		protected internal static RefAirline GetAirline(ExportAWBHeader aWB)
		{
			if (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Environment.Env.CurrentBranchPK, Guid.Empty))
			{
				return RefAirline.LoadFromAirlinePrefix(aWB.Factory, aWB.EH_AirlinePrefix);
			}
			else
			{
				RefAirline airline = null;

				if (!aWB.EH_By1st.IsEmpty)
				{
					airline = RefAirline.LoadFromAirline2LetterCode(aWB.Factory, aWB.EH_By1st);
				}

				if (airline == null && !aWB.EH_AirlinePrefix.IsEmpty)
				{
					airline = RefAirline.LoadFromAirlinePrefix(aWB.Factory, aWB.EH_AirlinePrefix);
				}

				return airline;
			}
		}

		protected static string GetAirlinePIMAAddress(ExportAWBHeader aWB)
		{
			RefAirline airline = GetAirline(aWB);
			ZString result = ZString.Empty;
			if (airline != null)
			{
				OrgHeader org = airline.GetCorrespondingCarrierOrganisation();
				if (org != null)
				{
					result = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.PIMAAddress);
				}
			}

			return result;
		}

		#endregion

		public ZString GetTransmissionMethodFooter(ExportAWBHeader aWB, EDIMessage message, CargoIMPTransmissionMethod method)
		{
			var builder = new StringBuilder();

			switch (method)
			{
				case CargoIMPTransmissionMethod.FTP:
					AppendFTPFooter(aWB, message.EM_MessageType, builder);
					break;
				case CargoIMPTransmissionMethod.eHub:
					AppendeHubFooter(aWB, message.EM_MessageType, builder);
					break;
				default:
					AppendStandardFooter(aWB, message.EM_MessageType, builder);
					break;
			}

			return builder.ToString();
		}

		protected virtual void AppendStandardFooter(ExportAWBHeader aWB, string messageType, StringBuilder builder)
		{
			builder.Append(EOT);
		}

		protected virtual void AppendFTPFooter(ExportAWBHeader aWB, string messageType, StringBuilder builder)
		{
			builder.Append(EOT);
		}

		protected virtual void AppendeHubFooter(ExportAWBHeader aWB, string messageType, StringBuilder builder)
		{
			builder.Append(CargoIMPInterchangeGenerator.CreateHubFooter());
		}
	}

	public class BTViaCCNCargoIMPServiceProvider : CargoIMPServiceProvider
	{
		const char SOH = '\x0001';
		const char STX = '\x0002';

		protected override void AppendFTPHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			builder.AppendLine();
			builder.Append(SOH);
			builder.Append(PriorityEnvelopeHeading);
			builder.Append(" ");
			builder.Append(SystemAddress);
			builder.AppendLine();

			builder.Append(".");
			if (string.IsNullOrEmpty(ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Value))
			{
				throw new CargoIMPApplicationException(Res.GetString("27d9d37f-6438-4bdd-9778-ce409560b7b0", "'{0}/{1}' is not set in registry",
					ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Category, ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Caption));
			}

			builder.Append(ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Value);
			builder.Append(" ");
			builder.Append(ZDateTime.UtcNow.ToString("ddHHmm", Culture.Invariant));
			builder.Append(" ");
			ZString pima = GetAirlinePIMAAddress(aWB);
			if (pima.IsEmpty)
			{
				throw new CargoIMPApplicationException(Res.GetString("9a2e5903-fde4-498c-83b9-ed8dcf4ff343", "Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline"));
			}

			builder.Append(pima);
			builder.AppendLine();
			builder.Append(STX);
		}

		protected override void AppendFTPFooter(ExportAWBHeader aWB, string messageType, StringBuilder builder)
		{
			builder.Append((NoResString)"\x03\r\n\n\n\x04\n");
		}
	}

	public class IATACargoIMPServiceProvider : CargoIMPServiceProvider
	{
		protected override void AppendStandardHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			AppendFTPHeader(aWB, message, builder);
		}

		protected override void AppendFTPHeader(ExportAWBHeader aWB, EDIMessage message, StringBuilder builder)
		{
			string senderPIMAAddress = ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Value;
			if (string.IsNullOrEmpty(senderPIMAAddress))
			{
				throw new CargoIMPApplicationException(Res.GetString("27d9d37f-6438-4bdd-9778-ce409560b7b0", "'{0}/{1}' is not set in registry",
					ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Category, ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.Caption));
			}

			ZString airlinePIMAAddress = GetAirlinePIMAAddress(aWB);
			if (airlinePIMAAddress.IsEmpty)
			{
				throw new CargoIMPApplicationException(Res.GetString("9a2e5903-fde4-498c-83b9-ed8dcf4ff343", "Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline"));
			}

			builder.Append(PriorityEnvelopeHeading);
			builder.Append(" ");
			builder.Append(SystemAddress);
			builder.AppendLine();

			builder.Append(".");
			builder.Append(senderPIMAAddress);
			builder.Append(" ");
			builder.Append(ZDateTime.UtcNow.ToString("ddHHmm", Culture.Invariant));
			builder.Append(" ");
			builder.Append(airlinePIMAAddress);
			builder.AppendLine();
		}
	}
}
