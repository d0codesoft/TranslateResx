using DeepL;
using Google.Cloud.Translation.V2;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources.NetStandard;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace TranslateResx
{
    public class ResourceKey
    {
        public string Key { get; set; }
        public string Type { get; set; }
    }

    public class LanguageReaderExtensions : IDisposable
    {
        private readonly ResXResourceReader _reader;
        private readonly string path;
        private bool disposedValue;

        public LanguageReaderExtensions(string path)
        {
            this.path = path;
            _reader = new ResXResourceReader(path);
        }

        public static IEnumerable<string> FindResxFiles(string path, string lang)
        {
            return Directory.EnumerateFiles(path, $"*.{lang}.resx", SearchOption.AllDirectories);
        }


        public IEnumerable<ResourceKey> GetResourceKeys()
        {
            var keys = new List<ResourceKey>();
            foreach (DictionaryEntry entry in _reader)
            {
                if (entry.Value != null)
                {
                    keys.Add(new ResourceKey { Key = entry.Key.ToString() ?? string.Empty, Type = entry.Value.GetType().ToString() });
                }
            }
            return keys;
        }

        public string? GetResourceValue(string key)
        {
            foreach (DictionaryEntry entry in _reader)
            {
                if (entry.Key.ToString() == key)
                {
                    return entry.Value.ToString();
                }
            }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _reader.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LanguageReaderExtensions()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class LanguageWriterExtensions : IDisposable
    {
        private readonly ResXResourceWriter _writer;
        private readonly string path;
        private bool disposedValue;

        public LanguageWriterExtensions(string path)
        {
            this.path = path;
            _writer = new ResXResourceWriter(path);
        }

        public static string GetResxFileName(string fileName, string langCode)
        {
            var regex = new Regex(@"\.\w{2}-\w{2}\.resx");
            return regex.Replace(fileName, $".{langCode}.resx");
        }

        public void SetResourceValue(string key, string newValue)
        {
            _writer.AddResource(key, newValue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Close();
                    _writer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LanguageWriterExtensions()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public interface ITranslatorExtensions
    {
        string Translate(string textTranslate, string langFrom, string langTo);
        string TranslateValue(string textTranslate, string langFrom, string langTo);
    }

    public abstract class TranslateExtensions : ITranslatorExtensions
    {
        internal bool IsHtml(string input)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input);

            return doc.ParseErrors.Count() == 0 && doc.DocumentNode.DescendantsAndSelf().Any();
        }

        public string TranslateValue(string textTranslate, string langFrom, string langTo)
        {
            if (IsHtml(textTranslate))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(textTranslate);
                TranslateNodes(doc.DocumentNode, langFrom, langTo);
                return doc.DocumentNode.OuterHtml;
            }
            else
            {
                return Translate(textTranslate, langFrom, langTo);
            }
        }

        private void TranslateNodes(HtmlNode node, string langFrom, string langTo)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                // Translate the text node
                var from = LanguageCode.RemoveRegionalVariant(langFrom);
                var to = LanguageCode.RemoveRegionalVariant(langTo);
                var translatedText = Translate(node.InnerHtml, langFrom, langTo); 
                node.InnerHtml = HttpUtility.HtmlEncode(translatedText);
            }
            else
            {
                foreach (var childNode in node.ChildNodes)
                {
                    TranslateNodes(childNode, langFrom, langTo);
                }
            }
        }

        public abstract string Translate(string textTranslate, string langFrom, string langTo);
    }

    public class TranslatorDeeplExtensions : TranslateExtensions, ITranslatorExtensions
    {
        private readonly Translator _translator;

        public TranslatorDeeplExtensions(string authKey)
        {
            _translator = new Translator(authKey);
        }

        public override string Translate(string textTranslate, string langFrom, string langTo )
        {
            var from = LanguageCode.RemoveRegionalVariant(langFrom);
            var to = LanguageCode.RemoveRegionalVariant(langTo);
            var translatedText = _translator.TranslateTextAsync(
                  textTranslate,
                  from,
                  to).Result;
            return translatedText.Text;
        }
    }

    public class TranslatorGoogleExtensions : TranslateExtensions, ITranslatorExtensions
    {
        private readonly TranslationClient _client;

        public TranslatorGoogleExtensions(string apiKey)
        {
            _client = TranslationClient.CreateFromApiKey(apiKey);
        }

        public override string Translate(string textTranslate, string langFrom, string langTo)
        {
            var response = _client.TranslateText(
                text: textTranslate,
                targetLanguage: langTo,
                sourceLanguage: langFrom);
            return response.TranslatedText;
        }
    }
}
