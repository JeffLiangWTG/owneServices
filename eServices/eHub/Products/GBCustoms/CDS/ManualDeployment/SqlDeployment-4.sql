USE eHubTransactions

BEGIN TRAN

-- Renamed shared transforms as part of WI00216656.

UPDATE [dbo].[eHubTransformationType]
SET [TT_TransformationType] = 'CargoWise.eHub.Products.GBCustoms.Core.BT.Transforms.GBCustoms2GBCustoms, CargoWise.eHub.Products.GBCustoms.Core.BT.Transforms.GBCustoms2GBCustoms, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'
WHERE [TT_PK] = N'e498e197-6737-46b0-8bf9-f13b26b798f8'

UPDATE [dbo].[eHubTransformationType]
SET [TT_TransformationType] = 'CargoWise.eHub.Products.GBCustoms.Core.BT.Transforms.TransportResponse2UniversalEvent, CargoWise.eHub.Products.GBCustoms.Core.BT.Transforms.TransportResponse2UniversalEvent, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'
WHERE [TT_PK] = N'21F6B924-8438-455D-8C03-0899441DBE1C'

ROLLBACK
--COMMIT