SET NOCOUNT ON

--create the database
IF db_id('$(db_name)') IS NULL
BEGIN
	CREATE DATABASE [$(db_name)];
END

--enable selective index
IF (
	((SELECT CAST(SUBSTRING(CAST(SERVERPROPERTY('productversion') AS [varchar](100)), 1, 2) AS [tinyint])) > 10)
	AND (SELECT CAST(SUBSTRING(CAST(SERVERPROPERTY('productversion') AS [varchar](100)), 6, 2) AS [tinyint])) > 21
)
BEGIN EXECUTE
	sys.sp_db_selective_xml_index '$(db_name)', 'true'
END