namespace FonoFileFormats;

public class FonoFormat
{
    public static FormatType GuessTypeFromFileExtension(string filename)
    {
        if (filename.EndsWith("xml") | filename.EndsWith("XML"))
            return FormatType.XML;
        else if (filename.EndsWith("kdd") | filename.EndsWith("KDD"))
            return FormatType.KDD;
        else
            return FormatType.UNKNOWN;
    }

    public static BaseFonoFormat? GetFonoFormat(string path, Progress progress, FormatType fileFormat)
    {
        return fileFormat switch
        {
            FormatType.KDD => new KddReader(path, progress),
            FormatType.XML => new FonoXmlReader(path, progress),
            _ => null,
        };
    }

    public static BaseFonoFormat? GetFonoFormat(Stream stream, Progress progress, FormatType fileFormat)
    {
        return fileFormat switch
        {
            FormatType.KDD => new KddReader(stream, progress),
            FormatType.XML => new FonoXmlReader(stream, progress),
            _ => null,
        };
    }

    public static async Task<BaseFonoFormat?> GetFonoFormatAsync(Stream stream, Progress progress, FormatType fileFormat)
    {
        return fileFormat switch
        {
            FormatType.KDD => await KddReaderAsync.CreateAsync(stream, progress),
            FormatType.XML => await FonoXmlReaderAsync.CreateAsync(stream, progress),
            _ => null,
        };
    }
}