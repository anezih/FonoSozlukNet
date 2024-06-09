namespace FonoConverter;

public class DictInfo
{
    public string FileName = "fono";
    public string Title = "fono";
    public string Author = "fono";
    public string Description = "fono";

    public DictInfo()
    {

    }

    public DictInfo(string fileName, string title, string author, string description)
    {
        this.FileName = fileName;
        this.Title = title;
        this.Author = author;
        this.Description = description;
    }
}
