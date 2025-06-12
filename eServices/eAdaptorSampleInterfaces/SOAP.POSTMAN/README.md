# eAdapter Sample Requests - Postman
## Import
On the top-left of the Postman desktop client, click the "Import" button, and then choose the `eHubAdapter Sample Requests.postman_collection.json` file.

## Background
This Postman collection contains sample requests demonstrating how to interact with the eHubGateway web service using SOAP requests. Each request uses the HTTP header `SOAPAction` to determine which action the web service should perform.

## Postman Variables
Set `eHubAdapter Sample Requests` > `Variables` before you use:

- eHubURL - Ends in `.svc`. Change it to the test or production eHub URL as you need.

- SenderID / Password - The eHub credentials provided to you. You are the sender.

For the `SendStream` request, the following must also be set:

- RecipientID - The eHub ID of the recipient of the message.

- ApplicationCode - The application code of the message. `UDM` for Universal XML, `NDM` for Native XML.

- SchemaName - `http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange` for Universal XML, `http://www.cargowise.com/Schemas/Native#UniversalInterchange` for Native XML

## SendStream Message Format
Apart from the above variables, each eHubGateway `SendStreamRequest` `Payload` contains the following elements:

- Message - the XML content being sent. Native and Universal XML messages are
wrapped within a `UniversalInterchange`. The XML is then Gzip compressed and base64 encoded.

- TrackingID – This is an identifying GUID for the message. It's stored on the EDI Interchange within CargoWise.

- SchemaType – Always `Xml`

## Other Requests
- Ping - this request verifies that the server is healthy. It still requires you to be authenticated to return a success message.

- RetrieveStream - this will retrieve all the messages sent to you, including the response for your `SendStream` request. The message content is Gzip compressed and base64 encoded. There will be a `TrackingID` in the Header of `RetrieveStreamResponse` that you will use for `FinaliseBatch`.

- FinaliseBatch - After you `RetrieveStream`, send this request to let eHub know you have already downloaded the batch of messages, otherwise you will download the same batch of messages every time you `RetrieveStream`. Change `trackingID` in the body of the request to be the `TrackingID` you got from the Header of `RetrieveStreamResponse`.

## Powershell scripts
Sample Powershell scripts are provided:

- CompressAndEncode - for processing your own XML before you put it into `Message` in `SendStreamRequest` and send it.

- DecodeAndDecompress - for viewing the default `Message` in `SendStreamRequest`, and for viewing responses for `RetrieveStream`.
