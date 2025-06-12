CREATE TYPE [dbo].[TVP_BillingTransaction] AS TABLE
(
    [Category]          VARCHAR (3)      NOT NULL,
    [PriceItemCode]     VARCHAR (3)      NOT NULL,
    [BillableCount]     INT              NOT NULL,
    [ReportingSource]   VARCHAR (3)      NOT NULL,
    [ServiceOccuredUTC] DATETIME2 (7)    NOT NULL,
    [ClientID]          VARCHAR (9)      NOT NULL,
    [ClientNumber]      VARCHAR (50)     NULL,
    [ClientStaffCode]   VARCHAR (3)      NULL,
    [Reference1]        VARCHAR (50)     NOT NULL,
    [Reference2]        VARCHAR (50)     NULL,
    [Reference3]        VARCHAR (50)     NULL,
    [Reference4]        VARCHAR (50)     NULL,
    [Reference5]        VARCHAR (50)     NULL,
    [Version]           INT              DEFAULT ((0)) NOT NULL,
    [Branch]            VARCHAR (3)      NULL,
    [MessageTrackingID] VARCHAR (36)     NULL
)

