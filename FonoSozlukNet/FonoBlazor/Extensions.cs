namespace FonoBlazor;

using Microsoft.AspNetCore.Components;

public static class MarkupEx
{
    public static MarkupString ToMarkup(this string str) => (MarkupString)str.Replace("<br>","");
}