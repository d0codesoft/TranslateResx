// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Resources.NetStandard;
using System.Xml.Linq;
using TranslateResx;

Console.WriteLine("Start translator resource");

if (args.Length < 3)
{
    Console.WriteLine("Usage: Translator <Folder path> <Code lang from ('en-US','uk-UA')> <Code lang to ('en-US','uk-UA')>");
    return;
}

var variableValue = Environment.GetEnvironmentVariable("DEPL_KEY_API");
if (string.IsNullOrEmpty(variableValue))
{
    Console.WriteLine("Environment variable DEPL_KEY_API not found, create Environment variable 'DEPL_KEY_API'");
    return;
}

string resxFolderPath = args[0];
string langFrom = args[1];
string langTo = args[2];

var resxFiles = LanguageReaderExtensions.FindResxFiles(resxFolderPath, langFrom);
if (resxFiles == null)
{
    Console.WriteLine("No files for translate found");
    return;
}

var translator = new TranslatorExtensions(variableValue);

foreach (var resxFile in resxFiles)
{
    Console.WriteLine($"Translate file: {resxFile}");
    var newNames = LanguageWriterExtensions.GetResxFileName(resxFile, langTo);

    using (var resxReader = new LanguageReaderExtensions(resxFile))
    using (var resxWriter = new LanguageWriterExtensions(newNames))
    {
        var keys = resxReader.GetResourceKeys();
        foreach (var key in keys)
        {
            Console.WriteLine($"    Translate key: {key.Key} {key.Type}");
            var value = resxReader.GetResourceValue(key.Key);
            if (value != null)
            {
                var translatedValue = translator.Translate(value, langFrom, langTo);
                if (translatedValue != null)
                {
                    resxWriter.SetResourceValue(key.Key, translatedValue);
                }
            }
            //Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
    Console.WriteLine($"Finish translate file: {newNames}");
}