using System.IO.Compression;
using System.Text;

namespace FonoFileFormats;

public class KddReader : BaseFonoFormat
{
    private readonly FileStream? kddFileStream;
    private readonly BinaryReader kddStream;

    private static readonly byte[] Header = [0x30, 0x31, 0x2E, 0x30, 0x31];
    private const int entryLengthsOffset = 0x1D;
    private const int headwordsOffset = 0x4D5;
    private static readonly byte[] Utf16CarriageReturn = [0x0D, 0x00];
    private static readonly byte[] Utf16Newline = [0x0A, 0x00];
    private static readonly byte[] ZlibLastThreeBytes = [0x00, 0x78, 0xDA];

    public KddReader(string path)
    {
        kddFileStream = File.Open(path, FileMode.Open);
        kddStream = new BinaryReader(kddFileStream);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ReadEntries();
    }

    public KddReader(Stream stream)
    {
        kddStream = new BinaryReader(stream);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ReadEntries();
    }
    ~KddReader()
    {
        if (kddStream != null)
        {
            kddStream.Close();
            kddStream.Dispose();
        }
        if (kddFileStream != null)
        {
            kddFileStream.Close();
            kddFileStream.Dispose();
        }
    }

    private bool CheckHeader()
    {
        if (kddStream.BaseStream.Length < 5)
            return false;
        var first5Bytes = kddStream.ReadBytes(5);
        if (first5Bytes.SequenceEqual(Header))
        {
            kddStream.BaseStream.Seek(-5, SeekOrigin.Current);
            return true;
        }
        else
        {
            Console.WriteLine("Unknown header.");
            kddStream.BaseStream.Seek(-5, SeekOrigin.Current);
            return false;
        }
    }

    private void FindZlib(BinaryReader binaryReader)
    {
        while (true)
        {
            var oneByte = binaryReader.ReadByte();
            if (oneByte == 0x00)
            {
                var threeBytes = binaryReader.ReadBytes(3);
                if (threeBytes.SequenceEqual(ZlibLastThreeBytes))
                {
                    binaryReader.BaseStream.Seek(-6, SeekOrigin.Current);
                    break;
                }
                else
                {
                    binaryReader.BaseStream.Seek(-3, SeekOrigin.Current);
                }
            }
        }
    }

    private string ReadZlib(BinaryReader binaryReader, int length, int skipFromBeginning = 0)
    {
        var bytes = binaryReader.ReadBytes(length);
        var msIn = new MemoryStream(bytes);
        var msOut = new MemoryStream();
        var zlibStream = new ZLibStream(msIn, CompressionMode.Decompress);
        zlibStream.CopyTo(msOut);
        var bytesOut = msOut.ToArray().Skip(skipFromBeginning).ToArray();
        var result = Encoding.UTF8.GetString(bytesOut);
        return result;
    }

    protected override void ReadEntries()
    {
        if (!CheckHeader() | !kddStream.BaseStream.CanSeek)
            return;
        var totalWords = 0;
        kddStream.BaseStream.Seek(entryLengthsOffset, SeekOrigin.Begin);
        while (true)
        {
            var firstLetter = Encoding.Unicode.GetString(kddStream.ReadBytes(2));
            if (firstLetter == "#")
                break;
            kddStream.BaseStream.Seek(14, SeekOrigin.Current);
            var length = kddStream.ReadUInt16();
            totalWords += length;
            kddStream.BaseStream.Seek(6, SeekOrigin.Current);
        }
        entries = new List<Entry>(totalWords);
        kddStream.BaseStream.Seek(headwordsOffset, SeekOrigin.Begin);
        List<byte> headword = new();
        int headwordCnt = 0;
        while (true)
        {
            if (headwordCnt == totalWords)
                break;
            var twoBytes = kddStream.ReadBytes(2);
            if (twoBytes.SequenceEqual(Utf16CarriageReturn))
            {
                var newlineTest = kddStream.ReadBytes(2);
                if (newlineTest.SequenceEqual(Utf16Newline))
                {
                    var skip = headword[0] == 0x00 ? 8 : 0;
                    var hw = HandleHeadword(Encoding.Unicode.GetString(headword.Skip(skip).ToArray()));
                    entries.Add(new Entry());
                    entries[headwordCnt].Headword = hw;
                    headwordCnt += 1;
                    headword.Clear();
                }
                else
                {
                    kddStream.BaseStream.Seek(-2, SeekOrigin.Current);
                    headword.AddRange(twoBytes);
                }
            }
            else
            {
                headword.AddRange(twoBytes);
            }
        }
        FindZlib(kddStream);
        var abbreviationsLength = kddStream.ReadUInt16();
        kddStream.BaseStream.Seek(2, SeekOrigin.Current);
        _Abbreviations = Rtf2Html(ReadZlib(kddStream, abbreviationsLength, 4));

        FindZlib(kddStream);
        var metadataLength = kddStream.ReadUInt16();
        kddStream.BaseStream.Seek(2, SeekOrigin.Current);
        _Metadata = ReadZlib(kddStream, metadataLength, 4);

        FindZlib(kddStream);
        int definitionCnt = 0;
        while (kddStream.BaseStream.Position < kddStream.BaseStream.Length)
        {
            var definitionLength = kddStream.ReadUInt16();
            kddStream.BaseStream.Seek(2, SeekOrigin.Current);
            var definition = Rtf2Html(ReadZlib(kddStream, definitionLength, 4));
            entries[definitionCnt].Definition = definition;
            definitionCnt += 1;
            kddStream.BaseStream.Seek(4, SeekOrigin.Current);
            Console.WriteLine(definitionCnt);
        }
    }
}