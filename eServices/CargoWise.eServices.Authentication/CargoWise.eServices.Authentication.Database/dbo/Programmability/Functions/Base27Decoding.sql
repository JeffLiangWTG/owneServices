CREATE FUNCTION [dbo].[Base27Decode]
(
	@Text varchar(7)
)
RETURNS int with schemabinding
AS
BEGIN
	declare @n int = len(@Text);
	if @n = 0
		return -1;

	declare @EncodingCharSet char(27) = 'BCDFGHJKMNPQRSTVWXYZ2345679';
	declare @i int = 1;
	declare @digit int;
	declare @num int = 0;
	while @i <= @n
	begin
		declare @ch char(1) = substring(@Text, @i, 1);
		set @i = @i + 1
		if (ASCII(@ch) >= ASCII('a') and ASCII(@ch) <= ASCII('z'))
			set @ch = CHAR(ASCII(@ch) + ASCII('A') - ASCII('a'));
		set @digit = CHARINDEX(@ch, @EncodingCharSet) - 1;
		if (@digit = -1)
		begin
			set @num = -1;
			break;
		end;

		set @num = (@num * 27) + @digit
	end;

	RETURN @num;
END