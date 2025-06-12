# Business Logic for Client Specific Development

## Shipment concepts

In order to map UniversalShipment xml to a 3rd party format, we need to understand the business logic of shipments at the form level and at the UniversalShipment level.
In CW1 shipment data is entered into the shipment or consol form which is saved to the database.
The shipment data is then exported to a UniversalShipment xml document.

### Consol (Consolidation)

You can find this form in CW1 by going: Operate -> Forwarding -> Forwarding -> Consolidation

Contains one or more shipments.

### Shipment

You can find this form in CW1 by going: Operate -> Forwarding -> Forwarding -> Shipments

A shipment!
Can exist by itself or as part of a consol.

### Container

You can find a form for containers in CW1 by going: Operate -> Forwarding -> Forwarding -> Containers
Note that the containers created here cannot be used in a consol.
Containers need to be created in the form of their consol.
You can however view containers that were created in consol forms.

Once a container is created, a shipment belonging to the same consol can assign a packing line to that container.

### PackingLine

A collection of goods to be shipped, fields include:
*   how the goods are packed together
*   how many there are
*   an optional container to put them in

Multiple Packinglines across different shipments can be assigned to the same container.

### CW1 level concept -> UniversalShipment xpath

*   Consol -> `UniversalShipment/Shipment`
*   Shipment -> `UniversalShipment/Shipment/SubShipmentCollection/SubShipment`
*   Container -> `UniversalShipment/Shipment/ContainerCollection/Container`, `UniversalShipment/Shipment/SubShipmentCollection/SubShipment/ContainerCollection/Container`
*   Packing line -> `UniversalShipment/Shipment/SubShipmentCollection/SubShipment/PackingLineCollection/PackingLine`

### Exported from shipment or consol

Shipments and Consols can both be exported to UniversalShipment and fill it out in similar ways.
However the consol has more data as it includes all of its shipments in `UniversalShipments/Shipment/SubShipmentCollection/SubShipment`.

To tell where a UniversalShipment was exported from, you can check the values in `UniversalShipment/Shipment/DataContext/DataSource/Type`:
*   ForwardingConsol - Export a Consol
*   ForwardingShipment - Export a Shipment that is not attached to a consol
*   ForwardingShipment and ForwardingConsol - Export a Shipment that is attached to a Consol

The "ForwardingConsol" case is similar to "ForwardingShipment and ForwardingConsol" case. In both cases the message contains the consol and shipments, with very small changes.
However the "ForwardingShipment" case is significantly different to the other cases, there is no consol, only one top level shipment.

## Create a Shipment and export it to UniversalShipment

1.  In CW1 Operate -> Forwarding -> Forwarding -> Shipments
2.  Click New
3.  In the new shipment window click Save
4.  Fix all validation issues then click save again. (Some fields let you type '=' to automatically input a valid value)
5.  Open the consignee company window by clicking the company name and pressing F3
6.  Save, Fix all validation issues, Save
7.  Details -> Config -> Edi Communications
8.  Enter the following in EDI Communication Modes
	1.  Module = SHP
	2.  Comm.Direction = TRX
	3.  File Format = XUS
9.  Enter the following in Mode
	1.  Comm. Transport = HUB
	2.  eHub Client ID = test
10. save and close returning to the shipment window
11. In shipment window, Actions -> Send Universal XML -> Universal Shipment (You will need to configure slightly differently above to send Universal Event)
12. set RecipientType to CNE
13. Send & Close
14. In shipment window -> workflow & tracking -> Events -> Data Export -> View -> Save To Disk

## Create a Consol and export it to UniversalShipment

1.  In CW1 Operate -> Forwarding -> Forwarding -> Consol
2.  Click New
3.  In the new consol window click Save
4.  Fix all validation issues then click save again. (Some fields let you type '=' to automatically input a valid value)
5.  Open the consignee company window by clicking the company name and pressing F3
6.  Save, Fix all validation issues, Save
7.  Details -> Config -> Edi Communications
8.  Enter the following in EDI Communication Modes
	1.  Module = CON
	2.  Comm.Direction = TRX
	3.  File Format = XUS
9.  Enter the following in Mode
	1.  Comm. Transport = HUB
	2.  eHub Client ID = test
10. save and close returning to the Consol window
11. Add any shipments you want via the Attach button.
12. In Consol window, Actions -> Send Universal XML -> Universal Shipment (You will need to configure slightly differently above to send Universal Event)
13. set RecipientType to CNE
14. Send & Close
15. In Consol window -> workflow & tracking -> Events -> Data Export -> View -> Save To Disk

## Export Customs Declaration to UniversalShipment

1.  In CW1 Operate -> Customs -> Customs -> Customs Declaration
2.  Click New
3.  In the new customs declaration window click Save
4.  Fix all validation issues then click save again. (Some fields let you type '=' to automatically input a valid value)
5.  Open the Importer company window by clicking the company name and pressing F3
6.  Save, Fix all validation issues, Save
7.  Details -> Config -> Edi Communications
8.  Enter the following in EDI Communication Modes
	1.  Module = BRK
	2.  Comm.Direction = TRX
	3.  File Format = XUS
9.  Enter the following in Mode
	1.  Comm. Transport = HUB
	2.  eHub Client ID = test
10. save and close returning to the shipment window
11. In shipment window, Actions -> Send Universal XML -> Universal Shipment
12. set RecipientType to CNE
13. Send & Close
14. In shipment window -> workflow & tracking -> Events -> Data Export -> View -> Save To Disk

## Export Account Receivables to UniversalTransaction

Receivables Transactions (AKA Account Receivables) is tied to a Customs Declaration and exports a lot data from it.
So if it looks like the client is exporting a Customs Declaration to a UniversalTransaction they are actually exporting a Receivables Transactions.

1.  In CW1 Maintain -> System -> Registry -> Accounting -> General Ledger Defaults -> Control Account -> Autoset all values using '='
2.  In CW1 Operate -> Customs -> Customs -> Customs Declaration
3.  Click New
4.  In the new customs declaration window click Save
5.  Billing -> Add a line
6.  Fix all validation issues then click save again. (Some fields let you type '=' to automatically input a valid value)
7.  In CW1 Manage -> Receivables -> Receivables Transactions
8.  Click Find
9.  Open the Receivables Transactions corresponding to your Customs Declaration
10.  Open the Debtor company window by clicking the company name and pressing F3
11.  Save, Fix all validation issues, Save
12.  Details -> Config -> Edi Communications
13.  Enter the following in EDI Communication Modes
	1.  Module = RNV
	2.  Comm.Direction = TRX
	3.  File Format = XUT
14. Enter the following in Mode
	1.  Comm. Transport = HUB
	2.  eHub Client ID = test
15. save and close returning to the Receivables Transactions window
16. In Receivables Transactions window, Actions -> Send Universal XML -> Universal Transaction 
17. set RecipientType to IDB
18. Send & Close
19. In Receivables Transactions window -> workflow & tracking -> Events -> Data Export -> View -> Save To Disk

## Use a company from a different country

CW1 forms will display differently depending on what country the current company is in.
In order to have the same form, as used by a specific customer, we will need to set CW1 to a company that is in the same country as the customer.

1.  CW1 -> Maintain -> Companies
2.  New
3.  set the country to the country you need
4.  set the code to an abbreviation of the country so you remember where this company is from.
5.  Fill in the remaining required fields
6.  Save
7.  CW1 -> Maintain -> Branches
8.  New
9.  Set the company to the one created earlier
10. Set the Country to the same country as the company created earlier.
11. Fill in remaining required fields
12. Save
13. CW1 -> Jump -> Options -> Change company, branch and department
14. Change to the company and branch you just created.
15. Ok
