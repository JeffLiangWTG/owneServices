create table edi.ConfigFirstMessageFilter
(
	MF_Category varchar(3) not null,
	MF_PriceItemCode varchar(3) not null,
	MF_RefIndex int not null,
	MF_RefValue varchar(50) not null,
	MF_Operator varchar(5) not null,
	constraint MF_Operator_Values CHECK(MF_Operator in ('!=')), -- add NOTIN, LIKE when supported
	constraint MF_RefIndex_Values CHECK(MF_RefIndex in (1,2,3,4,5))
)

go

create clustered index IX_ConfigFirstMessageFilter on edi.ConfigFirstMessageFilter(MF_Category, MF_PriceItemCode, MF_RefIndex, MF_Operator)