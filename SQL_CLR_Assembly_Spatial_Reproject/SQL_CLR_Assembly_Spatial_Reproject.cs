using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System;

public class SQL_CLR_Assembly_Spatial_Reproject
{
    public static string EPSG2193 = "+proj=tmerc +lat_0=0 +lon_0=173 +k=0.9996 +x_0=1600000 +y_0=10000000 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs";
    public static string EPSG4126 = "+proj=longlat +datum=WGS84 +no_defs";

    [SqlFunction(Name = "EPSG2193_to_GeoJSON")]
    public static SqlChars EPSG2193_to_GeoJSON(string Geometry_String)
    {
        return new SqlChars(Convert_To_GeoJSON(Geometry_String, EPSG2193));
    }
        
    [SqlFunction(Name = "Any_to_GeoJSON")]
    public static SqlChars Any_to_GeoJSON(string Geometry_String, string Source_Coordinate_System_String)
    {
        return new SqlChars(Convert_To_GeoJSON(Geometry_String, Source_Coordinate_System_String));
    }

    [SqlFunction(Name = "EPSG2193_Centroid_to_Latitude")]
    public static SqlChars EPSG2193_Centroid_to_Latitude(string Geometry_String)
    {
        string Destination_Coordinate_System_String = EPSG4126;
        String Geometry_Type = Geometry_String.Substring(0, Geometry_String.IndexOf(" "));

        switch (Geometry_Type)
        {
            case "POINT":
                Geometry_Type = "Point";
                break;
            default:
                Geometry_Type = "not_supported";
                return null;
        }

        Geometry_String = Geometry_String.Remove(0, Geometry_String.IndexOf('('));
        Geometry_String = Geometry_String.Replace("(", "");
        Geometry_String = Geometry_String.Replace(")", "");

        double[] xy = Array.ConvertAll(Geometry_String.Split(' '), double.Parse);
        double[] z = new double[1];
        z[0] = 0;

        DotSpatial.Projections.ProjectionInfo src = DotSpatial.Projections.ProjectionInfo.FromProj4String(EPSG2193);
        DotSpatial.Projections.ProjectionInfo trg = DotSpatial.Projections.ProjectionInfo.FromProj4String(EPSG4126);
        DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, src, trg, 0, 1);

        return new SqlChars(xy[1].ToString());
    }

    [SqlFunction(Name = "EPSG2193_Centroid_to_Longitude")]
    public static SqlChars EPSG2193_Centroid_to_Longitude(string Geometry_String)
    {
        string Destination_Coordinate_System_String = EPSG4126;
        String Geometry_Type = Geometry_String.Substring(0, Geometry_String.IndexOf(" "));

        switch (Geometry_Type)
        {
            case "POINT":
                Geometry_Type = "Point";
                break;
            default:
                Geometry_Type = "not_supported";
                return null;
        }

        Geometry_String = Geometry_String.Remove(0, Geometry_String.IndexOf('('));
        Geometry_String = Geometry_String.Replace("(", "");
        Geometry_String = Geometry_String.Replace(")", "");

        double[] xy = Array.ConvertAll(Geometry_String.Split(' '), double.Parse);
        double[] z = new double[1];
        z[0] = 0;

        DotSpatial.Projections.ProjectionInfo src = DotSpatial.Projections.ProjectionInfo.FromProj4String(EPSG2193);
        DotSpatial.Projections.ProjectionInfo trg = DotSpatial.Projections.ProjectionInfo.FromProj4String(EPSG4126);
        DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, src, trg, 0, 1);

        return new SqlChars(xy[0].ToString());
    }

    public static string Convert_To_GeoJSON(string Geometry_String, String Source_Coordinate_System_String)
    {
        string Destination_Coordinate_System_String = EPSG4126;
        String Geometry_Type = Geometry_String.Substring(0, Geometry_String.IndexOf(" "));

        switch (Geometry_Type)
        {
            case "MULTIPOLYGON":
                Geometry_Type = "MultiPolygon";
                break;
            case "MULTILINESTRING":
                Geometry_Type = "MultiLineString";
                break;
            case "MULTIPOINT":
                Geometry_Type = "MultiPoint";
                break;
            case "POLYGON":
                Geometry_Type = "Polygon";
                break;
            case "LINESTRING":
                Geometry_Type = "LineString";
                break;
            case "POINT":
                Geometry_Type = "Point";
                break;
            default:
                Geometry_Type = "not_supported";
                return null;
        }

        Geometry_String = Geometry_String.Replace(",", " , ");
        Geometry_String = Geometry_String.Remove(0, Geometry_String.IndexOf('('));
        Geometry_String = Geometry_String.Replace("(", "[ ");
        Geometry_String = Geometry_String.Replace(")", " ]");

        string[] splitbycomma = Geometry_String.Split(',');

        foreach (var itembycomma in splitbycomma)
        {

            string tmpitem = itembycomma;
            tmpitem = tmpitem.Replace('[', ' ');
            tmpitem = tmpitem.Replace(']', ' ');
            tmpitem = tmpitem.Trim();
            string[] splitbyspace = tmpitem.Split(' ');
            for (int ibs = 0; ibs < splitbyspace.Length - 1; ibs++)
            {
                double[] x = { double.Parse(splitbyspace[ibs]) };
                double[] y = { double.Parse(splitbyspace[ibs + 1]) };
                double[] z = new double[x.Length];
                //rewrite xy array for input into Proj4
                double[] xy = new double[2 * x.Length];
                int ixy = 0;
                for (int i = 0; i <= x.Length - 1; i++)
                {
                    xy[ixy] = x[i];
                    xy[ixy + 1] = y[i];
                    z[i] = 0;
                    ixy += 2;
                }
                double[] xy_geometry = new double[xy.Length];
                Array.Copy(xy, xy_geometry, xy.Length);

                DotSpatial.Projections.ProjectionInfo src = DotSpatial.Projections.ProjectionInfo.FromProj4String(Source_Coordinate_System_String);
                DotSpatial.Projections.ProjectionInfo trg = DotSpatial.Projections.ProjectionInfo.FromProj4String(Destination_Coordinate_System_String);
                DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, src, trg, 0, x.Length);

                ixy = 0;
                for (int i = 0; i <= x.Length - 1; i++)
                {
                    Geometry_String = Geometry_String.Replace(splitbyspace[ixy] + " ", "[" + xy[ixy].ToString("G17") + " , ");
                    Geometry_String = Geometry_String.Replace(splitbyspace[ixy + 1] + " ", xy[ixy + 1].ToString("G17") + " ] ");
                    Geometry_String = Geometry_String.Replace("- ", "-");
                    ixy += 2;
                }
            }
        }

        Geometry_String = Geometry_String.Replace("  ", " ");
        Geometry_String = Geometry_String.Replace(" [ ", "[");
        Geometry_String = Geometry_String.Replace(" ] ", "]");
        Geometry_String = Geometry_String.Replace(" , ", ",");
        Geometry_String = Geometry_String.Replace("[ [", "[[");
        if (Geometry_Type == "Point") Geometry_String = Geometry_String.Replace("[[", "[").Replace("]]", "]");
        Geometry_String = "{\"type\": \"" + Geometry_Type + "\", \"coordinates\": " + Geometry_String + "}";

        return Geometry_String;
    }
}
