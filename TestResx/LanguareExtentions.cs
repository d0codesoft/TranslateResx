using DeepL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources.NetStandard;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

    public class TranslatorExtensions
    {
        private readonly Translator _translator;

        public TranslatorExtensions(string authKey)
        {
            _translator = new Translator(authKey);
        }

        public string Translate(string textTranslate, string langFrom, string langTo )
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
}
