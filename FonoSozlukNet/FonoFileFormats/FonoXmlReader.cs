using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FonoFileFormats;

public class FonoXmlReader : BaseFonoFormat
{
    private XmlDocument fonoXml = new XmlDocument();

    public FonoXmlReader(string path, Progress progress)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        StreamReader streamReader;
        streamReader = new StreamReader(path, Encoding.GetEncoding(1254));
        if (Is1251Necessary(streamReader))
            streamReader = new StreamReader(path, Encoding.GetEncoding(1251));
        fonoXml.Load(streamReader);
        ReadEntries(progress);
    }

    public FonoXmlReader(Stream stream, Progress progress)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        StreamReader streamReader;
        streamReader = new StreamReader(stream, Encoding.GetEncoding(1254));
        if (Is1251Necessary(streamReader))
            streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
        fonoXml.Load(streamReader);
        ReadEntries(progress);
    }

    protected override void ReadEntries(Progress progress)
    {
        var kelimeler = fonoXml.GetElementsByTagName("KELIME");
        if (kelimeler.Count == 0)
            return;
        progress.Total = kelimeler.Count;
        entries = new List<Entry>(kelimeler.Count);
        int entriesCnt = 0;
        foreach (XmlNode kelime in kelimeler)
        {
            var sozcuk = kelime.SelectSingleNode(".//SOZCUK");
            var aciklama = kelime.SelectSingleNode(".//ACIKLAMA");
            if (!(sozcuk != null && aciklama != null))
                continue;
            var headword = sozcuk.InnerText.Trim();
            var definition = aciklama.InnerText.Trim();
            if (!string.IsNullOrEmpty(headword))
            {
                var entry = new Entry
                {
                    Headword = HandleHeadword(headword),
                    Definition = Rtf2Html(definition)
                };
                entries.Add(entry);
                entriesCnt++;
                progress.Step = entriesCnt;
            }
        }
    }

    private void ResetStreamReader(StreamReader sr)
    {
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        sr.DiscardBufferedData();
    }

    private bool Is1251Necessary(StreamReader sr)
    {
        var firstLine = sr.ReadLine();
        if (string.IsNullOrEmpty(firstLine))
        {
            ResetStreamReader(sr);
            return false;
        }
        var match = Regex.Match(firstLine, "encoding=\"(.*?)\"");
        if (match.Success
            && match.Groups.Count > 1
            && match.Groups[1].Value == "windows-1251")
        {
            ResetStreamReader(sr);
            return true;
        }
        ResetStreamReader(sr);
        return false;
    }
}