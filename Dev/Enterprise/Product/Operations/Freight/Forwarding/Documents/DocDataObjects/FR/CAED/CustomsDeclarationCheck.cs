using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CustomsDeclarationCheck : DocDataObject, IDataSourceProvider
	{
		public CustomsDeclarationCheck(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		#endregion

		#region Import / Export

		public ZBool IsImport
		{
			get => isImport;
			set
			{
				if (SetNonPersistentPropertyValue(IsImportInfo, ref isImport, value))
				{
					Validate(IsImportInfo);
				}
			}
		}
		ZBool isImport;

		public ZPropertyInfo IsImportInfo => GetZPropertyInfo(nameof(IsImport));

		#endregion

		#region Addresses

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		IAddress currentUser;

		#endregion

		#region SendingForwarderAddress

		public IAddress SendingForwarderAddress
		{
			get => sendingForwarderAddress;
			set => sendingForwarderAddress = SetChild(sendingForwarderAddress, value);
		}

		IAddress sendingForwarderAddress;

		#endregion

		#region ReceivingPartyAddress

		public IAddress ReceivingForwarderAddress
		{
			get => receivingForwarderAddress;
			set => receivingForwarderAddress = SetChild(receivingForwarderAddress, value);
		}

		IAddress receivingForwarderAddress;

		#endregion

		#region ExportBrokerAddress

		public IAddress ExportBrokerAddress
		{
			get => exportBrokerAddress;
			set => exportBrokerAddress = SetChild(exportBrokerAddress, value);
		}

		IAddress exportBrokerAddress;

		#endregion

		#region ImportBrokerAddress

		public IAddress ImportBrokerAddress
		{
			get => importBrokerAddress;
			set => importBrokerAddress = SetChild(importBrokerAddress, value);
		}

		IAddress importBrokerAddress;

		#endregion

		#region DepartureCTOAddress

		public IAddress DepartureCTOAddress
		{
			get => departureCTOAddress;
			set => departureCTOAddress = SetChild(departureCTOAddress, value);
		}

		IAddress departureCTOAddress;

		#endregion

		#region ArrivalCTOAddress

		public IAddress ArrivalCTOAddress
		{
			get => arrivalCTOAddress;
			set => arrivalCTOAddress = SetChild(arrivalCTOAddress, value);
		}

		IAddress arrivalCTOAddress;

		#endregion

		#endregion

		#region SendingPartySONCode

		public RegistrationNumber SendingPartySONCode
		{
			get => sendingPartySONCode;
			set => sendingPartySONCode = SetChild(sendingPartySONCode, value);
		}

		RegistrationNumber sendingPartySONCode;

		#endregion SendingPartyCI5Code

		#region SendingPartyCI5Code

		public RegistrationNumber SendingPartyCI5Code
		{
			get => sendingPartyCI5Code;
			set => sendingPartyCI5Code = SetChild(sendingPartyCI5Code, value);
		}

		RegistrationNumber sendingPartyCI5Code;

		#endregion SendingPartyCI5Code

		#region CTOSONCode

		public RegistrationNumber CTOSONCode
		{
			get => ctoSONCode;
			set => ctoSONCode = SetChild(ctoSONCode, value);
		}

		RegistrationNumber ctoSONCode;

		#endregion CTOCI5Code

		#region CTOCI5Code

		public RegistrationNumber CTOCI5Code
		{
			get => ctoCI5Code;
			set => ctoCI5Code = SetChild(ctoCI5Code, value);
		}

		RegistrationNumber ctoCI5Code;

		#endregion CTOCI5Code

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region TotalNumberOfPacks

		public ZInt TotalNumberOfPacks
		{
			get => totalNumberOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNumberOfPacksInfo, ref totalNumberOfPacks, value))
				{
					Validate(TotalNumberOfPacksInfo);
				}
			}
		}
		ZInt totalNumberOfPacks;

		public ZPropertyInfo TotalNumberOfPacksInfo => GetZPropertyInfo(nameof(TotalNumberOfPacks));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}
		ICodeDescription packageType;

		#endregion

		#region CommonAccessRef

		public ZString CommonAccessRef
		{
			get => commonAccessRef;
			set
			{
				if (SetNonPersistentPropertyValue(CommonAccessRefInfo, ref commonAccessRef, value))
				{
					Validate(CommonAccessRefInfo);
				}
			}
		}
		ZString commonAccessRef;

		public ZPropertyInfo CommonAccessRefInfo => GetZPropertyInfo(nameof(CommonAccessRef));

		#endregion

		#region AppliesToAllPacks

		public ZBool AppliesToAllPacks
		{
			get => appliesToAllPacks;
			set
			{
				if (SetNonPersistentPropertyValue(AppliesToAllPacksInfo, ref appliesToAllPacks, value))
				{
					Validate(AppliesToAllPacksInfo);
				}
			}
		}
		ZBool appliesToAllPacks;

		public ZPropertyInfo AppliesToAllPacksInfo => GetZPropertyInfo(nameof(AppliesToAllPacks));

		#endregion

		#region CustomsOfficeCode

		public ICodeDescription CustomsOfficeCode
		{
			get => customsOfficeCode;
			set => customsOfficeCode = SetChild(customsOfficeCode, value);
		}

		ICodeDescription customsOfficeCode;

		#endregion

		#region DeclarationType

		public ZString DeclarationType
		{
			get => declarationType;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationTypeInfo, ref declarationType, value))
				{
					Validate(DeclarationTypeInfo);
				}
			}
		}
		ZString declarationType;

		public ZPropertyInfo DeclarationTypeInfo => GetZPropertyInfo(nameof(DeclarationType));

		#endregion

		#region DeclarationFileNumber

		public ZString DeclarationFileNumber
		{
			get => declarationFileNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationFileNumberInfo, ref declarationFileNumber, value))
				{
					Validate(DeclarationFileNumberInfo);
				}
			}
		}
		ZString declarationFileNumber;

		public ZPropertyInfo DeclarationFileNumberInfo => GetZPropertyInfo(nameof(DeclarationFileNumber));

		#endregion

		#region DeclarantsSIRETNumber

		public RegistrationNumber DeclarantsSIRETNumber
		{
			get => declarantsSIRETNumber;
			set => declarantsSIRETNumber = SetChild(declarantsSIRETNumber, value);
		}

		RegistrationNumber declarantsSIRETNumber;

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		#endregion

		#region ContainerMessage

		public ZString ContainerMessage
		{
			get => containerMessage;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerMessageInfo, ref containerMessage, value))
				{
					Validate(ContainerMessageInfo);
				}
			}
		}

		ZString containerMessage;

		public ZPropertyInfo ContainerMessageInfo => GetZPropertyInfo(nameof(ContainerMessage));

		#endregion

		#region Port

		public IUnloco Port
		{
			get => port;
			set => port = SetChild(port, value);
		}

		IUnloco port;

		#endregion

		#region PortDuesAmount

		public ZDecimal PortDuesAmount
		{
			get => portDuesInfo;
			set
			{
				if (SetNonPersistentPropertyValue(PortDuesAmountInfo, ref portDuesInfo, value))
				{
					Validate(PortDuesAmountInfo);
				}
			}
		}
		ZDecimal portDuesInfo;

		public ZPropertyInfo PortDuesAmountInfo => GetZPropertyInfo(nameof(PortDuesAmount));

		#endregion

		#region PortDuesCurrency

		public ICodeDescription PortDuesCurrency
		{
			get => portDuesCurrency;
			set => portDuesCurrency = SetChild(portDuesCurrency, value);
		}

		ICodeDescription portDuesCurrency;

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion
	}
}
