use dw_ods20
go

sp_configure 'clr enabled', 1  
GO  
RECONFIGURE  
GO  

drop function if exists EPSG2193_to_GeoJSON
drop function if exists Any_to_GeoJSON
drop function if exists EPSG2193_Centroid_to_Latitude
drop function if exists EPSG2193_Centroid_to_Longitude

drop assembly if exists SQL_CLR_Assembly_Spatial_Reproject 

ALTER DATABASE dw_ods20 SET trustworthy ON
CREATE ASSEMBLY SQL_CLR_Assembly_Spatial_Reproject
FROM N'D:\SQLCLRLIBRARIES\SQL_CLR_Assembly_Spatial_Reproject.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE FUNCTION EPSG2193_to_GeoJSON(@Geometry_String nvarchar(max))
RETURNS nvarchar(max) 
AS
EXTERNAL NAME SQL_CLR_Assembly_Spatial_Reproject.SQL_CLR_Assembly_Spatial_Reproject.EPSG2193_to_GeoJSON
GO

CREATE FUNCTION Any_to_GeoJSON(@Geometry_String nvarchar(max),@Source_Coordinate_System_String nvarchar(max))
RETURNS nvarchar(max) 
AS
EXTERNAL NAME SQL_CLR_Assembly_Spatial_Reproject.SQL_CLR_Assembly_Spatial_Reproject.Any_to_GeoJSON
GO

CREATE FUNCTION EPSG2193_Centroid_to_Latitude(@Geometry_String nvarchar(max))
RETURNS nvarchar(max) 
AS
EXTERNAL NAME SQL_CLR_Assembly_Spatial_Reproject.SQL_CLR_Assembly_Spatial_Reproject.EPSG2193_Centroid_to_Latitude
GO

CREATE FUNCTION EPSG2193_Centroid_to_Longitude(@Geometry_String nvarchar(max))
RETURNS nvarchar(max) 
AS
EXTERNAL NAME SQL_CLR_Assembly_Spatial_Reproject.SQL_CLR_Assembly_Spatial_Reproject.EPSG2193_Centroid_to_Longitude
GO

use dw_ods20
select top 10 
dbo.EPSG2193_Centroid_to_Latitude(convert(varchar(max),shape.STCentroid())) as latitude
,dbo.EPSG2193_Centroid_to_Longitude(convert(varchar(max),shape.STCentroid())) as longitude
,dbo.EPSG2193_to_GeoJSON(convert(varchar(max),shape)) as geojson
from [dw_ods20].[geography].[hamilton_parcels]
