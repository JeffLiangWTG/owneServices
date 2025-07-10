using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public DeclarationXmlValueObjectSerializer()
			: base(typeof(Xsd.Consol))
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter iDataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			DeclarationValueObjectDataAdapter dataAdapter = (DeclarationValueObjectDataAdapter)iDataAdapter;
			Xsd.Consol consol = (Xsd.Consol)valueObject;

			foreach (Xsd.Shipment shipment in consol.Shipments)
			{
				Xsd.ConsolAndShipment consolAndShipment = new Xsd.ConsolAndShipment();
				consolAndShipment.Consol = consol;
				consolAndShipment.Shipment = shipment;
				isDeclarationRejected = false;

				Xsd.MasterAndHouseBill[] masterAndHouseBills = consolAndShipment.Shipment.GetMasterAndHouseBillIdentifiers(consolAndShipment.Consol);
				BaseJobDeclaration[] existingDeclarations = dataAdapter.FindJobDeclarations(consolAndShipment, context);

				if (existingDeclarations.Length == 0)
				{
					CreateOrUpdateDeclaration(collection, dataAdapter, collection.Factory.New<BaseJobDeclaration>(), consolAndShipment, context);
					declarationCreatedUpdatedRejected += System.Environment.NewLine + context.LastNotificationMessage;
				}
				else
				{
					string masterAndHouseBillsAsString = Xsd.MasterAndHouseBill.GetMasterAndHouseBillsAsString(masterAndHouseBills);

					if (!AllowDeclarationUpdate)
					{
						context.Notify(new InfoNotification(Res.GetString("723dd3e2-f9b0-46ed-993f-2b8749f3c673", "Declaration with master/house bill = {0} exists in the system. However, it will not be updated because: Registry > System > Data Import Settings > Customs Declaration > Allow Customs Declaration Update is set to 'No'", masterAndHouseBillsAsString) + "\r\n"));
						declarationCreatedUpdatedRejected += "\r\n\r\n" + Res.GetString("2acd5e7b-deab-4f3b-ab96-0acdff58a123", "Declaration {0} update rejected, update is not allowed", existingDeclarations[0].JE_DeclarationReference);

						isDeclarationRejected = true;
					}
					else
					{
						foreach (BaseJobDeclaration declaration in existingDeclarations)
						{
							if (HasSentToCustoms(declaration))
							{
								context.Notify(new InfoNotification(Res.GetString("ecf8bd22-ae3b-462e-8c64-158289aecc0c", "Declaration with master/house bill = {0} has CES Event(s) and therefore it will not be updated", masterAndHouseBillsAsString)));
								declarationCreatedUpdatedRejected += "\r\n" + Res.GetString("094713cc-2bed-48b2-a27e-45e4dd582307", "Declaration {0} update rejected, Customs messaging has commenced", existingDeclarations[0].JE_DeclarationReference);
								isDeclarationRejected = true;
							}
							else if (declaration.JE_IsCancelled)//declaration is loaded in two ways, by using master bill + house bill and agent reference. Searching by master bill + house bill, system excludes cancelled declarations, but by agent reference, it searches all declarations matching the number.
							{
								context.Notify(new InfoNotification(Res.GetString("5a610a24-7af1-4698-a163-038c9313ced1", "Declaration {0} cannot be updated as it has been inactivated.", declaration.JE_DeclarationReference)));
							}
							else
							{
								CreateOrUpdateDeclaration(collection, dataAdapter, declaration, consolAndShipment, context);
								declarationCreatedUpdatedRejected += System.Environment.NewLine + context.LastNotificationMessage;
							}
						}
					}
				}
			}
			return null;
		}

		void CreateOrUpdateDeclaration(IBusinessObjectCollection collection, DeclarationValueObjectDataAdapter dataAdapter, BaseJobDeclaration declaration, Xsd.ConsolAndShipment consolAndShipment, IValueObjectImportContext context)
		{
			dataAdapter.ImportFromValueObject(declaration, consolAndShipment, context);
			collection.Add(declaration);
		}

		bool HasSentToCustoms(BaseJobDeclaration jobDec)
		{
			return jobDec.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus) != null;
		}

		bool AllowDeclarationUpdate
		{
			get { return SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.Value; }
		}

		public bool IsDeclarationRejected
		{
			get { return isDeclarationRejected; }
		}
		bool isDeclarationRejected;

		public ZString GetDeclarationImportStatuses()
		{
			ZString declarationStatuses = declarationCreatedUpdatedRejected;
			declarationCreatedUpdatedRejected = "";
			return declarationStatuses;
		}
		ZString declarationCreatedUpdatedRejected;
	}
}
