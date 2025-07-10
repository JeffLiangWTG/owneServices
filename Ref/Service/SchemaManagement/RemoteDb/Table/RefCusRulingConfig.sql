CREATE TABLE RefCusRulingConfig
(
[ZZY_PK] UNIQUEIDENTIFIER NOT NULL,
[ZZY_Category] CHAR(3) NOT NULL,
[ZZY_Type] VARCHAR(15) NOT NULL,
[ZZY_Rate] DECIMAL(9,3) NOT NULL CONSTRAINT [DF_RefCusRulingConfig_ZZY_Rate] DEFAULT 0,
[ZZY_Value] VARCHAR(35) NOT NULL CONSTRAINT [DF_RefCusRulingConfig_ZZY_Value] DEFAULT '',
[ZZY_ZZX_CusRuling] UNIQUEIDENTIFIER NOT NULL,

CONSTRAINT [PK_RefCusRulingConfig] PRIMARY KEY CLUSTERED( [ZZY_PK] ASC ),
CONSTRAINT [FK_RefCusRulingConfig_ZZY_ZZX_CusRuling] FOREIGN KEY([ZZY_ZZX_CusRuling]) REFERENCES [RefCusRuling] ([ZZX_PK]),
CONSTRAINT [CK_RefCusRulingConfig_ZZY_Type] CHECK ([ZZY_Type] <>''), 
CONSTRAINT [CK_RefCusRulingConfig_ZZY_Category] CHECK ([ZZY_Category]='DTY' OR [ZZY_Category]='EXC' OR [ZZY_Category]='SIM' OR [ZZY_Category]='GST'),
CONSTRAINT [CK_RefCusRulingConfig_ZZY_Rate] CHECK ([ZZY_Rate] >= 0),
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusRulingConfig_ZZY_ZZX_CusRuling ON RefCusRulingConfig (ZZY_ZZX_CusRuling)
GO
