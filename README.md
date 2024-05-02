# Resx Translator

This project is a command-line tool for translating `.resx` files using either the DeepL or Google Cloud Translation service.

## Usage

TranslateResx    
- `<Folder path>`: The path to the folder containing the `.resx` files to translate.
- `<Code lang from>`: The language code of the original language (e.g., 'en-US', 'uk-UA').
- `<Code lang to>`: The language code of the target language (e.g., 'en-US', 'uk-UA').
- `<Type service>`: The translation service to use ('google' or 'deepl').

Example:

TranslateResx ./resx en-US uk-UA deepl

## Environment Variables

Depending on the translation service you choose, you need to set one of the following environment variables:

- `DEPL_KEY_API`: The API key for the DeepL service.
- `GOOGLE_CLOUD_KEY_API`: The API key for the Google Cloud Translation service.

## HTML Detection and Translation

The tool automatically detects if a string in a `.resx` file is HTML. It parses the HTML and translates each text node separately. This ensures that the HTML structure is preserved and only the text content is translated.

Please note that this feature requires the HTML to be well-formed. If the HTML is not well-formed, the translation may not be accurate.

## Limitations

This tool only translates strings (`System.String` entries in `.resx` files). Other types of resources are not translated.
