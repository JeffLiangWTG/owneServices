using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	public class ISFDummyObject : DummyBusinessObject, IImporterSecurityFiling
	{
		public ISFDummyObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public virtual ZString JobReference => ZString.Empty;

		public virtual void UpdateBill(ZString billNumber, ZString status, ZDateTime statusDate)
		{
		}

		public virtual void UpdateAcceptedDate(ZDateTime acceptedDate)
		{
		}

		public virtual ZString SFSubmissionType => ZString.Empty;

		public virtual ZString ShipmentTypeCode => ZString.Empty;

		public virtual ZString IORNumberQualifier => ZString.Empty;

		public virtual ZString IORNumber => ZString.Empty;

		public virtual ZDate DateOfBirth => ZDate.Empty;

		public virtual ZString ModeOfTransportation => ZString.Empty;

		public virtual ZString SFTransactionNumber { get; set; }

		public virtual ZString SCAC => ZString.Empty;

		public virtual ZString ISFImporterBondHolder => ZString.Empty;

		public virtual ZBool ISFBondIndicator => ZBool.False;

		public virtual ZString CountryOfIssuance => ZString.Empty;

		public virtual ZString ImporterFullName => ZString.Empty;

		public virtual IEnumerable<IShipmentReferenceID> ShipmentIDs => null;

		public virtual IEnumerable<IReferenceData> ReferenceData => null;

		public virtual IEnumerable<IContainerData> ContainerData => null;

		public virtual ZString ConsigneeNumberQualifier => ZString.Empty;

		public virtual ZString ConsigneeNumber => ZString.Empty;

		public virtual ZString ConsigneeFullName => ZString.Empty;

		public virtual IEnumerable<IISFDocAddress> RelatedOrganizationData => null;

		public virtual IEnumerable<ITariffData> Tariffs => null;

		public virtual ZString CodeQualifier1 => ZString.Empty;

		public virtual ZString ForeignPortOfUnlading => ZString.Empty;

		public virtual ZString CodeQualifier2 => ZString.Empty;

		public virtual ZString PlaceOfDelivery => ZString.Empty;

		public virtual IEnumerable<IManufacturerData> ManufacturerData => null;

		public virtual ZString ShipmentSubType { get; set; }

		public virtual ZDecimal EstimatedValue { get; set; }

		public virtual ZDecimal EstimatedQuantity { get; set; }

		public virtual ZString UnitOfMeasure { get; set; }

		public virtual ZDecimal EstimatedWeight { get; set; }

		public virtual ZString WeightQualifier { get; set; }

		public virtual MasterFiles.Business.GlbBranch Branch => null;

		public virtual ZString MessageStatus { get; set; }

		public virtual CBPEDIMessageCollection Messages => null;

		public BusinessObject TopLevelBusinessObject => null;

		public ControllerID ControllerID => null;

		public Guid BusinessObjectPK => Guid.Empty;

		string IMessageAttachee.TopLevelBizObjReferenceNumber => "";

		public Logs TopLevelBusinessObjectLogs => null;

		public virtual ZString ActionReasonCode => ZString.Empty;

		public virtual ZString ISFBondActivityCode => ZString.Empty;

		public virtual ZString ISFBondType => ZString.Empty;

		public virtual ZString ConsigneePassportCountryOfIssue => ZString.Empty;

		public virtual ZDate ConsigneePassportDateOfBirth => ZDate.Empty;
	}
}
