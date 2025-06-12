CREATE FUNCTION edi.Base27Encode
(
	@Num INT
)
RETURNS VARCHAR(7) WITH SCHEMABINDING
AS
BEGIN
	DECLARE @EncodingCharSet CHAR(27) = 'BCDFGHJKMNPQRSTVWXYZ2345679';
	DECLARE @Text VARCHAR(7) = '';
	DECLARE @Done BIT = 0;
	WHILE @Done = 0
	BEGIN
		SET @Text = substring(@EncodingCharSet, (@Num % 27) + 1, 1) + @Text;
		SET @Num = @Num / 27;
		IF @Num = 0
			SET @Done = 1;
	END;

	RETURN @Text
END
