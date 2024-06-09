using FonoFileFormats;

namespace FonoSozlukCli
{
    class Program
    {
        static void Main(string[] args)
        {   
            var fileName = args[0];
            var tsvPath = args[1];
            var fono = FonoFormat.GetFonoFormat(fileName, FonoFormat.GuessTypeFromFileExtension(fileName));
            if (fono != null && !string.IsNullOrEmpty(tsvPath))
            {
                fono.ToTsv(tsvPath);
            }
            else
            {
                Console.WriteLine(tsvPath);
            }
        }
    }
}