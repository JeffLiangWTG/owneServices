using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class ContainerMovementValueObjectDataAdapter : ValueObjectDataAdapter<ContainerMovement, Xsd.ContainerMovement>
	{
		protected override void ExportToValueObjectCore(ContainerMovement movement, Xsd.ContainerMovement valueObject, IValueObjectExportContext context)
		{
			RefContainerStock stock = movement.Stock;
			if (stock != null)
			{
				valueObject.ContainerNum = stock.R6_ContainerNum;

				RefContainer type = stock.Container;
				if (type != null)
				{
					valueObject.ContainerType.ContainerCode = type.RC_Code;
					valueObject.ContainerType.ISOCode = type.RC_ISOType;

					var usContainerCode = type.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					if (usContainerCode.IsEmpty)
					{
						usContainerCode = type.RC_USContainerCode;
					}
					valueObject.ContainerType.USContainerCode = usContainerCode;

					valueObject.ContainerType.Length = type.RC_Length;
					valueObject.ContainerType.Width = type.RC_Width;
					valueObject.ContainerType.Height = type.RC_Height;
					valueObject.ContainerType.LengthSpecified = true;
					valueObject.ContainerType.WidthSpecified = true;
					valueObject.ContainerType.HeightSpecified = true;
				}

				valueObject.Stock.OwnerType = AgencyContainerMovementOwnerTypeMappings.Instance.GetExternalCode(stock.R6_OwnerType, string.Empty, context);
				if (stock.Owner != null)
				{
					valueObject.Stock.Owner = ExportOrganisation(stock.Owner, context);
				}
			}

			valueObject.MovementType = movement.E9_MovementType;
			valueObject.MovementDate = movement.E9_MovementDate;
			valueObject.IsContainerEmpty = movement.E9_ContainerIsEmpty;
			valueObject.IsContainerEmptySpecified = true;
			valueObject.ContainerCondition = movement.E9_ContainerCondition;

			if (movement.Depot != null)
			{
				valueObject.Depot = ExportOrganisation(movement.Depot.Header, context);
			}

			ExportVoyageRef(movement.Voyage, valueObject.Voyage);
			ExportBillsOfLading(valueObject, movement, context);
		}

		Xsd.Organisation ExportOrganisation(OrgHeader orgHeader, IValueObjectExportContext context)
		{
			return new OrganisationValueObjectDataAdapter().ExportToValueObject(orgHeader, context);
		}

		void ExportVoyageRef(JobVoyage voyage, Xsd.ContainerMovementVoyage valueObject)
		{
			if (voyage == null)
			{
				valueObject.IsSpecified = false;
			}
			else
			{
				valueObject.VoyageNo = voyage.JV_VoyageFlight;

				RefVessel vessel = voyage.Vessel;

				if (vessel != null)
				{
					valueObject.Vessel = vessel.RV_Name;
					valueObject.Lloyds = vessel.RV_LloydsNumber;
				}
			}
		}

		void ExportBillsOfLading(Xsd.ContainerMovement valueObject, ContainerMovement movement, IValueObjectExportContext context)
		{
			ZGuid voyagePK = movement.E9_JV;
			ZString containerNum = movement.Stock.R6_ContainerNum;

			AgencyShipmentValueObjectDataAdapter<BillOfLading> adapter = new AgencyShipmentValueObjectDataAdapter<BillOfLading>();
			adapter.ContainerFilter = (container) => container.JC_ContainerNum == containerNum;

			BillOfLading[] bills = movement.Factory.Load<BillOfLading>(BillOfLadingFilter(voyagePK, containerNum));

			foreach (BillOfLading bill in bills)
			{
				valueObject.RelatedBills.Add(adapter.ExportToValueObject(bill, context));
			}
		}

		protected override void ImportFromValueObjectCore(ContainerMovement bizObj, Xsd.ContainerMovement value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#region Implementation

		public override string RootElementName
		{
			get { return "ContainerMovement"; }
		}

		public override string RootCollectionElementName
		{
			get { return "ContainerMovements"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleContainerMovementSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ContainerMovementSchema; }
		}

		ZQuery BillOfLadingFilter(ZGuid voyagePK, ZString containerNum)
		{
			if (voyagePK.IsEmpty || containerNum.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}
			else
			{
				ZDBOnlySubQuery origin = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				origin.AddToFilter(JobVoyOriginSchema.JA_JV, voyagePK);

				ZDBOnlySubQuery sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
				sailing.AddSubQuery(origin, JoinCondition.And);

				ZDBOnlySubQuery container = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				container.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNum);

				ZDBOnlyQuery shipment = new ZDBOnlyQuery(typeof(AgencyShipment));
				shipment.AddSubQuery(sailing, JoinCondition.And);
				shipment.AddSubQuery(container, JoinCondition.And);

				return shipment;
			}
		}

		#endregion
	}
}


