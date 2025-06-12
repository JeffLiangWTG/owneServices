CREATE TABLE edi.ClientMappingInterface
(
	Name varchar(50) not null,
	FirstCapturedUtc datetime2(0) not null
)

go

create unique clustered index UX_ClientMappingInterface on edi.ClientMappingInterface (FirstCapturedUtc, Name)

go


create unique index UX_ClientMappingInterface_Name on edi.ClientMappingInterface (Name)
